using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridManager))]
public class PathFinding : MonoBehaviour
{
    public GridManager gridManager;
    private void Start()
    {
        if (gridManager == null)
            gridManager = GridManager.Instance;
    }

    public List<Tile> GetMovableTiles(Vector2Int start, float movePoints)
    {
        Tile startTile = gridManager.GetTileAtPosition(start);
        List<Tile> movableTiles = new List<Tile>();
        Queue<(Tile tile, float remainingMovePoints)> queue = new Queue<(Tile, float)>();
        HashSet<Tile> visited = new HashSet<Tile>();

        // 初始化队列，从起点开始
        queue.Enqueue((startTile, movePoints));
        visited.Add(startTile);

        while (queue.Count > 0)
        {
            var (currentTile, remainingMove) = queue.Dequeue();

            // 把当前可移动的格子添加到列表
            movableTiles.Add(currentTile);

            // 遍历四个方向
            foreach (Tile neighbor in GetNeighbors(currentTile, true))
            {

                // 如果该邻居有效，且尚未访问过
                if (neighbor.IsMovable() && !visited.Contains(neighbor))
                {
                    int moveCost = neighbor.moveCost;

                    // 如果剩余的移动力足够移动到该格子
                    if (remainingMove >= moveCost)
                    {
                        visited.Add(neighbor);
                        queue.Enqueue((neighbor, remainingMove - moveCost));
                    }
                }
            }
        }

        return movableTiles;
    }

    public List<Tile> GetAttackableTiles(Vector2Int start, int attackRange, bool excludeSelf = true)
    {
        List<Tile> attackableTiles = new List<Tile>();

        // 遍历所有在攻击范围内的格子
        for (int x = -attackRange; x <= attackRange; x++)
        {
            for (int y = -attackRange; y <= attackRange; y++)
            {
                if (excludeSelf == false && x == 0 && y == 0)
                    continue;

                Vector2Int pos = new Vector2Int(start.x + x, start.y + y);

                // 使用曼哈顿距离判断是否在攻击范围内
                if (Mathf.Abs(x) + Mathf.Abs(y) <= attackRange)
                {
                    Tile tile = gridManager.GetTileAtPosition(pos);
                    if (tile == null || !tile.IsMovable())
                        continue;
                    attackableTiles.Add(tile);
                }
            }
        }

        return attackableTiles;
    }

    public List<Tile> FindPath(Vector2Int start, Vector2Int end)
    {
        Tile startTile = gridManager.GetTileAtPosition(start);
        Tile endTile = gridManager.GetTileAtPosition(end);

        List<Tile> openList = new List<Tile>();
        HashSet<Tile> closedList = new HashSet<Tile>();

        openList.Add(startTile);

        while (openList.Count > 0)
        {
            // 选择fCost最小的节点
            Tile currentTile = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentTile.fCost || (openList[i].fCost == currentTile.fCost && openList[i].hCost < currentTile.hCost))
                {
                    currentTile = openList[i];
                }
            }

            openList.Remove(currentTile);
            closedList.Add(currentTile);

            // 如果找到目标
            if (currentTile == endTile)
            {
                return RetracePath(startTile, endTile);
            }

            // 遍历当前节点的邻居
            foreach (Tile neighbor in GetNeighbors(currentTile))
            {
                if (!neighbor.IsMovable() || closedList.Contains(neighbor))
                    continue;

                int newMovementCostToNeighbor = currentTile.gCost + GetDistance(currentTile, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openList.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, endTile);
                    neighbor.parentTile = currentTile;

                    if (!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }
            }
        }

        // 未找到路径
        return null;
    }

    // 回溯找到的路径
    List<Tile> RetracePath(Tile startTile, Tile endTile)
    {
        List<Tile> path = new List<Tile>();
        Tile currentTile = endTile;

        while (currentTile != startTile)
        {
            path.Add(currentTile);
            currentTile = currentTile.parentTile;
        }
        path.Reverse();
        return path;
    }

    // 获取邻居节点
    List<Tile> GetNeighbors(Tile tile, bool use4Dir = false)
    {
        List<Tile> neighbors = new List<Tile>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
         new Vector2Int(-1, 1),  // 左上
        new Vector2Int(1, 1),   // 右上
        new Vector2Int(-1, -1), // 左下
        new Vector2Int(1, -1)   // 右下
        };
        if (use4Dir)
        {
            directions = new Vector2Int[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        }


        foreach (Vector2Int direction in directions)
        {
            Tile neighbor = gridManager.GetTileAtPosition(tile.Position + direction);
            if (neighbor != null)
                neighbors.Add(neighbor);
        }

        return neighbors;
    }

    // 计算两节点间的距离
    int GetDistance(Tile a, Tile b)
    {
        int dstX = Mathf.Abs(a.Position.x - b.Position.x);
        int dstY = Mathf.Abs(a.Position.y - b.Position.y);
        // 对角线距离：每次对角线移动的代价为 14（基于 √2 约为 1.414 的简化）
        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);  // 水平方向更多
        else
            return 14 * dstX + 10 * (dstY - dstX);  // 垂直方向更多
    }
}
