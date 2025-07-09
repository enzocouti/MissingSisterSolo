using UnityEngine;
using UnityEngine.SceneManagement;


// Holds game state such as mission progress and scene transitions
// Persists between scenes as a singleton

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    
    private bool isAlphaBaseCleared = false;
    private bool isBravoBaseCleared = false;
    private bool hasUnlockedSkyscraper = false;
    public DialogueSequence failureDialogue;

    
    [SerializeField] private int skullCount = 0;

    private void Awake()
    {
        // Make sure theres only one GameManager 
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Survive scene changes
    }

    
    // Call this when the player finishes a base ID Alpha or Bravo
    
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
                Debug.LogWarning($"[GameManager] Unknown base ID: {baseId} — typo?");
                break;
        }

        TryUnlockSkyscraper();
    }

    private void AddSkull()
    {
        skullCount++;
        Debug.Log($"[GameManager] Skull counter updated: {skullCount}/2");

        // TODO Hook into UI update method when we build that
    }

    private void TryUnlockSkyscraper()
    {
        if (!hasUnlockedSkyscraper && isAlphaBaseCleared && isBravoBaseCleared)
        {
            hasUnlockedSkyscraper = true;
            Debug.Log("[GameManager] All bases cleared — skyscraper is now unlocked!");
        }
    }

    public int GetSkullCount() => skullCount;
    public bool IsSkyscraperUnlocked() => hasUnlockedSkyscraper;

    public void LoadScene(string sceneName)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("[GameManager] Tried to load a scene with no name");
        }
    }
}
