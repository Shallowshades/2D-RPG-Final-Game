using UnityEngine;

public class Entity_Combat : MonoBehaviour
{
    private Entity_VFX vfx;
    public Entity_Stats stats;

    public DamageScaleData basicAttackScale;

    [Header("Target detection")]
    [SerializeField] private Transform targetCheck;
    [SerializeField] private float targetCheckRadius = 1f;
    [SerializeField] private LayerMask whatIsTarget;

    //[Header("Status effect details")]
    //[SerializeField] private float defaultDuration = 3;
    //[SerializeField] private float chillSlowMultiplier = 0.2f;
    //[SerializeField] private float electrifyChargeBuildUp = 0.4f;
    //[Space]
    //[SerializeField] private float fireScale = 0.8f;
    //[SerializeField] private float lightningScale = 2.5f;

    private void Awake()
    {
        vfx = GetComponent<Entity_VFX>();
        stats = GetComponent<Entity_Stats>();
    }

    public void PerformAttack()
    {
        foreach(var target in GetDetectedColliders())
        {
            IDamagable damagable = target.GetComponent<IDamagable>();

            if (damagable == null) continue;

            AttackData attackData = stats.GetAttackData(basicAttackScale);
            Entity_StatusHandler statusHandler = target.GetComponent<Entity_StatusHandler>();
            
            float physicalDamage = attackData.physicalDamage;
            bool isCrit = attackData.isCrit;
            float elementalDamage = attackData.elementalDamage;
            ElementType elementType = attackData.elementType;

            bool targetGotHit = damagable.TakeDamage(physicalDamage, elementalDamage, elementType, transform);
                        
            if (elementType != ElementType.None)
            {
                statusHandler?.ApplyStatusEffect(elementType, attackData.effectData);
            }
            
            if (targetGotHit)
            {
                vfx.CreateOnHitVFX(target.transform, isCrit, elementType);
            }
        }
    }

    protected Collider2D[] GetDetectedColliders()
    {
        return Physics2D.OverlapCircleAll(targetCheck.position, targetCheckRadius, whatIsTarget);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(targetCheck.position, targetCheckRadius);
    }
}
