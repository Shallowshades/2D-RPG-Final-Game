using UnityEngine;

public class Player : MonoBehaviour
{
    public Rigidbody2D rb {  get; private set; }
    public Animator animator { get; private set; }
    public PlayerInputSet input {  get; private set; }
    private StateMechine stateMechine;
    public Player_IdleState idleState {  get; private set; }
    public Player_MoveState moveState { get; private set; }
    public Player_JumpState jumpState { get; private set; }
    public Player_FallState fallState { get; private set; }
    public Player_WallSlideState wallSlideState { get; private set; }
    public Player_WallJumpState wallJumpState { get; private set; }

    [Header("Movement details")]
    public float moveSpeed = 8;
    public float jumpForce = 12;
    public Vector2 wallJumpForce;

    [Range(0f, 1f)]
    public float inAirMoveMultiplier = 0.7f;
    [Range(0f, 1f)]
    public float wallSlideSlowMultiplier = 0.3f;
    private bool facingRight = true;
    public int facingDir { get; private set; } = 1;

    public Vector2 moveInput { get; private set; }

    [Header("Collision detection")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask whatIsGround;
    public bool groundDetected { get; private set; }
    public bool wallDetected { get; private set; }



    private void Awake()
    {
        // 确保优先获取组件
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        // 其次再构造字段
        stateMechine = new StateMechine();
        input = new PlayerInputSet();

        idleState = new Player_IdleState(this, stateMechine, "idle");
        moveState = new Player_MoveState(this, stateMechine, "move");
        jumpState = new Player_JumpState(this, stateMechine, "jumpFall");
        fallState = new Player_FallState(this, stateMechine, "jumpFall");
        wallSlideState = new Player_WallSlideState(this, stateMechine, "wallSlide");
        wallJumpState = new Player_WallJumpState(this, stateMechine, "wallJump");
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
        stateMechine.Initialize(idleState);
    }

    private void Update()
    {
        HandleCollisionDetection();
        stateMechine.UpdateActiveState();
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
        wallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -groundCheckDistance));
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(wallCheckDistance * facingDir, 0));
    }
}
