using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAbility : Ability
{
    protected override void OnApply(Tile target_tile)
    {
        owner.StartCoroutine(ApplyMeleeAbiltyCoroutine(target_tile));
    }

    IEnumerator ApplyMeleeAbiltyCoroutine(Tile target_tile)
    {
        //face direction
        owner.ParentUnit.RotateUnit(target_tile.WorldPosition);
        //play anim
        owner.ParentUnit.PlayCharacterAnimation(abilityAnimation, true);
        //wait apply effect time
        yield return new WaitForSeconds(startEffectInterval);
        if (effectRangeType == AbilityEffectRangeType.Single)
        {
            if (target_tile.HasUnit())
            {
                Character target_c = target_tile.GetUnit().Character;
                if (effectType == AbilityEffectType.Damage && IsTarget(target_c))
                {
                    //roll 命中
                    int hitRate = GameManager.Instance.DiceManager.RollDice(DiceManager.DiceType.dice20, 1);
                    Debug.Log("roll 命中：" + hitRate + " 对方AC:" + target_c.AC);
                    if (hitRate >= target_c.AC)
                    {
                        //roll damage
                        target_tile.GetUnit().TakeDamage(owner.CalculateAbilityDamage());
                    }
                    else
                    {
                        GameManager.Instance.UIManager.CreateFloatingMessage(target_tile.WorldPosition, "Miss");
                    }
                }
                
            }
        }
        yield return new WaitForSeconds(animInterval - startEffectInterval);
        //stop anim
        owner.ParentUnit.PlayCharacterAnimation(abilityAnimation, false);
        //perform done
        owner.OnAbilityPerformDone();

    }
}
