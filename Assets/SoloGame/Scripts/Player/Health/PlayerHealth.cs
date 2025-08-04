using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("Invincibility & Knockdown")]
    public float invincibleTime = 0.8f;
    public float knockdownComboWindow = 1.2f;
    public int hitsForKnockdown = 3;

    [Header("UI")]
    public UnityEngine.UI.Scrollbar healthBar;

    [Header("References")]
    public Animator animator;
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

    void Awake()
    {
        currentHealth = maxHealth;
        if (!animator) animator = GetComponent<Animator>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!playerInput) playerInput = GetComponent<PlayerCombatInput>();
    }

    void Start()
    {
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible || isKnockedDown || isDead)
            return;

        
        float now = Time.time;
        if (now - lastHitTime < knockdownComboWindow)
            consecutiveHits++;
        else
            consecutiveHits = 1;
        lastHitTime = now;

        
        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateUI();

        
        animator.SetTrigger("Hurt");
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

    private IEnumerator HitFeedback()
    {
        isInvincible = true;

        
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.color = Color.red;
        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        
        Vector3 orig = transform.position;
        for (int i = 0; i < shakeCount; i++)
        {
            transform.position = orig + (Vector3)Random.insideUnitCircle * shakeAmount;
            yield return new WaitForSeconds(flashDuration / shakeCount);
        }
        transform.position = orig;

        if (sr) sr.color = Color.white;

        
        yield return new WaitForSeconds(invincibleTime - flashDuration);
        isInvincible = false;
    }

    private void UpdateUI()
    {
        if (healthBar)
            healthBar.size = (float)currentHealth / maxHealth;
    }

    private IEnumerator KnockdownCoroutine()
    {
        isKnockedDown = true;
        isInvincible = true;
        playerInput.SetInputEnabled(false);

       
        animator.SetTrigger("Knockback");

        
        float arcDuration = 0.45f, arcHeight = 1.5f, arcDistance = 2f;
        Vector3 start = transform.position;
        float dir = transform.localScale.x > 0 ? -1 : 1;
        Vector3 target = start + new Vector3(dir * arcDistance, 0, 0);

        float timer = 0f;
        while (timer < arcDuration)
        {
            float p = timer / arcDuration;
            float y = Mathf.Sin(p * Mathf.PI) * arcHeight;
            Vector3 hor = Vector3.Lerp(start, target, p);
            transform.position = new Vector3(hor.x, start.y + y, start.z);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector3(target.x, start.y, start.z);

        
        animator.SetTrigger("Knockdown");
        yield return new WaitForSeconds(0.45f);

        
        animator.SetTrigger("GetUp");
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        
        isKnockedDown = false;
        isInvincible = false;
        playerInput.SetInputEnabled(true);
    }

    private IEnumerator HandleDeath()
    {
        
        yield return StartCoroutine(KnockdownCoroutine());

        
        yield return new WaitForSeconds(0.3f);

        
        if (CombatZoneManager.Instance != null)
            CombatZoneManager.Instance.HandlePlayerDefeat();
        else
            GameManager.Instance.LoadScene("Overworld");
    }

    void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.Alpha8) && !isDead)
        {
            currentHealth = 0;
            UpdateUI();
            StartCoroutine(HandleDeath());
        }
#endif
    }
}
