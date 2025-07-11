using UnityEngine;
using System.Collections;

public class EnemyCombatController : MonoBehaviour
{
    public float moveSpeed = 2.5f;
    public float detectionRange = 7f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.4f;
    public int touchDamage = 2;

    [Header("Spacing")]
    public float minSeparation = 0.7f;
    public float verticalAlignThreshold = 0.32f;
    public float verticalSeparation = 0.5f;

    private Transform player;
    private bool isDead = false;
    private bool isHurt = false;
    private bool isLaunched = false;
    private float lastAttackTime = -99f;

    private SpriteRenderer spriteRenderer;
    private Color baseColor;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer ? spriteRenderer.color : Color.white;
    }

    void Update()
    {
        if (isDead || isHurt || isLaunched) return;
        if (player == null) return;

        Vector2 toPlayer = player.position - transform.position;
        float xDist = Mathf.Abs(toPlayer.x);
        float yDist = Mathf.Abs(toPlayer.y);
        float attackDist = toPlayer.magnitude;

        if (attackDist <= attackRange)
        {
            TryAttack();
        }
        else if (attackDist <= detectionRange)
        {
            if (yDist > verticalAlignThreshold)
            {
                float yDir = Mathf.Sign(toPlayer.y);
                transform.position += new Vector3(0, yDir * moveSpeed * Time.deltaTime, 0);
            }
            else
            {
                bool blocked = false;
                Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, minSeparation);
                foreach (var col in nearby)
                {
                    if (col != null && col != this.GetComponent<Collider2D>() && col.CompareTag("Enemy"))
                    {
                        Vector2 otherPos = col.transform.position;
                        float horizontalDiff = otherPos.x - transform.position.x;
                        float verticalDiff = Mathf.Abs(otherPos.y - transform.position.y);

                        bool sameDirection = Mathf.Sign(horizontalDiff) == Mathf.Sign(toPlayer.x);
                        bool closeVertically = verticalDiff < verticalSeparation;

                        if (Mathf.Abs(horizontalDiff) < minSeparation && sameDirection && closeVertically)
                        {
                            blocked = true;
                            break;
                        }
                    }
                }

                if (!blocked)
                {
                    float xDir = Mathf.Sign(toPlayer.x);
                    transform.position += new Vector3(xDir * moveSpeed * Time.deltaTime, 0, 0);

                    if (spriteRenderer)
                        spriteRenderer.flipX = (xDir < 0);
                }
            }
        }
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            Debug.Log("[Enemy] Attack player!");
            // add real player damage here later
        }
    }

    // Changed parameter name for clarity!
    public void OnHurt(PlayerAttackData attackData, bool playerIsFacingRight)
    {
        if (isDead) return;

        if (attackData.isLauncher)
        {
            StartCoroutine(LaunchCoroutine(attackData, playerIsFacingRight));
        }
        else if (attackData.isHeavy)
        {
            StartCoroutine(KnockbackCoroutine(attackData, playerIsFacingRight));
        }
        else
        {
            StartCoroutine(HurtFeedback());
        }
    }

    public void OnDeath()
    {
        isDead = true;
        StopAllCoroutines();
        if (spriteRenderer)
            spriteRenderer.color = Color.gray;
    }

    IEnumerator HurtFeedback()
    {
        isHurt = true;
        if (spriteRenderer)
            spriteRenderer.color = Color.red;

        float freeze = 0.07f;
        float t = 0;
        while (t < freeze)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        Vector3 originalPos = transform.position;
        float shakeAmt = 0.07f;
        for (int i = 0; i < 3; i++)
        {
            transform.position = originalPos + (Vector3)Random.insideUnitCircle * shakeAmt;
            yield return new WaitForSeconds(0.03f);
        }
        transform.position = originalPos;

        if (spriteRenderer)
            spriteRenderer.color = baseColor;

        isHurt = false;
    }

    // Use playerIsFacingRight for correct knockback direction!
    IEnumerator KnockbackCoroutine(PlayerAttackData attackData, bool playerIsFacingRight)
    {
        isLaunched = true;
        float duration = 0.35f;
        float height = 1f;
        float distance = attackData.knockbackForce;
        Vector3 start = transform.position;
        float direction = playerIsFacingRight ? 1f : -1f;
        Vector3 target = start + new Vector3(direction * distance, 0, 0);

        float timer = 0f;
        while (timer < duration)
        {
            float progress = timer / duration;
            float yOffset = Mathf.Sin(progress * Mathf.PI) * height;
            Vector3 horizontal = Vector3.Lerp(start, target, progress);
            transform.position = new Vector3(horizontal.x, start.y + yOffset, start.z);

            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector3(target.x, start.y, start.z);

        isLaunched = false;
    }

    IEnumerator LaunchCoroutine(PlayerAttackData attackData, bool playerIsFacingRight)
    {
        isLaunched = true;
        float duration = attackData.launchDuration;
        float height = attackData.launchHeight;
        Vector3 start = transform.position;

        float timer = 0f;
        // Upward arc
        while (timer < duration)
        {
            float progress = timer / duration;
            float yOffset = Mathf.Sin(progress * Mathf.PI) * height;
            transform.position = new Vector3(start.x, start.y + yOffset, start.z);

            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector3(start.x, start.y + height, start.z);

        float hang = attackData.launchHangTime;
        float hangTimer = 0f;
        while (hangTimer < hang)
        {
            hangTimer += Time.deltaTime;
            yield return null;
        }

        float fallTime = duration * 0.8f;
        Vector3 peak = transform.position;
        float fallTimer = 0f;
        while (fallTimer < fallTime)
        {
            float progress = fallTimer / fallTime;
            float yOffset = Mathf.Lerp(height, 0, progress);
            transform.position = new Vector3(start.x, start.y + yOffset, start.z);

            fallTimer += Time.deltaTime;
            yield return null;
        }
        transform.position = start;

        isLaunched = false;
    }
}
