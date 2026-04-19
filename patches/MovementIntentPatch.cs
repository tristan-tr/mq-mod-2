using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace mq_mod_2.patches;

[HarmonyPatch]
public static class MovementIntentPatch
{
    private static FieldInfo physField = AccessTools.Field(typeof(NetworkedWizard), "phys");
    private static FieldInfo idField = AccessTools.Field(typeof(NetworkedWizard), "id");
    private static FieldInfo lastMessageTimeField = AccessTools.Field(typeof(NetworkedWizard), "lastMessageTime");
    private static FieldInfo deltaTimeField = AccessTools.Field(typeof(NetworkedWizard), "deltaTime");
    private static FieldInfo correctPlayerPosField = AccessTools.Field(typeof(NetworkedWizard), "correctPlayerPos");
    private static FieldInfo correctPlayerRotField = AccessTools.Field(typeof(NetworkedWizard), "correctPlayerRot");
    private static FieldInfo deltaPositionField = AccessTools.Field(typeof(NetworkedWizard), "deltaPosition");
    private static FieldInfo lerpField = AccessTools.Field(typeof(NetworkedWizard), "lerp");
    private static FieldInfo thresholdField = AccessTools.Field(typeof(NetworkedWizard), "threshold");
    private static FieldInfo pbPhotonViewField = AccessTools.Field(typeof(PhysicsBody), "photonView");

    [HarmonyPatch(typeof(NetworkedWizard), "FixedUpdate")]
    [HarmonyPrefix]
    public static bool FixedUpdate_Prefix(NetworkedWizard __instance)
    {
        // Disable original FixedUpdate for remote wizards because we handle it in PhysicsBody_FixedUpdate_Postfix
        // to avoid double-application or overwriting issues.
        return __instance.photonView.isMine;
    }

    [HarmonyPatch(typeof(NetworkedWizard), "OnPhotonSerializeView")]
    [HarmonyPrefix]
    public static bool OnPhotonSerializeView_Prefix(NetworkedWizard __instance, PhotonStream stream, PhotonMessageInfo info)
    {
        PhysicsBody phys = (PhysicsBody)physField.GetValue(__instance);
        Identity id = (Identity)idField.GetValue(__instance);

        if (stream.isWriting)
        {
            stream.SendNext(__instance.transform.position);
            stream.SendNext(__instance.transform.rotation);
            stream.SendNext(phys.velocity);
            stream.SendNext(phys.onGround);
            stream.SendNext(id.owner);
            
            // SYNC INTENT
            stream.SendNext(phys.movementVelocity);
            stream.SendNext(phys.abilityVelocity);
            return false;
        }

        correctPlayerPosField.SetValue(__instance, (Vector3)stream.ReceiveNext());
        correctPlayerRotField.SetValue(__instance, (Quaternion)stream.ReceiveNext());
        phys.velocity = (Vector3)stream.ReceiveNext();
        phys.onGround = (bool)stream.ReceiveNext();
        id.owner = (int)stream.ReceiveNext();
        
        // RECEIVE INTENT
        phys.movementVelocity = (Vector3)stream.ReceiveNext();
        phys.abilityVelocity = (Vector3)stream.ReceiveNext();

        float time = Time.time;
        float lastMessageTime = (float)lastMessageTimeField.GetValue(__instance);
        float deltaTime = time - lastMessageTime;
        
        deltaTimeField.SetValue(__instance, deltaTime);
        lastMessageTimeField.SetValue(__instance, time);
        
        Vector3 correctPlayerPos = (Vector3)correctPlayerPosField.GetValue(__instance);
        Vector3 deltaPosition = correctPlayerPos - __instance.transform.position;
        deltaPositionField.SetValue(__instance, deltaPosition);

        float threshold = (float)thresholdField.GetValue(__instance);
        bool lerp = deltaTime == 0f || deltaPosition.sqrMagnitude > threshold * deltaTime * deltaTime;
        lerpField.SetValue(__instance, lerp);

        return false; // Skip original
    }

    [HarmonyPatch(typeof(PhysicsBody), "FixedUpdate")]
    [HarmonyPostfix]
    public static void PhysicsBody_FixedUpdate_Postfix(PhysicsBody __instance)
    {
        // If this is a remote wizard, we want to apply the correction from NetworkedWizard
        // which might have been overwritten by rig.velocity = vector6 in the original FixedUpdate.
        
        PhotonView pv = (PhotonView)pbPhotonViewField.GetValue(__instance);
        if (Globals.online && pv != null && !pv.isMine)
        {
            NetworkedWizard netWizard = __instance.GetComponent<NetworkedWizard>();
            if (netWizard != null && !netWizard.lerp)
            {
                float lastMessageTime = (float)lastMessageTimeField.GetValue(netWizard);
                float deltaTime = (float)deltaTimeField.GetValue(netWizard);
                
                if (lastMessageTime + deltaTime >= Time.time && deltaTime > 0)
                {
                    Vector3 deltaPosition = (Vector3)deltaPositionField.GetValue(netWizard);
                    // Add the correction to the velocity assigned by PhysicsBody.FixedUpdate
                    __instance.rig.velocity += deltaPosition / deltaTime;
                }
            }
        }
    }
}
