
using UnityEngine;

public class BoneDamage : MonoBehaviour
{
    [Header("Bone Damage")]
    public int damage = 1;

    [Header("Jump Lock")]
    public float jumpLockDuration = 2f;

    private bool alreadyHit = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyHit || !other.CompareTag("Player"))
            return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        PlayerMovement movement = other.GetComponent<PlayerMovement>();
        JumpLockVisual visual = other.GetComponent<JumpLockVisual>();

        if (health != null)
            health.TakeDamage(damage);

        if (movement != null)
            movement.DisableJumpForSeconds(jumpLockDuration);

        if (visual != null)
            visual.ShowLockedColor(jumpLockDuration);

        alreadyHit = true;
    }
}
