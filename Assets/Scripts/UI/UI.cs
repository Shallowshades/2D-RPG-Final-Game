using System;
using UnityEngine;

public class UI : MonoBehaviour
{
    public UI_SkillToolTip skillToolTip;
    public UI_ItemToolTip itemToolTip;
    public UI_PlayerStatsToolTip playerStatsToolTip;

    public UI_SkillTree skillTree;
    private bool skillTreeEnabled;

    public UI_Inventory inventory;
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
