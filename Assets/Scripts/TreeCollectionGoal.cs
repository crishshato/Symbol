using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class TreeCollectionGoal : MonoBehaviour
{
    [Header("Required in Order (edit in Inspector)")]
    public List<string> requiredIds = new() { "ItemA", "ItemB", "ItemC", "ItemD", "ItemE" };
    public bool enforceOrder = true;               // ordered ritual

    [Header("Refs")]
    public TreeGrowth tree;                        // drag your TreeGrowth here

    [Header("Events")]
    public UnityEvent<string, int, int> onItemDeposited; // (id, collected, total)
    public UnityEvent onAllCollected;

    int collectedOrdered = 0;
    public int CollectedCount => collectedOrdered;
    public int TotalRequired => requiredIds.Count;

    void Awake() { if (!tree) tree = GetComponent<TreeGrowth>(); }

    public bool IsNeeded(string id)
    {
        int idx = requiredIds.IndexOf(id);
        return idx == collectedOrdered; // only next-in-order
    }

    // Returns true if accepted & counted
    public bool TryDeposit(string id)
    {
        if (!IsNeeded(id)) return false;

        collectedOrdered = Mathf.Clamp(collectedOrdered + 1, 0, TotalRequired);

        // push normalized progress to TreeGrowth
        float progress = (float)CollectedCount / Mathf.Max(1, TotalRequired);
        if (tree) tree.SetProgress(progress);

        onItemDeposited?.Invoke(id, CollectedCount, TotalRequired);

        if (CollectedCount >= TotalRequired)
        {
            if (tree) tree.Grow();  // final flourish (optional)
            onAllCollected?.Invoke();
        }
        return true;
    }
}
