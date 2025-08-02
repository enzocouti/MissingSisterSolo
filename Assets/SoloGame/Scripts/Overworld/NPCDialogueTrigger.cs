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

    [Header("Dialogues to play in order")]
    [SerializeField] private List<DialogueSequence> dialogueSequences;

    [Tooltip("Child GameObject with '[E]' prompt")]
    [SerializeField] private GameObject interactPrompt;

    private int currentIndex;
    private bool playerInRange = false;
    private bool dialogueActive = false;

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

        
        currentIndex = GameManager.Instance.GetNPCDialogueIndex(npcID);
        currentIndex = Mathf.Clamp(currentIndex, 0, dialogueSequences.Count > 0 ? dialogueSequences.Count - 1 : 0);
    }

    private void Update()
    {
        
        if (interactPrompt != null)
            interactPrompt.SetActive(playerInRange && !dialogueActive);

        if (playerInRange && !dialogueActive &&
            Keyboard.current.eKey.wasPressedThisFrame &&
            dialogueSequences != null && dialogueSequences.Count > 0)
        {
            PlayCurrentDialogue();
        }
    }

    private void PlayCurrentDialogue()
    {
        var seq = dialogueSequences[currentIndex];
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

        
        if (currentIndex < dialogueSequences.Count - 1)
            currentIndex++;

        
        GameManager.Instance.SetNPCDialogueIndex(npcID, currentIndex);
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
