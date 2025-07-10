using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;                      // Name displayed above text
    [TextArea(2, 4)] public string lineText;        // Main dialogue text

    public Sprite leftPortrait;     // Portrait for the left side (set null to hide)
    public Sprite rightPortrait;    // Portrait for the right side 

    public DialogueHighlight highlightSide = DialogueHighlight.Left; // Which side is highlighted 
}

public enum DialogueHighlight
{
    None,
    Left,
    Right
}