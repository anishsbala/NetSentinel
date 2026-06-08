from __future__ import annotations

import ipaddress
import random
from datetime import UTC, datetime
from typing import Any, Literal

OsProfile = Literal["windows", "linux"]


def _utc_now() -> str:
    return datetime.now(UTC).isoformat().replace("+00:00", "Z")


def validate_lab_ip(value: str) -> str:
    try:
        address = ipaddress.ip_address(value)
    except ValueError as exc:
        raise ValueError("Agent IP must be a valid IPv4 address.") from exc
    if not isinstance(address, ipaddress.IPv4Address) or not (
        address.is_loopback or address.is_private
    ):
        raise ValueError("Agent IP must be loopback or RFC1918 private space.")
    return str(address)


def build_heartbeat(
    *,
    agent_id: str,
    hostname: str,
    ip_address: str,
    os_profile: OsProfile,
    seed: int = 42,
) -> dict[str, Any]:
    ip_address = validate_lab_ip(ip_address)
    randomizer = random.Random(seed)

    if os_profile == "windows":
        os_type = "Windows Server 2022"
        ports = [
            {"portNumber": 135, "protocol": "TCP", "serviceName": "rpc"},
            {"portNumber": 3389, "protocol": "TCP", "serviceName": "rdp"},
        ]
        services = [
            {"name": "Windows Defender", "status": "Running", "version": "simulated"},
            {"name": "Remote Desktop Services", "status": "Running", "version": None},
        ]
    elif os_profile == "linux":
        os_type = "Ubuntu 24.04 LTS"
        ports = [
            {"portNumber": 22, "protocol": "TCP", "serviceName": "ssh"},
            {"portNumber": 8080, "protocol": "TCP", "serviceName": "http-alt"},
        ]
        services = [
            {"name": "ufw", "status": "Active", "version": "simulated"},
            {"name": "sshd", "status": "Running", "version": "simulated"},
        ]
    else:
        raise ValueError("os_profile must be 'windows' or 'linux'.")

    return {
        "agentId": agent_id,
        "hostname": hostname,
        "ipAddress": ip_address,
        "osType": os_type,
        "firewallEnabled": True,
        "listeningPorts": ports,
        "services": services,
        "cpuPercent": round(randomizer.uniform(8, 42), 1),
        "memoryPercent": round(randomizer.uniform(30, 72), 1),
        "reportedAtUtc": _utc_now(),
    }
