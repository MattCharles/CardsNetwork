using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using intPair = System.Tuple<int, int>;

public struct SpaceAndOccupant
{
    public SpaceAndOccupant(NetworkTile _space, NetworkUnit _occupant = null)
    {
        space = _space;
        occupant = _occupant;
    }
    public NetworkTile space;
    public NetworkUnit? occupant;
}

public class GameMaster : NetworkBehaviour
{

    NetworkTile selectedTile;
    public GameObject reticlePrefab;
    public GameObject reticleInstance;
    public GameObject selectReticlePrefab;
    public GameObject selectReticleInstance;
    public GameObject tilePrefab;
    public GameObject spearPrefab;
    public GameObject kingPrefab;
    public Action<uint> action;
    public int max_i = 8;
    public int max_j = 8;
    public float offset = -3.5f;
    public Dictionary<intPair, SpaceAndOccupant> tiles = new Dictionary<intPair, SpaceAndOccupant>();
    public Dictionary<NetworkTile, intPair> findCoords = new Dictionary<NetworkTile, intPair>();
    public Dictionary<ushort, NetworkUnit> unitByID = new Dictionary<ushort, NetworkUnit>();
    public ulong blackId;
    public ulong whiteId;
    public ushort internalUnitId = 1;
    public ushort selectedUnitID;

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

        selectReticleInstance = Instantiate(selectReticlePrefab, Vector3.zero, Quaternion.identity);
        reticleInstance = Instantiate(reticlePrefab, Vector3.zero, Quaternion.identity);
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

                tiles[new intPair(i, j)] = new SpaceAndOccupant(go.GetComponent<NetworkTile>());
                findCoords[go.GetComponent<NetworkTile>()] = new intPair(i, j);
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
        go.GetComponent<NetworkUnit>().boardPosition = new intPair((int)boardPosition.x, (int)boardPosition.y);
        unitByID.Add(internalUnitId, go.GetComponent<NetworkUnit>());
        intPair tileIndex = new intPair(
            (int)boardPosition.x,
            (int)boardPosition.y
        );
        tiles[tileIndex] = new SpaceAndOccupant(tiles[tileIndex].space, go.GetComponent<NetworkUnit>());
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
        // TODO: assert that unit belongs to caller
        intPair target = new intPair(targetX, targetY);
        Debug.Log("Here");
        Debug.Log(canMove(MovementRuleEnum.CHESS_PAWN, pieceId, target));
        Debug.Log("Movement Rule = " + MovementRuleEnum.CHESS_PAWN);
        Debug.Log("pieceId = " + pieceId);
        Debug.Log("target = " + target);
        if (canMove(MovementRuleEnum.CHESS_PAWN, pieceId, target))
        {
            Debug.Log("Here!!");
            NetworkUnit unit = unitByID[pieceId];
            unit.transform.position = new Vector2(targetX, targetY);
        }
    }

    [ServerRpc]
    public void AttackServerRpc(ushort pieceId, int targetX, int targetY)
    {

    }

    //TODO: This will all only work for white

    private bool canMove(MovementRuleEnum movementRule, ushort pieceId, intPair target)
    {
        if (movementRule == MovementRuleEnum.CHESS_PAWN)
        {
            Debug.Log(tiles[new intPair(target.Item1 + 1, target.Item2)].occupant);
            Debug.Log("Left " + unitByID[pieceId].boardPosition.Item1 + 1); // TODO: This is way off, left is like 31 or something
            Debug.Log("Right: " + target.Item1);
            return
                unitByID[pieceId].boardPosition.Item1 + 1 == target.Item1 &&
                (tiles[new intPair(target.Item1 + 1, target.Item2)].occupant?.internalId?.Value ?? 0) == 0;
        }
        return false;
    }

    public void Update()
    {
        Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            selectedTile = hit.collider.gameObject.GetComponent<NetworkTile>();
        }

        reticleInstance.transform.position = selectedTile?.transform?.position ?? reticleInstance.transform.position;
        if (Input.GetMouseButtonDown(0))
        {
            selectReticleInstance.transform.position = selectedTile.transform.position;
            selectedUnitID = tiles[findCoords[selectedTile]].occupant.internalId.Value;

            Debug.Log("Selected Unit ID " + selectedUnitID);
        }

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit2D hit2 = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit2.collider != null)
            {
                selectedTile = hit.collider.gameObject.GetComponent<NetworkTile>();
            }
            Debug.Log("Right click! => " + " unit ID " + selectedUnitID + ": " + selectedTile.xCoord.Value + ", " + selectedTile.yCoord.Value);
            GameMaster.Instance.MoveServerRpc(selectedUnitID, selectedTile.xCoord.Value, selectedTile.yCoord.Value);
        }
    }
}
