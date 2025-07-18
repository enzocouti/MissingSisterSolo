using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private bool isAlphaBaseCleared = false;
    private bool isBravoBaseCleared = false;
    private bool hasUnlockedSkyscraper = false;
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
            Debug.Log("[GameManager] All bases cleared � skyscraper unlocked!");
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
