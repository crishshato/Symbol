using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartSplash : MonoBehaviour
{
    [Header("Slides")]
    public Image image;            // UI Image on your Canvas
    public Sprite[] slides;        // order you want to show
    public float[] durations;      // seconds per slide (optional, leave empty to use defaultDuration)
    public float defaultDuration = 2.5f;

    [Header("Fade")]
    public float fadeTime = 0.6f;  // fade in/out per slide

    [Header("Flow")]
    public string nextSceneName = "GameScene";
    public bool anyKeySkipsSlide = true;  // press any key/mouse to advance
    public bool anyKeySkipsAll = false;  // hold/press to jump straight to next scene

    void Awake()
    {
        if (!image) image = FindObjectOfType<Image>();
        if (image) image.color = new Color(1, 1, 1, 0); // start transparent
    }

    IEnumerator Start()
    {
        if (image == null || slides == null || slides.Length == 0)
        {
            SceneManager.LoadScene(nextSceneName);
            yield break;
        }

        for (int i = 0; i < slides.Length; i++)
        {
            image.sprite = slides[i];

            // fade in
            yield return StartCoroutine(FadeTo(1f));

            // wait for duration or skip
            float hold = (durations != null && i < durations.Length && durations[i] > 0f)
                         ? durations[i] : defaultDuration;

            float t = 0f;
            while (t < hold)
            {
                if (anyKeySkipsAll && Input.anyKeyDown)
                {
                    // skip everything → load scene
                    SceneManager.LoadScene(nextSceneName);
                    yield break;
                }
                if (anyKeySkipsSlide && Input.anyKeyDown) break;

                t += Time.deltaTime;
                yield return null;
            }

            // fade out
            yield return StartCoroutine(FadeTo(0f));
        }

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator FadeTo(float target)
    {
        float start = image.color.a;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, fadeTime);
            float a = Mathf.Lerp(start, target, t);
            var c = image.color; c.a = a; image.color = c;
            yield return null;
        }
        var c2 = image.color; c2.a = target; image.color = c2;
    }
}
