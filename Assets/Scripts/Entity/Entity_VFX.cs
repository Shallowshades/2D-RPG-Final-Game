using System.Collections;
using UnityEngine;

public class Entity_VFX : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Entity entity;

    [Header("On Damage VFX")]
    [SerializeField] private Material onDamageMaterial;
    [SerializeField] private float onDamageVfxDuration = 0.2f;
    private Material originalMaterial;
    private Coroutine onDamageVfxCoroutine;

    [Header("On Doing Damage VFX")]
    [SerializeField] private Color hitVfxColor = Color.white;
    [SerializeField] private Color critHitVfxColor = Color.white;
    [SerializeField] private GameObject hitVFX;
    [SerializeField] private GameObject critHitVFX;

    private void Awake()
    {
        entity = GetComponent<Entity>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalMaterial = spriteRenderer.material;
    }

    public void CreateOnHitVFX(Transform target, bool isCrit)
    {
        GameObject hitPrefab = isCrit ? critHitVFX : hitVFX;
        GameObject vfx = Instantiate(hitPrefab, target.position, Quaternion.identity);
        
        vfx.GetComponentInChildren<SpriteRenderer>().color = isCrit ? critHitVfxColor : hitVfxColor;

        if (entity.facingDir == -1 && isCrit)
        {
            vfx.transform.Rotate(0, 180, 0);
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
