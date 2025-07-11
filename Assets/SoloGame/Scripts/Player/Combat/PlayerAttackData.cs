using UnityEngine;
//attempt on configureable attacks
[CreateAssetMenu(menuName = "Combat/PlayerAttackData")]
public class PlayerAttackData : ScriptableObject
{
    public string attackName;
    public int damage = 2;
    public float attackDuration = 0.22f;
    public Vector2 hitboxSize = new Vector2(1, 1);
    public Vector2 hitboxOffset = new Vector2(0.7f, 0);
    public bool isLauncher = false;
    public bool isHeavy = false;
    public float knockbackForce = 3f;    // Used for heavy knockback
    public float launchHeight = 2f;      // Used for launchers
    public float launchDuration = 0.45f; // How long the launch arc lasts
    public float launchHangTime = 0.15f; // Pause at top for air combos
}

