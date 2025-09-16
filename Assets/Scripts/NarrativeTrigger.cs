using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class NarrativeTrigger : MonoBehaviour
{
    public NarrativeTextController narrative;
    public string playerTag = "Player";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] voiceLines;

    [Header("Optional dialogue (set here if you want this trigger to drive lines)")]
    [TextArea] public string[] dialogueLines;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag) || !narrative) return;

        if (dialogueLines != null && dialogueLines.Length > 0)
            narrative.SetLines(dialogueLines);

        narrative.Show();

        // Start VO sequence
        if (audioSource && voiceLines != null && voiceLines.Length > 0)
            StartCoroutine(PlayVoiceOverSequence());
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag) || !narrative) return;

        StopAllCoroutines(); // stop VO if player leaves early
        narrative.HideAndClear();
        if (audioSource) audioSource.Stop();
    }

    IEnumerator PlayVoiceOverSequence()
    {
        for (int i = 0; i < voiceLines.Length; i++)
        {
            if (voiceLines[i])
            {
                audioSource.clip = voiceLines[i];
                audioSource.Play();
                yield return new WaitForSeconds(audioSource.clip.length + 0.2f);
            }
        }
    }
}
