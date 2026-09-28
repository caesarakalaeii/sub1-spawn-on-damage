# sub1-spawn-on-damage

Subnautica BepInEx 5 plugin: every time the player takes damage, a creature
from a configurable pool spawns nearby. Default pool: Reaper Leviathan, Ghost
Leviathan, Sea Dragon. Default cooldown: 60 seconds.

## Build

Needs the dotnet SDK 10 (provided by the flake) and a Subnautica install at
the default Steam library path (override via `local.props`, see
`local.props.example`, or the `SUBNAUTICA_INSTALLATION_PATH` environment
variable).

```sh
nix develop -c dotnet test SpawnOnDamage.slnx          # core logic
nix develop -c dotnet build SpawnOnDamage.slnx -c Release
```

The plugin lands in
`src/SpawnOnDamage.Plugin/bin/Release/net472/` — `SpawnOnDamage.dll` plus
`SpawnOnDamage.Core.dll`.

## Install

The Windows installer builds the plugin first (needs the dotnet SDK 10:
`winget install Microsoft.DotNet.SDK.10`), then fetches BepInEx 5.4.23 win_x64
into the game dir and copies both mod DLLs into `BepInEx/plugins/`. The Linux
script installs pre-built DLLs (build them with the flake as above). Both
installers are idempotent.

```sh
# Linux (game at the default Steam path)
./install-linux.sh
```

```powershell
# Windows (PowerShell's default execution policy blocks .ps1 files)
powershell -ExecutionPolicy Bypass -File .\install-windows.ps1

# Game on another drive or library:
powershell -ExecutionPolicy Bypass -File .\install-windows.ps1 -GameDir 'F:\SteamLibrary\steamapps\common\Subnautica'
```

**Proton (Linux Steam) — required, or BepInEx never loads.** Wine ignores
the `winhttp.dll` proxy by default, so doorstop never runs and the game
starts vanilla. In Steam: Subnautica → Properties → Launch Options:

```text
WINEDLLOVERRIDES="winhttp=n,b" %command%
```

Then start the game once; the config appears at
`<game>/BepInEx/config/caesarakalaeii.spawnondamage.cfg`.

## Config

|Key|Default|Meaning|
|---|---|---|
|`Enabled`|`true`|Master switch.|
|`SpawnPool`|`ReaperLeviathan,GhostLeviathan,SeaDragon`|Comma-separated creature TechType names. Optional weight after a colon: `GhostLeviathan:2` is twice as likely as an unweighted entry. Unknown names are logged and skipped.|
|`CooldownSeconds`|`60`|Minimum seconds between spawns. Lower or equal to 0 spawns on every damage event.|

## Verify in game

1. Load a save, enable the console, `takedamage 20` (or let something bite you).
2. A leviathan should spawn ~30 m ahead, ~15 m below the surface.
3. Damage again immediately: no second spawn (cooldown). Wait 60 s: spawn.
4. `BepInEx/LogOutput.log` carries the plugin banner and each spawn line.

Note the pool accepts any TechType name (`Peeper`, `CrabSnake`, ...), not
just leviathans.

## Licence

MIT, see [LICENSE](LICENSE).
