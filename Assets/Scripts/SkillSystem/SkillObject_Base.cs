using UnityEngine;

public class SkillObject_Base : MonoBehaviour
{
    [SerializeField] protected LayerMask whatIsEnemy;
    [SerializeField] protected Transform targetCheck;
    [SerializeField] protected float checkRadius = 1;

    protected void DamageEnemiesInRadius(Transform transform, float radius)
    {
        foreach(var target in EnemiesAround(transform, radius))
        {
            IDamagable damagable = target.GetComponent<IDamagable>();

            if (damagable == null) continue;

            damagable.TakeDamage(1, 1, ElementType.None, transform);
        }
    }

    protected Collider2D[] EnemiesAround(Transform transform, float radius)
    {
        return Physics2D.OverlapCircleAll(transform.position, radius, whatIsEnemy);
    }

    protected virtual void OnDrawGizmos()
    {
        if (targetCheck == null) 
        { 
            targetCheck = transform;
        }

        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}
