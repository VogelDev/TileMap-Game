using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TGMap))]
public class TileMapMouse : MonoBehaviour
{

    TGMap _tileMap;

    Vector3 currentTileCoord;

    public Transform selectionCube;

    void Start()
    {
        _tileMap = GetComponent<TGMap>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        if (GetComponent<Collider>().Raycast(ray, out hitInfo, Mathf.Infinity))
        {
            int x = Mathf.FloorToInt(hitInfo.point.x / _tileMap.tileSize);
            int z = Mathf.FloorToInt(hitInfo.point.z / _tileMap.tileSize);
            //Debug.Log ("Tile: " + x + ", " + z);

            currentTileCoord.x = x;
            currentTileCoord.z = z + 1;

            selectionCube.transform.position = currentTileCoord * _tileMap.tileSize;
        }
        else {
            // Hide selection cube?
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Click!");
            Debug.Log(selectionCube.transform.position.x + ", " + selectionCube.transform.position.z);
        }
    }
}
