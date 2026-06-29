# Security Policy

NetSentinel is a defensive learning project for isolated local labs.

## Authorized use only

Run scans only against systems you own or have explicit permission to test. The
scanner intentionally rejects public IP addresses, public DNS names, IPv6
targets, and private subnets larger than 256 addresses.

The project does not include packet sniffing, credential collection, exploit
code, persistence mechanisms, malware behavior, or techniques intended to evade
monitoring. Do not add those capabilities in future contributions.

## Reporting a project security issue

Do not include secrets, real infrastructure details, or personal data in a
public issue. Report reproducible defects with sanitized local-lab examples.

The default development password is an example credential for the disposable
local SQL Server container. Change it for any shared or long-lived environment,
and provide the corresponding API connection string through environment
variables rather than committing credentials.
