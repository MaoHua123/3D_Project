using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State<EnemyController>
{
    [SerializeField] private float attackDistance = 1f;

    private bool isAttacking;
    
    private EnemyController enemy;
    
    public override void Enter(EnemyController owner)
    {
        enemy = owner;

        enemy.NavAgent.stoppingDistance = attackDistance;
    }

    public override void Execute()
    {
        if (isAttacking)
        {
            return;
        }
        
        enemy.NavAgent.SetDestination(enemy.Target.transform.position);

        if (Vector3.Distance(enemy.Target.transform.position, enemy.transform.position) <= attackDistance + 0.03f)
        {
            StartCoroutine(Attack(Random.Range(0, enemy.Fighter.Attacks.Count + 1)));
        }
    }

    public override void Exit()
    {
        enemy.NavAgent.ResetPath();
    }

    IEnumerator Attack(int comboCount = 1)
    {
        isAttacking = true;
        enemy.Animator.applyRootMotion = true;
        
        enemy.Fighter.TryToAttack();

        for (int i = 1; i < comboCount; i++)
        {
            yield return new WaitUntil(() => enemy.Fighter.AttackState == AttackStates.Cooldown);
            enemy.Fighter.TryToAttack();
        }
        
        yield return new WaitUntil(() => enemy.Fighter.AttackState == AttackStates.Idle);

        enemy.Animator.applyRootMotion = false;
        isAttacking = false;

        if (enemy.IsInState(EnemyStates.EnemyAttack))
        {
            enemy.ChangeState(EnemyStates.RetreatAfterAttack);
        }
    }
}
