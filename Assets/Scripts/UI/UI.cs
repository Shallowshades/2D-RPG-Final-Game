using UnityEngine;

public class UI : MonoBehaviour
{
    [Header("开局显示哪些面板(改这里, 不用再手动改物体的激活状态)")]
    [SerializeField] private bool startWithSkillTreeOpen;
    [SerializeField] private bool startWithCharacterOpen;

    public UI_SkillToolTip skillToolTip { get; private set; }
    public UI_ItemToolTip itemToolTip { get; private set; }
    public UI_PlayerStatsToolTip playerStatsToolTip { get; private set; }

    public UI_SkillTree skillTree { get; private set; }
    public UI_Inventory inventory { get; private set; }   // 它挂在 UI_Character 上

    private void Awake()
    {
        // 参数 true = 连"未激活的子物体"一起找, 所以面板在编辑器里是开是关都能找到
        itemToolTip = GetComponentInChildren<UI_ItemToolTip>(true);
        skillToolTip = GetComponentInChildren<UI_SkillToolTip>(true);
        playerStatsToolTip = GetComponentInChildren<UI_PlayerStatsToolTip>(true);

        skillTree = GetComponentInChildren<UI_SkillTree>(true);
        inventory = GetComponentInChildren<UI_Inventory>(true);
    }

    private void Start()
    {
        // 先补齐"默认解锁"的技能(必须在关闭面板之前执行, 此时所有 Awake 都已跑完)
        skillTree.UnlockDefaultNodes();

        // 初始可见性由代码统一决定(必须放在 Start: 此时所有 Awake 都已执行完)
        skillTree.gameObject.SetActive(startWithSkillTreeOpen);
        inventory.gameObject.SetActive(startWithCharacterOpen);

        HideAllToolTips();
    }

    public void ToggleSkillTreeUI()
    {
        bool willOpen = skillTree.gameObject.activeSelf == false;

        // 互斥: 两个面板在同一位置, 开一个就关另一个
        if (willOpen) inventory.gameObject.SetActive(false);

        skillTree.gameObject.SetActive(willOpen);

        // 打开时刷一次连线与节点位置, 保证首次打开布局正确
        if (willOpen) skillTree.UpdateAllConnections();

        HideAllToolTips();
    }

    public void ToggleInventoryUI()
    {
        bool willOpen = inventory.gameObject.activeSelf == false;

        if (willOpen) skillTree.gameObject.SetActive(false);

        inventory.gameObject.SetActive(willOpen);

        HideAllToolTips();
    }

    private void HideAllToolTips()
    {
        skillToolTip.ShowToolTip(false, null);
        itemToolTip.ShowToolTip(false, null);
        playerStatsToolTip.ShowToolTip(false, null);
    }
}
