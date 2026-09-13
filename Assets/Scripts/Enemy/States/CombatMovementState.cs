using UnityEngine;

public enum AICombatStates
{
    Idle,
    Chase,
    Circling
}

public class CombatMovementState : State<EnemyController>
{
    [SerializeField] private float circlingSpeed = 20f;
    [SerializeField] private float distanceToStand = 3f;
    [SerializeField] private float adjustDistanceThreshold = 1f;
    [SerializeField] private Vector2 idleTimeRange = new Vector2(2, 5);
    [SerializeField] private Vector2 circlingTimeRange = new Vector2(3, 6);

    private float timer = 0f;

    private int circlingDir = 1;
    
    private AICombatStates state;
    
    private EnemyController enemy;
    
    public override void Enter(EnemyController owner)
    {
        enemy = owner;

        enemy.NavAgent.stoppingDistance = distanceToStand;
        enemy.CombatMovementTimer = 0f;
        
        enemy.Animator.SetBool("combatMode", true);
    }

    public override void Execute()
    {
        if (enemy.Target == null)
        {
            enemy.Target = enemy.FindTarget();
            if (enemy.Target == null)
            {
                enemy.ChangeState(EnemyStates.Idle);
                return;
            }
        }
        
        if (Vector3.Distance(enemy.Target.transform.position,
                enemy.transform.position) > distanceToStand + adjustDistanceThreshold)
        {
            StartChase();
        }
        
        if (state == AICombatStates.Idle)
        {
            if (timer <= 0)
            {
                if (Random.Range(0, 2) == 0)
                {
                    StartIdle();
                }
                else
                {
                    StartCircling();
                }
            }
        }
        else if (state == AICombatStates.Chase)
        {
            if (Vector3.Distance(enemy.Target.transform.position,
                    enemy.transform.position) <= distanceToStand + 0.03f)
            {
                StartIdle();
                return;
            }
            
            enemy.NavAgent.SetDestination(enemy.Target.transform.position);
        }
        else if (state == AICombatStates.Circling)
        {
            if (timer <= 0)
            {
                StartIdle();
                return;
            }

            //transform.RotateAround(enemy.Target.transform.position, Vector3.up, circlingSpeed * circlingDir * Time.deltaTime);
            
            var vecToTarget = enemy.transform.position - enemy.Target.transform.position;
            var rotatePos = Quaternion.Euler(0, circlingSpeed * circlingDir * Time.deltaTime, 0) * vecToTarget;
            
            enemy.NavAgent.Move(rotatePos - vecToTarget);
            enemy.transform.rotation = Quaternion.LookRotation(-rotatePos);
        }

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

        enemy.CombatMovementTimer += Time.deltaTime;
    }

    private void StartIdle()
    {
        state = AICombatStates.Idle;
        timer = Random.Range(idleTimeRange.x, idleTimeRange.y);

        //停止寻路，否则NavMeshAgent仍会沿旧路径走向最后一次SetDestination的位置
        enemy.NavAgent.ResetPath();
    }
    
    private void StartChase()
    {
        state = AICombatStates.Chase;
    }

    private void StartCircling()
    {
        state = AICombatStates.Circling;
        
        enemy.NavAgent.ResetPath();
        timer = Random.Range(circlingTimeRange.x, circlingTimeRange.y);

        circlingDir = Random.Range(0, 2) == 0 ? 1 : -1;
    }

    public override void Exit()
    {
        enemy.CombatMovementTimer = 0f;
    }
}