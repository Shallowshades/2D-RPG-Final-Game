using System;
using UnityEngine;

public class UI : MonoBehaviour
{
    public UI_SkillToolTip skillToolTip { get; private set; }
    public UI_ItemToolTip itemToolTip { get; private set; }
    public UI_PlayerStatsToolTip playerStatsToolTip { get; private set; }

    public UI_SkillTree skillTree { get; private set; }
    private bool skillTreeEnabled;

    public UI_Inventory inventory { get; private set; }
    private bool inventoryEnabled;

    private void Awake()
    {
        itemToolTip = GetComponentInChildren<UI_ItemToolTip>();
        skillToolTip = GetComponentInChildren<UI_SkillToolTip>();
        playerStatsToolTip = GetComponentInChildren<UI_PlayerStatsToolTip>();

        skillTree = GetComponentInChildren<UI_SkillTree>(true);
        inventory = GetComponentInChildren<UI_Inventory>(true);
    }

    public void ToggleSkillTreeUI()
    {
        skillTreeEnabled = !skillTreeEnabled;
        skillTree.gameObject.SetActive(skillTreeEnabled);
        skillToolTip.ShowToolTip(false, null);
        itemToolTip.ShowToolTip(false, null);
        playerStatsToolTip.ShowToolTip(false, null);

        skillTreeEnabled = skillTree.gameObject.activeSelf;
        inventoryEnabled = inventory.gameObject.activeSelf;
    }

    internal void ToggleInventoryUI()
    {
        inventoryEnabled = !inventoryEnabled;
        inventory.gameObject.SetActive(inventoryEnabled);
        skillToolTip.ShowToolTip(false, null);
        itemToolTip.ShowToolTip(false, null);
        playerStatsToolTip.ShowToolTip(false, null);
    }
}
