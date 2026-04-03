using System.Collections.Generic;
using DG.Tweening;
using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches.balance;

[HarmonyPatch(typeof(RocketObject), "rpcCollision")]
public class RocketDividePatch
{
    static void Postfix(RocketObject __instance)
    {
        RocketObject leftRocket = __instance;
        RocketObject rightRocket = Object.Instantiate(__instance);
        
        leftRocket.transform.Rotate(Vector3.up, -45f);
        rightRocket.transform.Rotate(Vector3.up, 45f);
    }
}
