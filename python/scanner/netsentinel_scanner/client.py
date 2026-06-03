from __future__ import annotations

from typing import Any

import requests

from .safety import validate_api_url


def report_scan(
    api_url: str,
    payload: dict[str, Any],
    *,
    timeout: float = 10,
) -> dict[str, Any]:
    base_url = validate_api_url(api_url)
    response = requests.post(
        f"{base_url}/api/scans/results",
        json=payload,
        timeout=timeout,
    )
    response.raise_for_status()
    return response.json()
