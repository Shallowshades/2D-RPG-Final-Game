using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item Effect/Thunder Strike On Hit", fileName = "Item Effect Data - Thunder Strike On Hit")]
public class ItemEffect_ThunderStrikeOnHit : ItemEffect_DataSO
{
    [Header("Trigger")]
    [Range(0f, 1f)]
    [SerializeField] private float triggerChance = 0.15f;      // 15% 概率触发雷击

    [Header("Thunder strike details")]
    [SerializeField] private float thunderDamage = 200f;
    [SerializeField] private ElementalEffectData effectData;   // 命中后叠加的感电充能(shockCharge = 0 时不生效)

    [Header("Vfx objects")]
    [SerializeField] private GameObject thunderStrikeVfx;

    public override void ExecuteEffect()
    {
        
    }

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        player.combat.OnDoingPhysicalDamage -= ThunderStrikeOnHit;
        player.combat.OnDoingPhysicalDamage += ThunderStrikeOnHit;
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        player.combat.OnDoingPhysicalDamage -= ThunderStrikeOnHit;
        player = null;
    }

    private void ThunderStrikeOnHit(float damage, Transform target)
    {
        if (target == null) return;

        // 概率判定: Random.value 落在 [0,1], 大于 triggerChance 则本次不触发
        if (Random.value > triggerChance) return;

        // 在目标位置生成雷击特效
        if (thunderStrikeVfx != null)
        {
            player.playerVfx.CreateEffectOf(thunderStrikeVfx, target);
        }

        IDamagable damagable = target.GetComponent<IDamagable>();
        if (damagable == null) return;

        // 雷击只结算元素伤害, 不重复结算物理伤害(与 Ice Blast 的做法一致)
        bool targetGotHit = damagable.TakeDamage(0, thunderDamage, ElementType.Lightning, player.transform);

        if (targetGotHit && effectData != null)
        {
            // 顺带叠加感电充能, 充能满时由目标自身触发落雷
            Entity_StatusHandler statusHandler = target.GetComponent<Entity_StatusHandler>();
            statusHandler?.ApplyStatusEffect(ElementType.Lightning, effectData);
        }
    }
}
