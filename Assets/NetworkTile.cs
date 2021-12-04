using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkTile : NetworkBehaviour
{
    private NetworkVariable<bool> isBackrow = false;

    private NetworkVariable<bool> occupied = {
        Occupant.HasValue
    };

    public NetworkUnit? Occupant;

    public Occupy(NetworkUnit unit)
    {
        Occupant = unit;
    }
}
