using UnityEngine;

public class ItemEffect_DataSO : ScriptableObject
{
    [TextArea]
    public string effectDescription;

    virtual public void ExecuteEffect()
    {

    }
}
