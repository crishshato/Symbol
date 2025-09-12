using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasGroup))]
public class NarrativeTextController : MonoBehaviour
{
    [Header("Single line (fallback if no dialogue lines provided)")]
    [TextArea] public string fullText;

    [Header("Typing / Fade")]
    public float charDelay = 0.02f;
    public float fadeTime = 0.25f;

    [Header("Input (optional)")]
    public bool allowAdvanceInput = true;
    public KeyCode advanceKey = KeyCode.E;

    [Header("Behavior")]
    public bool clearOnHide = true;

    TMP_Text tmp;
    CanvasGroup cg;

    List<string> lines;     // dialogue lines
    int index = -1;         // current line index
    Coroutine routine;
    bool isVisible;

    void Awake()
    {
        tmp = GetComponentInChildren<TMP_Text>(true);
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        if (tmp) { tmp.text = ""; tmp.maxVisibleCharacters = 0; }
    }

    void Update()
    {
        if (!isVisible || !allowAdvanceInput) return;
        if (Input.GetKeyDown(advanceKey))
        {
            NextLine();
        }
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
        // reset dialogue state
        index = -1;
    }

    public bool NextLine()
    {
        // If we have dialogue lines, step to next; otherwise reuse fullText
        string next;
        if (lines != null && lines.Count > 0)
        {
            index++;
            if (index >= lines.Count) return false; // no more lines
            next = lines[index];
        }
        else
        {
            // single-line mode: pressing next when one line already shown does nothing
            if (index >= 0) return false;
            index = 0;
            next = fullText;
        }

        // (re)start typing this line
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(TypeLine(next));
        return true;
    }

    IEnumerator ShowRoutine()
    {
        // fade in
        yield return StartCoroutine(Fade(1f));
        isVisible = true;

        // kick off first line
        if (!NextLine())
        {
            // nothing to show, just ensure empty
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
        yield return null; // let TMP layout

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
            t += Time.deltaTime / Mathf.Max(0.0001f, fadeTime);
            cg.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        cg.alpha = target;
    }
}
