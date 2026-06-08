from __future__ import annotations

import ipaddress
from typing import Any
from urllib.parse import urlparse

import requests


def _validate_api_url(api_url: str) -> str:
    parsed = urlparse(api_url)
    if parsed.scheme not in {"http", "https"} or not parsed.hostname:
        raise ValueError("API URL must be a valid HTTP or HTTPS URL.")
    if parsed.hostname.lower() == "localhost":
        return api_url.rstrip("/")
    try:
        address = ipaddress.ip_address(parsed.hostname)
    except ValueError as exc:
        raise ValueError("API host must be localhost or a private IP address.") from exc
    if not address.is_private and not address.is_loopback:
        raise ValueError("API host must be localhost or a private IP address.")
    return api_url.rstrip("/")


def report_heartbeat(
    api_url: str,
    payload: dict[str, Any],
    *,
    timeout: float = 10,
) -> dict[str, Any]:
    base_url = _validate_api_url(api_url)
    response = requests.post(
        f"{base_url}/api/agents/heartbeats",
        json=payload,
        timeout=timeout,
    )
    response.raise_for_status()
    return response.json()
