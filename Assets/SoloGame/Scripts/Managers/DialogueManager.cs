using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;
    [SerializeField] private Image blackFade;

    [Header("Settings")]
    [SerializeField] private float typeSpeed = 0.03f;
    [SerializeField] private KeyCode advanceKey = KeyCode.E;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color fadedColor = new Color(1, 1, 1, 0.4f);

    [SerializeField] public string sceneToLoadAfterDialogue;
    public System.Action onDialogueEnd; // optional callback


    private DialogueSequence currentSequence;
    private int currentIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;


    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    private void Update()
    {
        if (dialogueActive && Input.GetKeyDown(advanceKey) && !isTyping)
        {
            NextLine();
        }
    }

    public void StartDialogue(DialogueSequence sequence)
    {
        currentSequence = sequence;
        currentIndex = 0;
        dialogueActive = true;

        DisablePlayerInput(); // Block input during dialogue
        StartCoroutine(FadeInAndBegin());
    }

    private IEnumerator FadeInAndBegin()
    {
        blackFade.gameObject.SetActive(true);
        blackFade.color = Color.black;

        yield return new WaitForSeconds(0.4f);

        dialogueUI.SetActive(true);
        ShowLine();

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2;
            blackFade.color = Color.Lerp(Color.black, Color.clear, t);
            yield return null;
        }

        blackFade.gameObject.SetActive(false);
    }

    private void ShowLine()
    {
        DialogueLine line = currentSequence.lines[currentIndex];

        nameText.text = line.speakerName;
        dialogueText.text = "";

        if (line.side == DialogueSide.Left)
        {
            leftPortrait.sprite = line.portrait;
            leftPortrait.color = normalColor;
            rightPortrait.color = fadedColor;
        }
        else
        {
            rightPortrait.sprite = line.portrait;
            rightPortrait.color = normalColor;
            leftPortrait.color = fadedColor;
        }

        StartCoroutine(TypeLine(line.lineText));
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
    }

    private void NextLine()
    {
        currentIndex++;
        if (currentIndex >= currentSequence.lines.Length)
        {
            StartCoroutine(FadeOutAndEnd());
        }
        else
        {
            ShowLine();
        }
    }


    private IEnumerator FadeOutAndEnd()
    {
        blackFade.gameObject.SetActive(true);
        blackFade.color = Color.clear;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2;
            blackFade.color = Color.Lerp(Color.clear, Color.black, t);
            yield return null;
        }

        dialogueUI.SetActive(false);
        dialogueActive = false;
        currentSequence = null;

        EnablePlayerInput(); // Renable input here

        
        if (onDialogueEnd != null)
        {
            onDialogueEnd.Invoke();
            onDialogueEnd = null;
        }
        else if (!string.IsNullOrEmpty(sceneToLoadAfterDialogue))
        {
            SceneManager.LoadScene(sceneToLoadAfterDialogue);
            sceneToLoadAfterDialogue = null; // Clear to avoid leftover
        }
        else
        {
            blackFade.gameObject.SetActive(false);
        }
    }

    // Disable player input during VN
    private void DisablePlayerInput()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerCombatInput input = player.GetComponent<PlayerCombatInput>();
            input?.SetInputEnabled(false);
        }
    }

    private void EnablePlayerInput()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerCombatInput input = player.GetComponent<PlayerCombatInput>();
            input?.SetInputEnabled(true);
        }
    }
}
