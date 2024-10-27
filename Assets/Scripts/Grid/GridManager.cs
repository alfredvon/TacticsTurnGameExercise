using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    Tile[,] grid;
    [SerializeField] int length = 25;
    [SerializeField] int width = 25;
    [SerializeField] float originY = 0f;
    [SerializeField] float rayCheckHeight = 100f;
    [SerializeField] float rayLength = float.MaxValue;
    [SerializeField] float cellSize = 1f;
    [SerializeField] float cellHeightDelta = 0f;
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] LayerMask gridLayer;
    [SerializeField] GameObject tilePrefab;
    [SerializeField] GameObject tileRoot;

    public TileFinding TileFinding { get; private set; }

    private void Awake()
    {
        TileFinding = GetComponent<TileFinding>();
    }

    public Tile GetTileWithWorldPosition(Vector3 world_position)
    {
        world_position -= transform.position;
        float half = cellSize / 2;
        int x = (int)((world_position.x + half) / cellSize);
        int y = (int)((world_position.z + half) / cellSize);
        if (x < 0 || x >= length || y < 0 || y >= width)
            return null;
        return grid[x, y];
    }

    public Tile GetTileAtPosition(Vector2Int position)
    {
        int x = position.x;
        int y = position.y;
        if (x < 0 || x >= length || y < 0 || y >= width)
            return null;
        return grid[x, y];
    }

    public void ShowHighlightTiles(TileHighlightType type, HashSet<Tile> tiles)
    {
        if (tiles == null)
            return;
        foreach (Tile tile in tiles)
        {
            tile.ShowHighlight(type);
        }
    }

    public void HideHighlightTiles(HashSet<Tile> tiles)
    {
        if (tiles == null)
            return;
        foreach (Tile tile in tiles)
        {
            tile.HideHighlight();
        }
    }

    public void GenerateGrid()
    {
        grid = new Tile[length, width];
        RaycastHit hit;
        for (int y = 0; y < width; ++y)
        {
            for (int x = 0; x < length; ++x)
            {
                Vector3 tilePos = GetTileWorldPosition(x, y);
                bool passable = true;
                //elevation
                Ray ray = new Ray(tilePos + Vector3.up * rayCheckHeight, Vector3.down);
                if (Physics.Raycast(ray, out hit, rayLength, gridLayer))
                {
                    if (Mathf.Abs(hit.point.y) > 0.01f)
                        tilePos.y = hit.point.y + cellHeightDelta;
                }
                else
                {
                    //in air
                    passable = false;
                }
                GameObject tileObject = Instantiate(tilePrefab, tilePos, Quaternion.identity, tileRoot.transform);
                Tile tile = tileObject.GetComponent<Tile>();
                tile.SetPos(x, y);
                tile.SetElevation(hit.point.y);
                //passable check obstacle
                if (passable == true)
                    passable = !Physics.CheckBox(tilePos, Vector3.one / 2 * cellSize, Quaternion.identity, obstacleLayer);
                tile.SetPassable(passable);
                tile.SetWorldPosition(tilePos);
                tile.HideHighlight();
                
                grid[x, y] = tile;
                
            }
        }
    }

    Vector3 GetTileWorldPosition(int x, int y, float elevation = 0f)
    {
        return new Vector3(transform.position.x + (x * cellSize), originY + elevation, transform.position.z + (y * cellSize));
    }

    private void OnDrawGizmos()
    {
        //if (grid == null) return;

        for (int y = 0; y < width; ++y)
        {
            for (int x = 0; x < length; ++x)
            {
                Vector3 pos = GetTileWorldPosition(x, y);
                if (grid != null)
                {
                    pos = grid[x, y].WorldPosition;
                    Gizmos.color = grid[x, y].Passable ? Color.white : Color.red;
                }

                Gizmos.DrawWireCube(pos, new Vector3(cellSize, 0, cellSize));
            }
        }
    }

}
