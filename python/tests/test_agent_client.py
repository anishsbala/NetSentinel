import pytest

from netsentinel_agent.client import report_heartbeat


class FakeResponse:
    def raise_for_status(self) -> None:
        return None

    def json(self) -> dict[str, str]:
        return {"status": "accepted"}


def test_report_heartbeat_posts_expected_endpoint(monkeypatch) -> None:
    captured = {}

    def fake_post(url, *, json, timeout):
        captured.update(url=url, payload=json, timeout=timeout)
        return FakeResponse()

    monkeypatch.setattr("netsentinel_agent.client.requests.post", fake_post)

    result = report_heartbeat("http://127.0.0.1:5080", {"agentId": "agent-01"})

    assert captured["url"] == "http://127.0.0.1:5080/api/agents/heartbeats"
    assert result == {"status": "accepted"}


def test_public_api_is_rejected_before_request() -> None:
    with pytest.raises(ValueError, match="private"):
        report_heartbeat("https://8.8.8.8", {})
