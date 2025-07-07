using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatZoneManager : MonoBehaviour
{
    [Header("Wave Triggers")]
    public Collider2D waveTrigger1;
    public Collider2D waveTrigger2;
    public Collider2D waveTrigger3;

    [Header("Enemy Prefabs Per Wave")]
    public List<GameObject> wave1Enemies;
    public List<GameObject> wave2Enemies;
    public GameObject bossPrefab;

    [Header("Spawn Points")]
    public Transform spawnPoint1;
    public Transform spawnPoint2;
    public Transform spawnPoint3;

    [Header("Blockades")]
    public GameObject blockade1;
    public GameObject blockade2;
    public GameObject blockade3;

    [Header("UI")]
    public GameObject goText;
    public GameObject baseClearedText;
    public GameObject bossHealthUI;

    [Header("Dialogue")]
    public DialogueSequence bossDialogueData;
    public DialogueManager dialogueManager;

    private GameObject player;
    private bool wave1Triggered, wave2Triggered, wave3Triggered;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        goText.SetActive(true);
        baseClearedText.SetActive(false);
        bossHealthUI.SetActive(false);

        // Make sure all blockades are off at start
        blockade1.SetActive(false);
        blockade2.SetActive(false);
        blockade3.SetActive(false);
    }

    void Update()
    {
        if (!wave1Triggered && waveTrigger1.bounds.Intersects(player.GetComponent<Collider2D>().bounds))
            StartCoroutine(HandleWave(wave1Enemies, spawnPoint1, blockade1, 1));

        else if (!wave2Triggered && waveTrigger2.bounds.Intersects(player.GetComponent<Collider2D>().bounds))
            StartCoroutine(HandleWave(wave2Enemies, spawnPoint2, blockade2, 2));

        else if (!wave3Triggered && waveTrigger3.bounds.Intersects(player.GetComponent<Collider2D>().bounds))
            StartCoroutine(StartBossSequence());
    }

    IEnumerator HandleWave(List<GameObject> enemyPrefabs, Transform spawnPoint, GameObject blockade, int waveNumber)
    {
        if (waveNumber == 1) wave1Triggered = true;
        else if (waveNumber == 2) wave2Triggered = true;

        LockCameraHere();
        blockade.SetActive(true);
        goText.SetActive(false);

        List<GameObject> spawnedEnemies = new List<GameObject>();
        foreach (var prefab in enemyPrefabs)
        {
            GameObject enemy = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
            spawnedEnemies.Add(enemy);
        }

        yield return new WaitUntil(() => spawnedEnemies.TrueForAll(e => e == null));

        blockade.SetActive(false);
        UnlockCamera();
        goText.SetActive(true);
    }

    IEnumerator StartBossSequence()
    {
        wave3Triggered = true;
        LockCameraHere();
        blockade3.SetActive(true);
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
        bossHealthUI.SetActive(true);
        GameObject boss = Instantiate(bossPrefab, spawnPoint3.position, Quaternion.identity);

        yield return new WaitUntil(() => boss == null);

        bossHealthUI.SetActive(false);
        baseClearedText.SetActive(true);
        goText.SetActive(false);

        GameManager.Instance.MarkBaseCleared("Bravo"); // Or "Alpha" if using for the other base

        yield return new WaitForSeconds(3f);
        GameManager.Instance.LoadScene("Overworld");
    }

    void LockCameraHere()
    {
        SideCameraFollow.Instance.LockCamera(player.transform.position);
    }

    void UnlockCamera()
    {
        SideCameraFollow.Instance.UnlockCamera();
    }
}
