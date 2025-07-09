using UnityEngine;
using TMPro;

public class OverworldManager : MonoBehaviour
{
    
    public TextMeshProUGUI skullCounterText;
    public GameObject policeBlockade;
    public Collider2D skyscraperCollider;

    
    public DialogueSequence unlockSkyscraperDialogue; // Scriptable dialogue

    private bool dialoguePlayed = false;

    private void Start()
    {
        RefreshState();
    }

    public void RefreshState()
    {
        int skulls = GameManager.Instance.GetSkullCount();
        skullCounterText.text = $"{skulls}/2";

        if (GameManager.Instance.IsSkyscraperUnlocked())
        {
            if (policeBlockade.activeSelf)
            {
                policeBlockade.SetActive(false);
                skyscraperCollider.enabled = true;
                PlayUnlockDialogue();
            }
        }
        else
        {
            policeBlockade.SetActive(true);
            skyscraperCollider.enabled = false;
        }
    }

    private void PlayUnlockDialogue()
    {
        // only play once
        if (!dialoguePlayed && unlockSkyscraperDialogue != null)
        {
            dialoguePlayed = true;
            DialogueManager.Instance.StartDialogue(unlockSkyscraperDialogue);
        }
    }
}
