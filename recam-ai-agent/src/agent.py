import asyncio
import json
from contextlib import AsyncExitStack

from anthropic import Anthropic
from mcp import ClientSession
from mcp.client.stdio import StdioServerParameters, stdio_client

from . import config

import json
from datetime import datetime, timezone

DANGEROUS_TOOLS = {"publish_listing"}
AUDIT_LOG_PATH = "audit.log"

MODEL = "claude-sonnet-5"

MAX_TURNS = 2  # demo 用,设小一点方便你亲眼看到压缩触发；真实项目里通常是 20~50


def split_into_turns(messages: list) -> list[list]:
    """Group flat message list into turns — each turn starts at a real user question
    (not a tool_result being fed back, which is also role=user but content is a list)."""
    turns = []
    current: list = []
    for m in messages:
        if m["role"] == "user" and isinstance(m["content"], str):
            if current:
                turns.append(current)
            current = [m]
        else:
            current.append(m)
    if current:
        turns.append(current)
    return turns


async def compact_history(client: Anthropic, messages: list) -> list:
    turns = split_into_turns(messages)
    if len(turns) <= MAX_TURNS:
        return messages

    old_turns, recent_turns = turns[:-MAX_TURNS], turns[-MAX_TURNS:]
    old_text = "\n".join(
        f"{m['role']}: {m['content'] if isinstance(m['content'], str) else '[tool interaction]'}"
        for turn in old_turns for m in turn
    )
    summary = client.messages.create(
        model=MODEL,
        max_tokens=200,
        messages=[{
            "role": "user",
            "content": f"Summarize the key facts/decisions from this earlier conversation in 2-3 sentences:\n\n{old_text}",
        }],
    )
    summary_text = "".join(b.text for b in summary.content if b.type == "text")
    print(f"[agent] compacted: summarized oldest {len(old_turns)} turn(s), kept last {len(recent_turns)} verbatim\n")

    compacted = [{"role": "user", "content": f"[Earlier conversation summary]: {summary_text}"}]
    for turn in recent_turns:
        compacted.extend(turn)
    return compacted


def mcp_tool_to_anthropic_schema(tool) -> dict:
    return {
        "name": tool.name,
        "description": tool.description or "",
        "input_schema": tool.input_schema,
    }

async def pre_tool_hook(tool_name: str, tool_input: dict) -> bool:
    if tool_name not in DANGEROUS_TOOLS:
        return True
    print(f"\n[hook] '{tool_name}' is a guarded action. Arguments: {tool_input}")
    answer = input("[hook] Allow this? [y/N]: ").strip().lower()
    return answer == "y"


def post_tool_hook(tool_name: str, tool_input: dict, result_text: str) -> None:
    entry = {
        "timestamp": datetime.now(timezone.utc).isoformat(),
        "tool": tool_name,
        "input": tool_input,
        "result_preview": result_text[:200],
    }
    with open(AUDIT_LOG_PATH, "a") as f:
        f.write(json.dumps(entry) + "\n")


async def call_tool_with_hooks(session: ClientSession, tool_name: str, tool_input: dict) -> str:
    allowed = await pre_tool_hook(tool_name, tool_input)
    if not allowed:
        result_text = json.dumps({"success": False, "reason": "Blocked by user at confirmation hook."})
        post_tool_hook(tool_name, tool_input, result_text)
        return result_text

    result = await session.call_tool(tool_name, tool_input)
    result_text = "".join(c.text for c in result.content if c.type == "text")
    post_tool_hook(tool_name, tool_input, result_text)
    return result_text



async def run_agent(user_question: str) -> str:
    server_params = StdioServerParameters(command="./.venv/bin/python", args=["-m", "src.server"])

    async with AsyncExitStack() as stack:
        read, write = await stack.enter_async_context(stdio_client(server_params))
        session = await stack.enter_async_context(ClientSession(read, write))
        await session.initialize()

        tools_result = await session.list_tools()
        anthropic_tools = [mcp_tool_to_anthropic_schema(t) for t in tools_result.tools]
        print(f"[agent] discovered tools from MCP server: {[t['name'] for t in anthropic_tools]}")

        client = Anthropic(api_key=config.ANTHROPIC_API_KEY)
        messages = [{"role": "user", "content": user_question}]

        while True:
            response = client.messages.create(
                model=MODEL,
                max_tokens=1024,
                tools=anthropic_tools,
                messages=messages,
            )

            if response.stop_reason != "tool_use":
                return "".join(b.text for b in response.content if b.type == "text")

            messages.append({"role": "assistant", "content": response.content})

            tool_results = []
            for block in response.content:
                if block.type != "tool_use":
                    continue
                print(f"[agent] calling tool: {block.name}({block.input})")
                text = await call_tool_with_hooks(session, block.name, block.input)

                tool_results.append(
                    {"type": "tool_result", "tool_use_id": block.id, "content": text}
                )

            messages.append({"role": "user", "content": tool_results})


async def chat():
    server_params = StdioServerParameters(command="./.venv/bin/python", args=["-m", "src.server"])

    async with AsyncExitStack() as stack:
        read, write = await stack.enter_async_context(stdio_client(server_params))
        session = await stack.enter_async_context(ClientSession(read, write))
        await session.initialize()

        tools_result = await session.list_tools()
        anthropic_tools = [mcp_tool_to_anthropic_schema(t) for t in tools_result.tools]

        client = Anthropic(api_key=config.ANTHROPIC_API_KEY)
        messages: list = []

        print("RECAM agent ready. Type 'exit' to quit.\n")
        while True:
            user_input = input("You: ").strip()
            if user_input.lower() in ("exit", "quit"):
                break

            messages.append({"role": "user", "content": user_input})

            while True:
                response = client.messages.create(
                    model=MODEL, max_tokens=1024, tools=anthropic_tools, messages=messages,
                )
                messages.append({"role": "assistant", "content": response.content})

                if response.stop_reason != "tool_use":
                    answer = "".join(b.text for b in response.content if b.type == "text")
                    print(f"\nAgent: {answer}\n")
                    break

                tool_results = []
                for block in response.content:
                    if block.type != "tool_use":
                        continue
                    print(f"[agent] calling tool: {block.name}({block.input})")
                    text = await call_tool_with_hooks(session, block.name, block.input)
                    tool_results.append({"type": "tool_result", "tool_use_id": block.id, "content": text})
                messages.append({"role": "user", "content": tool_results})

            messages = await compact_history(client, messages)


if __name__ == "__main__":
    asyncio.run(chat())

