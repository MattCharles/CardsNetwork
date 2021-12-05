using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using intPair = System.Tuple<int, int>;
using moveFunction = System.Func<ushort, System.Tuple<int, int>, bool>;

public class GameMaster : NetworkBehaviour
{
    public GameObject tilePrefab;
    public GameObject spearPrefab;
    public GameObject kingPrefab;
    public Action<uint> action;
    public int max_i = 8;
    public int max_j = 8;
    public float offset = -3.5f;
    public Dictionary<intPair, GameObject> tiles = new Dictionary<intPair, GameObject>();
    public Dictionary<ushort, NetworkUnit> unitByID = new Dictionary<ushort, NetworkUnit>();
    public Dictionary<int, moveFunction> movementDictionary = new Dictionary<int, moveFunction>();
    public Dictionary<int, moveFunction> attackDictionary = new Dictionary<int, moveFunction>();
    public Dictionary<int, moveFunction> canMoveDictionary = new Dictionary<int, moveFunction>();
    public Dictionary<int, moveFunction> canAttackDictionary = new Dictionary<int, moveFunction>();
    public ulong blackId;
    public ulong whiteId;
    public ushort internalUnitId = 1;

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

                tiles[new intPair(i, j)] = go;
                go.GetComponent<NetworkTile>().xCoord = new NetworkVariable<int>(i);
                go.GetComponent<NetworkTile>().yCoord = new NetworkVariable<int>(j);


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

            SpawnUnit(spearPrefab, position, whiteId);
        }

        // King on 2, 1
        Vector2 kingPosition = new Vector2(2, 1);

        SpawnUnit(kingPrefab, kingPosition, whiteId);
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
        go.GetComponent<NetworkUnit>().internalId = new NetworkVariable<ushort>(internalUnitId);
        unitByID.Add(internalUnitId, go.GetComponent<NetworkUnit>());
        intPair tileIndex = new intPair(
            (int)boardPosition.x,
            (int)boardPosition.y
        );
        tiles[tileIndex].GetComponent<NetworkTile>().OccupantID = new NetworkVariable<ushort>(internalUnitId);
        internalUnitId++;
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

    [ServerRpc]
    public void MoveServerRpc(ushort pieceId, int targetX, int targetY)
    {

    }

    [ServerRpc]
    public void AttackServerRpc(ushort pieceId, int targetX, int targetY)
    {

    }

    //TODO: This will all only work for white

    public void BuildMovementDictionary()
    {
        movementDictionary.Add((int)MovementRuleEnum.CHESS_PAWN, (pieceId, target) =>
        {
            if (canMoveDictionary[(int)MovementRuleEnum.CHESS_PAWN].Invoke(pieceId, target))
            {
                NetworkUnit unit = unitByID[pieceId];
                unit.boardPosition = new intPair(target.Item1, target.Item2);
                return true;
            }
            else
            {
                return false;
            }
        });
    }

    public void BuildCanMoveDictionary()
    {
        canMoveDictionary.Add((int)MovementRuleEnum.CHESS_PAWN, (pieceId, target) =>
        {
            return
                unitByID[pieceId].boardPosition.Item1 + 1 == target.Item1 &&
                (!tiles[new intPair(target.Item1 + 1, target.Item2)]?.GetComponent<NetworkTile>()?.occupied ?? false);
        });
    }

    public void BuildAttackDictionary()
    {
        attackDictionary.Add((int)MovementRuleEnum.CHESS_PAWN, (pieceId, target) =>
        {
            if (attackDictionary[(int)MovementRuleEnum.CHESS_PAWN].Invoke(pieceId, target))
            {
                NetworkUnit unit = unitByID[pieceId];
                unit.boardPosition = new intPair(target.Item1, target.Item2);
                return true;
            }
            else
            {
                return false;
            }
        });
    }

    public void BuildCanAttackDictionary()
    {
        canAttackDictionary.Add((int)MovementRuleEnum.CHESS_PAWN, (pieceId, target) =>
        {
            return
                unitByID[pieceId].boardPosition.Item1 + 1 == target.Item1 &&
                Math.Abs(unitByID[pieceId].boardPosition.Item2 - target.Item2) == 1 &&
                (tiles[new intPair(target.Item1 + 1, target.Item2)]?.GetComponent<NetworkTile>()?.occupied ?? false);
        });
    }
}
