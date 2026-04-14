using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches.balance.custom;

// ai slop, sorry

[HarmonyPatch(typeof(RocketObject), nameof(RocketObject.rpcCollision))]
public class RocketDividePatch
{
    private static readonly AccessTools.FieldRef<RocketObject, List<GameObject>> AlreadyHitRef =
        AccessTools.FieldRefAccess<RocketObject, List<GameObject>>("alreadyHit");

    static void Postfix(RocketObject __instance, List<GameObject> ___alreadyHit)
    {
        if (Globals.online)
        {
            if (__instance.photonView.isMine)
            {
                SpawnRocket(__instance, ___alreadyHit, 45f);
                SpawnRocket(__instance, ___alreadyHit, -45f);
            }
        }
        else
        {
            SpawnRocketOffline(__instance, ___alreadyHit, 45f);
            SpawnRocketOffline(__instance, ___alreadyHit, -45f);
        }
    }

    private static void SpawnRocket(RocketObject original, List<GameObject> alreadyHit, float angle)
    {
        WizardController wizard = GameUtility.GetWizard(original.GetComponent<Identity>().owner);
        if (wizard != null)
        {
            Identity identity = wizard.GetComponent<Identity>();
            Quaternion rotation = original.transform.rotation * Quaternion.Euler(0f, angle, 0f);
            Vector3 position = original.transform.position + rotation * Vector3.forward * 0.5f;
            GameObject gameObject = GameUtility.Instantiate("Objects/Rocket", position, rotation, 0);
            RocketObject newRocket = gameObject.GetComponent<RocketObject>();
            newRocket.Init(identity, original.curve, original.velocity);

            List<GameObject> newAlreadyHit = AlreadyHitRef(newRocket);
            newAlreadyHit.Clear();
            newAlreadyHit.AddRange(alreadyHit);
        }
    }

    private static void SpawnRocketOffline(RocketObject original, List<GameObject> alreadyHit, float angle)
    {
        RocketObject rocketObject = Object.Instantiate<RocketObject>(original);
        rocketObject.transform.Rotate(Vector3.up, angle);
        rocketObject.transform.position += rocketObject.transform.forward * 0.5f;

        // Ensure it has its own list and copy items to avoid shared reference issues
        AlreadyHitRef(rocketObject) = new List<GameObject>(alreadyHit);
    }
}
