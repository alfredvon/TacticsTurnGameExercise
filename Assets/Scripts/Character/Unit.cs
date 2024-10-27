using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public static readonly float moveTolerance = 0.05f;

    [SerializeField] UnitAnimator animator;
    [SerializeField] float moveSpeed = 1f;
    public Transform rayStartPoint;
    public Transform hitEffectPoint;

    public Tile CurrentTile { get; private set; }
    public bool IsMoving { get; private set; }
    public Character Character { get; private set; }

    List<Tile> movePath;
    HashSet<Tile> movableTiles;
    HashSet<Tile> abilityRangeTiles;
    
    AIController ai;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        
        Character = GetComponent<Character>();
        Character.SetParentUnit(this);
        Character.SetCharacterTurnState(CharacterTurnState.Generate);
        if (Character.IsAI == true)
        {
            ai = GetComponent<AIController>();
            ai.SetControlUnit(this);
        }
    }


    public void SetCurrentTile(Tile current_tile)
    {
        CurrentTile = current_tile;
    }

    public void SetMovableTiles(HashSet<Tile> tiles)
    {
        if (tiles == null || tiles.Count <= 0)
            return;
        movableTiles = tiles;
    }

    public bool IsInMovableTiles(Tile tile)
    {
        if (movableTiles == null)
            return false;
        foreach (Tile t in movableTiles) 
        {
            if (tile.Position == t.Position)
                return true;
        }
        return false;
    }

    public HashSet<Tile> GetMovableTiles()
    {
        return movableTiles;
    }

    public void SetAbilityRangeTiles(HashSet<Tile> tiles)
    {
        abilityRangeTiles = tiles;
    }

    public HashSet<Tile> GetAbilityRangeTiles()
    { 
        return abilityRangeTiles;
    }

    public bool IsInAbilityRangeTiles(Tile tile)
    {
        if (abilityRangeTiles == null)
            return false;
        
        return abilityRangeTiles.Contains(tile);
    }

    public void Move(List<Tile> path)
    {
        if (IsMoving == false && movePath == null) 
        {
            ChangeCharacterTurnState(CharacterTurnState.Move);
            Character.TurnMoveDone = true;
            IsMoving = true;
            movePath = path;
            animator?.PlayMoveOrOff(true);
            RotateUnit(movePath[0].WorldPosition);
        }
    }
    public void ConfirmAbilityTarget(Tile target_tile)
    {
        if (IsInAbilityRangeTiles(target_tile) == false)
            return;

        //if (Character.CurrentState == CharacterTurnState.AbilityPerform)
        //    return;

        Character.CurrentAbility.Perform(target_tile);
    }


    public AIController GetAI()
    {
        return ai;
    }

    public bool IsEqual(Unit other)
    { 
        if (other == null)
            return false;
        return Character.ID == other.Character.ID;
    }

    public void TakeDamage(int damage, GameObject hit_effect = null)
    { 
       StartCoroutine(HitCoroutine(damage, hit_effect)); 
    }

    IEnumerator HitCoroutine(int damage, GameObject hit_effect)
    {
        Character.TakeDamage(damage);
        
        GameManager.Instance.UIManager.CreateFloatingMessage(CurrentTile.WorldPosition, damage.ToString());
        
        if (Character.IsDeath())
        {
            animator.PlayDeathOrOff(true);
        }
        else
        {
            animator.PlayHitOrOff(true);
            if (hit_effect != null)
            {
                Instantiate(hit_effect, hitEffectPoint.position, hitEffectPoint.rotation, GameManager.Instance.FXTransform);
            }
        }
        //wait for hit animation
        yield return new WaitForSeconds(.5f);
       
        if (Character.IsDeath() == false)
            animator.PlayHitOrOff(false);
    }

    public void RotateUnit(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0f;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    public void PlayCharacterAnimation(CharacterAnimation anim, bool on)
    {
        switch (anim)
        {
            case CharacterAnimation.MeleeAttack:
                animator.PlayMeleeAttackOrOff(on);
                break;
            default:
                break;
        }

    }

    public void ClearAbilityRangeTiles()
    {
        if (abilityRangeTiles == null)
            return;
        foreach (Tile tile in abilityRangeTiles)
            tile.HideHighlight();
        abilityRangeTiles = null;
    }

    public void ClearMovableTiles()
    {
        if (movableTiles == null)
            return;
        foreach (Tile tile in movableTiles)
            tile.HideHighlight();
        movableTiles = null;
    }

    public void OnTurnEnd()
    {
        ClearAbilityRangeTiles();
        
        Character.OnTurnEnd();
    }

    private void ChangeCharacterTurnState(CharacterTurnState state)
    {
        Character.SetCharacterTurnState(state);
    }

    

    private void Update()
    {
        if (movePath != null && movePath.Count > 0)
        {
            Tile nextTile = movePath[0];
            transform.position = Vector3.MoveTowards(transform.position, nextTile.WorldPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, nextTile.WorldPosition) < moveTolerance)
            {
                transform.position = nextTile.WorldPosition;
                movePath.RemoveAt(0);
                Character.RemainingMovePoints--;
                //move complete
                if (movePath.Count <= 0)
                {
                    movePath = null;
                    CurrentTile.RemoveUnit();
                    CurrentTile = nextTile;
                    CurrentTile.PlaceUnit(this);
                    ClearMovableTiles();
                    animator?.PlayMoveOrOff(false);
                    IsMoving = false;
                    ChangeCharacterTurnState(CharacterTurnState.AbilityTargetSelect);
                }
                else
                    RotateUnit(movePath[0].WorldPosition);
            }
        }
    }
}
