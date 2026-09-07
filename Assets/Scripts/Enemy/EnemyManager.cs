using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private Vector2 timeRangeBetweenAttacks = new Vector2(1, 4);

    [SerializeField] private CombatController player;
    
    public static EnemyManager instance { get; private set; }

    private float notAttackingTimer = 2f;
    private List<EnemyController> enemiesInRange = new List<EnemyController>();

    private void Awake()
    {
        instance = this;
    }

    public void AddEnemyInRange(EnemyController enemy)
    {
        if (!enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Add(enemy);
        }
    }

    public void RemoveEnemyInRange(EnemyController enemy)
    {
        enemiesInRange.Remove(enemy);

        if (enemy == player.TargetEnemy)
        {
            enemy.MeshHighlighter?.HighlightMesh(false);
            player.TargetEnemy = GetClosestEnemyToDirection(player.GetTargetingDir());
            player.TargetEnemy?.MeshHighlighter?.HighlightMesh(true);
        }
    }

    private float timer = 0f;
    private void Update()
    {
        if (enemiesInRange.Count == 0)
        {
            return;
        }
        
        if (!enemiesInRange.Any(e => e.IsInState(EnemyStates.EnemyAttack)))
        {
            if (notAttackingTimer > 0)
            {
                notAttackingTimer -= Time.deltaTime;
            }

            if (notAttackingTimer <= 0)
            {
                var attackingEnemy = SelectEnemyForAttack();

                if (attackingEnemy != null)
                {
                    attackingEnemy.ChangeState(EnemyStates.EnemyAttack);
                    notAttackingTimer = Random.Range(timeRangeBetweenAttacks.x, timeRangeBetweenAttacks.y);
                }
            }
        }

        if (timer > 0.1f)
        {
            timer = 0f;
            var closestEnemy = GetClosestEnemyToDirection(player.GetTargetingDir());
            if (closestEnemy != null && closestEnemy != player.TargetEnemy)
            {
                var prevEnemy = player.TargetEnemy;
                player.TargetEnemy = closestEnemy;
                
                player?.TargetEnemy?.MeshHighlighter.HighlightMesh(true);
                prevEnemy?.MeshHighlighter?.HighlightMesh(false);
            }
        }
        
        timer += Time.deltaTime;
    }

    private EnemyController SelectEnemyForAttack()
    {
        return enemiesInRange.OrderByDescending(e => e.CombatMovementTimer)
            .FirstOrDefault(e => e.Target != null && e.IsInState(EnemyStates.CombatMovement));
    }

    public EnemyController GetAttackingEnemy()
    {
        return enemiesInRange.FirstOrDefault(e => e.IsInState(EnemyStates.EnemyAttack));
    }

    public EnemyController GetClosestEnemyToDirection(Vector3 direction)
    {
        float minDistance = Mathf.Infinity;
        EnemyController closestEnemy = null;

        foreach (var enemy in enemiesInRange)
        {
            var vecToEnemy = enemy.transform.position - player.transform.position;
            vecToEnemy.y = 0;

            float angle = Vector3.Angle(direction, vecToEnemy);
            float distance = vecToEnemy.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }
}
