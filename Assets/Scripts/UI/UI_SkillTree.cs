using UnityEngine;

public class UI_SkillTree : MonoBehaviour
{
    [SerializeField] private int skillPoints;
    [SerializeField] private UI_TreeConnectionHandler[] parentNodes;
    public Player_SkillManager skillManager {  get; private set; }

    private void Awake()
    {
        skillManager = FindAnyObjectByType<Player_SkillManager>();
    }

    private void Start()
    {
        UpdateAllConnections();
    }

    /// <summary>
    /// 补齐"默认解锁"的节点。幂等, 可重复调用。
    /// 由 UI.Start() 在面板被关闭之前调用, 因此不再依赖"面板是否被打开过"
    /// </summary>
    public void UnlockDefaultNodes()
    {
        // 面板若在编辑器里处于未激活状态, 本组件的 Awake 可能还没执行过 -> 这里兜底
        if (skillManager == null)
        {
            skillManager = FindAnyObjectByType<Player_SkillManager>();
        }

        foreach (UI_TreeNode node in GetComponentsInChildren<UI_TreeNode>(true))
        {
            node.UnlockIfDefault();
        }
    }

    [ContextMenu("Reset Skill Tree")]
    public void RefundAllSkills()
    {
        UI_TreeNode[] skillNodes = GetComponentsInChildren<UI_TreeNode>(true);

        foreach(var node in skillNodes)
        {
            node.Refund();
        }
    }

    public bool EnoughSkillPoints(int cost) => skillPoints >= cost;
    public void RemoveSkillPoints(int cost) => skillPoints -= cost;
    public void AddSkillPoints(int points) => skillPoints += points;

    [ContextMenu("Update All Connections")]
    public void UpdateAllConnections()
    {
        foreach (var node in parentNodes)
        {
            node.UpdateAllConnections();
        }
    }
}
