using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// Controls menu navigation via arrow keys or gamepad and Handles selector arrow movement and scene switching.

public class MainMenuController : MonoBehaviour
{
    [Header("Menu Elements")]
    public RectTransform[] menuOptions; // the text transforms
    public Image selectorArrow;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip moveSFX;
    public AudioClip confirmSFX;

    private int currentIndex = 0;
    private bool inputLocked = false;

    private void Start()
    {
        UpdateArrowPosition();
    }

    private void Update()
    {
        if (inputLocked) return;

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            ChangeSelection(1);
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            ChangeSelection(-1);

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)
            ConfirmSelection();
    }

    void ChangeSelection(int direction)
    {
        currentIndex = Mathf.Clamp(currentIndex + direction, 0, menuOptions.Length - 1);
        UpdateArrowPosition();
        PlaySound(moveSFX);
    }

    void UpdateArrowPosition()
    {
        selectorArrow.rectTransform.position = new Vector3(
     selectorArrow.rectTransform.position.x,
     menuOptions[currentIndex].position.y,
     selectorArrow.rectTransform.position.z);
    }

    void ConfirmSelection()
    {
        if (inputLocked) return;
        inputLocked = true;

        PlaySound(confirmSFX);

        switch (currentIndex)
        {
            case 0:
                StartCoroutine(LoadSceneAfterDelay("IntroScene", 0.5f)); //fix sound not playing before scene change
                break;
            case 1:
                StartCoroutine(QuitAfterDelay(0.5f));
                break;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (sfxSource && clip)
            sfxSource.PlayOneShot(clip);
    }

    //for testing
    private IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.LoadScene(sceneName);
    }

    private IEnumerator QuitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Application.Quit();
        Debug.Log("[Menu] Quit called.");
    }
}
