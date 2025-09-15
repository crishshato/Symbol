using UnityEngine;

[DisallowMultipleComponent]
public class ResourceItem : MonoBehaviour
{
    [Tooltip("Must match one entry in TreeCollectionGoal.requiredIds")]
    public string resourceId = "ItemA";
}
