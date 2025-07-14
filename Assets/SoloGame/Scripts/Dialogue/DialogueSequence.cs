using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Sequence")]
public class DialogueSequence : ScriptableObject
{
    public DialogueLine[] lines;

    
    public AudioClip bgmClip;
    public bool playBGM = true;

    
    public bool disablePlayerInput = true; // Set this per-dialogue
}
