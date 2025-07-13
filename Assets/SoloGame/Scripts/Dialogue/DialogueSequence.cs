using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Sequence")]
public class DialogueSequence : ScriptableObject
{
    public DialogueLine[] lines;

    [Header("BGM Options")]
    public AudioClip bgmClip;        //Set to use custom BGM for this dialogue
    public bool playBGM = true;      // If false, keeps current BGM 
}
