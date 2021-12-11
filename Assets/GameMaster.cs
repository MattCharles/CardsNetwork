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
    public NetworkTile tilePrefab;
    public NetworkUnit spearPrefab;
    public NetworkUnit kingPrefab;
    public Action<uint> action;
    public int max_i = 8;
    public int max_j = 8;
    public float offset = -3.5f;
    public List<List<NetworkTile>> tiles;
    public List<List<NetworkUnit>> units;
    public Dictionary<NetworkTile, intPair> findCoords = new Dictionary<NetworkTile, intPair>();
    public Dictionary<ushort, NetworkUnit> unitByID = new Dictionary<ushort, NetworkUnit>();
    public ulong blackId;
    public ulong whiteId;
    public ushort internalUnitId = 1;
    public ushort selectedUnitID;

    private static GameMaster _instance;

    public static GameMaster Instance { get { return _instance; } }
    public Canvas startGameUI;


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

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        startGameUI.gameObject.SetActive(false);
        Debug.Log("Host");
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        startGameUI.gameObject.SetActive(false);
        Debug.Log("Client");
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("On Network Spawn");
        base.OnNetworkSpawn();

        NetworkManager.Singleton.OnClientConnectedCallback += (ulong smth) => { };

        selectReticleInstance = Instantiate(selectReticlePrefab, Vector3.zero, Quaternion.identity);
        reticleInstance = Instantiate(reticlePrefab, Vector3.zero, Quaternion.identity);
        base.OnNetworkSpawn();

        SpawnGridServerRPC();
        SpawnBlackServerRPC();
        SpawnWhiteServerRPC();

    }

    public Vector2 gridSpaceToWorldSpace(Vector2Int gridSpace)
    {
        Vector2 tileSize = tilePrefab.transform.GetComponent<SpriteRenderer>().bounds.size;
        return gridSpace * tileSize;
    }

    // 0 => -.48, .48
    // 1 => .480...1, .96
    // x => 
    public Vector2Int worldSpaceToGridSpace(Vector2 worldSpace)
    {
        Vector2 tileSize = tilePrefab.transform.GetComponent<SpriteRenderer>().bounds.size; // .96
        Vector2 offset = tileSize / 2;

        return Vector2Int.FloorToInt((worldSpace + offset) / tileSize);
    }


    [ServerRpc]
    public void SpawnGridServerRPC()
    {
        List<List<NetworkUnit>> unitRows = new List<List<NetworkUnit>>();
        List<List<NetworkTile>> rows = new List<List<NetworkTile>>();
        for (int x = 0; x < max_i; x++)
        {
            List<NetworkUnit> unitColumn = new List<NetworkUnit>();
            List<NetworkTile> column = new List<NetworkTile>();
            unitRows.Add(unitColumn);
            rows.Add(column);
            for (int y = 0; y < max_j; y++)
            {
                Vector2 position = gridSpaceToWorldSpace(new Vector2Int(x, y));
                NetworkTile tile = Instantiate<NetworkTile>(
                    tilePrefab,
                    position,
                    Quaternion.identity
                );
                column.Add(tile);
                unitColumn.Add(null); //ensure that unitColumn list has enough spaces in it to hold units at any position

                tile.GetComponent<NetworkObject>().Spawn();
            }
        }
        tiles = rows;
        units = unitRows;
    }

    [ServerRpc]
    public void SpawnBlackServerRPC()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2Int position = new Vector2Int(i + 4, 5);

            SpawnUnit(spearPrefab, position, PlayerEnum.BLACK);
        }

        // King on 5, 6
        Vector2Int kingPosition = new Vector2Int(5, 6);

        SpawnUnit(kingPrefab, kingPosition, PlayerEnum.BLACK);
    }

    [ServerRpc]
    public void SpawnWhiteServerRPC()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2Int position = new Vector2Int(i + 1, 2);

            SpawnUnit(spearPrefab, position, PlayerEnum.WHITE);
        }

        // King on 2, 1
        Vector2Int kingPosition = new Vector2Int(2, 1);

        SpawnUnit(kingPrefab, kingPosition, PlayerEnum.WHITE);
    }

    private void SpawnUnit(NetworkUnit networkUnit, Vector2Int boardPosition, PlayerEnum team)
    {
        Vector2 newPosition = gridSpaceToWorldSpace(boardPosition);
        NetworkUnit unit = Instantiate(
            networkUnit,
            newPosition,
            Quaternion.identity
        );
        unit.GetComponent<NetworkObject>().Spawn();
        unit.team.Value = team;
        unit.internalId = new NetworkVariable<ushort>(internalUnitId);
        unitByID.Add(internalUnitId, unit.GetComponent<NetworkUnit>());
        units[boardPosition.x][boardPosition.y] = unit;
        internalUnitId++;
    }

    public bool isControlledPiece(ushort pieceId)
    {
        return unitByID[pieceId].team.Value == PlayerEnum.WHITE; //TODO: fix
    }

    [ServerRpc]
    public void MoveServerRpc(ushort pieceId, Vector2Int targetGridPosition)
    {
        // TODO: assert that unit belongs to caller
        Debug.Log(canMove(MovementRuleEnum.CHESS_PAWN, pieceId, targetGridPosition));
        if (canMove(MovementRuleEnum.CHESS_PAWN, pieceId, targetGridPosition))
        {
            moveUnit(pieceId, targetGridPosition);
        }
    }

    public void moveUnit(ushort pieceId, Vector2Int targetGridPosition)
    {
        NetworkUnit unit = unitByID[pieceId];
        Vector2Int currentPosition = unitToGridSpace(unit);
        units[currentPosition.x][currentPosition.y] = null;
        units[targetGridPosition.x][targetGridPosition.y] = unit;
        unit.transform.position = gridSpaceToWorldSpace(targetGridPosition);
    }

    public Vector2Int tileToGridSpace(NetworkTile tile)
    {
        for (int x = 0; x < tiles.Count; x++)
        {
            for (int y = 0; y < tiles[x].Count; y++)
            {
                if (tiles[x][y] == tile)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        throw new Exception("Tile not found in grid.");
    }

    public Vector2Int unitToGridSpace(NetworkUnit unit)
    {
        for (int x = 0; x < units.Count; x++)
        {
            for (int y = 0; y < units[x].Count; y++)
            {
                if (units[x][y] == unit)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        throw new Exception("Unit not found in grid.");
    }

    //TODO: This will all only work for white

    private bool canMove(MovementRuleEnum movementRule, ushort pieceId, Vector2Int targetTilePosition)
    {
        if (!isControlledPiece(pieceId)) // You don't own this
        {
            return false;
        }
        NetworkUnit unit = unitByID[pieceId];
        if (movementRule == MovementRuleEnum.CHESS_PAWN)
        {
            Vector2Int backwardFromTarget = targetTilePosition - new Vector2Int(0, 1);
            return
                unitToGridSpace(unitByID[pieceId]) == backwardFromTarget && // TODO: this is yucky and needs to flip according to player
                units[targetTilePosition.x][targetTilePosition.y] == null; // space is unoccupied
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
        if (!selectedTile)
        {
            return;
        }

        reticleInstance.transform.position = selectedTile?.transform?.position ?? reticleInstance.transform.position;
        Vector2Int gridPosition = worldSpaceToGridSpace(position);
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 tileSize = tilePrefab.transform.GetComponent<SpriteRenderer>().bounds.size;
            selectReticleInstance.transform.position = selectedTile.transform.position;
            selectedUnitID = units[gridPosition.x][gridPosition.y].internalId.Value;
        }

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("grid space: " + gridPosition);
            GameMaster.Instance.MoveServerRpc(selectedUnitID, gridPosition);
        }
    }
}
