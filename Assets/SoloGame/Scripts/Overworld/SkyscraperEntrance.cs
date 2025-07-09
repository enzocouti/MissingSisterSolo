using UnityEngine;

public class SkyscraperEntrance : MonoBehaviour
{
    public DialogueSequence enterSkyscraperDialogue; // scriptable dialogue

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            triggered = true;
            DialogueManager.Instance.StartDialogue(enterSkyscraperDialogue);
            // Optional effect?
        }
    }
}