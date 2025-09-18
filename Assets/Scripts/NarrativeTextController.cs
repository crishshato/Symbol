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

    [Header("Simple Timing")]
    [Tooltip("Base seconds each line stays (before postGap).")]
    public float holdBase = 0.8f;
    [Tooltip("Extra seconds per character (reading time).")]
    public float holdPerChar = 0.02f;
    [Tooltip("Keep line up at least as long as the voice clip (if provided).")]
    public bool useVoiceLength = true;
    [Tooltip("Small extra pause between lines.")]
    public float postGap = 0.10f;
    [Tooltip("Clamp final hold seconds (safety).")]
    public float minHold = 0.50f, maxHold = 12f;

    [Header("Legacy (optional)")]
    public bool autoAdvance = false;      // off by default
    public float autoAdvanceDelay = 2f;

    [Header("Behavior")]
    public bool clearOnHide = true;

    TMP_Text tmp;
    CanvasGroup cg;

    List<string> lines;
    int index = -1;
    Coroutine routine;

    void Awake()
    {
        tmp = GetComponentInChildren<TMP_Text>(true);
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        if (tmp) { tmp.text = ""; tmp.maxVisibleCharacters = 0; }
    }

    // ---------- Simple timing API ----------
    // voiceSeconds < 0 if unknown / none
    public float ComputeHoldSeconds(int charCount, float voiceSeconds = -1f)
    {
        float read = Mathf.Max(0f, holdBase + charCount * holdPerChar);
        if (useVoiceLength && voiceSeconds >= 0f)
            read = Mathf.Max(read, voiceSeconds);

        return Mathf.Clamp(read + Mathf.Max(0f, postGap), minHold, maxHold);
    }

    // ---------- Public controls ----------
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
        if (lines == null || lines.Count == 0) return false;
        index++;
        if (index >= lines.Count) return false;

        string next = lines[index];
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(TypeLine(next, -1f));
        return true;
    }

    // ---------- Internals ----------
    IEnumerator ShowRoutine()
    {
        yield return StartCoroutine(Fade(1f));
        if (!NextLine()) ClearNow();
    }

    IEnumerator FadeOutRoutine()
    {
        yield return StartCoroutine(Fade(0f));
        if (clearOnHide) ClearNow();
    }

    IEnumerator HideAndClearRoutine()
    {
        yield return StartCoroutine(Fade(0f));
        ClearNow();
    }

    // voiceSeconds: pass clip.length if you have it, else -1
    IEnumerator TypeLine(string text, float voiceSeconds)
    {
        if (!tmp) yield break;

        tmp.text = text;
        tmp.maxVisibleCharacters = 0;
        yield return null; // let TMP layout

        int total = tmp.textInfo.characterCount;
        for (int i = 0; i <= total; i++)
        {
            tmp.maxVisibleCharacters = i;
            yield return new WaitForSeconds(charDelay);
        }

        // Legacy auto-advance (optional)
        if (autoAdvance && lines != null && index >= 0 && index < lines.Count - 1)
        {
            float hold = Mathf.Max(autoAdvanceDelay, ComputeHoldSeconds(total, voiceSeconds));
            yield return new WaitForSeconds(hold);
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
