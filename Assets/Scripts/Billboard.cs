// Billboard.cs
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] float smooth = 10f;
    Camera cam;

    void Start() { cam = Camera.main; }
    void LateUpdate()
    {
        if (!cam) return;
        var targetRot = Quaternion.LookRotation(
            cam.transform.forward,    // face the way camera looks
            cam.transform.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smooth);
    }
}
