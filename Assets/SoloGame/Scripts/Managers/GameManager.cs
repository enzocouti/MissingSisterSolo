using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private bool isAlphaBaseCleared = false;
    private bool isBravoBaseCleared = false;
    private bool hasUnlockedSkyscraper = false;
    public DialogueSequence failureDialogue;
    [SerializeField] private int skullCount = 0;

    private bool shouldPlayFailureDialogue = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Call when a base is cleared
    public void MarkBaseCleared(string baseId)
    {
        switch (baseId)
        {
            case "Alpha":
                if (!isAlphaBaseCleared)
                {
                    isAlphaBaseCleared = true;
                    AddSkull();
                }
                break;
            case "Bravo":
                if (!isBravoBaseCleared)
                {
                    isBravoBaseCleared = true;
                    AddSkull();
                }
                break;
            default:
                Debug.LogWarning($"[GameManager] Unknown base ID: {baseId}");
                break;
        }
        TryUnlockSkyscraper();
    }

    private void AddSkull()
    {
        skullCount++;
        Debug.Log($"[GameManager] Skull counter updated: {skullCount}/2");
    }

    private void TryUnlockSkyscraper()
    {
        if (!hasUnlockedSkyscraper && isAlphaBaseCleared && isBravoBaseCleared)
        {
            hasUnlockedSkyscraper = true;
            Debug.Log("[GameManager] All bases cleared — skyscraper unlocked!");
        }
    }

    public int GetSkullCount() => skullCount;
    public bool IsSkyscraperUnlocked() => hasUnlockedSkyscraper;

    // Scene loader. If playFailureDialogue is true, triggers defeat dialogue after Overworld loads.
    public void LoadScene(string sceneName, bool playFailureDialogue = false)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            shouldPlayFailureDialogue = playFailureDialogue;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("[GameManager] Tried to load a scene with no name");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (shouldPlayFailureDialogue)
        {
            shouldPlayFailureDialogue = false;
            if (DialogueManager.Instance != null && failureDialogue != null)
                DialogueManager.Instance.StartDialogue(failureDialogue);
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // For debug/testing: manually play defeat dialogue
    public void StartFailureSequence()
    {
        if (DialogueManager.Instance != null && failureDialogue != null)
            DialogueManager.Instance.StartDialogue(failureDialogue);
    }
}
