import httpx


class RecamClient:
    """Thin async wrapper over the RECAM REST API — same role as the frontend's axios instance."""

    def __init__(self, base_url: str, email: str, password: str):
        self._email = email
        self._password = password
        self._token: str | None = None
        self._http = httpx.AsyncClient(base_url=base_url.rstrip("/"))

    async def _login(self) -> str:
        response = await self._http.post(
            "/api/auth/login",
            json={"email": self._email, "password": self._password},
        )
        response.raise_for_status()
        body = response.json()
        if not body["succeed"]:
            raise RuntimeError(f"RECAM login failed: {body.get('errorMessage')}")
        self._token = body["data"]
        return self._token

    async def _authed_get(self, path: str, params: dict | None = None) -> dict:
        if self._token is None:
            await self._login()

        response = await self._http.get(
            path, params=params, headers={"Authorization": f"Bearer {self._token}"}
        )
        if response.status_code == 401:
            await self._login()
            response = await self._http.get(
                path, params=params, headers={"Authorization": f"Bearer {self._token}"}
            )

        response.raise_for_status()
        body = response.json()
        if not body["succeed"]:
            raise RuntimeError(f"RECAM API error: {body.get('errorMessage')}")
        return body["data"]


    async def list_listing_cases(
        self,
        status: str | None = None,
        property_type: str | None = None,
        sale_type: str | None = None,
        address: str | None = None,
        page: int = 1,
        page_size: int = 10,
    ) -> dict:
        params = {
            "status": status,
            "propertyType": property_type,
            "saleType": sale_type,
            "address": address,
            "page": page,
            "pageSize": page_size,
        }
        params = {k: v for k, v in params.items() if v is not None}
        return await self._authed_get("/api/listingcase", params=params)

    async def get_media_for_listing(self, listing_case_id: int) -> list[dict]:
        return await self._authed_get(f"/api/media/{listing_case_id}")

    async def get_case_contacts(self, listing_case_id: int) -> list[dict]:
        return await self._authed_get(f"/api/casecontact/{listing_case_id}")

    async def publish_listing(self, listing_case_id: int) -> str:
        if self._token is None:
            await self._login()
        response = await self._http.post(
            f"/api/listingcase/{listing_case_id}/publish",
            headers={"Authorization": f"Bearer {self._token}"},
        )
        response.raise_for_status()
        body = response.json()
        if not body["succeed"]:
            raise RuntimeError(f"RECAM API error: {body.get('errorMessage')}")
        return body["data"]


    async def aclose(self) -> None:
        await self._http.aclose()
