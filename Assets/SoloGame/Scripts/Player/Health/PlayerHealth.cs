using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;

    public float invincibleTime = 0.8f;
    public float knockdownComboWindow = 1.2f;
    public int hitsForKnockdown = 3;

    public Scrollbar healthBar;
    public SpriteRenderer spriteRenderer;
    public PlayerCombatInput playerInput;

    private Rigidbody2D rb;
    private Color baseColor;

    private float lastHitTime = -99f;
    private int consecutiveHits = 0;
    private bool isInvincible = false;
    private bool isKnockedDown = false;
    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (!playerInput) playerInput = GetComponent<PlayerCombatInput>();
        baseColor = spriteRenderer.color;
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible || isDead || isKnockedDown) return;

        float now = Time.time;
        consecutiveHits = (now - lastHitTime < knockdownComboWindow) ? consecutiveHits + 1 : 1;
        lastHitTime = now;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
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
        yield return StartCoroutine(HitStop(0.08f));

        Vector3 originalPos = transform.position;
        for (int i = 0; i < 4; i++)
        {
            transform.position = originalPos + (Vector3)Random.insideUnitCircle * 0.08f;
            yield return new WaitForSeconds(0.03f);
        }

        transform.position = originalPos;
        spriteRenderer.color = baseColor;

        yield return new WaitForSeconds(invincibleTime);
        isInvincible = false;
    }

    IEnumerator KnockdownCoroutine()
    {
        isKnockedDown = true;
        isInvincible = true;
        playerInput.SetInputEnabled(false);

        float arcDuration = 0.45f;
        float arcHeight = 1.8f;
        float arcDistance = 2f;

        Vector3 start = transform.position;
        float dir = spriteRenderer.flipX ? 1f : -1f;
        Vector3 end = start + new Vector3(dir * arcDistance, 0f, 0f);

        float t = 0f;
        while (t < arcDuration)
        {
            float progress = t / arcDuration;
            float yOffset = Mathf.Sin(progress * Mathf.PI) * arcHeight;
            Vector3 horizontal = Vector3.Lerp(start, end, progress);
            transform.position = new Vector3(horizontal.x, start.y + yOffset, start.z);

            t += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(end.x, start.y, start.z);
        yield return new WaitForSeconds(0.5f);

        isKnockedDown = false;
        isInvincible = false;
        playerInput.SetInputEnabled(true);
    }

    IEnumerator HandleDeath()
    {
        playerInput.SetInputEnabled(false);
        yield return StartCoroutine(KnockdownCoroutine());
        yield return new WaitForSeconds(0.3f);

        gameObject.SetActive(false);

        GameManager.Instance.LoadScene("Overworld");
    }

    IEnumerator HitStop(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}

