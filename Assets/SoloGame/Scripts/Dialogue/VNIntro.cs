using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


// Test visual novel advances with E then loads overworld

public class VNIntro : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    private string[] lines = new string[]
    {
        "??? : You're finally awake.",
        "Protagonist : Ugh... don’t start with me...",
        "??? : Got a lead. Your sister might be at the skyscraper.",
        "Protagonist : That place? It’s crawling with enforcers.",
        "??? : Wreck some bases. Cause a distraction.",
        "Protagonist : Time to wake the city up."
    };

    private int currentIndex = -1;

    private void Start() => Advance();

    private void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
            Advance();
    }

    private void Advance()
    {
        currentIndex++;

        if (currentIndex >= lines.Length)
        {
            GameManager.Instance.LoadScene("Overworld");
            return;
        }

        var parts = lines[currentIndex].Split(':');
        nameText.text = parts[0].Trim();
        dialogueText.text = parts[1].Trim();
    }
}
