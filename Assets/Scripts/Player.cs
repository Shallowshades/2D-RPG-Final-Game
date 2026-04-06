using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb {  get; private set; }
    public Animator animator { get; private set; }
    public PlayerInputSet input {  get; private set; }
    private StateMachine stateMachine;
    public Player_IdleState idleState {  get; private set; }
    public Player_MoveState moveState { get; private set; }
    public Player_JumpState jumpState { get; private set; }
    public Player_FallState fallState { get; private set; }
    public Player_WallSlideState wallSlideState { get; private set; }
    public Player_WallJumpState wallJumpState { get; private set; }
    public Player_DashState dashState { get; private set; }
    public Player_BasicAttackState basicAttackState { get; private set; }
    public Player_JumpAttackState jumpAttackState { get; private set; }

    [Header("Attack details")]
    public Vector2[] attackVelocity;
    public Vector2 jumpAttackVelocity;
    public float attackVelocityDuration = 0.1f;
    public float comboxResetTime = 1f;
    private Coroutine queuedAttackCo;

    [Header("Movement details")]
    public float moveSpeed = 8;
    public float jumpForce = 12;
    public Vector2 wallJumpForce;

    [Range(0f, 1f)]
    public float inAirMoveMultiplier = 0.7f;
    [Range(0f, 1f)]
    public float wallSlideSlowMultiplier = 0.3f;
    [Space]
    public float dashDuration = 0.25f;
    public float dashSpeed = 20;

    private bool facingRight = true;
    public int facingDir { get; private set; } = 1;
    public Vector2 moveInput { get; private set; }

    [Header("Collision detection")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform primaryWallCheck;
    [SerializeField] private Transform seconderyWallCheck;
    public bool groundDetected { get; private set; }
    public bool wallDetected { get; private set; }



    private void Awake()
    {
        // 确保优先获取组件
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        // 其次再构造字段
        stateMachine = new StateMachine();
        input = new PlayerInputSet();

        idleState = new Player_IdleState(this, stateMachine, "idle");
        moveState = new Player_MoveState(this, stateMachine, "move");
        jumpState = new Player_JumpState(this, stateMachine, "jumpFall");
        fallState = new Player_FallState(this, stateMachine, "jumpFall");
        wallSlideState = new Player_WallSlideState(this, stateMachine, "wallSlide");
        wallJumpState = new Player_WallJumpState(this, stateMachine, "jumpFall");
        dashState = new Player_DashState(this, stateMachine, "dash");
        basicAttackState = new Player_BasicAttackState(this, stateMachine, "basicAttack");
        jumpAttackState = new Player_JumpAttackState(this, stateMachine, "jumpAttack");
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Movement.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Start()
    {
        stateMachine.Initialize(idleState);
    }

    private void Update()
    {
        HandleCollisionDetection();
        stateMachine.UpdateActiveState();
    }

    /// <summary>
    /// 协程延时
    /// </summary>
    public void EnterAttackStateWithDelay()
    {
        if (queuedAttackCo != null)
        {
            StopCoroutine(queuedAttackCo);
        }
        queuedAttackCo = StartCoroutine(EnterAttackStateWithDelayCo());
    }

    /// <summary>
    /// 由于连段攻击共用同一个字段作为进入攻击的判断, 同一帧改变状态, 该字段实则未变, 无法正常进入下一段
    /// 本帧结束时需basicAttack为false, 然后退出当前连段的攻击, 
    /// 下一帧直接进入下一连段的攻击
    /// </summary>
    /// <returns></returns>
    private IEnumerator EnterAttackStateWithDelayCo()
    {
        yield return new WaitForEndOfFrame();
        stateMachine.ChangeState(basicAttackState);
    }

    public void CallAnimationTrigger()
    {
        stateMachine.currentState.CallAnimationTrigger();
    }

    public void SetVelocity(float xVelocity, float yVelocity) 
    {
        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        HandleFlip(xVelocity);
    }

    private void HandleFlip(float xVelocity)
    {
        if (xVelocity < 0 && facingRight)
        {
            Flip();
        }
        else if (xVelocity > 0 && !facingRight)
        {
            Flip();
        }
    }

    public void Flip()
    {
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
        facingDir *= -1;
    }

    private void HandleCollisionDetection()
    {
        groundDetected = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        wallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround)
                    && Physics2D.Raycast(seconderyWallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));
        Gizmos.DrawLine(primaryWallCheck.position, primaryWallCheck.position + new Vector3(wallCheckDistance * facingDir, 0));
        Gizmos.DrawLine(seconderyWallCheck.position, seconderyWallCheck.position + new Vector3(wallCheckDistance * facingDir, 0));
    }
}
