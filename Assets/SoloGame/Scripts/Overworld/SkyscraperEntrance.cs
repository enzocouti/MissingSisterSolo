using UnityEngine;
using UnityEngine.SceneManagement;

public class SkyscraperEntrance : MonoBehaviour
{
    [Header("Dialogue to play when entering")]
    [SerializeField] private DialogueSequence enterSkyscraperDialogue;

    [Header("Scene to load after dialogue ends")]
    [SerializeField] private string sceneToLoad;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            triggered = true;

            // Hook into dialogue end
            var dm = DialogueManager.Instance;
            dm.onDialogueEnd = () =>
            {
                // Clear callback to avoid accidental repeats
                dm.onDialogueEnd = null;
                // Load your target scene
                if (!string.IsNullOrEmpty(sceneToLoad))
                    SceneManager.LoadScene(sceneToLoad);
                else
                    Debug.LogError("[SkyscraperEntrance] sceneToLoad is empty!");
            };

            // Start the dialogue
            dm.StartDialogue(enterSkyscraperDialogue);
        }
    }
}
