using UnityEngine;

// full conversation  of dialogue lines
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Sequence")]
public class DialogueSequence : ScriptableObject
{
    public DialogueLine[] lines;
}
