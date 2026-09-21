# sub1-spawn-on-damage

Subnautica BepInEx 5 plugin: every time the player takes damage, a creature
from a configurable pool spawns nearby. Default pool: Reaper Leviathan, Ghost
Leviathan, Sea Dragon. Default cooldown: 60 seconds.

## Build

Needs the dotnet SDK 10 (provided by the flake) and a Subnautica install at
`~/subnautica` (override via `local.props`, see `local.props.example`, or the
`SUBNAUTICA_INSTALLATION_PATH` environment variable).

```sh
nix develop -c dotnet test SpawnOnDamage.slnx          # core logic
nix develop -c dotnet build SpawnOnDamage.slnx -c Release
```

The plugin lands in
`src/SpawnOnDamage.Plugin/bin/Release/net472/` — `SpawnOnDamage.dll` plus
`SpawnOnDamage.Core.dll`.

## Install

1. Install [BepInEx 5.4.23.x](https://github.com/BepInEx/BepInEx/releases)
   into the game directory (the `BepInEx_win_x64` zip for Proton/Windows,
   `BepInEx_linux_x64` for native). Subnautica is a Windows game under
   Proton, so normally the win_x64 build.
2. Copy `SpawnOnDamage.dll` **and** `SpawnOnDamage.Core.dll` (both from the
   same output directory) into `<game>/BepInEx/plugins/`. The pool and
   cooldown logic live in the Core assembly; without it the plugin fails to
   load.
3. Start the game once; the config file appears at
   `<game>/BepInEx/config/caesarakalaeii.spawnondamage.cfg`.

## Config

|Key|Default|Meaning|
|---|---|---|
|`Enabled`|`true`|Master switch.|
|`SpawnPool`|`ReaperLeviathan,GhostLeviathan,SeaDragon`|Comma-separated creature TechType names. Optional weight after a colon: `GhostLeviathan:2` is twice as likely as an unweighted entry. Unknown names are logged and skipped.|
|`CooldownSeconds`|`60`|Minimum seconds between spawns. Lower or equal to 0 spawns on every damage event.|

## Verify in game

Not yet verified in game on this headless machine; these steps are its
first run.

1. Load a save, enable the console, `damage 10` (or let something bite you).
2. A leviathan should spawn ~30 m ahead, ~15 m below the surface.
3. Damage again immediately: no second spawn (cooldown). Wait 60 s: spawn.
4. `BepInEx/LogOutput.log` carries the plugin banner and each spawn line.

Note the pool accepts any TechType name (`Peeper`, `CrabSnake`, ...), not
just leviathans.
