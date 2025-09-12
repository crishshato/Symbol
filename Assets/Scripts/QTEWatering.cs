using UnityEngine;
using UnityEngine.UI;

public class QTEWatering : MonoBehaviour
{
    [Header("UI References")]
    public GameObject qteUIRoot;     // NEW: root panel to toggle
    public Slider progressBar;       // watering bar (0–1)
    public RectTransform marker;     // moving marker
    public RectTransform targetZone; // success zone
    public RectTransform qteBar;     // entire QTE bar rect

    [Header("QTE Settings")]
    public float markerSpeed = 250f;
    public float hitRange = 30f;
    public float progressPerHit = 0.1f;
    public float penaltyPerMiss = 0.05f;
    public bool autoCloseOnComplete = true;   // NEW

    [Header("Tree")]
    public TreeGrowth tree; // target tree

    float direction = 1f;
    bool isActive;          // NEW

    void Start()
    {
        ResetTargetZone();
        if (progressBar) progressBar.value = 0f;
        SetActive(false);   // start hidden
    }

    public void StartQTE(TreeGrowth treeRef = null)   // NEW
    {
        if (treeRef) tree = treeRef;
        ResetTargetZone();
        SetActive(true);
    }

    public void StopQTE(bool resetBar = false)        // NEW
    {
        SetActive(false);
        if (resetBar && progressBar) progressBar.value = 0f;
    }

    public void SetActive(bool value)                 // NEW
    {
        isActive = value;
        if (qteUIRoot) qteUIRoot.SetActive(value);
        enabled = value; // optional; keeps Update off when inactive
    }

    void Update()
    {
        if (!isActive) return;

        MoveMarker();

        // Left Mouse Button to attempt
        if (Input.GetMouseButtonDown(0))
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
            if (tree) tree.SetProgress(progressBar.value);

            ResetTargetZone();

            if (progressBar.value >= 1f)
            {
                if (tree) tree.Grow();
                if (autoCloseOnComplete) StopQTE(false);
            }
        }
        else
        {
            // fail
            progressBar.value = Mathf.Max(0f, progressBar.value - penaltyPerMiss);
            if (tree) tree.SetProgress(progressBar.value);
        }
    }

    void ResetTargetZone()
    {
        float halfWidth = qteBar.rect.width * 0.5f - 50f;
        float x = Random.Range(-halfWidth, halfWidth);
        targetZone.anchoredPosition = new Vector2(x, targetZone.anchoredPosition.y);
    }
}
