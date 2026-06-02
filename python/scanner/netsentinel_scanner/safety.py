from __future__ import annotations

import ipaddress
from urllib.parse import urlparse

MAX_HOSTS = 256
MAX_PORTS = 20
ALLOWED_NETWORKS = (
    ipaddress.ip_network("127.0.0.0/8"),
    ipaddress.ip_network("10.0.0.0/8"),
    ipaddress.ip_network("172.16.0.0/12"),
    ipaddress.ip_network("192.168.0.0/16"),
)


class TargetValidationError(ValueError):
    """Raised when a target is outside NetSentinel's defensive lab scope."""


def expand_target(target: str) -> list[ipaddress.IPv4Address]:
    normalized = target.strip().lower()
    if normalized == "localhost":
        return [ipaddress.IPv4Address("127.0.0.1")]

    try:
        network = ipaddress.ip_network(normalized, strict=False)
    except ValueError as exc:
        raise TargetValidationError(
            "Target must be localhost, an IPv4 address, or an IPv4 CIDR."
        ) from exc

    if not isinstance(network, ipaddress.IPv4Network):
        raise TargetValidationError("Only IPv4 lab targets are supported in the MVP.")
    if not any(network.subnet_of(allowed) for allowed in ALLOWED_NETWORKS):
        raise TargetValidationError(
            "Target is outside loopback and RFC1918 private lab ranges."
        )
    if network.num_addresses > MAX_HOSTS:
        raise TargetValidationError(
            f"Target contains {network.num_addresses} addresses; maximum is {MAX_HOSTS}."  # noqa: E501
        )

    if network.prefixlen >= 31:
        return list(network)
    return list(network.hosts())


def parse_ports(value: str) -> list[int]:
    try:
        ports = sorted({int(item.strip()) for item in value.split(",") if item.strip()})
    except ValueError as exc:
        raise TargetValidationError("Ports must be comma-separated integers.") from exc

    if not ports:
        raise TargetValidationError("At least one TCP port is required.")
    if len(ports) > MAX_PORTS:
        raise TargetValidationError(f"A maximum of {MAX_PORTS} ports may be scanned.")
    if any(port < 1 or port > 65535 for port in ports):
        raise TargetValidationError("Ports must be between 1 and 65535.")
    return ports


def validate_api_url(api_url: str) -> str:
    parsed = urlparse(api_url)
    if parsed.scheme not in {"http", "https"} or not parsed.hostname:
        raise TargetValidationError("API URL must be a valid HTTP or HTTPS URL.")

    hostname = parsed.hostname.lower()
    if hostname == "localhost":
        return api_url.rstrip("/")

    try:
        address = ipaddress.ip_address(hostname)
    except ValueError as exc:
        raise TargetValidationError(
            "API host must be localhost or a private/loopback IP address."
        ) from exc

    if not isinstance(address, ipaddress.IPv4Address) or not any(
        address in network for network in ALLOWED_NETWORKS
    ):
        raise TargetValidationError(
            "API host must be localhost or a private/loopback IPv4 address."
        )
    return api_url.rstrip("/")
