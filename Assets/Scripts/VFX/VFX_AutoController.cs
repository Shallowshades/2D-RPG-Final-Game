using System.Collections;
using UnityEngine;

public class VFX_AutoController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [SerializeField] private bool autoDestory = true;
    [SerializeField] private float destoryDelay = 1;
    [Space]
    [SerializeField] private bool randomOffset = true;
    [SerializeField] private bool randomRotation = true;

    [Header("Fade effect")]
    [SerializeField] private bool canFade;
    [SerializeField] private float fadeSpeed = 1;

    [Header("Random rotation")]
    [SerializeField] private float minRotation = 0;
    [SerializeField] private float maxRotation = 360;

    [Header("Random Position")]
    [SerializeField] private float xMinOffset = -0.3f;
    [SerializeField] private float xMaxOffset = 0.3f;
    [Space]
    [SerializeField] private float yMinOffset = -0.3f;
    [SerializeField] private float yMaxOffset = 0.3f;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (canFade)
        {
            StartCoroutine(FadeCoroutine());
        }

        ApplyRandomOffset();
        ApplyRandomRotation();

        if (autoDestory)
        {
            Destroy(gameObject, destoryDelay);
        }        
    }

    private IEnumerator FadeCoroutine()
    {
        Color targetColor = Color.white;

        while(targetColor.a > 0)
        {
            targetColor.a = targetColor.a - (fadeSpeed * Time.deltaTime);
            spriteRenderer.color = targetColor;
            yield return null;
        }

        spriteRenderer.color = targetColor;
    }

    /// <summary>
    /// 随机偏移
    /// </summary>
    private void ApplyRandomOffset()
    {
        if (randomOffset == false) return;

        float xOffset = Random.Range(xMinOffset, xMaxOffset);
        float yOffset = Random.Range(yMinOffset, yMaxOffset);

        transform.position += new Vector3(xOffset, yOffset);
    }

    /// <summary>
    /// 随机旋转
    /// </summary>
    private void ApplyRandomRotation()
    {
        if (randomRotation == false) return;

        float zRotation = Random.Range(minRotation, maxRotation);
        transform.Rotate(0, 0, zRotation);
    }
}
