using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//技能所属
public enum AbilityActionType
{ 
    Action,
    BonusAction,
    Reaction
}
//技能类型
public enum AbilityType
{ 
    MeleeAttack
}
//作用范围
public enum AbilityEffectRangeType
{ 
    Single,
    Area
}
//作用类型
public enum AbilityEffectType
{
    Damage,
    Heal
}
//消耗类型
public enum AbilityCostType
{ 
    Action,
    BonusAction,
    Reaction
}

public abstract class Ability
{
    public Character owner = null;

    public AbilityType abilityType = AbilityType.MeleeAttack;
    //select range 选取范围
    public int rangeMin;
    public int rangeMax;
    public bool rangeExcludeStart = true;

    //effect range 作用范围
    public AbilityEffectRangeType effectRangeType = AbilityEffectRangeType.Single;
    //effect type 作用类型
    public AbilityEffectType effectType = AbilityEffectType.Damage;
    //使用次数 
    public int useCouts = 1;
    //伤害掷骰
    public int diceType; //骰子类型
    public int diceCount; //骰子数量
    public CharacterModifierType modifierType; //需要的调整值类型 0：不需要 STR：1 DEX：2 CON：3 INT：4 WIS：5 CHA：6
    //cost 消耗类型
    public AbilityCostType costType = AbilityCostType.Action;
    //animation 使用动画
    public CharacterAnimation abilityAnimation = CharacterAnimation.MeleeAttack;
    //攻击和计算被击等待时间（后续可能用animation event来做）
    public float startEffectInterval = 0.5f;
    public float animInterval = 1.5f;

    //有的技能可以使用多次，只扣除一次行动资源
    private bool alreadyCostActPoint = false;
    

    public virtual HashSet<Tile> GetTilesInRange(Vector2Int start, TileFinding tile_finding)
    {
        HashSet<Tile> retTiles = new HashSet<Tile>();
        retTiles = tile_finding.GetTilesInRange(start, rangeMin, rangeMax, rangeExcludeStart);
        return retTiles;
    }

    public virtual void OnDestroy()
    {

    }

    public virtual void Perform(Tile target_tile)
    {
        //can perform?
        if (CanPerform() == false)
        {
            Debug.Log("No resource to perform ability");
            return;
        }
        //cost
        if (alreadyCostActPoint == false)
        {
            alreadyCostActPoint = true; 
            owner.DoAbilityCost(costType);
        }
        useCouts--;
        //change character turn state
        owner.SetCharacterTurnState(CharacterTurnState.AbilityPerform);
        //apply
        OnApply(target_tile);
        //if (abilityType == AbilityType.MeleeAttack)
        //    StartCoroutine(ApplyMeleeAbilty(target_tile));
    }

    protected virtual bool CanPerform()
    {
        bool pointOk = false;
        switch (costType)
        {
            case AbilityCostType.Action:
                pointOk = owner.ActionPoints > 0;
                break;
            case AbilityCostType.BonusAction:
                pointOk = owner.BonusActionPoints > 0;
                break;
            case AbilityCostType.Reaction:
                pointOk = owner.ReactionPoints > 0;
                break;
            default:
                break;
        }
        bool countOk = useCouts > 0;
        return pointOk || countOk;
    }

    protected virtual bool IsTarget(Character target)
    {
        return owner.Group != target.Group;
    }

   

    protected abstract void OnApply(Tile target_tile);
}
