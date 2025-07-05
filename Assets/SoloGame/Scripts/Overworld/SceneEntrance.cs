using UnityEngine;


// should load a new scene when the player enters a trigger zone

public class SceneEntrance : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //checktag
        {
            GameManager.Instance.LoadScene(sceneToLoad);
        }
    }
}
