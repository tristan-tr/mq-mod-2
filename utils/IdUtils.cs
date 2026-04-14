namespace mq_mod_2.utils;

public static class IdUtils
{
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
}
