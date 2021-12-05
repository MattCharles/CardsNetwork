using Unity.Netcode;
using UnityEngine;

public class NetworkTile : NetworkBehaviour
{
    public NetworkVariable<Vector2> position = new NetworkVariable<Vector2>();
    private NetworkVariable<bool> isBackrow = new NetworkVariable<bool>(false);

    private bool occupied
    {
        get
        {
            return OccupantID.Value != -1;
        }
    }

    public NetworkVariable<int> OccupantID = new NetworkVariable<int>(-1);

    [ServerRpc]
    public void OccupyTileServerRPC(NetworkBehaviourReference unit)
    {
        if (!occupied)
            OccupantID.Value = unit.TryGet(out NetworkUnit unit1) ? unit1.NetworkBehaviourId : -1;
    }
}
