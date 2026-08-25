<#
.SYNOPSIS
  Deploys the already-published backend to an IIS site: stops the app pool (so IIS
  releases its file locks), mirrors the published output into the site's physical
  path, then starts the app pool again.

.EXAMPLE
  ./Deploy-Backend.ps1 -SourcePath publish/api -AppPoolName WordGame-Api -PhysicalPath C:\inetpub\wwwroot\wordgame-api
#>
param(
    [Parameter(Mandatory)] [string]$SourcePath,
    [string]$AppPoolName = "WordGame-Api",
    [string]$PhysicalPath = "C:\inetpub\wwwroot\wordgame-api"
)

$ErrorActionPreference = "Stop"
Import-Module WebAdministration -ErrorAction Stop

if (-not (Test-Path $SourcePath)) {
    throw "Source path '$SourcePath' does not exist - did the publish step run?"
}

if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) {
    throw "App pool '$AppPoolName' does not exist in IIS. Create it first (see DEPLOY.md)."
}

Write-Host "==> Stopping app pool '$AppPoolName'..."
if ((Get-WebAppPoolState -Name $AppPoolName).Value -ne "Stopped") {
    Stop-WebAppPool -Name $AppPoolName
    $timeout = (Get-Date).AddSeconds(30)
    while ((Get-WebAppPoolState -Name $AppPoolName).Value -ne "Stopped" -and (Get-Date) -lt $timeout) {
        Start-Sleep -Seconds 1
    }
}

try {
    Write-Host "==> Syncing '$SourcePath' -> '$PhysicalPath'..."
    New-Item -ItemType Directory -Force -Path $PhysicalPath | Out-Null

    # /MIR mirrors the source exactly (deletes files removed since the last deploy too).
    robocopy $SourcePath $PhysicalPath /MIR /NFL /NDL /NJH /NJS /NC /NS
    if ($LASTEXITCODE -ge 8) {
        throw "robocopy failed with exit code $LASTEXITCODE"
    }
}
finally {
    Write-Host "==> Starting app pool '$AppPoolName'..."
    Start-WebAppPool -Name $AppPoolName
}

Write-Host "==> Backend deployed to '$PhysicalPath'."
$global:LASTEXITCODE = 0
