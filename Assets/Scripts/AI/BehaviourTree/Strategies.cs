using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

namespace SelfAI.BehaviourTree
{
    public interface IStrategy {
        Node.Status Process();

        void Reset() {
            // Noop
        }
    }

    public class ActionStrategy : IStrategy {
        readonly Action doSomething;
        
        public ActionStrategy(Action doSomething) {
            this.doSomething = doSomething;
        }
        
        public Node.Status Process() {
            doSomething();
            return Node.Status.Success;
        }
    }

    public class ConditionStrategy : IStrategy {
        readonly Func<bool> predicate;
        
        public ConditionStrategy(Func<bool> predicate) {
            this.predicate = predicate;
        }
        
        public Node.Status Process() => predicate() ? Node.Status.Success : Node.Status.Failure;
    }

    public class MoveToPositionStrategy : IStrategy
    {
        readonly AIController ai;

        public MoveToPositionStrategy(AIController ai)
        {
            this.ai = ai;
        }

        public Node.Status Process() {
            if (ai.controlUnit.Character.CurrentState == CharacterTurnState.AbilityTargetSelect)
            {
                HashSet<Tile> rangeTiles = ai.controlUnit.Character.GetAbilityRangeTiles(StageManager.Instance.GetGridManager().TileFinding);
                ai.controlUnit.SetAbilityRangeTiles(rangeTiles);
                return Node.Status.Success;
            }
            if (ai.controlUnit.Character.CurrentState == CharacterTurnState.Idle)
            {
                if (ai.controlUnit.CurrentTile.Position == ai.movePosition)
                {
                    HashSet<Tile> rangeTiles = ai.controlUnit.Character.GetAbilityRangeTiles(StageManager.Instance.GetGridManager().TileFinding);
                    ai.controlUnit.SetAbilityRangeTiles(rangeTiles);
                    return Node.Status.Success;
                }
                else
                    ai.controlUnit.Move(StageManager.Instance.GetGridManager().TileFinding.FindPath(ai.controlUnit.CurrentTile.Position, ai.movePosition, ai.controlUnit.GetMovableTiles()));
            }
            return Node.Status.Running;
        }
    }

    public class AttackTargetStrategy : IStrategy
    {
        readonly AIController ai;

        public AttackTargetStrategy(AIController ai)
        {
            this.ai = ai;
        }

        public Node.Status Process()
        {
            if (ai.controlUnit.Character.CurrentState == CharacterTurnState.AbilityPerformDone)
            {
                return Node.Status.Success;
            }
                
            if (ai.controlUnit.Character.CurrentState == CharacterTurnState.AbilityTargetSelect)
            {
                ai.controlUnit.ConfirmAbilityTarget(ai.attackTarget.CurrentTile);
            }
            return Node.Status.Running;
        }
    }
    public class DoTurnEndStrategy : IStrategy
    {
        readonly AIController ai;

        public DoTurnEndStrategy(AIController ai)
        {
            this.ai = ai;
        }

        public Node.Status Process()
        {
            ai.controlUnit.OnTurnEnd();
            ai.StopAI();
            return Node.Status.Success;
        }
    }
}
