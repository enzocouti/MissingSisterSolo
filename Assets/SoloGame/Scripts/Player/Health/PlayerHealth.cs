using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Invincibility & Knockdown")]
    public float invincibleTime = 0.8f;
    public float knockdownComboWindow = 1.2f;
    public int hitsForKnockdown = 3;

    [Header("UI")]
    public Scrollbar healthBar;

    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public PlayerCombatInput playerInput;
    public Rigidbody2D rb;

    [Header("Feedback")]
    public float flashDuration = 0.12f;
    public float shakeAmount = 0.1f;
    public int shakeCount = 4;

    private float lastHitTime = -99f;
    private int consecutiveHits = 0;
    private bool isInvincible = false;
    private bool isKnockedDown = false;
    private bool isDead = false;

    private Color baseColor;

    void Awake()
    {
        currentHealth = maxHealth;
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!playerInput) playerInput = GetComponent<PlayerCombatInput>();
        baseColor = spriteRenderer ? spriteRenderer.color : Color.white;
    }

    void Start()
    {
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        // Make player invincible to all damage if dashing (from PlayerCombat)
        var combat = GetComponent<PlayerCombat>();
        if (isInvincible || isKnockedDown || isDead || (combat != null && combat.isDashInvincible))
            return;

        float now = Time.time;
        if (now - lastHitTime < knockdownComboWindow)
            consecutiveHits++;
        else
            consecutiveHits = 1;
        lastHitTime = now;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();

        StartCoroutine(HitFeedback());

        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            StartCoroutine(HandleDeath());
            return;
        }

        if (consecutiveHits >= hitsForKnockdown)
        {
            StartCoroutine(KnockdownCoroutine());
            consecutiveHits = 0;
        }
    }


    void UpdateUI()
    {
        if (healthBar)
            healthBar.size = (float)currentHealth / maxHealth;
    }

    IEnumerator HitFeedback()
    {
        isInvincible = true;

        if (spriteRenderer) spriteRenderer.color = Color.red;

        float freeze = 0.07f;
        float t = 0;
        while (t < freeze)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        Vector3 originalPos = transform.position;
        for (int i = 0; i < shakeCount; i++)
        {
            transform.position = originalPos + (Vector3)Random.insideUnitCircle * shakeAmount;
            yield return new WaitForSeconds(flashDuration / shakeCount);
        }
        transform.position = originalPos;

        if (spriteRenderer) spriteRenderer.color = baseColor;

        yield return new WaitForSeconds(invincibleTime - freeze);

        isInvincible = false;
    }

    IEnumerator KnockdownCoroutine()
    {
        isKnockedDown = true;
        isInvincible = true;
        if (playerInput) playerInput.SetInputEnabled(false);

        float arcDuration = 0.45f;
        float arcHeight = 1.5f;
        float arcDistance = 2f;

        Vector3 start = transform.position;
        float direction = spriteRenderer && spriteRenderer.flipX ? 1f : -1f;

        Vector3 target = start + new Vector3(direction * arcDistance, 0, 0);

        float timer = 0f;
        while (timer < arcDuration)
        {
            float progress = timer / arcDuration;
            float yOffset = Mathf.Sin(progress * Mathf.PI) * arcHeight;
            Vector3 horizontal = Vector3.Lerp(start, target, progress);
            transform.position = new Vector3(horizontal.x, start.y + yOffset, start.z);

            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector3(target.x, start.y, start.z);

        yield return new WaitForSeconds(0.45f);

        isKnockedDown = false;
        isInvincible = false;
        if (playerInput) playerInput.SetInputEnabled(true);
    }

    IEnumerator HandleDeath()
    {
        isInvincible = true;
        if (playerInput) playerInput.SetInputEnabled(false);

        yield return StartCoroutine(KnockdownCoroutine());
        yield return new WaitForSeconds(0.3f);

        
        if (CombatZoneManager.Instance != null)
        {
            CombatZoneManager.Instance.HandlePlayerDefeat();
        }
        else
        {
            
            GameManager.Instance?.LoadScene("Overworld");
        }
    }

    void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.Alpha8) && !isDead)
        {
            Debug.Log("[DEBUG] Forcing player death!");
            currentHealth = 0;
            UpdateUI();
            StartCoroutine(HandleDeath());
        }
#endif
    }
}
