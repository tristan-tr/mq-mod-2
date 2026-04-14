using System.Collections.Generic;
using HarmonyLib;
using mq_mod_2.utils;
using UnityEngine;

namespace mq_mod_2.patches;

[HarmonyPatch(typeof(RocketObject), nameof(RocketObject.rpcCollision))]
public class RocketDividePatch
{
    private static readonly AccessTools.FieldRef<RocketObject, List<GameObject>> AlreadyHitRef =
        AccessTools.FieldRefAccess<RocketObject, List<GameObject>>("alreadyHit");

    static void Postfix(RocketObject __instance, List<GameObject> ___alreadyHit)
    {
        // Use the game's IsMine extension to check if we should spawn more rockets
        if (PatchUtils.IsMine(__instance.photonView))
        {
            SpawnRocket(__instance, ___alreadyHit, 45f);
            SpawnRocket(__instance, ___alreadyHit, -45f);
        }
    }

    private static void SpawnRocket(RocketObject original, List<GameObject> alreadyHit, float angle)
    {
        Identity identity = original.GetComponent<Identity>();
        Quaternion rotation = original.transform.rotation * Quaternion.Euler(0f, angle, 0f);
        Vector3 position = original.transform.position + rotation * Vector3.forward * 0.5f;
        
        // GameUtility.Instantiate handles both online and offline cases
        GameObject gameObject = GameUtility.Instantiate("Objects/Rocket", position, rotation, 0);
        RocketObject newRocket = gameObject.GetComponent<RocketObject>();
        newRocket.Init(identity, original.curve, original.velocity);

        // Ensure it has its own list and copy items to avoid shared reference issues
        AlreadyHitRef(newRocket) = new List<GameObject>(alreadyHit);
    }
}
