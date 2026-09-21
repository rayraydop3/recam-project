from mcp.server.mcpserver import MCPServer

from . import config
from .recam_client import RecamClient

server = MCPServer(name="recam-agent", version="0.1.0")

_client = RecamClient(config.RECAM_API_BASE_URL, config.RECAM_EMAIL, config.RECAM_PASSWORD)


@server.tool()
async def list_listing_cases(
    status: str | None = None,
    property_type: str | None = None,
    sale_type: str | None = None,
    address: str | None = None,
    page: int = 1,
    page_size: int = 10,
) -> list[dict]:
    """List RECAM listing cases (real estate properties), filtered and paginated.

    Filter by status ("Created", "Pending", "Delivered"), property_type
    ("House", "Unit", "Townhouse", "Villa", "Others"), sale_type ("ForSale",
    "ForRent", "Auction"), or address (partial match). Omit any filter you
    don't need — only matching rows are retrieved from the database.
    """
    result = await _client.list_listing_cases(
        status=status, property_type=property_type, sale_type=sale_type,
        address=address, page=page, page_size=page_size,
    )
    return [
        {
            "id": item["id"], "address": item["address"], "status": item["status"],
            "propertyType": item["propertyType"], "saleType": item["saleType"],
            "bedrooms": item["bedrooms"], "bathrooms": item["bathrooms"],
            "landSize": item["landSize"],
        }
        for item in result["items"]
    ]


@server.tool()
async def publish_listing(listing_case_id: int) -> dict:
    """Publish a listing case to generate a public shareable link.

    Enforces business rules the raw API does not: the listing must have
    a cover image set and at least one contact before it can be published.
    Returns {"success": False, "reason": "..."} if not ready, or
    {"success": True, "shareLink": "..."} once published.
    """
    media = await _client.get_media_for_listing(listing_case_id)
    has_cover = any(item["isCoverImage"] for item in media)
    if not has_cover:
        return {"success": False, "reason": "No cover image has been set for this listing yet."}

    contacts = await _client.get_case_contacts(listing_case_id)
    if not contacts:
        return {"success": False, "reason": "This listing has no contact added yet."}

    token = await _client.publish_listing(listing_case_id)
    return {"success": True, "shareLink": f"{config.RECAM_API_BASE_URL}/api/listingcase/view/{token}"}



if __name__ == "__main__":
    server.run(transport="stdio")
