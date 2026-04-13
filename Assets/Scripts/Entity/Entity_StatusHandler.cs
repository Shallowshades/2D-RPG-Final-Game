using NUnit.Framework.Internal.Commands;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Entity_StatusHandler : MonoBehaviour
{
    private Entity entity;
    private Entity_VFX entityVfx;
    private Entity_Stats entityStats;
    private Entity_Health entityHealth;
    private ElementType currentEffect = ElementType.None;

    [Header("Shock effect details")]
    [SerializeField] private GameObject lightningStrikeVfx;
    [SerializeField] private float currentCharge;
    [SerializeField] private float maximumCharge = 1;

    private Coroutine shockCoroutine;

    private void Awake()
    {
        entity = GetComponent<Entity>();
        entityVfx = GetComponent<Entity_VFX>();
        entityStats = GetComponent<Entity_Stats>();
        entityHealth = GetComponent<Entity_Health>();
    }

    public void RemoveAllNegativeEffects()
    {
        StopAllCoroutines();
        currentEffect = ElementType.None;
        entityVfx.StopAllVfx();
    }

    public void ApplyStatusEffect(ElementType element, ElementalEffectData effectData)
    {
        if (element == ElementType.Ice && CanBeApplied(ElementType.Ice)) 
        {
            ApplyChillEffect(effectData.chillDuration, effectData.chillSlowMultiplier);
        }

        if (element == ElementType.Fire && CanBeApplied(ElementType.Fire))
        {
            ApplyBurnEffect(effectData.burnDuration, effectData.totalBurnDamage);
        }

        if (element == ElementType.Lightning && CanBeApplied(ElementType.Lightning))
        {
            ApplyShockEffect(effectData.shockDuration, effectData.shockDamage, effectData.shockCharge);
        }
    }

    private void ApplyShockEffect(float duration, float damage, float charge)
    {
        float lightningResistance = entityStats.GetElementalResistance(ElementType.Lightning);
        float finalCharge = charge * (1 - lightningResistance);
        currentCharge = currentCharge + finalCharge;
        
        if (currentCharge >= maximumCharge)
        {
            DoLightingStrike(damage);
            StopShockEffect();
            return;
        }

        if (shockCoroutine != null)
        {
            StopCoroutine(shockCoroutine);
        }

        shockCoroutine = StartCoroutine(ShockEffectCoroutine(duration));

    }

    private void StopShockEffect()
    {
        currentEffect = ElementType.None;
        currentCharge = 0;
        entityVfx.StopAllVfx();
    }

    private void DoLightingStrike(float damage)
    {
        Instantiate(lightningStrikeVfx, transform.position, Quaternion.identity);
        entityHealth.ReduceHp(damage);
    }

    private IEnumerator ShockEffectCoroutine(float duration)
    {
        currentEffect = ElementType.Lightning;
        entityVfx.PlayOnStatusVfs(duration, ElementType.Lightning);

        yield return new WaitForSeconds(duration);
        StopShockEffect();
    }

    private void ApplyBurnEffect(float duration, float fireDamage)
    {
        float fireResistance = entityStats.GetElementalResistance(ElementType.Fire);
        float finalDamage = fireDamage * (1 - fireResistance);

        StartCoroutine(BurnEffectCortinue(duration, finalDamage));
    }

    private IEnumerator BurnEffectCortinue(float duration, float totalDamage)
    {
        currentEffect = ElementType.Fire;
        entityVfx.PlayOnStatusVfs(duration, ElementType.Fire);

        int ticksPerSecond = 2;
        int tickCount = Mathf.RoundToInt(ticksPerSecond * duration);

        float damagePerTick = totalDamage / tickCount;
        float tickInterval = 1f / ticksPerSecond;

        for (int i = 0; i < tickCount; ++i)
        {
            Debug.Log("tick + " + damagePerTick);
            entityHealth.ReduceHp(damagePerTick);
            yield return new WaitForSeconds(tickInterval);
        } 

        currentEffect = ElementType.None;
    }

    private void ApplyChillEffect(float duration, float slowMultiplier)
    {
        float iceResistance = entityStats.GetElementalResistance(ElementType.Ice);
        float finalDuration = duration * (1 - iceResistance);

        StartCoroutine(ChilledEffectCortinue(finalDuration, slowMultiplier));
    }

    private IEnumerator ChilledEffectCortinue(float duration, float slowMultiplier)
    {
        entity.SlowDownEntity(duration, slowMultiplier);
        currentEffect = ElementType.Ice;
        entityVfx.PlayOnStatusVfs(duration, ElementType.Ice);

        yield return new WaitForSeconds(duration);
        currentEffect = ElementType.None;
    }

    public bool CanBeApplied(ElementType element) 
    {
        if (element == ElementType.Lightning && currentEffect == ElementType.Lightning)
        {
            return true;
        }

        return currentEffect == ElementType.None; 
    }
}
