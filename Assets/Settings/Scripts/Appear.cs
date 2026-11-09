
using UnityEngine;
using System.Collections;

public class TimedVisibility : MonoBehaviour
{
    [Header("Timing")]
    public float appearTime = 3f;
    public float disappearTime = 5f;

    [Header("Settings")]
    public bool repeat = false;

    private SpriteRenderer spriteRenderer;
    private Collider2D[] colliders;
    private Coroutine visibilityCoroutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        colliders = GetComponents<Collider2D>();
    }

    void OnEnable()
    {
        SetVisible(false);

        visibilityCoroutine = StartCoroutine(ShowAndHide());
    }

    void OnDisable()
    {
        if (visibilityCoroutine != null)
        {
            StopCoroutine(visibilityCoroutine);
            visibilityCoroutine = null;
        }
    }

    IEnumerator ShowAndHide()
    {
        do
        {
            SetVisible(false);

            yield return new WaitForSeconds(appearTime);

            SetVisible(true);

            yield return new WaitForSeconds(disappearTime);

            SetVisible(false);

        } while (repeat);

        visibilityCoroutine = null;
    }

    void SetVisible(bool visible)
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;

        foreach (Collider2D col in colliders)
        {
            if (col != null)
                col.enabled = visible;
        }
    }
}
