using HarmonyLib;

namespace mq_mod_2.patches;

[HarmonyPatch(typeof(NetworkManager), "Awake")]
public static class NetworkSyncRatePatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        // Increase photon sync rates by three times
        // Default sendRate is 20, sendRateOnSerialize is 10
        PhotonNetwork.sendRate = 60;
        PhotonNetwork.sendRateOnSerialize = 30;
    }
}
