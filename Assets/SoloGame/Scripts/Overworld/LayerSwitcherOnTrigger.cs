using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LayerSwitcherOnTrigger : MonoBehaviour
{
    [Header("Sorting Layers")]
    [Tooltip("The sorting layer to use while the player is inside the trigger.")]
    public string insideLayer = "Foreground";

    [Tooltip("The sorting layer to revert to when the player leaves.")]
    public string defaultLayer = "Default";

    [Header("Reference")]
    [Tooltip("If your SpriteRenderer is on a child, assign it here. Otherwise it will grab the one on this GameObject.")]
    public SpriteRenderer targetRenderer;

    private void Reset()
    {
        
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();

        if (targetRenderer == null)
            Debug.LogError($"[{nameof(LayerSwitcherOnTrigger)}] No SpriteRenderer found on {name} or its root.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!targetRenderer) return;

        if (other.CompareTag("Player"))
            targetRenderer.sortingLayerName = insideLayer;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!targetRenderer) return;

        if (other.CompareTag("Player"))
            targetRenderer.sortingLayerName = defaultLayer;
    }
}
