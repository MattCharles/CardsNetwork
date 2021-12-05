using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GridMaster : NetworkBehaviour
{
    public GameObject myPrefab;
    public int max_i = 8;
    public int max_j = 8;
    public int offset = 4;
    public Dictionary<Vector2, GameObject> tiles = new Dictionary<Vector2, GameObject>();
    public override void OnNetworkSpawn()
    {
        SpawnGridServerRPC();
        base.OnNetworkSpawn();
    }

    [ServerRpc]
    public void SpawnGridServerRPC()
    {
        for (int i = 0; i < max_i; i++)
        {
            for (int j = 0; j < max_j; j++)
            {
                Debug.Log(i + ", " + j + " = i, j");
                Vector2 position = new Vector2(i - offset, j - offset);
                GameObject go = Instantiate(
                    myPrefab,
                    position,
                    Quaternion.identity
                );

                tiles[position] = go;

                go.GetComponent<NetworkObject>().Spawn();
            }
        }
    }
}
