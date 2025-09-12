using UnityEngine;
using UnityEngine.UI;

public class QTEWatering : MonoBehaviour
{
    [Header("UI References")]
    public Slider progressBar;       // watering bar (0–1)
    public RectTransform marker;     // moving marker
    public RectTransform targetZone; // success zone
    public RectTransform qteBar;     // entire QTE bar rect

    [Header("QTE Settings")]
    public float markerSpeed = 250f;
    public float hitRange = 30f;
    public float progressPerHit = 0.1f;
    public float penaltyPerMiss = 0.05f;

    [Header("Tree")]
    public TreeGrowth tree; // tree here

    float direction = 1f;

    void Start()
    {
        ResetTargetZone();
        if (progressBar) progressBar.value = 0f;
    }

    void Update()
    {
        MoveMarker();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckHit();
        }
    }

    void MoveMarker()
    {
        marker.anchoredPosition += Vector2.right * direction * markerSpeed * Time.deltaTime;
        float halfWidth = qteBar.rect.width * 0.5f;

        if (marker.anchoredPosition.x > halfWidth) direction = -1f;
        if (marker.anchoredPosition.x < -halfWidth) direction = 1f;
    }

    void CheckHit()
    {
        float dist = Mathf.Abs(marker.anchoredPosition.x - targetZone.anchoredPosition.x);

        if (dist <= hitRange)
        {
            // success
            progressBar.value += progressPerHit;
            tree.SetProgress(progressBar.value); // sync tree

            ResetTargetZone();

            if (progressBar.value >= 1f)
            {
                tree.Grow();
                Debug.Log("Tree fully watered!");
            }
        }
        else
        {
            // fail
            progressBar.value = Mathf.Max(0f, progressBar.value - penaltyPerMiss);
            tree.SetProgress(progressBar.value);
        }
    }

    void ResetTargetZone()
    {
        float halfWidth = qteBar.rect.width * 0.5f - 50f;
        float x = Random.Range(-halfWidth, halfWidth);
        targetZone.anchoredPosition = new Vector2(x, targetZone.anchoredPosition.y);
    }
}
