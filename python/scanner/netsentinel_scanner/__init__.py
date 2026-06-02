"""Safe, conservative TCP scanner for authorized NetSentinel labs."""

from .safety import TargetValidationError, expand_target, parse_ports
from .scanner import scan_target

__all__ = ["TargetValidationError", "expand_target", "parse_ports", "scan_target"]
