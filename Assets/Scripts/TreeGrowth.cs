using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class TreeGrowth : MonoBehaviour
{
    public enum GrowthMode { SwapStages, AnimateScale, AnimatorTrigger }

    [Header("Mode")]
    public GrowthMode mode = GrowthMode.SwapStages;

    [Header("Progress")]
    [Range(0f, 1f)] public float progress = 0f;  // overall watering progress
    public float[] stageThresholds = new float[] { 0.33f, 0.66f, 1.0f };
    // 33% = stage 1, 66% = stage 2, 100% = final stage

    [Header("Stage Swap (if using SwapStages)")]
    public GameObject[] stages; // Small, Medium, Big (set in this order)

    [Header("Scale Animate (if using AnimateScale)")]
    public Transform scaleTarget;     // usually the mesh root
    public Vector3 minScale = new Vector3(0.6f, 0.6f, 0.6f);
    public Vector3 maxScale = Vector3.one;
    public float scaleTweenTime = 0.6f;
    public AnimationCurve scaleEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Animator Trigger (if using AnimatorTrigger)")]
    public Animator animator;
    public string growTriggerName = "Grow";   // trigger called at each stage
    public float animBlendDelay = 0.1f;       // small delay for blending

    [Header("FX per Stage (optional, same length as stages/thresholds)")]
    public ParticleSystem[] stageVfx;
    public AudioClip[] stageSfx;
    public AudioSource audioSource;

    [Header("Events")]
    public UnityEvent onAnyStageUp;           // fires every time we advance
    public UnityEvent<int> onStageChanged;    // passes new stage index

    int currentStageIndex = -1;
    Coroutine scaleRoutine;

    // --- PUBLIC API ---

    // Call this from QTE code: pass progress in [0..1]
    public void SetProgress(float normalized)
    {
        float clamped = Mathf.Clamp01(normalized);
        if (Mathf.Approximately(progress, clamped)) return;
        progress = clamped;
        EvaluateStage();
    }

    // Convenience: add a chunk of progress and update
    public void AddProgress(float delta)
    {
        SetProgress(progress + delta);
    }

    
    public void Grow()
    {
        // “Grow” means: move to the lowest stage whose threshold > current stage
        for (int i = 0; i < stageThresholds.Length; i++)
        {
            if (i > currentStageIndex)
            {
                progress = Mathf.Max(progress, stageThresholds[i]);
                EvaluateStage();
                break;
            }
        }
    }

    // --- INTERNAL ---

    void Start()
    {
        // Initialize stage visuals
        EvaluateStage(force: true);
    }

    void EvaluateStage(bool force = false)
    {
        int targetStage = StageIndexFromProgress(progress);

        if (force || targetStage != currentStageIndex)
        {
            currentStageIndex = targetStage;
            ApplyStage(targetStage);
            onAnyStageUp?.Invoke();
            onStageChanged?.Invoke(targetStage);
        }
    }

    int StageIndexFromProgress(float p)
    {
        // Returns 0..(N-1) based on thresholds
        for (int i = 0; i < stageThresholds.Length; i++)
        {
            if (p <= stageThresholds[i])
                return i;
        }
        return stageThresholds.Length - 1;
    }

    void ApplyStage(int stageIndex)
    {
        // 1) Visuals by mode
        switch (mode)
        {
            case GrowthMode.SwapStages:
                ApplyStage_Swap(stageIndex);
                break;
            case GrowthMode.AnimateScale:
                ApplyStage_Scale(stageIndex);
                break;
            case GrowthMode.AnimatorTrigger:
                ApplyStage_Animator(stageIndex);
                break;
        }

        // 2) FX (optional)
        PlayStageFx(stageIndex);
    }

    void ApplyStage_Swap(int stageIndex)
    {
        if (stages == null || stages.Length == 0) return;
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i]) stages[i].SetActive(i == stageIndex);
        }
    }

    void ApplyStage_Scale(int stageIndex)
    {
        if (!scaleTarget) return;

        // Lerp scale according to stage index vs total stages
        float t = stageThresholds.Length <= 1 ? 1f : (float)stageIndex / (stageThresholds.Length - 1);

        Vector3 targetScale = Vector3.Lerp(minScale, maxScale, t);

        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(TweenScale(scaleTarget, targetScale, scaleTweenTime, scaleEase));
    }

    IEnumerator TweenScale(Transform tr, Vector3 to, float time, AnimationCurve ease)
    {
        Vector3 from = tr.localScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, time);
            float e = ease.Evaluate(t);
            tr.localScale = Vector3.LerpUnclamped(from, to, e);
            yield return null;
        }
        tr.localScale = to;
    }

    void ApplyStage_Animator(int stageIndex)
    {
        if (!animator || string.IsNullOrEmpty(growTriggerName)) return;
        
        StartCoroutine(FireAnimatorTrigger());
    }

    IEnumerator FireAnimatorTrigger()
    {
        yield return new WaitForSeconds(animBlendDelay);
        animator.ResetTrigger(growTriggerName);
        animator.SetTrigger(growTriggerName);
    }

    void PlayStageFx(int stageIndex)
    {
        // Particles
        if (stageVfx != null && stageIndex >= 0 && stageIndex < stageVfx.Length)
        {
            var fx = stageVfx[stageIndex];
            if (fx) fx.Play();
        }
        // Sound
        if (audioSource && stageSfx != null && stageIndex >= 0 && stageIndex < stageSfx.Length)
        {
            var clip = stageSfx[stageIndex];
            if (clip) audioSource.PlayOneShot(clip);
        }
    }
}
