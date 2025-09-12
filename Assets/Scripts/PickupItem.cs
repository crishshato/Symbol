using UnityEngine;

public enum ItemKind { Generic, WateringCan }   // NEW

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PickupItem : MonoBehaviour, IInteractable
{
    [Header("Item")]
    public ItemKind itemKind = ItemKind.Generic;  // NEW

    [Header("Carry Settings")]
    public float holdDistance = 2.0f;
    public float followStrength = 20f;
    public float maxCarrySpeed = 10f;
    public float throwForce = 8f;

    Rigidbody rb;
    bool isCarried;
    public bool IsCarried => isCarried;          // NEW
    Transform carryAnchor;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (!isCarried) PickUp(interactor);
        else Drop();
    }

    public void SetTargeted(bool targeted) { /* optional */ }

    void FixedUpdate()
    {
        if (!isCarried || !carryAnchor) return;

        Vector3 toTarget = carryAnchor.position - rb.worldCenterOfMass;
        Vector3 desiredVel = Vector3.ClampMagnitude(toTarget * followStrength, maxCarrySpeed);
        rb.velocity = desiredVel;

        Quaternion targetRot = carryAnchor.rotation;
        Quaternion delta = targetRot * Quaternion.Inverse(rb.rotation);
        delta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;
        Vector3 angular = axis * angle * Mathf.Deg2Rad * followStrength;
        rb.angularVelocity = Vector3.ClampMagnitude(angular, maxCarrySpeed);
    }

    void PickUp(PlayerInteractor interactor)
    {
        isCarried = true;
        carryAnchor = interactor.EnsureCarryAnchor(holdDistance);
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void Drop()
    {
        isCarried = false;
        carryAnchor = null;
        rb.useGravity = true;
        rb.angularVelocity = Vector3.zero;
    }

    public void Throw(Vector3 dir)
    {
        Drop();
        rb.AddForce(dir.normalized * throwForce, ForceMode.VelocityChange);
    }
}
