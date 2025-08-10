using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class NPCDialogueTrigger : MonoBehaviour
{
    [Header("Unique NPC Identifier")]
    [Tooltip("Must be unique per NPC to persist dialogue progress")]
    [SerializeField] private string npcID;

    [Header("Dialogues to play in order (DEFAULT)")]
    [SerializeField] private List<DialogueSequence> dialogueSequences;

    
    [Header("Conditional Dialogues (Optional)")]
    [Tooltip("Use if you want to show special dialogue when Alpha or Bravo base is cleared")]
    [SerializeField] private List<DialogueSequence> afterAlphaCleared;
    [SerializeField] private List<DialogueSequence> afterBravoCleared;
    [Tooltip("Use if you want to show special dialogue when BOTH bases are cleared")]
    [SerializeField] private List<DialogueSequence> afterBothBasesCleared;

    [Tooltip("Child GameObject with '[E]' prompt")]
    [SerializeField] private GameObject interactPrompt;

    // Trackers for exhausted dialogue
    private int currentIndex;
    private int conditionalIndex; // For alternate dialogue exhaustion
    private bool playerInRange = false;
    private bool dialogueActive = false;
    private string activeBranch = ""; // Tracks which branch is in use "", "Alpha", "Bravo", "Both"

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Awake()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(npcID))
            Debug.LogWarning($"[NPCDialogueTrigger] {name} has no NPC ID set.");

        // Decide which branch to use at load
        ChooseDialogueBranch();
    }

    private void Update()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(playerInRange && !dialogueActive);

        if (playerInRange && !dialogueActive &&
            Keyboard.current.eKey.wasPressedThisFrame &&
            GetCurrentSequences().Count > 0)
        {
            PlayCurrentDialogue();
        }
    }

    private void ChooseDialogueBranch()
    {
        // Check for both bases first highest priority
        if (GameManager.Instance.IsAlphaBaseCleared() && GameManager.Instance.IsBravoBaseCleared() && afterBothBasesCleared != null && afterBothBasesCleared.Count > 0)
        {
            activeBranch = "Both";
            conditionalIndex = GameManager.Instance.GetNPCDialogueIndex(npcID + "_Both");
            conditionalIndex = Mathf.Clamp(conditionalIndex, 0, afterBothBasesCleared.Count - 1);
            return;
        }
        // Alpha only
        if (GameManager.Instance.IsAlphaBaseCleared() && afterAlphaCleared != null && afterAlphaCleared.Count > 0)
        {
            activeBranch = "Alpha";
            conditionalIndex = GameManager.Instance.GetNPCDialogueIndex(npcID + "_Alpha");
            conditionalIndex = Mathf.Clamp(conditionalIndex, 0, afterAlphaCleared.Count - 1);
            return;
        }
        // Bravo only
        if (GameManager.Instance.IsBravoBaseCleared() && afterBravoCleared != null && afterBravoCleared.Count > 0)
        {
            activeBranch = "Bravo";
            conditionalIndex = GameManager.Instance.GetNPCDialogueIndex(npcID + "_Bravo");
            conditionalIndex = Mathf.Clamp(conditionalIndex, 0, afterBravoCleared.Count - 1);
            return;
        }
        // Default
        activeBranch = "";
        currentIndex = GameManager.Instance.GetNPCDialogueIndex(npcID);
        currentIndex = Mathf.Clamp(currentIndex, 0, dialogueSequences.Count > 0 ? dialogueSequences.Count - 1 : 0);
    }

    private List<DialogueSequence> GetCurrentSequences()
    {
        switch (activeBranch)
        {
            case "Both":
                return afterBothBasesCleared;
            case "Alpha":
                return afterAlphaCleared;
            case "Bravo":
                return afterBravoCleared;
            default:
                return dialogueSequences;
        }
    }

    private int GetCurrentIndex()
    {
        return (activeBranch == "") ? currentIndex : conditionalIndex;
    }
    private void SetCurrentIndex(int idx)
    {
        if (activeBranch == "") currentIndex = idx;
        else conditionalIndex = idx;
    }

    private string GetSaveKey()
    {
        if (activeBranch == "") return npcID;
        return npcID + "_" + activeBranch;
    }

    private void PlayCurrentDialogue()
    {
        // Always check branch 
        ChooseDialogueBranch();

        var sequences = GetCurrentSequences();
        int idx = GetCurrentIndex();

        var seq = sequences[idx];
        var dm = DialogueManager.Instance;

        dialogueActive = true;
        interactPrompt?.SetActive(false);

        Action prev = dm.onDialogueEnd;
        dm.onDialogueEnd = () =>
        {
            prev?.Invoke();
            OnDialogueEnded();
        };

        dm.StartDialogue(seq);
    }

    private void OnDialogueEnded()
    {
        dialogueActive = false;

        // Update progress exhaustion
        int idx = GetCurrentIndex();
        var seqs = GetCurrentSequences();
        if (idx < seqs.Count - 1)
            idx++;

        SetCurrentIndex(idx);

        // Save progress using active branch as part of key
        GameManager.Instance.SetNPCDialogueIndex(GetSaveKey(), idx);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            dialogueActive = false;
        }
    }
}
