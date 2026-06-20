import pytest

from netsentinel_scanner.safety import (
    TargetValidationError,
    expand_target,
    parse_ports,
    validate_api_url,
)


def test_localhost_is_allowed() -> None:
    assert [str(address) for address in expand_target("localhost")] == ["127.0.0.1"]


def test_private_subnet_is_expanded() -> None:
    addresses = expand_target("192.168.50.0/30")
    assert [str(address) for address in addresses] == [
        "192.168.50.1",
        "192.168.50.2",
    ]


@pytest.mark.parametrize("target", ["8.8.8.8", "example.com", "::1"])
def test_non_lab_target_is_rejected(target: str) -> None:
    with pytest.raises(TargetValidationError):
        expand_target(target)


def test_large_private_subnet_is_rejected() -> None:
    with pytest.raises(TargetValidationError, match="maximum"):
        expand_target("10.0.0.0/16")


def test_port_parser_deduplicates_and_sorts() -> None:
    assert parse_ports("443,22,443") == [22, 443]


def test_public_api_url_is_rejected() -> None:
    with pytest.raises(TargetValidationError):
        validate_api_url("https://8.8.8.8")
