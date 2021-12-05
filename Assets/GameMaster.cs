using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameMaster : NetworkBehaviour
{
    public GameObject tilePrefab;
    public GameObject spearPrefab;
    public GameObject kingPrefab;
    public Action<uint> action;
    public int max_i = 8;
    public int max_j = 8;
    public float offset = -3.5f;
    public Dictionary<Vector2, GameObject> tiles = new Dictionary<Vector2, GameObject>();
    public Dictionary<int, NetworkUnit> unitsByID = new Dictionary<int, NetworkUnit>();
    public ulong blackId;
    public ulong whiteId;

    private static GameMaster _instance;

    public static GameMaster Instance { get { return _instance; } }


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SpawnGridServerRPC();
        SpawnBlackServerRPC();
        SpawnWhiteServerRPC();

        NetworkManager.Singleton.OnClientConnectedCallback += StartGameServerRPC;
    }

    [ServerRpc]
    public void SpawnGridServerRPC()
    {
        for (int i = 0; i < max_i; i++)
        {
            for (int j = 0; j < max_j; j++)
            {
                Vector2 position = new Vector2(i + offset, j + offset);
                GameObject go = Instantiate(
                    tilePrefab,
                    position,
                    Quaternion.identity
                );

                tiles[position] = go;
                go.GetComponent<NetworkTile>().position = new NetworkVariable<Vector2>(position);

                go.GetComponent<NetworkObject>().Spawn();
            }
        }
    }

    [ServerRpc]
    public void SpawnBlackServerRPC()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 position = new Vector2(i + 4, 5);

            SpawnUnit(spearPrefab, position, blackId);
        }

        // King on 5, 6
        Vector2 kingPosition = new Vector2(5, 6);

        SpawnUnit(kingPrefab, kingPosition, blackId);
    }

    [ServerRpc]
    public void SpawnWhiteServerRPC()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 position = new Vector2(i + 1, 2);

            SpawnUnit(spearPrefab, position, blackId);
        }

        // King on 2, 1
        Vector2 kingPosition = new Vector2(2, 1);

        SpawnUnit(kingPrefab, kingPosition, blackId);
    }

    private void SpawnUnit(GameObject prefab, Vector2 boardPosition, ulong clientId)
    {
        Vector2 newPosition = new Vector2(boardPosition.x + offset, boardPosition.y + offset);
        GameObject go = Instantiate(
            prefab,
            newPosition,
            Quaternion.identity
        );
        go.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
    }

    [ServerRpc]
    public void StartGameServerRPC(ulong cliendId)
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count == 1)
        {
            return; // Only one person; Don't assign white or black yet
        }
        else if (NetworkManager.Singleton.ConnectedClientsIds.Count == 2)
        {
            // second person has joined
            // TODO: might be weird if second and third leave but let's not worry about that just yet

            int oneOrZero = new System.Random().Next(1);
            List<ulong> ConnectedClientsIds = (List<ulong>)NetworkManager.Singleton.ConnectedClientsIds;
            blackId = ConnectedClientsIds[oneOrZero];
            whiteId = ConnectedClientsIds[1 - oneOrZero];
        }
    }
}