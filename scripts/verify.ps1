$ErrorActionPreference = "Stop"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw ".NET 10 SDK is required. Install it from https://dotnet.microsoft.com/download."
}

dotnet restore NetSentinel.sln
dotnet build NetSentinel.sln --configuration Release --no-restore
dotnet test NetSentinel.sln --configuration Release --no-build

$python = if (Test-Path ".venv\Scripts\python.exe") {
    ".venv\Scripts\python.exe"
} else {
    "python"
}

& $python -m ruff check python
& $python -m pytest python/tests
