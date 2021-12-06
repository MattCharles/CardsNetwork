using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    NetworkTile selectedTile;
    ulong? id;
    public List<NetworkUnit> units = new List<NetworkUnit>();
    public GameObject reticlePrefab;
    public GameObject reticleInstance;
    public GameObject selectReticlePrefab;
    public GameObject selectReticleInstance;

    // Can't use actual Unit object since it can't be serialized cleanly.
    // Just pass these to GameMaster and let it do the movement of units and stuff
    ushort selectedUnitID;

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += StartPlayerServerRPC;
        selectReticleInstance = Instantiate(selectReticlePrefab, Vector3.zero, Quaternion.identity);
        reticleInstance = Instantiate(reticlePrefab, Vector3.zero, Quaternion.identity);
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

        reticleInstance.transform.position = selectedTile?.transform?.position ?? reticleInstance.transform.position;
        if (Input.GetMouseButtonDown(0))
        {
            selectReticleInstance.transform.position = selectedTile.transform.position;
            selectedUnitID = selectedTile?.OccupantID.Value ?? (ushort)0;

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
