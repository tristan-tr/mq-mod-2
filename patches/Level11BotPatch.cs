using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace mq_mod_2.patches;

[HarmonyPatch(typeof(SelectionMenu), nameof(SelectionMenu.ChangeBotDifficulty))]
public static class SelectionMenu_ChangeBotDifficulty_Patch
{
    public static bool Prefix(SelectionMenu __instance, bool up)
    {
        int num = PlayerManager.gameSettings.botDifficulty;
        if (__instance.online)
        {
            // Original: num = (num + (up ? 1 : 10)) % 11;
            num = (num + (up ? 1 : 11)) % 12;
        }
        else
        {
            // Original: num--; num = (num + (up ? 1 : 9)) % 10; num++;
            num--;
            num = (num + (up ? 1 : 10)) % 11;
            num++;
        }
        PlayerManager.gameSettings.botDifficulty = num;
        Traverse.Create(__instance).Method("ShowBotDifficulty").GetValue();
        return false;
    }
}

[HarmonyPatch(typeof(SelectionMenu), "ShowBotDifficulty")]
public static class SelectionMenu_ShowBotDifficulty_Patch
{
    public static void Postfix(SelectionMenu __instance)
    {
        if (PlayerManager.gameSettings.botDifficulty == 11)
        {
            __instance.botDifficultyText.text = "Level 11";
            __instance.descriptionText.text = "Bots will all be Level 11 of 10. Good luck.";
        }
    }
}

[HarmonyPatch(typeof(AiController.AiStats), nameof(AiController.AiStats.SetAiStatsUsingDifficulty))]
public static class AiStats_SetAiStatsUsingDifficulty_Patch
{
    public static void Postfix(AiController.AiStats __instance, int difficulty)
    {
        if (difficulty == 11)
        {
            __instance.accuracy = 2.0f;
            __instance.dodge = 2.0f;
            __instance.curves = true;
            __instance.response = 0f;
            __instance.aggression = 1.5f;
            __instance.idle = 0f;
            __instance.opportunism = 1.5f;
            __instance.focus = 1f;
            __instance.twitch = 1.5f;
            __instance.draftResponse = 1.0f;
        }
    }
}

[HarmonyPatch(typeof(SpellComponent), "RandomRate")]
public static class SpellComponent_RandomRate_Patch
{
    public static void Postfix(ref float __result)
    {
        if (PlayerManager.gameSettings.botDifficulty == 11)
        {
            __result = Random.Range(0.01f, 0.05f);
        }
    }
}

[HarmonyPatch(typeof(Spell), nameof(Spell.GetPredictedPosition))]
public static class Spell_GetPredictedPosition_Patch
{
    public static bool Prefix(Spell __instance, ref Vector3? __result, bool includeWindUp, Transform targetTransform, Vector3 casterPos, float additionalTime)
    {
        if (PlayerManager.gameSettings.botDifficulty != 11) return true;

        if (targetTransform == null)
        {
            __result = null;
            return false;
        }

        WizardController component = targetTransform.GetComponent<WizardController>();
        if (!(component != null) || component.isClone)
        {
            __result = null;
            return false;
        }

        PositionTracker component2 = targetTransform.GetComponent<PositionTracker>();
        if (component2 == null)
        {
            __result = null;
            return false;
        }

        Vector3 targetVelocity = component2.PredictedMovementVector();
        float spellSpeed = Spell.sInitialVelocity;
        
        // If spell has no speed or target has no velocity, fallback to basic logic
        if (spellSpeed == 0f)
        {
            float mag = (targetTransform.position - casterPos).magnitude;
            float t_fallback = 0f;
            if (includeWindUp) t_fallback += Spell.sWindUp;
            t_fallback += additionalTime;
            __result = new Vector3?(targetTransform.position + targetVelocity * t_fallback);
            return false;
        }

        Vector3 targetPos = targetTransform.position;

        // Advance target position by wind up and additional time first, as they happen before the spell even starts traveling
        float preTravelTime = 0f;
        if (includeWindUp) preTravelTime += Spell.sWindUp;
        preTravelTime += additionalTime;
        
        Vector3 initialPos = targetPos + targetVelocity * preTravelTime;
        Vector3 distanceVector = initialPos - casterPos;

        // Quadratic equation coefficients
        // (V_target^2 - V_spell^2) * t^2 + 2 * Dot(distanceVector, V_target) * t + distanceVector^2 = 0
        float a = targetVelocity.sqrMagnitude - (spellSpeed * spellSpeed);
        float b = 2f * Vector3.Dot(distanceVector, targetVelocity);
        float c = distanceVector.sqrMagnitude;

        float t = -1f;

        if (Mathf.Abs(a) < 0.001f) // a is close to 0
        {
            if (Mathf.Abs(b) > 0.001f)
            {
                t = -c / b;
            }
        }
        else
        {
            float discriminant = b * b - 4f * a * c;
            if (discriminant >= 0f)
            {
                float sqrtDisc = Mathf.Sqrt(discriminant);
                float t1 = (-b + sqrtDisc) / (2f * a);
                float t2 = (-b - sqrtDisc) / (2f * a);

                if (t1 > 0f && t2 > 0f) t = Mathf.Min(t1, t2);
                else if (t1 > 0f) t = t1;
                else if (t2 > 0f) t = t2;
            }
        }

        if (t < 0f) 
        {
            // Fallback if no valid collision time (e.g. target running away faster than spell)
            float mag = distanceVector.magnitude;
            t = mag / spellSpeed;
        }

        __result = new Vector3?(initialPos + targetVelocity * t);
        return false;
    }
}
