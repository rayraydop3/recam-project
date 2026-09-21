# RECAM Agent

An MCP-based AI agent that manages [RECAM](../Management-tool-backend) real estate listings through natural language. Built as a hands-on learning project while transitioning from traditional .NET/C# backend development into AI engineering, targeting the Australian job market's agentic-AI roles.

Rather than a toy demo with fake data, this agent talks to a real, previously-built production backend (ASP.NET Core + SQL Server, full auth, real business rules) over the same REST API the React frontend uses.

## What it does

Ask it things like:

- "What listings do you have in Sydney?"
- "How many bedrooms does the Sydney one have?"
- "Publish listing case 2002 and give me the share link."

The agent reasons about what it needs, calls tools to get real data from the backend, and — for actions with real-world consequences — checks with you before acting.

## Architecture

```
You (terminal chat)
   │
   ▼
agent.py            MCP Client + ReAct loop (Reason → Act → Observe)
   │  spawns as subprocess, talks over stdio
   ▼
server.py           MCP Server — exposes tools (list_listing_cases, publish_listing)
   │
   ▼
recam_client.py     Thin async HTTP client (login, JWT, retry) — same role as the
   │                frontend's axios instance
   ▼
RECAM backend (ASP.NET Core, localhost:5262)
   │
   ▼
SQL Server           Real data
```

The agent process never touches the database directly — every action goes through the same auth, validation, and business rules the human-facing app does. That's a deliberate safety property, not just a layering convenience: the backend's `[Authorize]` checks and FluentValidation rules cap what the agent can possibly do, before any agent-specific guardrail even runs.

## Design decisions (and why)

**Separate Python service, not embedded in the C# backend.**
The agent is a client of the API, exactly like the React frontend is — not a rewrite of the backend in Python. This is also how AI features get bolted onto existing production systems in practice: nobody rewrites a working backend to add an LLM layer.

**MCP instead of hardcoded function calling.**
Tools are defined once in `server.py` and discovered at runtime via `session.list_tools()` — the client never hardcodes what tools exist. This is what lets tools be reused across different clients/agents without rewriting integration code per client.

**A skill layer on top of raw tools, not raw API calls exposed directly.**
`publish_listing` isn't a 1:1 wrapper around the backend's publish endpoint. The raw endpoint (`ListingCaseService.GenerateShareTokenAsync`) does zero validation — it'll happily generate a public link for a listing with no cover image or contact info. The skill enforces the business rules a human operator would apply by hand: check cover image → check contacts → only then publish. Failures return a structured reason instead of throwing, so the agent can explain *why* in plain language instead of crashing.

**Context engineering: filtered retrieval + history compaction.**
Tools accept real filter parameters (`status`, `address`, etc.) that get pushed down to the actual database query — the model never has to read through irrelevant rows to find what it needs. Separately, `compact_history()` summarizes older conversation turns once a session runs long, keeping recent turns verbatim so multi-turn conversations don't grow the context window (and cost) without bound.

**Hooks: guardrails + audit logging.**
`call_tool_with_hooks()` wraps every tool call with a pre-hook (blocks/confirms actions with real side effects, e.g. publishing) and a post-hook (writes a JSON-line audit entry for every call — accepted, executed, or rejected). This is generic infrastructure independent of any one tool, unlike the business-rule checks inside the skill layer above.

## Project structure

```
src/
  config.py          Loads .env (API base URL, RECAM credentials, Anthropic key)
  recam_client.py     Async HTTP client wrapping the RECAM REST API
  server.py           MCP server: tool definitions (list_listing_cases, publish_listing)
  agent.py             MCP client + ReAct loop + context compaction + hooks
                        - run_agent(question): single-shot, used for scripted tests
                        - chat(): interactive multi-turn REPL (the __main__ entry point)
```

## Setup

1. RECAM backend running locally (`dotnet run` in `../Management-tool-backend`, requires
   the SQL Server Docker container up first).
2. Python env:
   ```bash
   python3 -m venv .venv
   ./.venv/bin/pip install -r requirements.txt
   ```
3. Copy `.env.example` to `.env` and fill in:
   - `RECAM_EMAIL` / `RECAM_PASSWORD` — a RECAM account (a dedicated service account,
     not a personal login, is recommended)
   - `ANTHROPIC_API_KEY` — from [platform.claude.com](https://platform.claude.com)

## Running it

```bash
./.venv/bin/python -m src.agent
```

Starts an interactive chat. Type `exit` to quit. Every tool call is logged to
`audit.log` (gitignored — local run data, not committed).

## What's next

- **Evaluation set** — a systematic test suite (not just ad-hoc manual testing) to verify the agent behaves consistently across question phrasings and edge cases.
- **More skills** — `publish_listing` is currently the only business-rule-aware skill; assigning an agent to a listing or bulk-selecting media are natural next candidates.
- **Long-term memory** — history compaction is lossy (summarized, not stored); a real memory tier would persist facts externally (e.g. a vector store) and retrieve them on demand instead of compressing them into the conversation.
