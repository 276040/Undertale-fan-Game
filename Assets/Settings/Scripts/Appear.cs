
using UnityEngine;
using System.Collections;

public class TimedVisibility : MonoBehaviour
{
    public float appearTime = 3f;
    public float disappearTime = 5f;

    private SpriteRenderer spriteRenderer;
    private Collider2D[] colliders;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        colliders = GetComponents<Collider2D>();
    }

    void Start()
    {
        StartCoroutine(ShowAndHide());
    }

    IEnumerator ShowAndHide()
    {
        // Hide the sprite and colliders
        SetVisible(false);

        // Wait before appearing
        yield return new WaitForSeconds(appearTime);

        // Show the sprite and enable colliders
        SetVisible(true);

        // Wait before disappearing
        yield return new WaitForSeconds(disappearTime);

        // Hide the sprite and disable colliders
        SetVisible(false);
    }

    void SetVisible(bool visible)
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;

        foreach (Collider2D col in colliders)
        {
            col.enabled = visible;
        }
    }
}