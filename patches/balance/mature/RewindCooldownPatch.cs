using HarmonyLib;

namespace mq_mod_2.patches.balance.mature;

[HarmonyPatch(typeof(Player), nameof(Player.RegisterCooldown))]
public static class RewindCooldownPatch
{
    public static void Prefix(SpellName spellName, ref float cooldown)
    {
        if (spellName == SpellName.Rewind)
        {
            cooldown -= 4f;
        }
    }
}
