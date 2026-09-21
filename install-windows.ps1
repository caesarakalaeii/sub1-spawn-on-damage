# Install the runtime dependencies into the Subnautica install: BepInEx 5.4.23
# x64 and this mod's two DLLs. Idempotent: re-running overwrites in place.
#
# Usage: .\install-windows.ps1 [-GameDir path]
#   GameDir defaults to the standard Steam library path.

param(
    [string]$GameDir = "${env:ProgramFiles(x86)}\Steam\steamapps\common\Subnautica"
)

$ErrorActionPreference = 'Stop'
$BepInExVersion = '5.4.23.5'
$BepInExUrl = "https://github.com/BepInEx/BepInEx/releases/download/v$BepInExVersion/BepInEx_win_x64_$BepInExVersion.zip"
$ModOut = Join-Path $PSScriptRoot 'src\SpawnOnDamage.Plugin\bin\Release\net472'

if (-not (Test-Path (Join-Path $GameDir 'Subnautica_Data\Managed'))) {
    Write-Error "no Subnautica install at '$GameDir' (pass -GameDir)"
}
if (-not (Test-Path (Join-Path $ModOut 'SpawnOnDamage.dll'))) {
    Write-Error "no built plugin at '$ModOut' - run: dotnet build SpawnOnDamage.slnx -c Release"
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
