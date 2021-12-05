using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameMaster : NetworkBehaviour
{
    public GameObject tilePrefab;
    public GameObject spearPrefab;
    public GameObject kingPrefab;
    public int max_i = 8;
    public int max_j = 8;
    public float offset = -3.5f;
    public Dictionary<Vector2, GameObject> tiles = new Dictionary<Vector2, GameObject>();
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        SpawnGridServerRPC();
        SpawnBlackServerRPC();
        SpawnWhiteServerRPC();
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
            Vector2 position = new Vector2(i + 4 + offset, 5 + offset);
            GameObject go = Instantiate(
                spearPrefab,
                position,
                Quaternion.identity
            );
            go.GetComponent<NetworkObject>().Spawn();
        }

        // King on 5, 6
        Vector2 kingPosition = new Vector2(5 + offset, 6 + offset);
        GameObject kingGo = Instantiate(
            kingPrefab,
            kingPosition,
            Quaternion.identity
        );
        kingGo.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    public void SpawnWhiteServerRPC()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 position = new Vector2(i + 1 + offset, 2 + offset);
            GameObject go = Instantiate(
                spearPrefab,
                position,
                Quaternion.identity
            );
            go.GetComponent<NetworkObject>().Spawn();
        }

        // King on 2, 1
        Vector2 kingPosition = new Vector2(2 + offset, 1 + offset);
        GameObject kingGo = Instantiate(
            kingPrefab,
            kingPosition,
            Quaternion.identity
        );
        kingGo.GetComponent<NetworkObject>().Spawn();
    }
}
