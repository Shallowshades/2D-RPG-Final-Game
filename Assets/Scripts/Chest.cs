using UnityEngine;

public class Chest : MonoBehaviour, IDamagable
{
    private Rigidbody2D rb => GetComponentInChildren<Rigidbody2D>();
    private Animator animator => GetComponentInChildren<Animator>();
    private Entity_VFX fx => GetComponent<Entity_VFX>();

    [Header("Open Details")]
    [SerializeField] private Vector2 knockback;

    [Header("Broken Details")]
    [SerializeField] private int hitCount = 0;
    [SerializeField] private int hitCountLimit = 5;

    public void TakeDamage(float damage, Transform damageDealer)
    {
        fx.PlayOnDamageVfx();
        animator.SetBool("open", true);
        rb.linearVelocity = knockback;
        rb.angularVelocity = Random.Range(-200f, 200f);

        if (++hitCount == hitCountLimit)
        {
            animator.SetBool("broken", true);
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
