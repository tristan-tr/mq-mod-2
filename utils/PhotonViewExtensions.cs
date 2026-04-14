namespace mq_mod_2.utils;

public static class PhotonViewExtensions
{
    /// <summary>
    /// Checks if a PhotonView belongs to the local player.
    /// Mimics the game's internal extension method.
    /// </summary>
    public static bool IsMine(this PhotonView photonView)
    {
        return !Globals.online || photonView == null || photonView.isMine || (photonView.isSceneView && PhotonNetwork.isMasterClient);
    }
}
