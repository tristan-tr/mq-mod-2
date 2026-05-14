using HarmonyLib;

namespace mq_mod_2.patches;

[HarmonyPatch(typeof(Player), nameof(Player.RegisterCooldown))]
public static class EarthTombCooldownPatch
{
    public static void Prefix(SpellName spellName, ref float cooldown)
    {
        if (spellName == SpellName.EarthTomb)
        {
            cooldown -= 2f;
        }
    }
}
