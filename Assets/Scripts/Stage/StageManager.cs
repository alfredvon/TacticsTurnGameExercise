using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class StageManager : Singleton<StageManager>
{
    [SerializeField] StageMainController mainController;
    public GameObject cameraRig;

    [SerializeField] List<Unit> playerUnits = new List<Unit>();
    [SerializeField] List<Unit> enemyUnits = new List<Unit>();

    GridManager gridManager;
    UIManager uiManager;

    List<Unit> allUnits = new List<Unit>();
    Queue<Unit> activeUnits = new Queue<Unit>();
    HashSet<int> turnDoneUnitIDs = new HashSet<int>();
    Unit curUnit;   //当前行动回合的目标
    Unit selectUnit;    //当前点击的目标
    int turnID;
    bool stageWin = false;
    bool stageEnd = false;

    StageState curState = StageState.None;
    StageWinCondition winCondition = StageWinCondition.DefeatAllEnemies;

    public GridManager GetGridManager() => gridManager;

    public List<Unit> GetUnitsWithGroupType(UnitGroupType group_type)
    {
        List<Unit> units = new List<Unit>();
        foreach (Unit unit in allUnits)
        {
            if (unit.Character.Group == group_type)
            {
                units.Add(unit);
            }
        }
        return units;
    }

    public void OnClickCommandPanelAttack()
    {
        if (curUnit == null || curUnit.Character.TurnAttackDone)
            return;
        CharacterTurnState cState = curUnit.Character.CurrentState;
        if (cState == CharacterTurnState.Move)
            return;
        if (cState == CharacterTurnState.Idle)
            gridManager.HideHighlightTiles(curUnit.GetMovableTiles());
        HashSet<Tile> rangeTiles = curUnit.Character.GetAbilityRangeTiles(gridManager.TileFinding);
        curUnit.SetAbilityRangeTiles(rangeTiles);
        ShowAbilityRangeTiles(curUnit);
    }

    public void OnClickCommandPanelTurnEnd()
    {
        if (curUnit == null || curUnit.Character.TurnEnd == true)
            return;
        OnDeselectUnit();
        curUnit.OnTurnEnd();
    }

    public void OnClickCommandPanelCancel()
    {
        if (curUnit == null)
            return;
        OnDeselectUnit();
    }

    public void OnSelectTile(Tile select_tile)
    {
        if (curUnit.Character.IsAI)
            return;
        
        if (selectUnit != null)
        {
            //if change select unit
            if (select_tile.HasUnit())
            {
                CharacterTurnState cState = selectUnit.Character.CurrentState;
                bool isSameUnit = selectUnit.IsEqual(select_tile.GetUnit());
                if (isSameUnit == false
                    && (cState == CharacterTurnState.Idle || cState == CharacterTurnState.AbilityPerformDone || cState == CharacterTurnState.TurnDone))
                {
                    OnDeselectUnit();
                    OnSelectUnit(select_tile.GetUnit());
                }
                else
                {
                    OnSelectUnit(selectUnit);
                    DoUnitActBySelectTile(select_tile);
                }
            }
            else
            {
                DoUnitActBySelectTile(select_tile);
            }
        }
        else
        {
            if (select_tile.HasUnit())
            {
                OnSelectUnit(select_tile.GetUnit());
            }
        }

    }

    public void UnitStateNotify(Unit notify_unit)
    {
        Character character = notify_unit.Character;
        Debug.Log("unit:" + character.Name + " state :" + character.CurrentState);

        if (character.IsDeath())
        {
            OnUnitDeath();
        }else if (character.CurrentState == CharacterTurnState.TurnDone)
        {
            NextUnitTurn();

        }
        //else if (character.CurrentState == CharacterTurnState.ReadyForAttack)
        //{
        //    //notify_unit.SetAttackableTiles(gridManager.TileFinding.GetAttackableTiles(notify_unit.CurrentTile.Position, character.AttackRange));
        //    //ShowAttackTiles(notify_unit, true);
        //}
        else if (character.CurrentState == CharacterTurnState.Move)
        {
            ShowMovableTiles(notify_unit);
        }
        else if (character.CurrentState == CharacterTurnState.Generate)
        {
            allUnits.Add(notify_unit);
            if (allUnits.Count >= (playerUnits.Count + enemyUnits.Count)) 
            {
                ChangeState(StageState.TurnStart);
            }
        }
        else if (character.CurrentState == CharacterTurnState.AbilityPerform)
        {
            if (notify_unit.IsEqual(selectUnit))
                OnSelectUnit(selectUnit);
        }

    }

    public void ChangeState(StageState state)
    {
        Debug.Log("stage change state:" + state);
        if (stageEnd)
            return;
        switch (state) 
        {
            case StageState.Start:
                OnStart();
                break;
            case StageState.GenerateGrid:
                OnGenerateGrid();
                break;
            case StageState.PlacePlayerUnits:
                OnPlacePlayerUnits();
                break;
            case StageState.PlaceEnemyUnits:
                OnPlaceEnemyUnits();
                break;
            case StageState.TurnStart:
                OnTurnStart();
                break;
            case StageState.Battle:
                OnBattle();
                break;
            case StageState.BattleResult:
                OnBattleResult();
                break;
        }
        curState = state;
    }

    private void OnStart()
    {
        if (gridManager == null)
        {
            GameObject gb = GameObject.FindWithTag("GridManager");
            if (gb != null)
            {
                gridManager = gb.GetComponent<GridManager>();
            }
            else
                Debug.LogError("cant find grid manager object");
        }
        
        uiManager = GameManager.Instance.UIManager;

        GameManager.Instance.ChangeController(mainController);
        GameManager.Instance.SetInputLock(true);

        ResetStage();
        ChangeState(StageState.GenerateGrid);
    }

    private void ResetStage()
    {
        activeUnits.Clear();
        allUnits.Clear();
        turnDoneUnitIDs.Clear();
        curUnit = null;
        turnID = 0;
        stageWin = false;
        stageEnd = false;
    }

    private void OnGenerateGrid()
    {
        gridManager.GenerateGrid();
        ChangeState(StageState.PlacePlayerUnits);
    }

    private void OnPlacePlayerUnits()
    {
        foreach (var unit in playerUnits) 
        {
            PlaceUnitByWorldPosition(unit);
            //allUnits.Add(unit);
        }
        ChangeState(StageState.PlaceEnemyUnits);
    }

    private void OnPlaceEnemyUnits()
    {
        foreach (var unit in enemyUnits)
        {
            PlaceUnitByWorldPosition(unit);
            //allUnits.Add(unit);
        }
        //ChangeState(StageState.TurnStart);
        //Debug.Log("game start");
    }

    private void OnTurnStart()
    {
        //根据先攻排序
        allUnits.Sort((a, b) => b.Character.Initiative.CompareTo(a.Character.Initiative));
        activeUnits.Clear();
        foreach (var unit in allUnits)
        {
            if (unit.Character.IsDeath() == false)
            {
                unit.Character.ResetTurnRes();
                activeUnits.Enqueue(unit);
            }
                
        }
        turnID++;
        Debug.Log("Turn Start :" + turnID);
        ChangeState(StageState.Battle);
    }

    private void PlaceUnitByWorldPosition(Unit unit)
    {
        Tile tile = gridManager.GetTileWithWorldPosition(unit.gameObject.transform.position);
        if (tile != null)
        {
            tile.PlaceUnit(unit, true);
        }
        else
            Debug.Log("place unit failed unit name:" + unit.gameObject.name);
    }

    private void OnBattle()
    {
        NextUnitTurn();
    }

    private void NextUnitTurn()
    {
        if (activeUnits.Count > 0)
        {
            curUnit = activeUnits.Dequeue();
            GameManager.Instance.SetInputLock(curUnit.Character.IsAI);
            mainController.cameraController.follow = curUnit.gameObject.transform;
            mainController.gridController.ShowOrHideIndicator(!curUnit.Character.IsAI);

            curUnit.SetMovableTiles(gridManager.TileFinding.GetMovableTiles(curUnit.CurrentTile.Position, curUnit.Character.MovePoints));
            
            if (curUnit.Character.IsAI)
            {
                GameManager.Instance.SetInputLock(curUnit);
                curUnit.GetAI().TakeTurn();
            }
        }
        else
            ChangeState(StageState.TurnStart);
    }

    private void OnBattleResult()
    {
        Debug.Log("stage result:" + stageWin);
        stageEnd = true;
    }

    private void CheckWinCondition()
    {
        if (winCondition == StageWinCondition.DefeatAllEnemies)
        {
            bool win = true;
            foreach (var unit in enemyUnits) 
            {
                if (unit.Character.IsDeath() == false)
                { 
                    win = false; 
                    break;
                }
            }
            if (win) 
            {
                stageWin = true;
                ChangeState(StageState.BattleResult);
                return;
            }
        }

        //check lose all player unit death
        bool lose = true;
        foreach (var unit in playerUnits)
        {
            if (unit.Character.IsDeath() == false)
            {
                lose = false;
                break;
            }
        }
        if (lose)
        {
            stageWin = false;
            ChangeState(StageState.BattleResult);
        }
    }

    private void OnSelectUnit(Unit unit)
    {
        if (unit == null)
            return;
        if (curUnit.Character.IsAI)
            return;
        if (selectUnit != null && selectUnit.IsEqual(unit))
        {
            uiManager.ShowCommandPanel(selectUnit.IsEqual(curUnit), unit.Character);
            return;
        }
        selectUnit = unit;
        //show command panel and update btn show
        uiManager.ShowCommandPanel(selectUnit.IsEqual(curUnit), unit.Character);

        //show or hide tile highlight
        CharacterTurnState CharacterTurnState = selectUnit.Character.CurrentState;
        if (CharacterTurnState == CharacterTurnState.AbilityTargetSelect)
        {
            ShowAbilityRangeTiles(selectUnit);
        }
        else if (CharacterTurnState == CharacterTurnState.Idle || CharacterTurnState == CharacterTurnState.TurnDone)
        {
            if (selectUnit.GetMovableTiles() == null)
                selectUnit.SetMovableTiles(gridManager.TileFinding.GetMovableTiles(selectUnit.CurrentTile.Position, selectUnit.Character.MovePoints));
            ShowMovableTiles(selectUnit);
        }
    }

    private void OnDeselectUnit()
    {
        if (selectUnit == null)
            return;

        //show command panel and update btn show
        uiManager.ShowCommandPanel(false);

        //show or hide tile highlight
        CharacterTurnState CharacterTurnState = selectUnit.Character.CurrentState;
        if (CharacterTurnState == CharacterTurnState.AbilityTargetSelect)
        {
            gridManager.HideHighlightTiles(selectUnit.GetAbilityRangeTiles());
        }
        else if (CharacterTurnState == CharacterTurnState.Idle || CharacterTurnState == CharacterTurnState.TurnDone)
        {
            gridManager.HideHighlightTiles(selectUnit.GetMovableTiles());
        }

        selectUnit = null;
    }

    private void ShowAbilityRangeTiles(Unit unit)
    {
       gridManager.ShowHighlightTiles(TileHighlightType.Ability, unit.GetAbilityRangeTiles());
    }

    private void ShowMovableTiles(Unit unit)
    {
       gridManager.ShowHighlightTiles(TileHighlightType.Move, unit.GetMovableTiles());
    }

    private void DoUnitActBySelectTile(Tile select_tile)
    {
        if (selectUnit == null)
            return;
        if (selectUnit.IsEqual(curUnit) == false)
            return;
        CharacterTurnState cState = selectUnit.Character.CurrentState;
        if (cState == CharacterTurnState.AbilityTargetSelect || cState == CharacterTurnState.AbilityPerform)
        {
            selectUnit.ConfirmAbilityTarget(select_tile);
        }
        else if (cState == CharacterTurnState.Idle)
        {
            if (select_tile.IsMovable() && selectUnit.IsMoving == false && selectUnit.IsInMovableTiles(select_tile))
            {
                Tile tileStart = selectUnit.CurrentTile;
                List<Tile> tempTiles = gridManager.TileFinding.FindPath(tileStart.Position, select_tile.Position, selectUnit.GetMovableTiles());
                selectUnit.Move(tempTiles);
            }
        }
    }

    private void OnUnitDeath()
    {
        Queue<Unit> temp = new Queue<Unit>();
        while (activeUnits.Count > 0)
        {
            Unit unit = activeUnits.Dequeue();
            if (unit.Character.IsDeath() == false)
                temp.Enqueue(unit);
        }
        activeUnits = temp;
        CheckWinCondition();
    }
}
