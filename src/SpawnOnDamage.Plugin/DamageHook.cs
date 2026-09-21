using HarmonyLib;

namespace SpawnOnDamage.Plugin;

/// <summary>Prefix+postfix on LiveMixin.TakeDamage. Player has no TakeDamage of
/// its own; the player's health lives in Player.main.liveMixin. The guard is
/// the whole point: without it every creature bite anywhere on the map fires
/// the gate. TakeDamage returns bool and refuses silently when invincible, so
/// actual damage taken is measured by comparing health before and after —
/// the same pattern the sibling repo's ZoneEnforcer uses.</summary>
[HarmonyPatch(typeof(LiveMixin), nameof(LiveMixin.TakeDamage))]
internal static class DamageHook
{
    private static float _healthBefore;

    [HarmonyPrefix]
    private static void Before(LiveMixin __instance)
    {
        if (__instance == Player.main?.liveMixin)
        {
            _healthBefore = __instance.health;
        }
    }

    [HarmonyPostfix]
    private static void After(LiveMixin __instance)
    {
        if (__instance != Player.main?.liveMixin)
        {
            return;
        }

        // Prefix ran for the same instance only when the guard above held, so
        // a stale _healthBefore cannot leak across instances: non-player
        // TakeDamage calls never touch it.
        if (__instance.health >= _healthBefore)
        {
            return;
        }

        Plugin.OnPlayerDamaged();
    }
}
