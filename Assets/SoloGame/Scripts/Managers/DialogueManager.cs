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

    [Header("Extra UI to Hide")]
    [SerializeField] private GameObject[] uiToHideDuringDialogue;

    [Header("Audio")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip defaultBGM;
    [SerializeField] private AudioClip typewriterSFX;

    [Header("Settings")]
    [SerializeField] private float typeSpeed = 0.03f;
    [SerializeField] private KeyCode advanceKey = KeyCode.Return;
    [SerializeField] private KeyCode backKey = KeyCode.Backspace;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color fadedColor = new Color(1, 1, 1, 0.4f);
    [SerializeField] private float bgmFadeTime = 1.2f;
    [SerializeField] private float fadeDuration = 0.8f;

    [SerializeField] public string sceneToLoadAfterDialogue;
    public System.Action onDialogueEnd;

    private DialogueSequence currentSequence;
    private int currentIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;

    private AudioClip previousBGM;
    private Coroutine bgmFadeCoroutine;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    // For defeat blackout
    public void ShowBlackFadeImmediately()
    {
        if (blackFade != null)
        {
            blackFade.gameObject.SetActive(true);
            blackFade.color = Color.black;
        }
    }

    private void Update()
    {
        if (dialogueActive)
        {
            if (Input.GetKeyDown(advanceKey) && !isTyping)
            {
                NextLine();
            }
            else if (Input.GetKeyDown(backKey) && !isTyping)
            {
                PreviousLine();
            }
        }
    }

    public void StartDialogue(DialogueSequence sequence)
    {
        currentSequence = sequence;
        currentIndex = 0;
        dialogueActive = true;

        // Handle BGM fade & switch
        if (sequence.playBGM && sequence.bgmClip != null && bgmSource != null)
        {
            previousBGM = bgmSource.clip;
            StartCoroutine(FadeOutAndChangeBGM(sequence.bgmClip));
        }
        else
        {
            previousBGM = bgmSource != null ? bgmSource.clip : null;
        }

        // Hide extra UI if needed
        HideExtraUI();

        // Lock player input if requested 
        if (sequence.disablePlayerInput)
            DisablePlayerInput();

        StartCoroutine(FadeInAndBegin());
    }

    private IEnumerator FadeInAndBegin()
    {
        blackFade.gameObject.SetActive(true);
        blackFade.color = Color.black;

        yield return new WaitForSeconds(fadeDuration * 0.3f); // short pause before fade

        dialogueUI.SetActive(true);
        ShowLine();

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
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

        if (line.leftPortrait != null)
        {
            leftPortrait.gameObject.SetActive(true);
            leftPortrait.sprite = line.leftPortrait;
            leftPortrait.color = (line.highlightSide == DialogueHighlight.Left) ? normalColor : fadedColor;
        }
        else
        {
            leftPortrait.gameObject.SetActive(false);
        }

        if (line.rightPortrait != null)
        {
            rightPortrait.gameObject.SetActive(true);
            rightPortrait.sprite = line.rightPortrait;
            rightPortrait.color = (line.highlightSide == DialogueHighlight.Right) ? normalColor : fadedColor;
        }
        else
        {
            rightPortrait.gameObject.SetActive(false);
        }

        StartCoroutine(TypeLine(line.lineText));
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;
        int letterCount = 0;
        foreach (char c in text)
        {
            dialogueText.text += c;
            if (!char.IsWhiteSpace(c) && typewriterSFX != null && sfxSource != null)
            {
                if (letterCount % 2 == 0)
                    sfxSource.PlayOneShot(typewriterSFX, 0.7f);
            }
            letterCount++;
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
            t += Time.deltaTime / fadeDuration;
            blackFade.color = Color.Lerp(Color.clear, Color.black, t);
            yield return null;
        }

        dialogueUI.SetActive(false);
        dialogueActive = false;

        // Show UI back if it was hidden
        ShowExtraUI();

        // Restore input if needed
        if (currentSequence != null && currentSequence.disablePlayerInput)
            EnablePlayerInput();

        // Restore previous/default BGM if needed
        if (currentSequence != null && currentSequence.playBGM && currentSequence.bgmClip != null && previousBGM != null)
        {
            yield return StartCoroutine(FadeOutAndChangeBGM(previousBGM));
        }
        else if (currentSequence != null && currentSequence.playBGM && currentSequence.bgmClip != null && defaultBGM != null)
        {
            yield return StartCoroutine(FadeOutAndChangeBGM(defaultBGM));
        }

        currentSequence = null;

        if (onDialogueEnd != null)
        {
            onDialogueEnd.Invoke();
            onDialogueEnd = null;
        }
        else if (!string.IsNullOrEmpty(sceneToLoadAfterDialogue))
        {
            SceneManager.LoadScene(sceneToLoadAfterDialogue);
            sceneToLoadAfterDialogue = null;
        }
        else
        {
            blackFade.gameObject.SetActive(false);
        }
    }

    private IEnumerator FadeOutAndChangeBGM(AudioClip newClip)
    {
        if (bgmSource == null) yield break;

        float startVol = bgmSource.volume;
        float t = 0f;

        while (t < bgmFadeTime)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / bgmFadeTime);
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.clip = newClip;
        bgmSource.Play();

        t = 0f;
        while (t < bgmFadeTime)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, startVol, t / bgmFadeTime);
            yield return null;
        }
        bgmSource.volume = startVol;
    }

    private void HideExtraUI()
    {
        if (uiToHideDuringDialogue == null) return;
        foreach (var go in uiToHideDuringDialogue)
            if (go != null) go.SetActive(false);
    }
    private void ShowExtraUI()
    {
        if (uiToHideDuringDialogue == null) return;
        foreach (var go in uiToHideDuringDialogue)
            if (go != null) go.SetActive(true);
    }

    private void DisablePlayerInput()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var combatInput = player.GetComponent<PlayerCombatInput>();
            combatInput?.SetInputEnabled(false);

            var overworldInput = player.GetComponent<OverworldPlayerController>();
            overworldInput?.SetInputEnabled(false);
        }
    }

    private void EnablePlayerInput()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var combatInput = player.GetComponent<PlayerCombatInput>();
            combatInput?.SetInputEnabled(true);

            var overworldInput = player.GetComponent<OverworldPlayerController>();
            overworldInput?.SetInputEnabled(true);
        }
    }

    private void PreviousLine()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            ShowLine();
        }
    }
    
}
