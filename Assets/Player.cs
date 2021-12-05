using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    NetworkTile selectedTile;
    ulong? id;
    public List<NetworkUnit> units = new List<NetworkUnit>();

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += StartPlayerServerRPC;
        base.OnNetworkSpawn();
    }

    [ServerRpc]
    public void StartPlayerServerRPC(ulong cliendId)
    {
        id = id ?? cliendId;
    }

    public void Update()
    {
        Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            selectedTile = hit.collider.gameObject.GetComponent<NetworkTile>();
        }
        Debug.Log("Tile selected at position: (" + selectedTile.xCoord.Value + ", " + selectedTile.yCoord.Value + ")");
    }
}
