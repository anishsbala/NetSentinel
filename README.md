# NetSentinel

NetSentinel is an enterprise-style network security and automation platform for
an isolated local lab. It combines an ASP.NET Core API, SQL Server persistence,
a conservative Python TCP scanner, simulated endpoint telemetry, firewall-rule
analysis, and a lightweight operations dashboard.

> **Safety:** Scan only systems you own or are explicitly authorized to test.
> The scanner accepts only `localhost`, loopback, Docker/private IPv4 addresses,
> and RFC1918 lab networks. It rejects public targets before opening a socket.

## Overview

The MVP demonstrates:

- Device, scan, port, firewall-rule, alert, and heartbeat APIs
- Entity Framework Core persistence in SQL Server
- Safe, bounded TCP connection checks with Python
- Simulated Windows and Linux host telemetry
- Detection of exposed RDP, SSH, SQL Server, and conflicting rules
- A dependency-free dashboard served by ASP.NET Core
- Docker-based local infrastructure and automated tests

## Architecture

```text
Python scanner ---- scan results ----\
                                      > ASP.NET Core API --> EF Core --> SQL Server
Python agent ------ heartbeats ------/          |
                                                +--> firewall analyzer --> alerts
Browser dashboard <------ read APIs ------------+
```

See [docs/architecture.md](docs/architecture.md) for the component data flow.

## Tech stack

- C# / .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10 / SQL Server 2022
- Python 3.12+ / Requests / Pytest / Ruff
- HTML / CSS / JavaScript
- Docker Compose / GitHub Actions

## Safety scope

This project is strictly defensive and local-lab only. It does not perform raw
packet capture, credential access, vulnerability exploitation, persistence,
evasion, or malware-like behavior.

The scanner enforces:

- IPv4 loopback or RFC1918 targets only
- No public hostnames or public IP addresses
- At most 256 addresses and 20 ports per run
- At most 20 workers, bounded timeouts, and configurable pacing
- TCP connection attempts only

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Python 3.12 or newer
- Docker Desktop with Linux containers
- PowerShell 7 or Windows PowerShell 5.1

## Setup

```powershell
git clone <your-repository-url>
cd NetSentinel
Copy-Item .env.example .env
docker compose up -d

python -m venv .venv
.\.venv\Scripts\Activate.ps1
python -m pip install -r python\requirements-dev.txt

dotnet restore NetSentinel.sln
```

The API's development connection string matches `.env.example`. If you change
the SQL password or port, set `ConnectionStrings__DefaultConnection` in your
shell before starting the API. Do not commit `.env`.

## Running locally

Start SQL Server and wait for a healthy status:

```powershell
docker compose up -d
docker compose ps
```

Start the API and dashboard:

```powershell
dotnet run --project src\NetSentinel.Api
```

Open <http://localhost:5080>. OpenAPI JSON is available at
<http://localhost:5080/openapi/v1.json> and health status at
<http://localhost:5080/health>.

Send one simulated Windows heartbeat:

```powershell
python .\python\agent\main.py --os windows --ip 192.168.56.20
```

Run a safe localhost scan:

```powershell
python .\python\scanner\main.py --target 127.0.0.1 --ports 80,443,1433,3389,5080
```

Preview either Python payload without calling the API by adding `--no-report`.

## Demo flow

With SQL Server and the API running:

```powershell
.\scripts\demo.ps1
```

The script sends a simulated heartbeat, scans localhost, and submits an
intentionally risky lab firewall rule. Refresh the dashboard to show the device,
open ports, scan history, heartbeat, firewall rule, and generated critical
alert. The script refuses non-local API hosts.

Manual API examples are in [docs/api-examples.http](docs/api-examples.http).

## API summary

| Method | Path | Purpose |
| --- | --- | --- |
| GET | `/api/devices` | List discovered and reported devices |
| GET | `/api/scans` | List recent scan history |
| POST | `/api/scans/results` | Ingest a completed scanner report |
| GET | `/api/open-ports` | List observed TCP ports |
| GET/POST | `/api/firewall-rules` | Store rules and generate findings |
| GET | `/api/alerts` | List generated security alerts |
| GET/POST | `/api/agents/heartbeats` | Read or ingest telemetry |
| GET | `/health` | Check API and database health |

## Testing

Run every local check:

```powershell
.\scripts\verify.ps1
```

Or run components separately:

```powershell
dotnet build NetSentinel.sln --configuration Release
dotnet test NetSentinel.sln --configuration Release
python -m ruff check python
python -m pytest python\tests
```

## Troubleshooting

- If the API cannot connect, run `docker compose ps` and wait for SQL Server to
  become healthy.
- If port 1433 is occupied, change `SQLSERVER_PORT` in `.env` and update the API
  connection string.
- If PowerShell blocks virtual-environment activation, run Python through
  `.\.venv\Scripts\python.exe` directly.
- The API applies the checked-in EF Core migration at startup by default.

## Future improvements

- Authentication and role-based access for shared lab deployments
- Real opt-in agents with platform-specific telemetry collectors
- Alert acknowledgement and investigation workflows
- Background scan scheduling and queue-based ingestion
- Rule import adapters for Windows Firewall and Linux nftables
- Historical metrics, retention policies, and dashboard charts
- Container images and deployment manifests for the full stack

## License

No license has been selected yet. Treat the repository as all rights reserved
until a license is added.
