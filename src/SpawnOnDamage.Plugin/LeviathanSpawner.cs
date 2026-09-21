using System;
using System.Collections;
using SpawnOnDamage.Core;
using UnityEngine;

namespace SpawnOnDamage.Plugin;
/// <summary>Spawns a creature near the player. Subnautica's prefab pipeline is
/// async, so this runs as a coroutine on UWE.CoroutineHost, the game's global
/// dispatcher — no plugin-owned MonoBehaviour has to survive scene loads
/// (the plugin object was destroyed by one, which is how the first in-game
/// run NRE'd inside StartCoroutine). Null prefab results are logged, never
/// thrown.</summary>
internal sealed class LeviathanSpawner
{
    private const float SpawnDistance = 30f;
    private const float SpawnDepth = 15f;

    public void BeginSpawn(string creatureName)
    {
        UWE.CoroutineHost.StartCoroutine(SpawnAsync(creatureName));
    }

    private static IEnumerator SpawnAsync(string creatureName)
    {
        if (!TechTypeExtensions.FromString(creatureName, out TechType tech, true) || tech == TechType.None)
        {
            Plugin.Log.LogWarning($"{Plugin.PluginGuid}: '{creatureName}' is not a TechType this game knows; skipped.");
            yield break;
        }

        TaskResult<GameObject> result = new TaskResult<GameObject>();
        yield return CraftData.InstantiateFromPrefabAsync(tech, result);

        GameObject spawned = result.Get();
        if (spawned == null)
        {
            Plugin.Log.LogWarning($"{Plugin.PluginGuid}: the game made no {tech}; skipped.");
            yield break;
        }

        Player player = Player.main;
        if (player == null)
        {
            UnityEngine.Object.Destroy(spawned);
            yield break;
        }

        // Ahead of the player, below the surface: leviathans spawn swimming,
        // not dropping out of the air onto a lifepod.
        Vector3 forward = player.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 position = player.transform.position + forward * SpawnDistance;
        position.y = Mathf.Min(position.y, -SpawnDepth);

        spawned.transform.position = position;
        spawned.SetActive(true);
        Plugin.Log.LogInfo($"{Plugin.PluginGuid}: spawned {tech} at {position:0.#}");
    }
}
