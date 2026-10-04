using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item Effect/Refund All Skills", fileName = "Item Effect Data - Refund All Skills")]
public class ItemEffect_RefundSkillTree : ItemEffect_DataSO
{
    public override void ExecuteEffect()
    {
        UI ui = FindFirstObjectByType<UI>();
        ui.skillTree.RefundAllSkills();
    }
}
