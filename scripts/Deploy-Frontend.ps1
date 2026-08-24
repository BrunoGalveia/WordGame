<#
.SYNOPSIS
  Deploys the built frontend (client/dist) to an IIS site the same way as the backend:
  stop the app pool, mirror the files, start it again.

.EXAMPLE
  ./Deploy-Frontend.ps1 -SourcePath client/dist -AppPoolName WordGame-Web -PhysicalPath C:\inetpub\wwwroot\wordgame-web
#>
param(
    [Parameter(Mandatory)] [string]$SourcePath,
    [string]$AppPoolName = "WordGame-Web",
    [string]$PhysicalPath = "C:\inetpub\wwwroot\wordgame-web"
)

$ErrorActionPreference = "Stop"
Import-Module WebAdministration -ErrorAction Stop

if (-not (Test-Path $SourcePath)) {
    throw "Source path '$SourcePath' does not exist - did 'npm run build' run?"
}

if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) {
    throw "App pool '$AppPoolName' does not exist in IIS. Create it first (see DEPLOY.md)."
}

Write-Host "==> Stopping app pool '$AppPoolName'..."
if ((Get-WebAppPoolState -Name $AppPoolName).Value -ne "Stopped") {
    Stop-WebAppPool -Name $AppPoolName
    Start-Sleep -Seconds 2
}

try {
    Write-Host "==> Syncing '$SourcePath' -> '$PhysicalPath'..."
    New-Item -ItemType Directory -Force -Path $PhysicalPath | Out-Null

    robocopy $SourcePath $PhysicalPath /MIR /NFL /NDL /NJH /NJS /NC /NS
    if ($LASTEXITCODE -ge 8) {
        throw "robocopy failed with exit code $LASTEXITCODE"
    }
}
finally {
    Write-Host "==> Starting app pool '$AppPoolName'..."
    Start-WebAppPool -Name $AppPoolName
}

Write-Host "==> Frontend deployed to '$PhysicalPath'."
