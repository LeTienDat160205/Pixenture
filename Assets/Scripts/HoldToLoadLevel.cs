using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>Fills the player's circle for the supplied duration, then signals level load.</summary>
public class HoldToLoadLevel : MonoBehaviour
{
    [SerializeField] private Image fillCircle;

    private Coroutine loadRoutine;
    private Action onLoadComplete;

    public bool BeginLoading(float duration, Action completed)
    {
        CancelLoading();
        if (fillCircle == null)
        {
            Debug.LogError("HoldToLoadLevel needs a Fill Circle Image.", this);
            return false;
        }

        onLoadComplete = completed;
        loadRoutine = StartCoroutine(FillForDuration(Mathf.Max(0.01f, duration)));
        return true;
    }

    private IEnumerator FillForDuration(float duration)
    {
        float elapsed = 0f;
        fillCircle.fillAmount = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fillCircle.fillAmount = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        fillCircle.fillAmount = 1f;
        loadRoutine = null;
        Action completed = onLoadComplete;
        onLoadComplete = null;
        completed?.Invoke();
    }

    public void CancelLoading()
    {
        if (loadRoutine != null)
        {
            StopCoroutine(loadRoutine);
            loadRoutine = null;
        }

        onLoadComplete = null;
        if (fillCircle != null)
            fillCircle.fillAmount = 0f;
    }

    // Kept so the existing Player Input event remains valid; E no longer starts loading.
    public void OnHold(InputAction.CallbackContext context) { }

    private void OnDisable()
    {
        CancelLoading();
    }
}
