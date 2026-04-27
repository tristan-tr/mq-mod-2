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
