using HarmonyLib;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace mq_mod_2.patches;

public static class SuspendPatch
{
    [HarmonyPatch(typeof(Player), nameof(Player.RegisterCooldown))]
    [HarmonyPrefix]
    public static void RegisterCooldownPrefix(SpellName spellName, ref float cooldown)
    {
        if (spellName == SpellName.Suspend)
        {
            cooldown -= 2f;
        }
    }
}
