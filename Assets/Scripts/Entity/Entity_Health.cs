using UnityEngine;
using UnityEngine.UI;

public class Entity_Health : MonoBehaviour, IDamagable
{
    private Slider healthBar;
    private Entity entity;
    private Entity_VFX entityVfx;
    private Entity_Stats entityStats;

    [SerializeField] private float currentHealth;
    [SerializeField] protected bool isDead;

    [Header("Health regenerate")]
    [SerializeField] private float regenInterval = 1;
    [SerializeField] private bool canRegenerateHealth = true;

    [Header("On Damage Knockback")]
    [SerializeField] private Vector2 knockbackPower = new Vector2(1.5f, 2.5f);
    [SerializeField] private Vector2 heavyKnockbackPower = new Vector2(7f, 7f);
    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField] private float heavyKnockbackDuration = 0.5f;   // 重击伤害阈值

    [Header("On Heavy Damage")]
    [SerializeField] private float heavyDamageThreshold = 0.3f;

    protected virtual void Awake()
    {
        entity = GetComponent<Entity>();
        entityVfx = GetComponent<Entity_VFX>();
        entityStats = GetComponent<Entity_Stats>();
        healthBar = GetComponentInChildren<Slider>();
        
        currentHealth = entityStats.GetMaxHealth();
        UpdateHealthBar();
        InvokeRepeating(nameof(RegenerateHealth), 0, regenInterval);
    }

    public virtual bool TakeDamage(float damage, float elementalDamage, ElementType element, Transform damageDealer)
    {
        if (isDead) return false;

        if (AttackEvaded())
        {
            Debug.Log($"{gameObject.name} evaded the attack");
            return false;
        }

        Entity_Stats attackerStats = damageDealer.GetComponent<Entity_Stats>();
        float armorReduction = attackerStats != null ? attackerStats.GetArmorReduction() : 0;

        float mitigation = entityStats.GetArmorMitigation(armorReduction);
        float physicalDamageTaken = damage * (1 - mitigation);

        float resistance = entityStats.GetElementalResistance(element);
        float elementalDamageTaken = elementalDamage * (1 - resistance);

        TakeKnockback(damageDealer, physicalDamageTaken);

        ReduceHp(physicalDamageTaken + elementalDamageTaken);

        return true;
    }

    private bool AttackEvaded() => Random.Range(0, 100) < entityStats.GetEvasion();
    
    private void RegenerateHealth()
    {
        if (canRegenerateHealth == false) return;

        float regenerateAmount = entityStats.resources.healthRegen.GetValue();
        IncreaseHealth(regenerateAmount);
    }

    public void IncreaseHealth(float healAmount)
    {
        if (isDead) return;

        float newHealth = currentHealth + healAmount;
        float maxHealth = entityStats.GetMaxHealth();
        currentHealth = Mathf.Min(newHealth, maxHealth);
        UpdateHealthBar();
    }
    
    public void ReduceHp(float damage)
    {
        entityVfx?.PlayOnDamageVfx();
        currentHealth -= damage;
        UpdateHealthBar();

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        entity.EntityDeath();
    }

    public float GetHealthPercent() => currentHealth / entityStats.GetMaxHealth();

    public void SetHealthToPercent(float percent)
    {
        currentHealth = entityStats.GetMaxHealth() * Mathf.Clamp01(percent);
        UpdateHealthBar() ;
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null) return;

        healthBar.value = currentHealth / entityStats.GetMaxHealth();
    }

    private void TakeKnockback(Transform damageDealer, float finalDamage)
    {
        Vector2 knockback = CalculateKnockback(finalDamage, damageDealer);
        float duration = CalculateDuration(finalDamage);

        entity?.ReciveKnockback(knockback, duration);
    }
    private Vector2 CalculateKnockback(float damage, Transform damageDealer)
    {
        int direction = transform.position.x > damageDealer.position.x ? 1 : -1;

        Vector2 knockback = CalculateKnockback(damage);
        knockback.x *= direction;

        return knockback;
    }
    private Vector2 CalculateKnockback(float damage) => IsHeavyDamage(damage) ? heavyKnockbackPower : knockbackPower;
    private float CalculateDuration(float damage) => IsHeavyDamage(damage) ? heavyKnockbackDuration : knockbackDuration;
    private bool IsHeavyDamage(float damage) => damage / entityStats.GetMaxHealth() > heavyDamageThreshold;
}
