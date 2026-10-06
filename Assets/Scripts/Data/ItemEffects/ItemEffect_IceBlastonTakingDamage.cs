using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item Effect/Ice Blast", fileName = "Item Effect Data - Ice Blast On Taking Damage")]
public class ItemEffect_IceBlastonTakingDamage : ItemEffect_DataSO
{
    public override void ExecuteEffect()
    {
        base.ExecuteEffect();

        // ice blast when health below %

        Debug.Log("ICE BLAST!");
    }

    public override void Subscribe(Player player)
    {
        base.Subscribe(player);
        player.health.OnTakingDamage += ExecuteEffect;
    }

    public override void Unsubscribe()
    {
        base.Unsubscribe();
        player.health.OnTakingDamage -= ExecuteEffect;
        player = null;
    }
}
