using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class NarrativeTextController : MonoBehaviour
{
    [Header("Typing / Fade")]
    public float charDelay = 0.02f;
    public float fadeTime = 0.25f;

    [Header("Auto Advance")]
    public bool autoAdvance = true;
    public float autoAdvanceDelay = 2f;

    [Header("Behavior")]
    public bool clearOnHide = true;

    TMP_Text tmp;
    CanvasGroup cg;

    List<string> lines;
    int index = -1;
    Coroutine routine;
    bool isVisible;

    void Awake()
    {
        tmp = GetComponentInChildren<TMP_Text>(true);
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        if (tmp) { tmp.text = ""; tmp.maxVisibleCharacters = 0; }
    }

    // Call before Show() when you want a dialogue
    public void SetLines(IEnumerable<string> newLines)
    {
        lines = new List<string>(newLines ?? System.Array.Empty<string>());
        index = -1;
    }

    public void Show()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine());
    }

    public void Hide()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FadeOutRoutine());
    }

    public void HideAndClear()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(HideAndClearRoutine());
    }

    public void ClearNow()
    {
        if (tmp)
        {
            tmp.text = "";
            tmp.maxVisibleCharacters = 0;
        }
        index = -1;
    }

    public bool NextLine()
    {
        string next;
        if (lines != null && lines.Count > 0)
        {
            index++;
            if (index >= lines.Count) return false;
            next = lines[index];
        }
        else
        {
            return false;
        }

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(TypeLine(next));
        return true;
    }

    IEnumerator ShowRoutine()
    {
        yield return StartCoroutine(Fade(1f));
        isVisible = true;

        if (!NextLine())
        {
            ClearNow();
        }
    }

    IEnumerator FadeOutRoutine()
    {
        isVisible = false;
        yield return StartCoroutine(Fade(0f));
        if (clearOnHide) ClearNow();
    }

    IEnumerator HideAndClearRoutine()
    {
        isVisible = false;
        yield return StartCoroutine(Fade(0f));
        ClearNow();
    }

    IEnumerator TypeLine(string text)
    {
        if (!tmp) yield break;

        tmp.text = text;
        tmp.maxVisibleCharacters = 0;
        yield return null;

        int total = tmp.textInfo.characterCount;
        for (int i = 0; i <= total; i++)
        {
            tmp.maxVisibleCharacters = i;
            yield return new WaitForSeconds(charDelay);
        }

        if (autoAdvance)
        {
            yield return new WaitForSeconds(autoAdvanceDelay);
            if (NextLine()) yield break;
        }
    }

    IEnumerator Fade(float target)
    {
        float start = cg.alpha, t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, fadeTime);
            cg.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        cg.alpha = target;
    }
}