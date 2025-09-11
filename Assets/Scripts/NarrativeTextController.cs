// NarrativeTextController.cs
using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class NarrativeTextController : MonoBehaviour
{
    [TextArea] public string fullText;
    public float charDelay = 0.02f;   // typing speed
    public float fadeTime = 0.25f;

    TMP_Text tmp;
    CanvasGroup cg;
    Coroutine routine;

    void Awake()
    {
        tmp = GetComponentInChildren<TMP_Text>(true);
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        if (tmp) tmp.text = "";
    }

    public void Show()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine());
    }

    public void Hide()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Fade(0f));
    }

    IEnumerator ShowRoutine()
    {
        yield return StartCoroutine(Fade(1f));
        if (!tmp) yield break;

        tmp.maxVisibleCharacters = 0;
        tmp.text = fullText;
        // ensure layout is ready
        yield return null;

        int total = tmp.textInfo.characterCount;
        for (int i = 0; i <= total; i++)
        {
            tmp.maxVisibleCharacters = i;
            yield return new WaitForSeconds(charDelay);
        }
    }

    IEnumerator Fade(float target)
    {
        float start = cg.alpha, t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fadeTime;
            cg.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        cg.alpha = target;
    }
}
