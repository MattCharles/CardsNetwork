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
    public NetworkVariable<int> xCoord = new NetworkVariable<int>(0);
    public NetworkVariable<int> yCoord = new NetworkVariable<int>(0);
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
