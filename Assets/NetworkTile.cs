using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkTile : NetworkBehaviour
{
    private NetworkVariable<bool> isBackrow = new NetworkVariable<bool>(false);
    public NetworkVariable<ushort> OccupantID = new NetworkVariable<ushort>();

    public bool occupied
    {
        get
        {
            return OccupantID.Value != 0;
        }
    }


    [ServerRpc]
    public void OccupyTileServerRPC(NetworkBehaviourReference unit)
    {
        if (!occupied)
            OccupantID.Value = unit.TryGet(out NetworkUnit networkUnit) ? networkUnit.internalId.Value : (ushort)0;
    }
}
