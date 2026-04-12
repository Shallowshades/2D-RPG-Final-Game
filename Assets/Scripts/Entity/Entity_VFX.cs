using System.Collections;
using UnityEngine;

public class Entity_VFX : MonoBehaviour
{
    protected SpriteRenderer spriteRenderer;
    private Entity entity;

    [Header("On Damage VFX")]
    [SerializeField] private Material onDamageMaterial;
    [SerializeField] private float onDamageVfxDuration = 0.2f;
    private Material originalMaterial;
    private Coroutine onDamageVfxCoroutine;

    [Header("On Doing Damage VFX")]
    [SerializeField] private Color hitVfxColor = Color.white;
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private GameObject critHitVFX;

    [Header("Element Colors")]
    [SerializeField] private Color chillVfx = Color.cyan;
    [SerializeField] private Color burnVfx = Color.red;
    [SerializeField] private Color shockVfx = Color.yellow;
    private Color originalHitVfxColor;
    private Coroutine statusVfxCoroutine;

    private void Awake()
    {
        entity = GetComponent<Entity>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;
        originalHitVfxColor = hitVfxColor;
    }

    public void PlayOnStatusVfs(float duration, ElementType element)
    {
        if (element == ElementType.Ice)
        {
            StartCoroutine(PlayStatusVfxCoroutine(duration, chillVfx));
        }

        if (element == ElementType.Fire)
        {
            StartCoroutine(PlayStatusVfxCoroutine(duration, burnVfx));
        }

        if (element == ElementType.Lightning)
        {
            StartCoroutine(PlayStatusVfxCoroutine(duration, shockVfx));
        }
    }

    public void StopAllVfx()
    {
        StopAllCoroutines();

        spriteRenderer.color = Color.white;
        spriteRenderer.material = originalMaterial;
    }

    private IEnumerator PlayStatusVfxCoroutine(float duration, Color effectColor)
    {
        float tickInterval = 0.25f;
        float timeHasPassed = 0;

        Color lightColor = effectColor * 1.2f;
        Color darkColor = effectColor * 0.8f;

        bool toggle = false;

        while (timeHasPassed < duration)
        {
            spriteRenderer.color = toggle ? lightColor : darkColor;
            toggle = !toggle;

            yield return new WaitForSeconds(tickInterval);
            timeHasPassed += tickInterval;
        }

        spriteRenderer.color = Color.white;
    }

    public void CreateOnHitVFX(Transform target, bool isCrit, ElementType elementType)
    {
        GameObject hitPrefab = isCrit ? critHitVFX : hitVFX;
        GameObject vfx = Instantiate(hitPrefab, target.position, Quaternion.identity);
        
        // 有状态效果的情况下, 打击效果改变颜色会变得不太显眼
        // vfx.GetComponentInChildren<SpriteRenderer>().color = GetElementColor(elementType);

        if (entity.facingDir == -1 && isCrit)
        {
            vfx.transform.Rotate(0, 180, 0);
        } 
    }

    public Color GetElementColor(ElementType elementType)
    {
        switch(elementType)
        {
            case ElementType.Ice: return chillVfx;
            case ElementType.Fire: return burnVfx;
            case ElementType.Lightning: return shockVfx;
            default: return Color.white;
        } 
    }

    public void PlayOnDamageVfx()
    {
        if (onDamageVfxCoroutine != null)
        {
            StopCoroutine(onDamageVfxCoroutine);
        }

        onDamageVfxCoroutine = StartCoroutine(OnDamageVfxCo());
    }

    private IEnumerator OnDamageVfxCo()
    {
        spriteRenderer.material = onDamageMaterial;

        yield return new WaitForSeconds(onDamageVfxDuration);
        spriteRenderer.material = originalMaterial;
    }
}
