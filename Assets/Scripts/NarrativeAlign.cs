// NarrativeAlign.cs
// Drop this on your world-space text prefab (the Canvas root).
// Then assign the target object you want it to sit above.

using UnityEngine;

[ExecuteAlways] // works in Edit Mode and Play Mode
[DisallowMultipleComponent]
public class NarrativeAlign : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                 // Object to align above
    public bool useParentIfTargetMissing = true;

    [Header("Placement")]
    public float verticalOffset = 0.25f;     // meters above the top of target
    public bool faceSceneCameraInEditor = true; // for easy scene placement readability

    [Header("Auto Scale (relative to target size)")]
    [Tooltip("0 = keep current scale. >0 = scale to largest X/Z of target bounds * factor * 0.1")]
    [Range(0f, 1f)]
    public float scaleRelativeToBounds = 0.15f;
    public float minUniformScale = 0.001f;

    [Header("Update Mode")]
    public bool followContinuously = true;   // if off, use Snap Now to place once

    // --- Public one-click action ---
    [ContextMenu("Snap Now")]
    public void SnapNow()
    {
        Transform t = ResolveTarget();
        if (!t) return;

        if (!TryGetBounds(t, out Bounds b)) return;

        // position at top-center + offset
        Vector3 topCenter = new Vector3(b.center.x, b.max.y, b.center.z);
        transform.position = topCenter + Vector3.up * verticalOffset;

        // optional uniform scaling
        if (scaleRelativeToBounds > 0f)
        {
            float d = Mathf.Max(b.size.x, b.size.z);
            float s = Mathf.Max(minUniformScale, d * scaleRelativeToBounds * 0.1f);
            transform.localScale = Vector3.one * s;
        }

#if UNITY_EDITOR
        // make it readable in Scene view while placing
        if (!Application.isPlaying && faceSceneCameraInEditor)
        {
            var sv = UnityEditor.SceneView.lastActiveSceneView;
            if (sv && sv.camera)
            {
                transform.rotation = Quaternion.LookRotation(sv.camera.transform.forward, Vector3.up);
            }
        }
#endif
    }

    void LateUpdate()
    {
        if (!followContinuously) return;
        SnapNow(); // cheap enough; runs in editor & play
    }

    Transform ResolveTarget()
    {
        if (target) return target;
        if (useParentIfTargetMissing && transform.parent) return transform.parent;
        return null;
    }

    bool TryGetBounds(Transform tr, out Bounds b)
    {
        // Prefer Renderer bounds; fallback to Collider
        var rend = tr.GetComponentInChildren<Renderer>();
        if (rend)
        {
            b = rend.bounds;
            return true;
        }
        var col = tr.GetComponentInChildren<Collider>();
        if (col)
        {
            b = col.bounds;
            return true;
        }
        b = default;
        return false;
    }

    // keep things tidy when values change in Inspector
    void OnValidate()
    {
        minUniformScale = Mathf.Max(0.00001f, minUniformScale);
        if (!Application.isPlaying && followContinuously) SnapNow();
    }
}
