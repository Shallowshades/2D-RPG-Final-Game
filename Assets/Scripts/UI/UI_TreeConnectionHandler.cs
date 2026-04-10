using System;
using UnityEditor.Search;
using UnityEngine;

[Serializable]
public class UI_TreeConnectDetails
{
    public UI_TreeConnectionHandler childNode;
    public NodeDirectionType direction;
    [Range(100f, 350f)] public float length;
}

public class UI_TreeConnectionHandler : MonoBehaviour
{
    private RectTransform rect;
    [SerializeField] private UI_TreeConnection[] connections;
    [SerializeField] private UI_TreeConnectDetails[] connectiondetails;

    private void OnValidate()
    {
        if (rect == null)
        {
            rect = GetComponent<RectTransform>();
        }

        if (connectiondetails.Length != connections.Length)
        {
            Debug.Log("Amount of details should be same as amount of connections. - " + gameObject.name);
            return;
        }

        UpdateConnections();
    }

    private void UpdateConnections()
    {
        for (int i = 0; i < connectiondetails.Length; ++i)
        {
            var detail = connectiondetails[i];
            var connection = connections[i];
            Vector2 targetPosition = connection.GetConnectionPoint(rect);

            connection.DirectionConnetion(detail.direction, detail.length);
            detail.childNode.SetPosition(targetPosition);        
        }
    }

    public void SetPosition(Vector2 position) => rect.anchoredPosition = position;
}
