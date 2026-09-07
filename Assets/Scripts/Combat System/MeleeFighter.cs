using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackStates
{
    Idle,
    Windup,
    Impact,
    Cooldown
}

public class MeleeFighter : MonoBehaviour
{
    [SerializeField] private List<AttackData> attacks;
    [SerializeField] private GameObject sword;

    [SerializeField] private float rotationSpeed = 500f;
    
    public event Action OnGotHit;
    public event Action OnHitComplet;
    
    public bool InAction { get; private set; } = false;
    public bool InCounter { get; set; } = false;

    public AttackStates AttackState { get; private set; }
    private bool doCombo;
    private int comboCount = 0;

    private Animator animator;
    private BoxCollider swordCollider;
    private SphereCollider leftHandCollider, rightHandCollider, leftFootCollider, rightFootCollider;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (sword != null)
        {
            swordCollider = sword.GetComponent<BoxCollider>();
            leftHandCollider = animator.GetBoneTransform(HumanBodyBones.LeftHand).GetComponent<SphereCollider>();
            rightHandCollider = animator.GetBoneTransform(HumanBodyBones.RightHand).GetComponent<SphereCollider>();
            leftFootCollider = animator.GetBoneTransform(HumanBodyBones.LeftFoot).GetComponent<SphereCollider>();
            rightFootCollider = animator.GetBoneTransform(HumanBodyBones.RightFoot).GetComponent<SphereCollider>();
            
            DisableAllHitBoxs();
        }
    }

    public void TryToAttack(Vector3? attackDir = null)
    {
        if (!InAction)
        {
            StartCoroutine(Attack(attackDir));
        }
        else if (AttackState == AttackStates.Impact || AttackState == AttackStates.Cooldown)
        {
            doCombo = true;
        }
    }

    IEnumerator Attack(Vector3? attackDir = null)
    {
        InAction = true;
        AttackState = AttackStates.Windup;
        
        animator.CrossFade(attacks[comboCount].AnimName, 0.2f);
        yield return null;
        
        var animState = animator.GetNextAnimatorStateInfo(1);

        float timer = 0f;
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / animState.length;

            if (attackDir != null)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(attackDir.Value),
                    rotationSpeed * Time.deltaTime);
            }

            if (AttackState == AttackStates.Windup)
            {
                if (InCounter)
                {
                    break;
                }
                
                if (normalizedTime >= attacks[comboCount].ImpactStartTime)
                {
                    AttackState = AttackStates.Impact;
                    EnableHitBox(attacks[comboCount]);
                }
            }
            else if (AttackState == AttackStates.Impact)
            {
                if (normalizedTime >= attacks[comboCount].ImpactEndTime)
                {
                    AttackState = AttackStates.Cooldown;
                    DisableAllHitBoxs();
                }
            }
            else if (AttackState == AttackStates.Cooldown)
            {
                if (doCombo)
                {
                    doCombo = false;
                    comboCount = (comboCount + 1) % attacks.Count;

                    StartCoroutine(Attack());
                    yield break;
                }
            }
            
            yield return null;
        }

        AttackState = AttackStates.Idle;
        comboCount = 0;
        InAction = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HitBox") && !InAction)
        {
            StartCoroutine(PlayerHitReaction(other.GetComponentInParent<MeleeFighter>().transform));
        }
    }
    
    IEnumerator PlayerHitReaction(Transform attacker)
    {
        InAction = true;

        var dispVec = attacker.position - transform.position;
        dispVec.y = 0;
        transform.rotation = Quaternion.LookRotation(dispVec);

        OnGotHit?.Invoke();
        
        animator.CrossFade("OnSwordImpact", 0.2f);
        yield return null;
        
        var animState = animator.GetNextAnimatorStateInfo(1);
        
        yield return new WaitForSeconds(animState.length * 0.8f);

        OnHitComplet?.Invoke();
        InAction = false;
    }
    
    public IEnumerator PerformCounterattack(EnemyController opponent)
    {
        InAction = true;

        InCounter = true;
        opponent.Fighter.InCounter = true;
        opponent.ChangeState(EnemyStates.Dead);

        var dispVec = opponent.transform.position - transform.position;
        dispVec.y = 0f;
        transform.rotation = Quaternion.LookRotation(dispVec);
        opponent.transform.rotation = Quaternion.LookRotation(-dispVec);

        var targetPos = opponent.transform.position - dispVec.normalized * 1f;
        
        animator.CrossFade("Counterattack", 0.2f);
        opponent.Animator.CrossFade("BeCounterattacked", 0.2f);
        yield return null;
        
        var animState = animator.GetNextAnimatorStateInfo(1);

        //yield return new WaitForSeconds(animState.length * 0.8f);
        float timer = 0f;
        while (timer <= animState.length)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 5 * Time.deltaTime);
            
            yield return null;

            timer += Time.deltaTime;
        }
        
        InCounter = false;
        opponent.Fighter.InCounter = false;

        InAction = false;
    }

    private void EnableHitBox(AttackData attackData)
    {
        switch (attackData.HitBoxToUse)
        {
            case AttackHitBox.LeftHand:
                leftHandCollider.enabled = true;
                break;
            case AttackHitBox.RightHand:
                rightHandCollider.enabled = true;
                break;
            case AttackHitBox.LeftFoot:
                leftFootCollider.enabled = true;
                break;
            case AttackHitBox.RightFoot:
                rightFootCollider.enabled = true;
                break;
            case AttackHitBox.Sword:
                swordCollider.enabled = true;
                break;
            default:
                break;
        }
    }
    
    private void DisableAllHitBoxs()
    {
        swordCollider.enabled = false;

        if (leftHandCollider != null)
        {
            leftHandCollider.enabled = false;
        }

        if (rightHandCollider != null)
        {
            rightHandCollider.enabled = false;
        }

        if (leftFootCollider != null)
        {
            leftFootCollider.enabled = false;
        }

        if (rightFootCollider != null)
        {
            rightFootCollider.enabled = false;
        }
    }

    public List<AttackData> Attacks => attacks;

    public bool IsCounterable => AttackState == AttackStates.Windup && comboCount == 0;
}
