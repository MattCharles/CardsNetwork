using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public List<NetworkUnit> units = new List<NetworkUnit>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
}
