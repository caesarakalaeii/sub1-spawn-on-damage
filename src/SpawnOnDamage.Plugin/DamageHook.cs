using HarmonyLib;

namespace SpawnOnDamage.Plugin;

/// <summary>Postfix on LiveMixin.TakeDamage. Player has no TakeDamage of its
/// own; the player's health lives in Player.main.liveMixin. The guard is the
/// whole point: without it every creature bite anywhere on the map fires the
/// gate.</summary>
[HarmonyPatch(typeof(LiveMixin), nameof(LiveMixin.TakeDamage))]
internal static class DamageHook
{
    [HarmonyPostfix]
    private static void After(LiveMixin __instance)
    {
        if (__instance != Player.main?.liveMixin)
        {
            return;
        }

        Plugin.OnPlayerDamaged();
    }
}
