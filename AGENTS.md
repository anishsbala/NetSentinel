# NetSentinel Agent Guide

## Purpose and safety boundary

NetSentinel is a defensive network-security and automation platform for local,
authorized labs. Code must not add exploitation, credential access, packet
sniffing, stealth, malware behavior, or unrestricted scanning.

Scanner changes must preserve these invariants:

- Targets are validated before sockets are opened.
- Only loopback, Docker/private IPv4, and RFC1918 networks are accepted.
- A scan covers at most 256 addresses and 20 explicitly selected TCP ports.
- Concurrency, connection timeouts, and pacing remain bounded.
- Tests prove that public targets are rejected.

## Repository structure

- `src/NetSentinel.Api`: ASP.NET Core API, EF Core persistence, analyzer, and dashboard.
- `tests/NetSentinel.Api.Tests`: C# unit tests using EF Core InMemory.
- `python/scanner`: conservative TCP scanner and API client.
- `python/agent`: simulated Windows/Linux telemetry agent.
- `python/tests`: Python safety, payload, and client tests.
- `docs`: architecture and API examples.
- `scripts`: local verification and demo automation.

## Engineering conventions

- Target .NET 10 with nullable reference types and warnings as errors.
- Keep controllers thin; persistence and analysis belong in scoped services.
- Use asynchronous EF Core APIs and pass cancellation tokens.
- Store timestamps as UTC `DateTimeOffset` values.
- Treat external payloads as untrusted and validate at the API boundary.
- Use Python type hints, standard logging, and explicit request timeouts.
- Keep the dashboard dependency-free unless a later task justifies a framework.
- Add focused tests with every behavior change.

## Required verification

Run `.\scripts\verify.ps1` before completing a change. At minimum:

```powershell
dotnet build NetSentinel.sln --configuration Release
dotnet test NetSentinel.sln --configuration Release
python -m ruff check python
python -m pytest python/tests
```

Never commit `.env`, build output, virtual environments, IDE state, logs, real
credentials, production host data, or unrelated personal files. Do not push,
publish, alter Git history, or run scans without explicit user authorization.
