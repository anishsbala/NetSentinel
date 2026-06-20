import pytest

from netsentinel_agent.simulator import build_heartbeat


def test_windows_profile_is_deterministic() -> None:
    first = build_heartbeat(
        agent_id="agent-01",
        hostname="win-lab",
        ip_address="192.168.56.20",
        os_profile="windows",
        seed=7,
    )
    second = build_heartbeat(
        agent_id="agent-01",
        hostname="win-lab",
        ip_address="192.168.56.20",
        os_profile="windows",
        seed=7,
    )

    assert first["cpuPercent"] == second["cpuPercent"]
    assert first["listeningPorts"][1]["portNumber"] == 3389
    assert first["firewallEnabled"] is True


def test_public_agent_address_is_rejected() -> None:
    with pytest.raises(ValueError, match="private"):
        build_heartbeat(
            agent_id="agent-01",
            hostname="invalid",
            ip_address="8.8.8.8",
            os_profile="linux",
        )
