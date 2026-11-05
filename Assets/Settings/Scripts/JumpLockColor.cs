
using UnityEngine;
using System.Collections;

public class JumpLockVisual : MonoBehaviour
{
    [Header("Hit Color")]
    public Color lockedColor = Color.red;

    private SpriteRenderer[] sprites;
    private Color[] originalColors;
    private Coroutine colorRoutine;

    void Awake()
    {
        sprites = GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[sprites.Length];

        for (int i = 0; i < sprites.Length; i++)
        {
            originalColors[i] = sprites[i].color;
        }
    }

    public void ShowLockedColor(float duration)
    {
        if (colorRoutine != null)
            StopCoroutine(colorRoutine);

        colorRoutine = StartCoroutine(ColorRoutine(duration));
    }

    IEnumerator ColorRoutine(float duration)
    {
        foreach (SpriteRenderer sprite in sprites)
        {
            if (sprite != null)
                sprite.color = lockedColor;
        }

        yield return new WaitForSeconds(duration);

        RestoreColor();
        colorRoutine = null;
    }

    void RestoreColor()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
                sprites[i].color = originalColors[i];
        }
    }
}
