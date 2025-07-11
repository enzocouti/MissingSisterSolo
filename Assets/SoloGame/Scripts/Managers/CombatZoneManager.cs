using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


// Manages the wave system UI boss VN and base completion

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
    public GameObject goText;
    public GameObject baseClearedText;
    public GameObject bossHealthUI;

    [Header("Boss VN")]
    public DialogueSequence bossDialogueData;
    public DialogueManager dialogueManager;

    [Header("Settings")]
    public string baseID = "Bravo"; // or Alpha
    public string overworldSceneName = "Overworld";

    private GameObject currentBoss;
    private List<GameObject> currentEnemies = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (goText) goText.SetActive(true);
        if (baseClearedText) baseClearedText.SetActive(false);
        if (bossHealthUI) bossHealthUI.SetActive(false);

        blockade1?.SetActive(false);
        blockade2?.SetActive(false);
        blockade3?.SetActive(false);
    }

    public void TriggerWave(int waveIndex)
    {
        goText.SetActive(false);

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
        goText.SetActive(true);
    }

    IEnumerator StartBossSequence()
    {
        blockade3?.SetActive(true);
        goText.SetActive(false);

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

        // Assign health bar to boss after spawning
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


    // Optional Debug keys
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

    public void NotifyEnemyKilled(GameObject enemy) //notification communication
    {
        if (currentEnemies.Contains(enemy))
        {
            currentEnemies.Remove(enemy);
        }
    }
}
