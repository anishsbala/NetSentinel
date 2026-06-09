from __future__ import annotations

import argparse
import json
import logging
import time

import requests

from .client import report_heartbeat
from .simulator import build_heartbeat


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="NetSentinel simulated telemetry agent"
    )
    parser.add_argument("--agent-id", default="demo-agent-01")
    parser.add_argument("--hostname", default="demo-windows-01")
    parser.add_argument("--ip", default="192.168.56.20")
    parser.add_argument("--os", choices=("windows", "linux"), default="windows")
    parser.add_argument("--api-url", default="http://localhost:5080")
    parser.add_argument(
        "--interval",
        type=int,
        default=0,
        help="seconds between reports; 0 sends once",
    )
    parser.add_argument("--seed", type=int, default=42)
    parser.add_argument("--no-report", action="store_true")
    return parser


def main() -> int:
    args = build_parser().parse_args()
    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s %(levelname)s %(name)s %(message)s",
    )
    if args.interval != 0 and args.interval < 5:
        logging.error("Interval must be 0 or at least 5 seconds.")
        return 2

    while True:
        try:
            payload = build_heartbeat(
                agent_id=args.agent_id,
                hostname=args.hostname,
                ip_address=args.ip,
                os_profile=args.os,
                seed=args.seed,
            )
            if args.no_report:
                print(json.dumps(payload, indent=2))
            else:
                result = report_heartbeat(args.api_url, payload)
                print(json.dumps(result, indent=2))
        except (ValueError, requests.RequestException) as exc:
            logging.error("Heartbeat failed: %s", exc)
            return 1

        if args.interval == 0:
            return 0
        time.sleep(args.interval)
