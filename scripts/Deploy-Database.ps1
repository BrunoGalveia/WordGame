<#
.SYNOPSIS
  Applies any pending EF Core migrations to the target database. Safe to run repeatedly -
  EF Core only applies migrations that haven't been applied yet (tracked in __EFMigrationsHistory).

.PARAMETER ConnectionString
  Full Npgsql connection string for the target database. Passed as a parameter (not read
  from appsettings) so CI can supply it from a GitHub secret without writing it to disk.

.EXAMPLE
  ./Deploy-Database.ps1 -ConnectionString "Host=localhost;Port=5432;Database=wordgame;Username=postgres;Password=..."
#>
param(
    [Parameter(Mandatory)] [string]$ConnectionString
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot

if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    throw "ConnectionString is empty. In CI this means the 'PROD_DB_CONNECTION_STRING' " +
        "secret isn't set on the GitHub repo (Settings -> Secrets and variables -> Actions " +
        "-> Secrets -> New repository secret). See DEPLOY.md section 3."
}

if (-not (Get-Command dotnet-ef -ErrorAction SilentlyContinue)) {
    Write-Host "==> dotnet-ef not found, installing it as a global tool..."
    dotnet tool install --global dotnet-ef
}

Write-Host "==> Applying pending migrations..."
$env:ConnectionStrings__Default = $ConnectionString
try {
    dotnet ef database update `
        --project "$repoRoot/server/WordGame.Infrastructure/WordGame.Infrastructure.csproj" `
        --startup-project "$repoRoot/server/WordGame.Api/WordGame.Api.csproj"

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet ef database update failed with exit code $LASTEXITCODE"
    }
}
finally {
    Remove-Item Env:\ConnectionStrings__Default -ErrorAction SilentlyContinue
}

Write-Host "==> Database is up to date."
