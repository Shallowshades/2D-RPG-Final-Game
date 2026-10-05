using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Item Effect/Grant Skill Point", fileName = "Item Effect Data - Grant Skill Point")]
public class ItemEffect_GrantSkillPoint : ItemEffect_DataSO
{
    [SerializeField] private int pointsToAdd;

    public override void ExecuteEffect()
    {
        UI ui = FindFirstObjectByType<UI>();
        ui.skillTree.AddSkillPoints(pointsToAdd);
    }
}
