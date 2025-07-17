using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static bool ShouldPlayFailureDialogue = false; // Set to true to play defeat dialogue on next scene load

    private bool isAlphaBaseCleared = false;
    private bool isBravoBaseCleared = false;
    private bool hasUnlockedSkyscraper = false;
    public DialogueSequence failureDialogue;
    [SerializeField] private int skullCount = 0;

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

    // MAIN universal scene loader for respawn, handles defeat dialogue trigger via static flag.
    public void LoadScene(string sceneName, bool playFailureDialogue = false)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            ShouldPlayFailureDialogue = playFailureDialogue;
            SceneManager.sceneLoaded += OnSceneLoaded; // Register
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("[GameManager] Tried to load a scene with no name");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Use DialogueManager if present and we need to play the defeat dialogue
        if (ShouldPlayFailureDialogue && failureDialogue != null)
        {
            ShouldPlayFailureDialogue = false;
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.PlayDefeatDialogueAfterSceneLoad();
        }
        SceneManager.sceneLoaded -= OnSceneLoaded; // Always unregister!
    }

    // For debug/testing: manually play defeat dialogue
    public void StartFailureSequence()
    {
        if (DialogueManager.Instance != null && failureDialogue != null)
            DialogueManager.Instance.StartDialogue(failureDialogue);
    }
}
