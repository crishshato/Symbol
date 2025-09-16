using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TreeDepositZone : MonoBehaviour
{
    public TreeCollectionGoal goal;
    public PlayerInteractor interactor;
    public string playerTag = "Player";
    public bool requireItemInside = true;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip depositSfx;
    public AudioClip rejectSfx;

    bool playerInside;
    Collider zoneCol;

    void Awake() { zoneCol = GetComponent<Collider>(); }
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

        if (requireItemInside && !IsHeldItemInsideZone(held)) return;

        var res = held.GetComponent<ResourceItem>();
        if (!res) return;

        if (goal.TryDeposit(res.resourceId))
        {
            if (audioSource && depositSfx) audioSource.PlayOneShot(depositSfx);
            Destroy(held.gameObject);
        }
        else
        {
            if (audioSource && rejectSfx) audioSource.PlayOneShot(rejectSfx);
        }
    }

    bool IsHeldItemInsideZone(PickupItem held)
    {
        var itemCol = held.GetComponent<Collider>();
        if (!itemCol || !zoneCol) return false;

        if (zoneCol is BoxCollider || zoneCol is SphereCollider)
        {
            return itemCol.bounds.Intersects(zoneCol.bounds);
        }

        return itemCol.bounds.Intersects(zoneCol.bounds);
    }
}
