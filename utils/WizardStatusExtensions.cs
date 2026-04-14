namespace mq_mod_2.utils;

public static class WizardStatusExtensions
{
    /// <summary>
    /// Checks if the wizard status belongs to the local player.
    /// Excludes clones.
    /// </summary>
    public static bool IsLocalPlayer(this WizardStatus wizardStatus)
    {
        if (wizardStatus == null) return false;
        
        Identity identity = wizardStatus.GetComponent<Identity>();
        if (identity == null) return false;
        
        int localOwner = identity.localOwner;
        int? only_local_player_id = BattleManager.only_local_player_id;

        if (only_local_player_id.HasValue && localOwner == only_local_player_id.Value)
        {
            WizardController wc = wizardStatus.GetComponent<WizardController>();
            if (wc != null && !wc.isClone)
            {
                return true;
            }
        }

        return false;
    }
}
