using UnityEngine;

/**
 * @brief 可拾取交互对象
 */
public class Object_ItemPickup : MonoBehaviour
{
    private SpriteRenderer sr;

    [SerializeField] public ItemData itemData;

    private void OnValidate()
    {
        if (itemData == null) return;

        sr = GetComponent<SpriteRenderer>();
        sr.sprite = itemData.itemIcon;
        gameObject.name = "Object_ItemPickup - " + itemData.itemName;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Player picked up item - " + itemData.itemName);
        Destroy(gameObject);
    }
}
