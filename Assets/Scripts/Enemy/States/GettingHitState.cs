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
        enemy.Fighter.OnHitComplet += () => StartCoroutine(GoToCombatMovement());
    }

    IEnumerator GoToCombatMovement()
    {
        yield return new WaitForSeconds(stumTime);
        enemy.ChangeState(EnemyStates.CombatMovement);
    }
}
