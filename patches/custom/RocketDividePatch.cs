using System.Collections.Generic;
using DG.Tweening;
using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches.custom;

// ai slop, sorry

[HarmonyPatch(typeof(RocketObject), "rpcCollision")]
public class RocketDividePatch
{
    static void Postfix(RocketObject __instance)
    {
        var alreadyHit = Traverse.Create(__instance).Field("alreadyHit").GetValue<List<GameObject>>();

        if (Globals.online)
        {
            if (__instance.photonView.isMine)
            {
                WizardController wizard = GameUtility.GetWizard(__instance.GetComponent<Identity>().owner);
                if (wizard != null)
                {
                    Identity identity = wizard.GetComponent<Identity>();
                    Quaternion rotation = __instance.transform.rotation * Quaternion.Euler(0f, 45f, 0f);
                    Vector3 position = __instance.transform.position + rotation * Vector3.forward * 0.5f;
                    GameObject gameObject = GameUtility.Instantiate("Objects/Rocket", position, rotation, 0);
                    RocketObject newRocket = gameObject.GetComponent<RocketObject>();
                    newRocket.Init(identity, __instance.curve, __instance.velocity);

                    var newAlreadyHit = Traverse.Create(newRocket).Field("alreadyHit").GetValue<List<GameObject>>();
                    newAlreadyHit.Clear();
                    newAlreadyHit.AddRange(alreadyHit);
                }
            }
            __instance.transform.Rotate(Vector3.up, -45f);
        }
        else
        {
            RocketObject rocketObject = Object.Instantiate<RocketObject>(__instance);
            __instance.transform.Rotate(Vector3.up, -45f);
            rocketObject.transform.Rotate(Vector3.up, 45f);
            rocketObject.transform.position += rocketObject.transform.forward * 0.5f;

            var newAlreadyHit = Traverse.Create(rocketObject).Field("alreadyHit").GetValue<List<GameObject>>();
            newAlreadyHit.Clear();
            newAlreadyHit.AddRange(alreadyHit);
        }
    }
}
