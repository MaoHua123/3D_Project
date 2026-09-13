using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 500f;
    
    [Header("地面检查设置")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Vector3 groundCheckOffset;
    [SerializeField] private LayerMask groundLayer;

    private bool isGrounded;
    private Quaternion targetRotation;
    
    public Vector3 InputDir { get; private set; }

    private float ySpeed;
    private Animator animator;
    private CameraController cameraController;
    private CharacterController characterController;
    private MeleeFighter meleeFighter;
    private CombatController combatController;
    public static PlayerController instance { get; private set; }

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        meleeFighter = GetComponent<MeleeFighter>();
        combatController = GetComponent<CombatController>();

        instance = this;
    }

    private void Update()
    {
        if (meleeFighter.InAction)
        {
            targetRotation = transform.rotation;
            animator.SetFloat("forwardSpeed", 0f);
            return;
        }
        
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

        var moveInput = new Vector3(h, 0, v).normalized;

        var moveDir = cameraController.PlanarRotation * moveInput;
        InputDir = moveDir;
        
        GroundCheck();

        if (isGrounded)
        {
            ySpeed = -0.5f;
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
        }
        
        var velocity = moveDir * moveSpeed;
        
        if (combatController.CombatMode)
        {
            velocity /= 4f;

            var targetVec = combatController.TargetEnemy.transform.position - transform.position;
            targetVec.y = 0;
            
            if (moveAmount > 0)
            {
                targetRotation = Quaternion.LookRotation(targetVec);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 
                    rotationSpeed * Time.deltaTime);
            }

            // 参数必须在每帧写入，无输入时velocity为0，自然归零回 Idle
            // 若只在moveAmount>0时更新，停止输入后参数会冻结在最后非零值，动画卡在Walk
            float forwardSpeed = Vector3.Dot(velocity, transform.forward);
            animator.SetFloat("forwardSpeed", forwardSpeed / moveSpeed, 0.2f, Time.deltaTime);

            float angle = Vector3.SignedAngle(transform.forward, velocity, Vector3.up);
            float strafeSpeed = Mathf.Sin(angle * Mathf.Deg2Rad);
            animator.SetFloat("strafeSpeed", strafeSpeed, 0.2f, Time.deltaTime);
        }
        else
        {
            if (moveAmount > 0)
            {
                targetRotation = Quaternion.LookRotation(moveDir);
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 
                rotationSpeed * Time.deltaTime);
        
            animator.SetFloat("forwardSpeed", moveAmount, 0.2f, Time.deltaTime);
        }
        
        velocity.y = ySpeed;
        characterController.Move(velocity * Time.deltaTime);
    }

    void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), 
            groundCheckRadius, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }
}
