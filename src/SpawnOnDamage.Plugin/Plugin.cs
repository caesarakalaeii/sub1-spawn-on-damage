using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using SpawnOnDamage.Core;

namespace SpawnOnDamage.Plugin;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInProcess("Subnautica.exe")]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "caesarakalaeii.spawnondamage";
    public const string PluginName = "Spawn On Damage";
    public const string PluginVersion = "0.1.0";

    internal static ManualLogSource Log = null!;

    private static SpawnGate _gate = null!;
    private static SpawnPool _pool = null!;
    private static Random _rng = null!;

    private static LeviathanSpawner _spawner = null!;

    private void Awake()
    {
        Log = base.Logger;


        // Bind the Core constants, not literals: the cfg defaults cannot drift
        // from the tested defaults because they ARE the tested defaults.
        ConfigEntry<bool> enabled = Config.Bind("Spawn", "Enabled", true,
            "Spawn a creature when the player takes damage.");
        ConfigEntry<string> pool = Config.Bind("Spawn", "SpawnPool", SpawnPool.DefaultPool,
            "Comma-separated creature TechType names; optional weight after a colon (Name:Weight).");
        ConfigEntry<double> cooldown = Config.Bind("Spawn", "CooldownSeconds", SpawnGate.DefaultCooldownSeconds,
            "Minimum seconds between spawns.");

        _pool = SpawnPool.Parse(pool.Value);
        if (_pool.Entries.Count == 0)
        {
            Log.LogWarning($"{PluginGuid}: SpawnPool config parsed to nothing; check the cfg file.");
        }

        _gate = new SpawnGate(() => UnityEngine.Time.realtimeSinceStartup, cooldown.Value)
        {
            Enabled = enabled.Value
        };
        _rng = new Random();
        _spawner = new LeviathanSpawner();

        var harmony = new Harmony(PluginGuid);
        harmony.PatchAll(typeof(DamageHook));
        Log.LogInfo($"{PluginName} {PluginVersion} loaded; pool=[{string.Join(", ", _pool.Entries)}] cooldown={cooldown.Value}s");
    }

    internal static void OnPlayerDamaged()
    {
        if (!_gate.TrySpawn())
        {
            return;
        }

        string? pick = _pool.Pick(_rng);
        if (pick == null)
        {
            return;
        }

        _spawner.BeginSpawn(pick);
    }
}
