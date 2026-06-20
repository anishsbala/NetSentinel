from netsentinel_scanner.client import report_scan


class FakeResponse:
    def raise_for_status(self) -> None:
        return None

    def json(self) -> dict[str, str]:
        return {"status": "accepted"}


def test_report_scan_posts_expected_endpoint(monkeypatch) -> None:
    captured = {}

    def fake_post(url, *, json, timeout):
        captured.update(url=url, payload=json, timeout=timeout)
        return FakeResponse()

    monkeypatch.setattr("netsentinel_scanner.client.requests.post", fake_post)

    result = report_scan("http://localhost:5080", {"target": "localhost"})

    assert captured["url"] == "http://localhost:5080/api/scans/results"
    assert result == {"status": "accepted"}
