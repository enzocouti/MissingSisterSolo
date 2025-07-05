using UnityEngine;

// testing one line of dialogue from a character
[System.Serializable]
public class DialogueLine
{
    public string speakerName;          //name
    public Sprite portrait;             //image
    public DialogueSide side;           //Left or Right
    [TextArea(2, 4)] public string lineText; //text
}

public enum DialogueSide
{
    Left,
    Right
}
