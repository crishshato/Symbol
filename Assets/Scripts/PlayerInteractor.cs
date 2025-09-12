using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Raycast")]
    public Camera cam;
    public float interactDistance = 3f;
    public LayerMask interactMask = ~0;

    [Header("Input Keys")]
    public KeyCode interactKey = KeyCode.E;
    public KeyCode throwKey = KeyCode.Mouse0; // left click to throw

    IInteractable current;
    PickupItem carried;

    public PickupItem CarriedItem => carried;   // NEW

    Transform carryAnchor; // lazily created

    void Update()
    {
        UpdateTarget();

        if (Input.GetKeyDown(interactKey))
        {
            if (current != null)
            {
                current.Interact(this);
                carried = current as PickupItem;
            }
        }

        if (carried != null && Input.GetKeyDown(throwKey))
        {
            carried.Throw(cam.transform.forward);
            carried = null;
        }
    }

    void UpdateTarget()
    {
        // Clear previous highlight
        if (current != null) current.SetTargeted(false);
        current = null;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out var hit, interactDistance, interactMask, QueryTriggerInteraction.Ignore))
        {
            current = hit.collider.GetComponentInParent<IInteractable>();
            if (current != null) current.SetTargeted(true);
        }
    }

    public Transform EnsureCarryAnchor(float distance)
    {
        if (carryAnchor == null)
        {
            carryAnchor = new GameObject("CarryAnchor").transform;
            carryAnchor.SetParent(cam.transform, false);
        }
        carryAnchor.localPosition = new Vector3(0, 0, distance);
        carryAnchor.localRotation = Quaternion.identity;
        return carryAnchor;
    }
}
