using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TreeDepositZone : MonoBehaviour
{
    public TreeCollectionGoal goal;
    public PlayerInteractor interactor;
    public string playerTag = "Player";
    public bool requireItemInside = true;          // NEW

    bool playerInside;
    Collider zoneCol;                               // NEW

    void Awake() { zoneCol = GetComponent<Collider>(); } // NEW
    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag)) playerInside = false;
    }

    void Update()
    {
        if (!playerInside) return;
        if (Input.GetMouseButtonDown(0)) TryDepositHeld();
    }

    void TryDepositHeld()
    {
        if (!interactor || !goal) return;

        var held = interactor.CarriedItem;
        if (!held) return;

        // NEW: ensure the carried item's collider is inside/intersecting the zone
        if (requireItemInside && !IsHeldItemInsideZone(held)) return;

        var res = held.GetComponent<ResourceItem>();
        if (!res) return;

        if (goal.TryDeposit(res.resourceId))
        {
            Destroy(held.gameObject);
        }
    }

    // NEW: supports Box/Sphere; falls back to bounds check
    bool IsHeldItemInsideZone(PickupItem held)
    {
        var itemCol = held.GetComponent<Collider>();
        if (!itemCol || !zoneCol) return false;

        // Fast overlap test
        if (zoneCol is BoxCollider || zoneCol is SphereCollider)
        {
            return itemCol.bounds.Intersects(zoneCol.bounds);
        }

        // Generic: AABB vs AABB
        return itemCol.bounds.Intersects(zoneCol.bounds);
    }
}
