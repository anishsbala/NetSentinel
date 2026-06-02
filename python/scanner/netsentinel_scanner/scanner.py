from __future__ import annotations

import errno
import logging
import socket
import time
from concurrent.futures import ThreadPoolExecutor, as_completed
from datetime import UTC, datetime
from typing import Any

from .safety import expand_target

LOGGER = logging.getLogger(__name__)
SERVICE_NAMES = {
    22: "ssh",
    80: "http",
    443: "https",
    1433: "sql-server",
    3389: "rdp",
    5432: "postgresql",
    6379: "redis",
    8080: "http-alt",
}
REFUSED_CODES = {errno.ECONNREFUSED, 10061}


def _utc_now() -> str:
    return datetime.now(UTC).isoformat().replace("+00:00", "Z")


def probe_tcp(host: str, port: int, timeout: float) -> tuple[bool, bool]:
    """Return (host_responded, port_open) using a normal TCP connect."""
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as client:
        client.settimeout(timeout)
        result = client.connect_ex((host, port))
    if result == 0:
        return True, True
    if result in REFUSED_CODES:
        return True, False
    return False, False


def scan_target(
    target: str,
    ports: list[int],
    *,
    timeout: float = 0.5,
    max_workers: int = 10,
    probe_delay: float = 0.05,
) -> dict[str, Any]:
    if timeout <= 0 or timeout > 5:
        raise ValueError("Timeout must be greater than 0 and at most 5 seconds.")
    if max_workers < 1 or max_workers > 20:
        raise ValueError("max_workers must be between 1 and 20.")
    if probe_delay < 0 or probe_delay > 2:
        raise ValueError("probe_delay must be between 0 and 2 seconds.")

    addresses = expand_target(target)
    started_at = _utc_now()
    observations: dict[str, dict[str, Any]] = {
        str(address): {"responded": False, "open_ports": []} for address in addresses
    }

    LOGGER.info(
        "Starting authorized lab scan target=%s hosts=%d ports=%d",
        target,
        len(addresses),
        len(ports),
    )
    with ThreadPoolExecutor(max_workers=max_workers) as executor:
        futures = {}
        for address in addresses:
            for port in ports:
                future = executor.submit(probe_tcp, str(address), port, timeout)
                futures[future] = (str(address), port)
                if probe_delay:
                    time.sleep(probe_delay)

        for future in as_completed(futures):
            host, port = futures[future]
            try:
                responded, is_open = future.result()
            except OSError as exc:
                LOGGER.debug("Probe failed host=%s port=%d error=%s", host, port, exc)
                continue
            observations[host]["responded"] |= responded
            if is_open:
                observations[host]["open_ports"].append(
                    {
                        "portNumber": port,
                        "protocol": "TCP",
                        "serviceName": SERVICE_NAMES.get(port),
                    }
                )

    hosts = []
    for address, observation in observations.items():
        if not observation["responded"] and not observation["open_ports"]:
            continue
        hostname = "localhost" if address.startswith("127.") else address
        hosts.append(
            {
                "hostname": hostname,
                "ipAddress": address,
                "osType": "Unknown",
                "openPorts": sorted(
                    observation["open_ports"], key=lambda item: item["portNumber"]
                ),
            }
        )

    completed_at = _utc_now()
    LOGGER.info("Scan complete live_hosts=%d", len(hosts))
    return {
        "target": target,
        "startedAtUtc": started_at,
        "completedAtUtc": completed_at,
        "hosts": hosts,
    }
