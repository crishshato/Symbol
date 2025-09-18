using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class TreeDepositZone : MonoBehaviour
{
    [Header("Core")]
    public TreeCollectionGoal goal;
    public PlayerInteractor interactor;
    public bool requireItemInside = true;   // only item overlap required

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip depositSfx;
    public AudioClip rejectSfx;

    [Header("Narrative per Item (multi-line)")]
    public NarrativeTextController narrative;   // world-space subtitle controller
    public AudioSource voiceSource;             // VO source near the tree

    [System.Serializable]
    public class PerItemNarrative
    {
        [TextArea] public string[] lines;       // subtitles for THIS item
        public AudioClip[] voice;               // VO clips (same order/length as lines)
    }
    public PerItemNarrative[] perItem;          // size = goal.TotalRequired

    [Header("Optional HUD")]
    public TMP_Text progressLabel;              // "Offering X/Y"

    Collider zoneCol;
    bool playingNarrative = false;              // guard so only one runs at a time

    void Awake()
    {
        zoneCol = GetComponent<Collider>();
        if (zoneCol) zoneCol.isTrigger = true;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TryDepositHeld();
    }

    void TryDepositHeld()
    {
        if (!interactor || !goal) return;

        var held = interactor.CarriedItem;
        if (!held) { PlayReject(); return; }

        if (requireItemInside && !IsHeldItemInsideZone(held)) { PlayReject(); return; }

        var res = held.GetComponent<ResourceItem>();
        if (!res) { PlayReject(); return; }

        if (goal.TryDeposit(res.resourceId))
        {
            if (sfxSource && depositSfx) sfxSource.PlayOneShot(depositSfx);

            int idx = goal.CollectedCount - 1;

            if (progressLabel) progressLabel.text = $"Offering {goal.CollectedCount}/{goal.TotalRequired}";

            Destroy(held.gameObject);

            if (!playingNarrative && narrative && perItem != null &&
                idx >= 0 && idx < perItem.Length &&
                perItem[idx] != null && perItem[idx].lines != null && perItem[idx].lines.Length > 0)
            {
                StartCoroutine(PlayItemNarrative(idx));
            }
        }
        else
        {
            PlayReject();
        }
    }

    IEnumerator PlayItemNarrative(int idx)
    {
        playingNarrative = true;
        narrative.ClearNow();

        var pack = perItem[idx];
        for (int i = 0; i < pack.lines.Length; i++)
        {
            string line = pack.lines[i];

            // show one line
            narrative.SetLines(new[] { line });
            narrative.Show();

            // start matching VO in parallel (if present)
            float clipLen = -1f;
            if (voiceSource && pack.voice != null && i < pack.voice.Length && pack.voice[i])
            {
                voiceSource.Stop();
                voiceSource.clip = pack.voice[i];
                voiceSource.Play();
                clipLen = voiceSource.clip.length;
            }

            // ✅ use controller's timing policy
            float hold = narrative.ComputeHoldSeconds(line.Length, clipLen);
            yield return new WaitForSeconds(hold);

            narrative.ClearNow();
        }

        playingNarrative = false;
    }

    bool IsHeldItemInsideZone(PickupItem held)
    {
        var itemCol = held.GetComponent<Collider>();
        if (!itemCol || !zoneCol) return false;
        return itemCol.bounds.Intersects(zoneCol.bounds);
    }

    void PlayReject()
    {
        if (sfxSource && rejectSfx) sfxSource.PlayOneShot(rejectSfx);
    }
}
