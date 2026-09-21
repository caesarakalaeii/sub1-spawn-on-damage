using HarmonyLib;

namespace SpawnOnDamage.Plugin;

/// <summary>Postfix on LiveMixin.TakeDamage. Player has no TakeDamage of its
/// own; the player's health lives in Player.main.liveMixin. The guard is the
/// whole point: without it every creature bite anywhere on the map fires the
/// gate. The returned damage gates on damage actually taken — TakeDamage
/// refuses silently when invincible, and a refused hit is not "took damage".</summary>
[HarmonyPatch(typeof(LiveMixin), nameof(LiveMixin.TakeDamage))]
internal static class DamageHook
{
    [HarmonyPostfix]
    private static void After(LiveMixin __instance, float __result)
    {
        if (__result <= 0f || __instance != Player.main?.liveMixin)
        {
            return;
        }

        Plugin.OnPlayerDamaged();
    }
}
