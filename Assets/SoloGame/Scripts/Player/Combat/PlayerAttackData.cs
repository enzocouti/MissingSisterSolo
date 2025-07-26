using UnityEngine;

[CreateAssetMenu(menuName = "Combat/PlayerAttackData")]
public class PlayerAttackData : ScriptableObject
{
    public string attackName = "Punch";
    public int damage = 1;
    public float attackDuration = 0.25f;
    public Vector2 hitboxSize = new Vector2(1f, 1f);
    public Vector2 hitboxOffset = new Vector2(1f, 0f);
    public bool isHeavy = false;
    public bool isLauncher = false;

    [Header("Launch Settings")]
    public float launchHeight = 2.5f;
    public float launchDuration = 0.4f;
    public float launchHangTime = 0.15f;

    [Header("Animation Info")]
    public string animationTriggerName = "Punch1";

    [Header("Knockback Settings")]
    public float knockbackForce = 2.5f;

    [Header("Special Moves")]
    public bool isSlam = false;
}
