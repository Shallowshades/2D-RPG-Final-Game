using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item Effect/Ice Blast", fileName = "Item Effect Data - Ice Blast On Taking Damage")]
public class ItemEffect_IceBlastonTakingDamage : ItemEffect_DataSO
{
    [SerializeField] private ElementalEffectData effectData;
    [SerializeField] private float iceDamage;
    [SerializeField] private LayerMask whatIsEnemy;

    [Space]
    [SerializeField] private float healthPercentTrigger = 0.25f;
    [SerializeField] private float cooldown = 40.0f;
    private float lastTimeUsed = float.NegativeInfinity; // TODO: 那我穿上再脱下, 那不就是无CD

    [Header("Vfx objects")]
    [SerializeField] private GameObject iceBlastVfx;
    [SerializeField] private GameObject onHitVfx;

    public override void ExecuteEffect()
    {
        bool noCooldown = Time.time >= lastTimeUsed + cooldown;
        bool reachedThreshold = player.health.GetHealthPercent() <= healthPercentTrigger;

        if (noCooldown && reachedThreshold)
        {
            player.playerVfx.CreateEffectOf(iceBlastVfx, player.transform);
            lastTimeUsed = Time.time;
            DamageEnemiesWithIce();
        }
    }

    private void DamageEnemiesWithIce()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(player.transform.position, 1.5f, whatIsEnemy);

        foreach (var target in enemies)
        {
            IDamagable damagable = target.GetComponent<IDamagable>();

            if (damagable == null)
            {
                continue;
            }

            bool targetGotHit = damagable.TakeDamage(0, iceDamage, ElementType.Ice, player.transform);

            Entity_StatusHandler statusHandler = target.GetComponent<Entity_StatusHandler>();
            statusHandler?.ApplyStatusEffect(ElementType.Ice, effectData);

            if (targetGotHit)
            {
                player.playerVfx.CreateEffectOf(onHitVfx, target.transform);
            }
        }
    }

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        // 先减后加 → 幂等，防重复订阅
        player.health.OnTakingDamage -= ExecuteEffect;
        player.health.OnTakingDamage += ExecuteEffect;
        
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        player.health.OnTakingDamage -= ExecuteEffect;
        player = null;
        // 不重置cooldown的值, 防刷CD
    }
}
