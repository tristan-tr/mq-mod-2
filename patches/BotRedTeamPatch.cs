using HarmonyLib;
using System.Collections.Generic;

namespace mq_mod_2.patches;

public static class BotTeamConfig
{
    public const TeamColor COLOR = TeamColor.Red;
    public const string NAME = "Red Team";
}

[HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.DetermineBots))]
public static class PlayerManager_DetermineBots_Patch
{
    public static void Postfix()
    {
        foreach (var player in PlayerManager.players.Values)
        {
            if (player != null && player.inputType == InputType.AI)
            {
                player.teamColor = BotTeamConfig.COLOR;
                player.teamName = BotTeamConfig.NAME;
            }
        }
    }
}

[HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.DetermineBotsForOnlineUI))]
public static class PlayerManager_DetermineBotsForOnlineUI_Patch
{
    public static bool Prefix(ref List<TeamColor> __result)
    {
        __result = new List<TeamColor>();
        for (int i = 0; i < PlayerManager.gameSettings.numberOfBots; i++)
        {
            __result.Add(BotTeamConfig.COLOR);
        }
        return false;
    }
}

[HarmonyPatch(typeof(NetworkManager), nameof(NetworkManager.AddPlayer))]
public static class NetworkManager_AddPlayer_Patch
{
    public static void Postfix(int index)
    {
        if (PlayerManager.players.TryGetValue(index, out Player player))
        {
            if (player.inputType == InputType.AI)
            {
                player.teamColor = BotTeamConfig.COLOR;
                player.teamName = BotTeamConfig.NAME;
            }
        }
    }
}