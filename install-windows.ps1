# Build the mod and install the runtime dependencies (BepInEx 5.4.23 win_x64
# plus the two mod DLLs) into the Subnautica install. Idempotent: re-running
# rebuilds and overwrites in place.
#
# Usage: .\install-windows.ps1 [-GameDir path]
#   GameDir defaults to the standard Steam library path and is also passed to
#   the build, so no SUBNAUTICA_INSTALLATION_PATH or local.props is needed.

param(
    [string]$GameDir = "${env:ProgramFiles(x86)}\Steam\steamapps\common\Subnautica"
)

$ErrorActionPreference = 'Stop'
$BepInExVersion = '5.4.23.5'
$BepInExUrl = "https://github.com/BepInEx/BepInEx/releases/download/v$BepInExVersion/BepInEx_win_x64_$BepInExVersion.zip"
$PluginProject = Join-Path $PSScriptRoot 'src\SpawnOnDamage.Plugin\SpawnOnDamage.Plugin.csproj'
$ModOut = Join-Path $PSScriptRoot 'src\SpawnOnDamage.Plugin\bin\Release\net472'

if (-not (Test-Path (Join-Path $GameDir 'Subnautica_Data\Managed'))) {
    Write-Error "no Subnautica install at '$GameDir' (pass -GameDir)"
}
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error 'dotnet SDK not found - install it first: winget install Microsoft.DotNet.SDK.10'
}

Write-Host '==> build (Release)'
dotnet build $PluginProject -c Release "-p:SubnauticaDir=$GameDir"
if ($LASTEXITCODE -ne 0) {
    Write-Error 'dotnet build failed'
}

Write-Host "==> BepInEx $BepInExVersion (win_x64)"
if (-not (Test-Path (Join-Path $GameDir 'BepInEx\core\BepInEx.dll'))) {
    $zip = Join-Path ([System.IO.Path]::GetTempPath()) "bepinex-$BepInExVersion.zip"
    Invoke-WebRequest -Uri $BepInExUrl -OutFile $zip
    Expand-Archive -Path $zip -DestinationPath $GameDir -Force
    Remove-Item $zip
    Write-Host '    installed'
} else {
    Write-Host '    already present, skipping'
}

Write-Host '==> mod DLLs'
$plugins = Join-Path $GameDir 'BepInEx\plugins'
New-Item -ItemType Directory -Force -Path $plugins | Out-Null
Copy-Item (Join-Path $ModOut 'SpawnOnDamage.dll') $plugins -Force
Copy-Item (Join-Path $ModOut 'SpawnOnDamage.Core.dll') $plugins -Force

Write-Host "==> done. Start the game once; the config appears at"
Write-Host "    $(Join-Path $GameDir 'BepInEx\config\caesarakalaeii.spawnondamage.cfg')"
