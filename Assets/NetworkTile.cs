using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkTile : NetworkBehaviour
{
    public Tuple<int, int> position
    {
        get
        {
            return new Tuple<int, int>(xCoord.Value, yCoord.Value);
        }
    }
    public NetworkVariable<int> xCoord = new NetworkVariable<int>();
    public NetworkVariable<int> yCoord = new NetworkVariable<int>();
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
