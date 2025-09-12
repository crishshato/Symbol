using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NarrativeTrigger : MonoBehaviour
{
    public NarrativeTextController narrative;
    public string playerTag = "Player";

    [Header("Optional dialogue (set here if you want this trigger to drive lines)")]
    [TextArea] public string[] dialogueLines;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag) || !narrative) return;

        if (dialogueLines != null && dialogueLines.Length > 0)
            narrative.SetLines(dialogueLines);

        narrative.Show();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag) || !narrative) return;

        // Hide and clear the TMP text so it’s empty when the player leaves
        narrative.HideAndClear();
    }
}
