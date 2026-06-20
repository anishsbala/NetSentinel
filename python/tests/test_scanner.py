from netsentinel_scanner import scanner


def test_scan_target_builds_api_payload(monkeypatch) -> None:
    def fake_probe(host: str, port: int, timeout: float) -> tuple[bool, bool]:
        del host, timeout
        return True, port == 8080

    monkeypatch.setattr(scanner, "probe_tcp", fake_probe)

    result = scanner.scan_target(
        "127.0.0.1",
        [80, 8080],
        probe_delay=0,
        max_workers=1,
    )

    assert result["target"] == "127.0.0.1"
    assert result["hosts"][0]["hostname"] == "localhost"
    assert result["hosts"][0]["openPorts"] == [
        {"portNumber": 8080, "protocol": "TCP", "serviceName": "http-alt"}
    ]


def test_scan_target_omits_unresponsive_host(monkeypatch) -> None:
    monkeypatch.setattr(
        scanner, "probe_tcp", lambda host, port, timeout: (False, False)
    )

    result = scanner.scan_target("127.0.0.1", [65535], probe_delay=0, max_workers=1)

    assert result["hosts"] == []
