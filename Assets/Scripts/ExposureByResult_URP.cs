using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ExposureByResult_URP : MonoBehaviour
{
    [Header("References")]
    public Volume volume;

    [Header("Exposure targets (EV)")]
    // 0 is "normal". Negative = darker, Positive = brighter.
    public float loseExposureEV = -2.0f;
    public float winExposureEV  =  1.0f;

    [Header("Timing")]
    public float fadeDuration = 0.6f;

    private ColorAdjustments colorAdjustments;
    private Coroutine running;

    void Awake()
    {
        if (volume == null)
        {
            Debug.LogError("ExposureByResult_URP: Volume reference is missing.");
            enabled = false;
            return;
        }

        if (!volume.profile.TryGet(out colorAdjustments))
        {
            Debug.LogError("ExposureByResult_URP: Volume profile has no Color Adjustments override.");
            enabled = false;
            return;
        }

        // Ensure override is active
        colorAdjustments.postExposure.overrideState = true;
    }

    public void OnLose()
    {
        FadeTo(loseExposureEV);
    }

    public void OnWin()
    {
        FadeTo(winExposureEV);
        // Optional: return to normal after a short time:
        // StartCoroutine(ReturnToNormalAfter(0.4f));
    }

    public void SetNormal()
    {
        FadeTo(0f);
    }

    public void SetImmediate(float exposureEV)
    {
        if (running != null) StopCoroutine(running);
        colorAdjustments.postExposure.value = exposureEV;
    }

    void FadeTo(float targetEV)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FadeRoutine(targetEV));
    }

    IEnumerator FadeRoutine(float targetEV)
    {
        float startEV = colorAdjustments.postExposure.value;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float ev = Mathf.Lerp(startEV, targetEV, t / fadeDuration);
            colorAdjustments.postExposure.value = ev;
            yield return null;
        }

        colorAdjustments.postExposure.value = targetEV;
        running = null;
    }

    IEnumerator ReturnToNormalAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        SetNormal();
    }
}
