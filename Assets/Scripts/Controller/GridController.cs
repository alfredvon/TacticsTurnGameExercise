using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridController : BaseController
{
    [SerializeField] LayerMask terrainLayerMask;
    [SerializeField] Transform tileSelectionIndicator;
    [SerializeField] Vector3 indicatorDelta;

    List<Tile> tempTiles;
    Ray ray;
    RaycastHit hit;

    public override void OnEnter()
    {
        base.OnEnter();
        terrainLayerMask = LayerMask.NameToLayer("Grid");
    }

    public void ShowOrHideIndicator(bool is_show)
    {
        tileSelectionIndicator.gameObject.SetActive(is_show);
    }

    public override void Tick()
    {

        Tile selectTile = GetSelectTile();
        if (selectTile == null)
            return;
        tileSelectionIndicator.position = selectTile.WorldPosition + indicatorDelta;
        if (Input.GetMouseButtonDown(0))
        {
            bool isOverUI = EventSystem.current.IsPointerOverGameObject();
            if (isOverUI)
                return;
            StageManager.Instance.OnSelectTile(selectTile);
        }

    }

    private Tile GetSelectTile()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, float.MaxValue, terrainLayerMask))
        {
            Tile selectTile = StageManager.Instance.GetGridManager().GetTileWithWorldPosition(hit.point);
            return selectTile;
        }
        return null;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawCube(hitTest, Vector3.one);
        if (tempTiles == null || tempTiles.Count == 0) return;
        for (int i = 0; i < tempTiles.Count - 1; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(tempTiles[i].WorldPosition, tempTiles[i + 1].WorldPosition);
        }
    }
}
