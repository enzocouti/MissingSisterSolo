using UnityEngine;

public class SceneEntrance : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    [Header("Base Lockout")]
    public string baseId;         
    public GameObject baseIcon;   

    private Collider2D entranceCollider;

    private void Awake()
    {
        entranceCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        
        if (!string.IsNullOrEmpty(baseId))
        {
            if (IsBaseCleared(baseId))
            {
                if (entranceCollider) entranceCollider.enabled = false;
                if (baseIcon) baseIcon.SetActive(false); 
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (entranceCollider && entranceCollider.enabled && collision.CompareTag("Player"))
        {
            GameManager.Instance.LoadScene(sceneToLoad);
        }
    }

    private bool IsBaseCleared(string id)
    {
        if (id == "Alpha")
            return GameManager.Instance.IsAlphaBaseCleared();
        if (id == "Bravo")
            return GameManager.Instance.IsBravoBaseCleared();
        return false;
    }
}
