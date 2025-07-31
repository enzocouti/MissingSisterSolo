using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CombatZoneManager : MonoBehaviour
{
    public static CombatZoneManager Instance;

    [Header("Enemy Spawning")]
    public List<GameObject> wave1Enemies;
    public List<GameObject> wave2Enemies;
    public GameObject bossPrefab;

    public Transform spawnPoint1;
    public Transform spawnPoint2;
    public Transform spawnPoint3;

    [Header("Blockades (Pre-placed, deactivated at start)")]
    public GameObject blockade1;
    public GameObject blockade2;
    public GameObject blockade3;

    [Header("UI")]
    public GameObject goIconObject;  
    public Image goIconImage;        
    public GameObject baseClearedText;
    public GameObject bossHealthUI;
    public GameObject defeatBlackout;

    [Header("GO! Icon Flash Settings")]
    public Color goIconColor1 = Color.black;
    public Color goIconColor2 = Color.red;
    public float goIconFlashSpeed = 0.7f;

    [Header("Boss VN")]
    public DialogueSequence bossDialogueData;
    public DialogueSequence defeatDialogue;
    public DialogueManager dialogueManager;

    [Header("Settings")]
    public string baseID = "Bravo"; // or Alpha
    public string overworldSceneName = "Overworld";

    private GameObject currentBoss;
    private List<GameObject> currentEnemies = new List<GameObject>();

    private Coroutine goFlashCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (goIconObject)
        {
            goIconObject.SetActive(true);
            StartGOIconFlashing();
        }
        if (baseClearedText) baseClearedText.SetActive(false);
        if (bossHealthUI) bossHealthUI.SetActive(false);
        if (defeatBlackout) defeatBlackout.SetActive(false);

        blockade1?.SetActive(false);
        blockade2?.SetActive(false);
        blockade3?.SetActive(false);
    }

    public void TriggerWave(int waveIndex)
    {
        SetGOIconActive(false);

        switch (waveIndex)
        {
            case 0:
                StartCoroutine(HandleWave(wave1Enemies, spawnPoint1, blockade1));
                break;
            case 1:
                StartCoroutine(HandleWave(wave2Enemies, spawnPoint2, blockade2));
                break;
            case 2:
                StartCoroutine(StartBossSequence());
                break;
            default:
                Debug.LogWarning($"No wave handler for index {waveIndex}");
                break;
        }
    }

    IEnumerator HandleWave(List<GameObject> enemyPrefabs, Transform spawnPoint, GameObject blockade)
    {
        currentEnemies.Clear();

        blockade?.SetActive(true);

        foreach (var enemy in enemyPrefabs)
        {
            GameObject e = Instantiate(enemy, spawnPoint.position, Quaternion.identity);
            currentEnemies.Add(e);
        }

        yield return new WaitUntil(() => currentEnemies.TrueForAll(e => e == null));

        blockade?.SetActive(false);
        SideCameraFollow.Instance?.Unlock();
        SetGOIconActive(true);
    }

    IEnumerator StartBossSequence()
    {
        blockade3?.SetActive(true);
        SetGOIconActive(false);

        dialogueManager.onDialogueEnd = () =>
        {
            StartCoroutine(HandleBossFight());
        };

        dialogueManager.StartDialogue(bossDialogueData);
        yield return null;
    }

    IEnumerator HandleBossFight()
    {
        bossHealthUI?.SetActive(true);
        currentBoss = Instantiate(bossPrefab, spawnPoint3.position, Quaternion.identity);

        BossHealth bossHealth = currentBoss.GetComponent<BossHealth>();
        if (bossHealth != null && bossHealthUI != null)
        {
            Scrollbar bossBar = bossHealthUI.GetComponentInChildren<Scrollbar>();
            bossHealth.SetHealthBar(bossBar);
        }

        yield return new WaitUntil(() => currentBoss == null);

        bossHealthUI?.SetActive(false);
        baseClearedText?.SetActive(true);

        GameManager.Instance.MarkBaseCleared(baseID);
        yield return new WaitForSeconds(3f);

        GameManager.Instance.LoadScene(overworldSceneName);
    }

    public void HandlePlayerDefeat()
    {
        StartCoroutine(HandlePlayerDefeatRoutine());
    }

    private IEnumerator HandlePlayerDefeatRoutine()
    {
        if (defeatBlackout != null)
            defeatBlackout.SetActive(true);

        if (dialogueManager != null && defeatDialogue != null)
        {
            dialogueManager.StartDialogue(defeatDialogue);
            dialogueManager.onDialogueEnd = () =>
            {
                GameManager.Instance.LoadScene(overworldSceneName);
            };
        }
        else
        {
            GameManager.Instance.LoadScene(overworldSceneName);
        }

        yield return null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            Debug.Log("DEBUG: Forcing wave clear");
            foreach (var enemy in currentEnemies)
                if (enemy != null) Destroy(enemy);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Debug.Log("DEBUG: Killing boss");
            if (currentBoss != null) Destroy(currentBoss);
        }
    }

    public void NotifyEnemyKilled(GameObject enemy)
    {
        if (currentEnemies.Contains(enemy))
        {
            currentEnemies.Remove(enemy);
        }
    }

   

    private void SetGOIconActive(bool isActive)
    {
        if (goIconObject)
        {
            goIconObject.SetActive(isActive);
            if (isActive)
            {
                StartGOIconFlashing();
            }
            else
            {
                StopGOIconFlashing();
            }
        }
    }

    private void StartGOIconFlashing()
    {
        StopGOIconFlashing();
        goFlashCoroutine = StartCoroutine(FlashGOIcon());
    }

    private void StopGOIconFlashing()
    {
        if (goFlashCoroutine != null)
        {
            StopCoroutine(goFlashCoroutine);
            goFlashCoroutine = null;
        }
        SetGOIconColor(goIconColor1);
    }

    private IEnumerator FlashGOIcon()
    {
        if (goIconImage == null)
        {
            goIconImage = goIconObject.GetComponent<Image>();
            if (goIconImage == null) yield break;
        }

        float t = 0f;
        while (goIconObject.activeSelf)
        {
            t += Time.unscaledDeltaTime * (1f / goIconFlashSpeed);
            float lerp = Mathf.PingPong(t, 1f);
            goIconImage.color = Color.Lerp(goIconColor1, goIconColor2, lerp);
            yield return null;
        }
        goIconImage.color = goIconColor1;
    }

    private void SetGOIconColor(Color color)
    {
        if (goIconImage == null)
            goIconImage = goIconObject.GetComponent<Image>();
        if (goIconImage != null)
            goIconImage.color = color;
    }
}
