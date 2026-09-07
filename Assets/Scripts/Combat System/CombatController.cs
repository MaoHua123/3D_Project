using System;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    private EnemyController targetEnemy;

    public EnemyController TargetEnemy
    {
        get => targetEnemy;
        set
        {
            targetEnemy = value;

            if (targetEnemy == null)
            {
                CombatMode = false;
            }
        }
    }


    private bool combatMode;
    public bool CombatMode
    {
        get => combatMode;
        set
        {
            combatMode = value;

            if (TargetEnemy == null)
            {
                combatMode = false;
            }
            
            animator.SetBool("combatMode", combatMode);
        }
    }

    private MeleeFighter meleeFighter;
    private Animator animator;
    private CameraController cam;

    private void Awake()
    {
        meleeFighter = GetComponent<MeleeFighter>();
        animator = GetComponent<Animator>();
        cam = Camera.main.GetComponent<CameraController>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Attack1"))
        {
            var enemy = EnemyManager.instance.GetAttackingEnemy();
            if(enemy != null && enemy.Fighter.IsCounterable && !meleeFighter.InAction)//TODO:InAction有疑问
            {
                StartCoroutine(meleeFighter.PerformCounterattack(enemy));
            }
            else
            {
                var enemyToAttack = EnemyManager.instance.GetClosestEnemyToDirection(PlayerController.instance.InputDir);
                Vector3? dirToAttack = null;
                if (enemyToAttack != null)
                {
                    dirToAttack = enemyToAttack.transform.position - transform.position;
                }
                
                meleeFighter.TryToAttack(dirToAttack);

                CombatMode = true;
            }
        }

        if (Input.GetButtonDown("LockOn"))
        {
            CombatMode = !CombatMode;
        }
    }

    private void OnAnimatorMove()
    {
        if (!meleeFighter.InCounter)
        {
            transform.position += animator.deltaPosition;
        }
        
        transform.rotation *= animator.deltaRotation;
    }

    public Vector3 GetTargetingDir()
    {
        if (!CombatMode)
        {
            var vecFromCamera = transform.position - cam.transform.position;
            vecFromCamera.y = 0f;
            return vecFromCamera.normalized;
        }
        else
        {
            return transform.forward;
        }
    }
}
