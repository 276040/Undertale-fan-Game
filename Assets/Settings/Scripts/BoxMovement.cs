 
using UnityEngine;
using System.Collections;

public class BattleBox : MonoBehaviour
{
    [Header("Box Sizes")]
    public Vector3 smallSize = new Vector3(3f, 3f, 1f);
    public Vector3 bigSize = new Vector3(6f, 6f, 1f);

    [Header("Timing")]
    public float startBigTime = 5f;
    public float shrinkTime = 15f;
    public float transitionDuration = 1f;

    private bool isBig = false;

    void Start()
    {
        transform.localScale = smallSize;
        StartCoroutine(BoxRoutine());
    }

    IEnumerator BoxRoutine()
    {
        // Wait until it's time to become big
        yield return new WaitForSeconds(startBigTime);

        // Make the box big
        yield return StartCoroutine(ChangeSize(bigSize));
        isBig = true;

        // Stay big until it's time to shrink
        float waitTime = shrinkTime - startBigTime;
        if (waitTime > 0)
            yield return new WaitForSeconds(waitTime);

        // Shrink the box
        yield return StartCoroutine(ChangeSize(smallSize));
        isBig = false;
    }

    IEnumerator ChangeSize(Vector3 targetSize)
    {
        Vector3 startSize = transform.localScale;
        float timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / transitionDuration);

            transform.localScale = Vector3.Lerp(
                startSize,
                targetSize,
                progress
            );

            yield return null;
        }

        transform.localScale = targetSize;
    }
}