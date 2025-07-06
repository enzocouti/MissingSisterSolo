using UnityEngine;

//attempt on making configureable attacks
[CreateAssetMenu(menuName = "Combat/Player Attack Data")]
public class PlayerAttackData : ScriptableObject
{
    public string attackName = "Punch";
    public float damage = 10f;
    public float attackDuration = 0.3f;
    public Vector2 hitboxOffset;
    public Vector2 hitboxSize = new Vector2(1f, 1f);
    public Vector2 knockbackForce = new Vector2(5f, 2f);
    public bool launches = false;
}