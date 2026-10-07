using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_TreeNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI ui;
    private RectTransform rect;
    private UI_SkillTree skillTree;
    private UI_TreeConnectionHandler connectHandler;

    [Header("Unlock details")]
    public UI_TreeNode[] neededNodes;
    public UI_TreeNode[] conflictNodes;
    public bool isUnlocked;     // 是否花费技能点解锁
    public bool isLocked;       // 是否冲突锁定

    [Header("Skill details")]
    public SkillData skillData;
    [SerializeField] private string skillName;
    [SerializeField] private Image skillIcon;
    [SerializeField] private int skillCost;
    [SerializeField] private string lockedColorHex = "#9F9797";
    private Color lastColor;

    private void Awake()
    {
        CacheReferences();
        UpdateIconColor(GetColorByHex(lockedColorHex));
    }

    /// <summary>
    /// 缓存引用。做成幂等, 以便在 Awake 还没执行过时(例如面板一度处于未激活状态)
    /// 也能从外部安全地调用解锁逻辑
    /// </summary>
    private void CacheReferences()
    {
        if (ui == null) ui = GetComponentInParent<UI>();
        if (rect == null) rect = GetComponent<RectTransform>();
        if (skillTree == null) skillTree = GetComponentInParent<UI_SkillTree>();
        if (connectHandler == null) connectHandler = GetComponent<UI_TreeConnectionHandler>();
    }

    private void Start()
    {
        // 面板被激活时也会走到这里, 作为 UnlockDefaultNodes 的兜底(Unlock 是幂等的)
        UnlockIfDefault();
    }

    /// <summary>
    /// 若该节点配置为"默认解锁", 则执行解锁。幂等, 可重复调用
    /// </summary>
    public void UnlockIfDefault()
    {
        if (skillData == null || skillData.unlockedByDefault == false) return;

        CacheReferences();
        Unlock();
    }

    public void Refund()
    {
        if (isUnlocked == false || skillData.unlockedByDefault)
        {
            return;
        }

        isUnlocked = false;
        isLocked = false;
        UpdateIconColor(GetColorByHex(lockedColorHex));

        skillTree.AddSkillPoints(skillData.cost);
        connectHandler?.UnlockConnectionImage(false);

        // skill manager and reset skill

    }

    private void Unlock()
    {
        if (isUnlocked) return;      // 幂等: 默认解锁与手动解锁重复调用时不要重复扣技能点

        isUnlocked = true;
        UpdateIconColor(Color.white);
        LockConflictNodes();

        skillTree.RemoveSkillPoints(skillData.cost);
        connectHandler?.UnlockConnectionImage(true);

        skillTree.skillManager.GetSkillByType(skillData.skillType).SetSkillUpgrade(skillData.upgradeData);
    }

    private bool CanBeUnLocked()
    {
        if (isLocked || isUnlocked) return false;

        if (skillTree.EnoughSkillPoints(skillData.cost) == false) return false; 

        foreach (var node in neededNodes)
        {
            if (node.isUnlocked == false) return false;
        }

        foreach (var node in conflictNodes) 
        { 
            if (node.isUnlocked) return false;
        }

        return true;
    }

    private void LockConflictNodes()
    {
        foreach (var node in conflictNodes)
        {
            node.isLocked = true;
            node.LockChildNodes();
        } 
    }

    public void LockChildNodes()
    {
        CacheReferences();

        isLocked = true;

        if (connectHandler == null) return;      // 没有连接组件的节点就没有子节点

        foreach(var node in connectHandler.GetChildNodes())
        {
            node.LockChildNodes();
        }
    }

    private void UpdateIconColor(Color color)
    {
        if (skillIcon == null) return;

        lastColor = skillIcon.color;
        skillIcon.color = color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (CanBeUnLocked())
        {
            Unlock();
        }
        else if (isLocked)
        {
            ui.skillToolTip.LockedSkillEffect();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ui.skillToolTip.ShowToolTip(true, rect, this);

        if (isUnlocked || isLocked) return;

        ToggleNodeHighlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.skillToolTip.ShowToolTip(false, rect);

        if (isUnlocked || isLocked) return;

        ToggleNodeHighlight(false);

        // TODO: stop coroutine
        if (ui.skillToolTip.textEffectCoroutine != null)
        {
            ui.skillToolTip.StopCoroutine(ui.skillToolTip.textEffectCoroutine);
        }
    }

    private void ToggleNodeHighlight(bool highlight)
    {
        Color highlightColor = Color.white * 0.9f;
        highlightColor.a = 1f;
        Color colorToApply = highlight ? highlightColor : lastColor;

        UpdateIconColor(colorToApply);
    }

    private Color GetColorByHex(string hexNumber)
    {
        ColorUtility.TryParseHtmlString(hexNumber, out Color color);
        return color;
    }

    private void OnDisable()
    {
        if (isLocked)
        {
            UpdateIconColor(GetColorByHex(lockedColorHex));
        }        

        if (isUnlocked)
        {
            UpdateIconColor(Color.white);
        }
    }

    private void OnValidate()
    {
        if (skillData == null) return;

        skillName = skillData.displayName;
        skillIcon.sprite = skillData.icon;
        skillCost = skillData.cost;
        gameObject.name = "UI_TreeNode - " + skillData.displayName;
    }
}
