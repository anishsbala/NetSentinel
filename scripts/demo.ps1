param(
    [string]$ApiUrl = "http://localhost:5080"
)

$ErrorActionPreference = "Stop"
if ([Uri]$ApiUrl -and ([Uri]$ApiUrl).Host -notin @("localhost", "127.0.0.1")) {
    throw "The demo script only targets a localhost API."
}

python .\python\agent\main.py `
    --agent-id demo-windows-01 `
    --hostname demo-windows-01 `
    --ip 192.168.56.20 `
    --os windows `
    --api-url $ApiUrl

python .\python\scanner\main.py `
    --target 127.0.0.1 `
    --ports 80,443,1433,3389,5080 `
    --api-url $ApiUrl

$rule = @{
    name = "Demo risky RDP rule"
    direction = "Inbound"
    action = "Allow"
    protocol = "TCP"
    portNumber = 3389
    sourceCidr = "0.0.0.0/0"
    destinationCidr = "192.168.56.20/32"
    enabled = $true
} | ConvertTo-Json

Invoke-RestMethod `
    -Method Post `
    -Uri "$ApiUrl/api/firewall-rules" `
    -ContentType "application/json" `
    -Body $rule

Write-Host "Demo data loaded. Open $ApiUrl in your browser."
