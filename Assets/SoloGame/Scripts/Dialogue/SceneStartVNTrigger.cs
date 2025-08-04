using UnityEngine;

public class SceneStartVNTrigger : MonoBehaviour
{
    [SerializeField] private DialogueSequence introSequence; //trigger dialogue on start test

    private void Start()
    {
        DialogueManager.Instance.StartDialogue(introSequence);
    }
}
