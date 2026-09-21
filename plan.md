# sub1-spawn-on-damage — plan (rev 2, post plan-council)

Subnautica BepInEx 5 mod: on player damage, spawn a creature picked from a
configurable pool (default: Reaper, Ghost, Sea Dragon), gated by a
configurable cooldown (default 60 s).

## Interview outcome (Phase 0)

Three questions, answered; not skipped:
- Framework: plain BepInEx 5 plugin (BepInEx config + HarmonyX), no Nautilus.
- Spawn pool: any creature TechType name, invalid names logged and skipped.
- Default cooldown: 60 s.

## Machine facts (verified this session and by the plan council)

- Game install: `~/subnautica/` — Assembly-CSharp.dll and all UnityEngine
  module DLLs present. No BepInEx installed there yet.
- `nix develop` with `dotnetCorePackages.sdk_10_0` builds net472 and
  netstandard2.0 on this machine (proven by sibling repo
  ../sub1-last-man-standing; a bare net472 project built exit 0 under its
  shell with no reference-assembly package — the earlier claim that SDK 10
  needs `microsoft.netframework.referenceassemblies.net472` was wrong, its own
  citation HANDOFF.md:1801-1802 says the opposite).
- Damage API (Cecil-verified against this install): **`Player` has no
  `TakeDamage`.** Real targets: `Player.OnTakeDamage(DamageInfo)` (public) and
  `LiveMixin.TakeDamage(float originalDamage, Vector3 position, DamageType
  type, GameObject dealer)` (public). Patch: Harmony postfix on
  `LiveMixin.TakeDamage` guarded by `__instance == Player.main?.liveMixin` —
  the seam the sibling repo uses at src/Lms.Game/ZoneEnforcer.cs:411. Without
  the guard every creature bite anywhere would fire the gate.
- Spawn API (Cecil-verified): no `Spawner.Spawn`, no sync
  `CraftData.GetPrefabForTechType`. Real: `CraftData.GetPrefabForTechTypeAsync(TechType, bool)`
  returning a UWE task, `CraftData.InstantiateFromPrefabAsync(TechType,
  IOut<GameObject>, bool)`, `PrefabDatabase.GetPrefabAsync(string)`. Spawn must
  run in a coroutine; the pattern is proven in the sibling repo at
  src/Lms.Game/RoundKit.cs:207-208 and by the game's own
  `SpawnConsoleCommand.SpawnAsync`.
- TechType names `ReaperLeviathan`, `GhostLeviathan`, `SeaDragon` exist in
  this build.
- BepInEx nuget packages (`BepInEx.BaseLib`, `BepInEx.Core`) do not exist on
  nuget.org (NU1101, verified). Vendor BepInEx 5.4.23 DLLs instead; that zip
  also supplies `0Harmony.dll`, so no separate HarmonyX reference.
- Local nuget cache already has MSTest 3.11.1, FluentAssertions 7.2.2 (do not
  bump: 8.x is Xceed-licensed), PolySharp 1.16.0.

## Acceptance criteria (commands, exit 0)

1. `nix develop -c dotnet test` — Core suite green. The suite pins the
   operator-confirmed defaults: `SpawnGate.DefaultCooldownSeconds == 60`,
   default pool parses to exactly Reaper/Ghost/SeaDragon — these tests FAIL if
   a default drifts, which is what makes the defaults gated rather than
   prose. Also covers pool parsing, weighted pick distribution (seeded),
   cooldown state transitions, enabled toggle.
2. `nix develop -c dotnet build SpawnOnDamage.slnx -c Release` — net472 plugin
   compiles against the vendored BepInEx DLLs and the real game assemblies at
   `~/subnautica`.
3. Config surface (Enabled=true, SpawnPool="ReaperLeviathan,GhostLeviathan,SeaDragon",
   CooldownSeconds=60) is `Config.Bind` in plugin Awake wired to the tested
   Core values — verified by criterion 2's build plus the handoff below, not a
   separate machine gate (BepInEx writes the cfg file at first game launch,
   which this machine cannot run).
4. In-game behavior — operator handoff, not a gate: damage event → creature
   spawns near player when cooldown elapsed, none within cooldown.

## Architecture

Two projects, the sibling repo's lane split (testable half needs no game):

- `src/SpawnOnDamage.Core/` — netstandard2.0, no game types.
  - `SpawnPool`: parse comma-separated `Name[:weight]` entries; invalid
    weight → entry skipped (parse errors surfaced); `Pick(rng)` weighted
    random over entries, null when empty; `DefaultPool` =
    "ReaperLeviathan,GhostLeviathan,SeaDragon".
  - `SpawnGate`: injected clock (`Func<double>`) + `Random`; `Enabled`
    property; `DefaultCooldownSeconds = 60`; `TrySpawn()` false when disabled
    (without consuming cooldown) or within cooldown, else true and timestamp
    recorded.
- `src/SpawnOnDamage.Plugin/` — net472 BepInEx 5 plugin.
  - `Plugin.cs` (Awake): Config.Bind the three values — binding the Core
    constants (`SpawnPool.DefaultPool`, `SpawnGate.DefaultCooldownSeconds`)
    as defaults, not literals, so the cfg surface cannot drift from the
    tested defaults — then feed Core, apply Harmony patch.
  - `DamageHook.cs`: postfix on `LiveMixin.TakeDamage`, guarded to the
    player's liveMixin; on damage → gate → if spawn granted, start coroutine.
  - `LeviathanSpawner.cs`: coroutine — `CraftData.GetPrefabForTechTypeAsync`
    → instantiate at position offset ahead of the player, below surface;
    null-prefab checks logged, never thrown. Invalid pool names logged and
    skipped at Awake via `TechTypeExtensions.FromString(name, out var tt,
    true)` — the real API (Cecil-verified; TechType has no TryParse).
- `src/SpawnOnDamage.Core.Tests/` — net10.0, MSTest + FluentAssertions 7.2.2.

Config: BepInEx-managed cfg, no hand-written parsing.

## Files

```
lib/bepinex/BepInEx.dll       # vendored from BepInEx 5.4.23 x64
lib/bepinex/0Harmony.dll      # also from that zip; supplies Harmony
src/SpawnOnDamage.Core/{SpawnOnDamage.Core.csproj, SpawnPool.cs, SpawnGate.cs}
src/SpawnOnDamage.Core.Tests/{csproj, SpawnPoolTests.cs, SpawnGateTests.cs}
src/SpawnOnDamage.Plugin/{csproj, Plugin.cs, DamageHook.cs, LeviathanSpawner.cs}
```

plan.md: committed pre-work, dropped when README (step 8) supersedes it.

## Order of work (test-first)

1. Scaffold: flake.nix, slnx, three csprojs, .gitignore, local props example.
   Feature branch `feat-spawn-on-damage`. Commit.
2. Vendor BepInEx 5.4.23 x64 core DLLs into lib/bepinex/ (download release
   zip, extract BepInEx.dll + 0Harmony.dll). Commit.
3. Core SpawnPool, red-green: parse "Reaper" unweighted, "Ghost:2" weighted,
   invalid weight skipped, whitespace tolerated, empty pool → Pick null,
   DefaultPool parses to the three leviathans, seeded weighted pick
   distribution (A:2,B:1 favours A deterministically with fixed seed),
   unweighted pick uniform over entries.
4. Core SpawnGate, red-green: first TrySpawn true, within cooldown false,
   after cooldown true, disabled → false and re-enable does not punish,
   DefaultCooldownSeconds == 60, cooldown <= 0 clamped to act-per-damage.
5. Plugin: config binding + LiveMixin.TakeDamage postfix + coroutine spawn,
   wired to Core. Build against real game DLLs until exit 0. The overload set
   and spawn API were verified by the council (facts above); if the build
   contradicts a fact above, stop and re-verify rather than inventing an API.
6. Proving ladder re-run: dotnet test + dotnet build, exit codes quoted.
7. README: install (BepInEx 5.4.23 into ~/subnautica, plugin into
   BepInEx/plugins), config documentation, in-game verification steps for the
   operator.
8. Drop plan.md (README supersedes), final commit.

## Test strategy

- Core fully unit-tested net10.0, red-green for every behavior including the
  pinned defaults (that is criterion 3's gate, per the criteria lens).
- Plugin: verification rung is the compile against real assemblies plus the
  operator in-game handoff. Named, not silently downgraded.

## Risks

- `Player.OnTakeDamage` coverage unverified — not used; the LiveMixin seam is
  the proven one.
- Prefab async returning null for a valid TechType: guarded, logged.
- Spawned creature aggression/out-of-water: out of scope; spawn position aims
  below surface.
- Proton/BepInEx install on Linux: README instructions; plugin DLL itself is
  platform-agnostic.
- Scope creep (Nitrox, minimap, balance): out of scope.
