using System;
using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public event Action OnFlipped;

    public Rigidbody2D rb { get; private set; }
    public Animator animator { get; private set; }
    public Entity_Stats stats { get; private set; }
    protected StateMachine stateMachine;

    private bool facingRight = true;
    public int facingDir { get; private set; } = 1;

    [Header("Collision detection")]
    public LayerMask whatIsGround;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform primaryWallCheck;
    [SerializeField] private Transform seconderyWallCheck;
    public bool groundDetected { get; private set; }
    public bool wallDetected { get; private set; }

    //Condition variables
    private bool isKnocked;
    private Coroutine knockbackCoroutine;
    private Coroutine slowDownCoroutine;

    protected virtual void Awake()
    {
        // 确保优先获取组件
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<Entity_Stats>();

        // 其次再构造字段
        stateMachine = new StateMachine();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {
        HandleCollisionDetection();
        stateMachine.UpdateActiveState();
    }

    public void CurrentStateAnimationTrigger()
    {
        stateMachine.currentState.AnimationTrigger();
    }

    public virtual void EntityDeath()
    {

    }

    public virtual void SlowDownEntity(float duration, float slowMultiplier, bool canOverrideSlowEffect = false)
    {
        if (slowDownCoroutine != null)
        {
            if (canOverrideSlowEffect)
            {
                StopCoroutine(slowDownCoroutine);
            }
            else
            {
                return;
            }
        }

        slowDownCoroutine = StartCoroutine(SlowDownEntityCoroutine(duration, slowMultiplier));
    }

    protected virtual IEnumerator SlowDownEntityCoroutine(float duration, float slowMultiplier)
    {
        yield return null;
    }

    // 直接赋值为null会停止协程吗
    public virtual void StopSlowDown()
    {
        slowDownCoroutine = null;
    }

    public void ReciveKnockback(Vector2 knockback, float duration)
    {
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(KnockbackCoroutine(knockback, duration));
    }

    private IEnumerator KnockbackCoroutine(Vector2 knockback, float duration) 
    {
        isKnocked = true;
        rb.linearVelocity = knockback;

        yield return new WaitForSeconds(duration);

        rb.linearVelocity = Vector2.zero;
        isKnocked = false;
    }

    public void SetVelocity(float xVelocity, float yVelocity)
    {
        if (isKnocked)
        {
            return;
        }

        rb.linearVelocity = new Vector2(xVelocity, yVelocity);
        HandleFlip(xVelocity);
    }

    public void HandleFlip(float xVelocity)
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

        OnFlipped?.Invoke();
    }

    private void HandleCollisionDetection()
    {
        groundDetected = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, whatIsGround);
        if (seconderyWallCheck != null)
        {
            wallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround)
                    && Physics2D.Raycast(seconderyWallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
        }
        else
        {
            wallDetected = Physics2D.Raycast(primaryWallCheck.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + new Vector3(0, -groundCheckDistance));
        Gizmos.DrawLine(primaryWallCheck.position, primaryWallCheck.position + new Vector3(wallCheckDistance * facingDir, 0));
        
        if (seconderyWallCheck != null)
        {
            Gizmos.DrawLine(seconderyWallCheck.position, seconderyWallCheck.position + new Vector3(wallCheckDistance * facingDir, 0));
        }
    }
}
