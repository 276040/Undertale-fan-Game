
using UnityEngine;
using System.Collections;

public class TimedVisibility : MonoBehaviour
{
    [Header("Timing")]
    public float appearTime = 3f;
    public float disappearTime = 5f;

    [Header("Settings")]
    public bool repeat = false;

    private SpriteRenderer[] spriteRenderers;
    private Collider2D[] colliders;
    private Coroutine visibilityCoroutine;

    public bool IsVisible { get; private set; }

    void Awake()
    {
        // Find renderers and colliders on this object and its children.
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        colliders = GetComponentsInChildren<Collider2D>(true);
    }

    void OnEnable()
    {
        SetVisible(false);

        if (visibilityCoroutine != null)
            StopCoroutine(visibilityCoroutine);

        visibilityCoroutine = StartCoroutine(ShowAndHide());
    }

    void OnDisable()
    {
        if (visibilityCoroutine != null)
        {
            StopCoroutine(visibilityCoroutine);
            visibilityCoroutine = null;
        }

        SetVisible(false);
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

    public void SetVisible(bool visible)
    {
        IsVisible = visible;

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            if (sr != null)
                sr.enabled = visible;
        }

        foreach (Collider2D col in colliders)
        {
            if (col != null)
                col.enabled = visible;
        }
    }
}
