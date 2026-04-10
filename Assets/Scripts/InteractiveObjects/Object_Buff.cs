using System.Collections;
using UnityEngine;

[System.Serializable]
public class Buff
{
    public StatsType type;
    public float value;
}

public class Object_Buff : MonoBehaviour
{
    private SpriteRenderer spriteRender;
    private Entity_Stats statsToModify;

    [Header("Buff details")]
    [SerializeField] private Buff[] buffs;
    [SerializeField] private string buffName;
    [SerializeField] private float buffDuration = 20;
    [SerializeField] private bool canBeUsed = true;

    [Header("Floaty movement")]
    [SerializeField] private float floatSpeed = 1.0f;
    [SerializeField] private float floatRange = 0.1f;
    private Vector3 startPosition;

    private void Awake()
    {
        spriteRender = GetComponentInChildren<SpriteRenderer>();

        startPosition = transform.position;
    }

    private void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
        transform.position = startPosition + new Vector3(0, yOffset);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (canBeUsed == false) return;

        statsToModify = collision.GetComponent<Entity_Stats>();
        StartCoroutine(BuffCoroutine(buffDuration));
    }

    private IEnumerator BuffCoroutine(float duration)
    {
        canBeUsed = false;
        spriteRender.color = Color.clear;
        
        ApplyBuff(true);

        yield return new WaitForSeconds(duration);

        ApplyBuff(false);

        Destroy(gameObject);
    }

    private void ApplyBuff(bool enable)
    {
        if (enable == false) return;
        foreach (var buff in buffs)
        {
            statsToModify.GetStatsByType(buff.type).AddModifier(buff.value, buffName);
        }
    }

}
