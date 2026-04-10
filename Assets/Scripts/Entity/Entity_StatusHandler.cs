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

    [Header("Electrify effect details")]
    [SerializeField] private GameObject lightningStrikeVfx;
    [SerializeField] private float currentCharge;
    [SerializeField] private float maximumCharge = 1;

    private Coroutine electrifyCoroutine;

    private void Awake()
    {
        entity = GetComponent<Entity>();
        entityVfx = GetComponent<Entity_VFX>();
        entityStats = GetComponent<Entity_Stats>();
        entityHealth = GetComponent<Entity_Health>();
    }

    public void ApplyElectrifyEffect(float duration, float damage, float charge)
    {
        float lightningResistance = entityStats.GetElementalResistance(ElementType.Lightning);
        float finalCharge = charge * (1 - lightningResistance);
        currentCharge = currentCharge + finalCharge;
        
        if (currentCharge >= maximumCharge)
        {
            DoLightingStrike(damage);
            StopElectrifyEffect();
            return;
        }

        if (electrifyCoroutine != null)
        {
            StopCoroutine(electrifyCoroutine);
        }

        electrifyCoroutine = StartCoroutine(ElectrifyEffectCoroutine(duration));

    }

    private void StopElectrifyEffect()
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

    private IEnumerator ElectrifyEffectCoroutine(float duration)
    {
        currentEffect = ElementType.Lightning;
        entityVfx.PlayOnStatusVfs(duration, ElementType.Lightning);

        yield return new WaitForSeconds(duration);
        StopElectrifyEffect();
    }

    public void ApplyBurnEffect(float duration, float fireDamage)
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

    public void ApplyChillEffect(float duration, float slowMultiplier)
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
