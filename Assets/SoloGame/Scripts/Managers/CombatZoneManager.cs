using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CombatZoneManager : MonoBehaviour
{
    public static CombatZoneManager Instance;

    [Header("Waves")]
    public GameObject[] waveGroups;

    [Header("UI")]
    public TextMeshProUGUI goText;

    [Header("Boss VN")]
    public DialogueSequence bossDialogue;
    public string returnScene = "Overworld";
    public string baseID = "Alpha"; // Alpha or Bravo

    private void Awake() => Instance = this;

    public void TriggerWave(int index)
    {
        if (index >= waveGroups.Length)
        {
            Debug.LogWarning("No wave found for index " + index);
            return;
        }

        GameObject wave = waveGroups[index];
        wave.SetActive(true);
        ShowGoText();

        // Boss logic
        if (index == waveGroups.Length - 1 && bossDialogue != null)
        {
            DialogueManager.Instance.sceneToLoadAfterDialogue = SceneManager.GetActiveScene().name;
            DialogueManager.Instance.StartDialogue(bossDialogue);
        }
    }

    public void ShowGoText()
    {
        goText.gameObject.SetActive(true);
        goText.text = "GO!";
        Invoke(nameof(HideGoText), 1.5f);
    }

    private void HideGoText()
    {
        goText.gameObject.SetActive(false);
    }

    // For use later 
    public void OnWaveCleared()
    {
        Camera.main.GetComponent<SideCameraFollow>().Unlock();
    }

    public void CompleteCombatZone()
    {
        GameManager.Instance.MarkBaseCleared(baseID);
        SceneManager.LoadScene(returnScene);
    }

    private void Update() //for testing purposes
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("Wave clear");
            OnWaveCleared();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("Full base clear");
            CompleteCombatZone();
        }
    }
}
