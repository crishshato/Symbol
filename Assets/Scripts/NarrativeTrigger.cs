// NarrativeTrigger.cs
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NarrativeTrigger : MonoBehaviour
{
    public NarrativeTextController narrative;
    public string playerTag = "Player";

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && narrative) narrative.Show();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) && narrative) narrative.Hide();
    }
}
