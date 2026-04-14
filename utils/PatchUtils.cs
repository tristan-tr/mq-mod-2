namespace mq_mod_2.utils;

public static class PatchUtils
{
    /// <summary>
    /// Checks if the wizard status belongs to the local player.
    /// Excludes clones.
    /// </summary>
    public static bool IsLocalPlayer(WizardStatus wizardStatus)
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

    /// <summary>
    /// Gets the name of a player from their owner ID.
    /// </summary>
    public static string GetPlayerName(int ownerId)
    {
        if (PlayerManager.players.TryGetValue(ownerId, out var player))
        {
            return player.name;
        }
        return ownerId == 0 ? "Environment/Self" : $"Player {ownerId}";
    }

    /// <summary>
    /// Gets the name of a spell from its source ID.
    /// </summary>
    public static string GetSpellName(int sourceId)
    {
        if (System.Enum.IsDefined(typeof(SpellName), sourceId))
        {
            return ((SpellName)sourceId).ToString();
        }
        return sourceId == -1 ? "Collision/Other" : $"Source {sourceId}";
    }

    /// <summary>
    /// Checks if a PhotonView belongs to the local player.
    /// Mimics the game's internal extension method.
    /// </summary>
    public static bool IsMine(PhotonView photonView)
    {
        return !Globals.online || photonView == null || photonView.isMine || (photonView.isSceneView && PhotonNetwork.isMasterClient);
    }
}
