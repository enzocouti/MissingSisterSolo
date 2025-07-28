using UnityEngine;

[System.Serializable]
public class ComboEntry
{
    public string comboPattern;        // e.g., "J", "JJ", "JK", "JJK"
    public PlayerAttackData attackData;
}
