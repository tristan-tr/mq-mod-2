using System.Collections.Generic;
using DG.Tweening;
using HarmonyLib;
using UnityEngine;

namespace mq_mod_2.patches.custom;

[HarmonyPatch(typeof(RocketObject), "rpcCollision")]
public class RocketDividePatch
{
    static void Postfix(RocketObject __instance)
    {
        if (Globals.online)
        {
            if (__instance.photonView.isMine)
            {
                WizardController wizard = GameUtility.GetWizard(__instance.GetComponent<Identity>().owner);
                if (wizard != null)
                {
                    Identity identity = wizard.GetComponent<Identity>();
                    Quaternion rotation = __instance.transform.rotation * Quaternion.Euler(0f, 45f, 0f);
                    GameObject gameObject = GameUtility.Instantiate("Objects/Rocket", __instance.transform.position, rotation, 0);
                    gameObject.GetComponent<RocketObject>().Init(identity, __instance.curve, __instance.velocity);
                }
            }
            __instance.transform.Rotate(Vector3.up, -45f);
        }
        else
        {
            RocketObject rocketObject = Object.Instantiate<RocketObject>(__instance);
            __instance.transform.Rotate(Vector3.up, -45f);
            rocketObject.transform.Rotate(Vector3.up, 45f);
        }
    }
}
