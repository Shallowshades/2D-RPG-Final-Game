using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class UI_TreeConnectDetails
{
    public UI_TreeConnectionHandler childNode;
    public NodeDirectionType direction;
    [Range(100f, 350f)] public float length;
    [Range(-50f, 50f)] public float rotation;
}

public class UI_TreeConnectionHandler : MonoBehaviour
{
    private RectTransform rect => GetComponent<RectTransform>();
    [SerializeField] private UI_TreeConnection[] connections;
    [SerializeField] private UI_TreeConnectDetails[] connectionDetails;

    private Image connectionImage;
    private Color originalColor;

    private void Awake()
    {
        if (connectionImage != null)
        {
            originalColor = connectionImage.color;
        }
    }

    public UI_TreeNode[] GetChildNodes()
    {
        List<UI_TreeNode> childrenToReturn = new List<UI_TreeNode>();

        foreach(var node in connectionDetails)
        {
            if (node.childNode != null)
            {
                childrenToReturn.Add(node.childNode.GetComponent<UI_TreeNode>());
            }
        }

        return childrenToReturn.ToArray();
    }

    private void UpdateConnections(bool reorderSiblings = true)
    {
        for (int i = 0; i < connectionDetails.Length; ++i)
        {
            var detail = connectionDetails[i];
            var connection = connections[i];
            
            Vector2 targetPosition = connection.GetConnectionPoint(rect);
            Image connectionImage = connection.GetConnectionImage();

            connection.DirectionConnetion(detail.direction, detail.length, detail.rotation);

            if (detail.childNode == null) continue;

            detail.childNode?.SetPosition(targetPosition);
            detail.childNode?.SetConnectionImage(connectionImage);
            // 改层级顺序会触发 OnTransformChildrenChanged(SendMessage),
            // 而 OnValidate 期间禁止 SendMessage, 故校验时跳过, 交给运行时 Start
            if (reorderSiblings)
            {
                detail.childNode?.transform.SetAsLastSibling();
            }
        }
    }

    public void UpdateAllConnections()
    {
        UpdateConnections();

        foreach(var node in connectionDetails)
        {
            if (node.childNode == null) continue;

            node.childNode?.UpdateConnections();
        } 
    }

    public void UnlockConnectionImage(bool unlocked)
    {
        if (connectionImage == null) return;

        connectionImage.color = unlocked ? Color.white : originalColor;
    }

    public void SetConnectionImage(Image image) => connectionImage = image;

    public void SetPosition(Vector2 position) => rect.anchoredPosition = position;

    private void OnValidate()
    {
        if (connectionDetails == null || connectionDetails.Length <= 0) return;

        if (connectionDetails.Length != connections.Length)
        {
            Debug.Log("Amount of details should be same as amount of connections. - " + gameObject.name);
            return;
        }

        // 只预览位置与连线, 不改层级结构(OnValidate 期间禁止 SendMessage)
        UpdateConnections(reorderSiblings: false);
    }
}
