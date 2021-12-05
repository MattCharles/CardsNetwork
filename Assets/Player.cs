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
        Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log(string.Format("Co-ords of mouse is [X: {0} Y: {0}]", pos.x, pos.y));
        selectedTile = GameMaster.Instance.tiles[pos].GetComponent<NetworkTile>();
    }
}
