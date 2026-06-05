from __future__ import annotations

import argparse
import json
import logging

import requests

from .client import report_scan
from .safety import TargetValidationError, parse_ports
from .scanner import scan_target

DEFAULT_PORTS = "22,80,443,1433,3389,5432,6379,8080"


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Conservative TCP scanner for authorized private/local labs only."
    )
    parser.add_argument(
        "--target", required=True, help="localhost or an RFC1918 target"
    )
    parser.add_argument("--ports", default=DEFAULT_PORTS, help="up to 20 TCP ports")
    parser.add_argument("--api-url", default="http://localhost:5080")
    parser.add_argument("--timeout", type=float, default=0.5)
    parser.add_argument("--workers", type=int, default=10)
    parser.add_argument("--delay", type=float, default=0.05)
    parser.add_argument(
        "--no-report",
        action="store_true",
        help="print results without sending them to the API",
    )
    parser.add_argument("--verbose", action="store_true")
    return parser


def main() -> int:
    args = build_parser().parse_args()
    logging.basicConfig(
        level=logging.DEBUG if args.verbose else logging.INFO,
        format="%(asctime)s %(levelname)s %(name)s %(message)s",
    )

    try:
        ports = parse_ports(args.ports)
        payload = scan_target(
            args.target,
            ports,
            timeout=args.timeout,
            max_workers=args.workers,
            probe_delay=args.delay,
        )
        if args.no_report:
            print(json.dumps(payload, indent=2))
        else:
            result = report_scan(args.api_url, payload)
            print(json.dumps(result, indent=2))
    except (TargetValidationError, ValueError) as exc:
        logging.error("Safety validation failed: %s", exc)
        return 2
    except requests.RequestException as exc:
        logging.error("Could not report scan to NetSentinel API: %s", exc)
        return 1
    return 0
