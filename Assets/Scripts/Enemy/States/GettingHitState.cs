using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GettingHitState : State<EnemyController>
{
    [SerializeField] private float stumTime = 0.5f;
    
    private EnemyController enemy;

    public override void Enter(EnemyController owner)
    {
        enemy = owner;

        //受击时停下寻路，避免敌人一边挨打一边继续走向旧目标
        enemy.NavAgent.ResetPath();

        enemy.Fighter.OnHitComplet += () => StartCoroutine(GoToCombatMovement());
    }

    IEnumerator GoToCombatMovement()
    {
        yield return new WaitForSeconds(stumTime);
        enemy.ChangeState(EnemyStates.CombatMovement);
    }
}
