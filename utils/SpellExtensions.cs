using System.Collections.Generic;

public static class SpellExtensions
{
    private static readonly Dictionary<SpellName, float> SpellDamageMap = new Dictionary<SpellName, float>
    {
        { SpellName.Fireball, 10f },
        { SpellName.Rocket, 10f }, // Assuming baseline
        { SpellName.Rockshot, 12f },
        { SpellName.Snowball, 8f },
        { SpellName.Glaive, 8f },
        { SpellName.Boomerang, 7f },
        { SpellName.Bombshell, 10f },
        { SpellName.Brrage, 4f }, // Per projectile
        { SpellName.ChainLightning, 12f },
        { SpellName.Capacitor, 15f },
        { SpellName.BullRush, 15f },
        { SpellName.FlameLeash, 8f },
        { SpellName.Scorch, 10f },
        { SpellName.Sunder, 13f },
        { SpellName.PetRock, 15f },
        { SpellName.Urchain, 10f },
        { SpellName.Static, 10f },
        { SpellName.Petrify, 0f },
        { SpellName.Tetherball, 10f },
        { SpellName.Geyser, 10f },
        { SpellName.ColdFusion, 10f },
        { SpellName.Spitfire, 15f },
        { SpellName.Fissure, 20f },
        { SpellName.Tsunami, 15f },
        { SpellName.JetStream, 15f },
        { SpellName.DoubleStrike, 0f }, // Buff
        { SpellName.PillarOfFire, 6f }
    };

    public static float GetDamage(this Spell spell)
    {
        if (SpellDamageMap.TryGetValue(spell.spellName, out float damage))
        {
            return damage;
        }
        return 10f; // Default baseline damage
    }
}
