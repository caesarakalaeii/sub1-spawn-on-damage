using HarmonyLib;

namespace SpawnOnDamage.Plugin;

/// <summary>Prefix+postfix on LiveMixin.TakeDamage. The postfix classifies the
/// damaged instance: the player, the vehicle the player is piloting (Seamoth,
/// Prawn — Player.GetVehicle), or the Cyclops the player is aboard
/// (Player.currentSub, isCyclops so base interiors do not count). Anything else
/// — every creature bite anywhere on the map — is ignored. TakeDamage returns
/// bool and refuses silently when invincible, so actual damage taken is
/// measured by comparing health before and after. The prefix records an
/// (instance, health) pair rather than a bare float because TakeDamage can
/// nest (an explosion during a bite): the postfix only fires when the pair
/// still names this instance, so a nested call cannot leave a stale value
/// behind for the outer one.</summary>
[HarmonyPatch(typeof(LiveMixin), nameof(LiveMixin.TakeDamage))]
internal static class DamageHook
{
    private static LiveMixin _tracked = null!;
    private static float _healthBefore;

    [HarmonyPrefix]
    private static void Before(LiveMixin __instance)
    {
        _tracked = __instance;
        _healthBefore = __instance.health;
    }

    [HarmonyPostfix]
    private static void After(LiveMixin __instance)
    {
        if (__instance != _tracked || __instance.health >= _healthBefore)
        {
            return;
        }

        Player player = Player.main;
        if (player == null)
        {
            return;
        }

        if (__instance == player.liveMixin)
        {
            Plugin.OnPlayerDamaged();
            return;
        }

        Vehicle piloted = player.GetVehicle();
        if (piloted != null && __instance == piloted.GetComponent<LiveMixin>())
        {
            Plugin.OnVehicleDamaged();
            return;
        }

        SubRoot sub = player.currentSub;
        if (sub != null && sub.isCyclops && __instance == sub.GetComponent<LiveMixin>())
        {
            Plugin.OnVehicleDamaged();
        }
    }
}
