using UnityEngine;
using System.Collections;

public class EnemyCombatController : MonoBehaviour
{
    public float moveSpeed = 2.5f;
    public float detectionRange = 7f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.4f;
    public int touchDamage = 2; //damage

    private Transform player;
    private bool isDead = false;
    private bool isHurt = false;
    private float lastAttackTime = -99f;

    private SpriteRenderer spriteRenderer;
    private Color baseColor;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer ? spriteRenderer.color : Color.white;
    }

    private void Update()
    {
        if (isDead || isHurt) return;
        if (player == null) return;

        float dist = Mathf.Abs(player.position.x - transform.position.x);

        // Chase or Attack
        if (dist <= attackRange)
        {
            TryAttack();
        }
        else if (dist <= detectionRange)
        {
            // Move toward player only X axis test
            float dir = Mathf.Sign(player.position.x - transform.position.x);
            transform.position += new Vector3(dir * moveSpeed * Time.deltaTime, 0, 0);

            // flip sprite to face player?
            if (spriteRenderer)
                spriteRenderer.flipX = (dir < 0);
        }
        else
        {
            // Idle
        }
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            Debug.Log("[Enemy] Attack player!");

            // add real player damage here later?
        }
    }

    //called by enemyhealth
    public void OnHurt(PlayerAttackData attackData, bool hitFromRight)
    {
        if (isDead) return;

        StartCoroutine(HurtFeedback());
    }

    
    public void OnDeath()
    {
        isDead = true;
        StopAllCoroutines();
        if (spriteRenderer)
            spriteRenderer.color = Color.gray;
    }

    //feedback
    IEnumerator HurtFeedback()
    {
        isHurt = true;
        if (spriteRenderer)
            spriteRenderer.color = Color.red;

        // Freeze frame (hit pause)
        float freeze = 0.07f;
        float t = 0;
        while (t < freeze)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // Shake
        Vector3 originalPos = transform.position;
        float shakeAmt = 0.07f;
        for (int i = 0; i < 3; i++)
        {
            transform.position = originalPos + (Vector3)Random.insideUnitCircle * shakeAmt;
            yield return new WaitForSeconds(0.03f);
        }
        transform.position = originalPos;

        // Restore color
        if (spriteRenderer)
            spriteRenderer.color = baseColor;

        isHurt = false;
    }
}
