using UnityEngine;

public class IntroDialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueSequence introSequence; //trigger dialogue on start test

    private void Start()
    {
        DialogueManager.Instance.StartDialogue(introSequence);
    }
}
