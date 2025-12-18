using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles animations for menu papers including focus/unfocus effects.
/// Requirements: 2.2, 2.3 - Paper focus with visual highlighting and dimming
/// </summary>
public class PaperAnimator : MonoBehaviour
{
    [Header("Focus Animation")]
    [SerializeField] private float focusScale = 1.1f;
    [SerializeField] private float focusZOffset = -0.05f;
    [SerializeField] private float focusDuration = 0.25f;
    [SerializeField] private AnimationCurve focusCurve;
    
    [Header("Unfocus Animation")]
    [SerializeField] private float unfocusedAlpha = 0.6f;
    [SerializeField] private float unfocusDuration = 0.2f;
    
    [Header("Slide-In Animation (for unlock)")]
    [SerializeField] private float slideDistance = 0.5f;
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private AnimationCurve slideCurve;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip focusSound;
    [SerializeField] private AudioClip paperRustleSound;
    
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private CanvasGroup canvasGroup;
    private Coroutine currentAnimation;
    private MeshRenderer[] meshRenderers;
    private Color[] originalColors;
    
    /// <summary>
    /// Whether an animation is currently playing.
    /// </summary>
    public bool IsAnimating => currentAnimation != null;
    
    /// <summary>
    /// Event fired when focus animation completes.
    /// </summary>
    public event Action OnFocusComplete;
    
    /// <summary>
    /// Event fired when unfocus animation completes.
    /// </summary>
    public event Action OnUnfocusComplete;
    
    /// <summary>
    /// Event fired when slide-in animation completes.
    /// </summary>
    public event Action OnSlideInComplete;


    private void Awake()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        canvasGroup = GetComponent<CanvasGroup>();
        
        // Cache mesh renderers for dimming effect
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers.Length > 0)
        {
            originalColors = new Color[meshRenderers.Length];
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i].material != null)
                {
                    originalColors[i] = meshRenderers[i].material.color;
                }
            }
        }
        
        // Create default curves if not set
        if (focusCurve == null || focusCurve.length == 0)
        {
            focusCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
        
        if (slideCurve == null || slideCurve.length == 0)
        {
            slideCurve = CreateSlideInCurve();
        }
    }

    private AnimationCurve CreateSlideInCurve()
    {
        var curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0f, 0f, 0f, 2f));
        curve.AddKey(new Keyframe(0.7f, 1.05f, 0f, 0f));
        curve.AddKey(new Keyframe(1f, 1f, 0f, 0f));
        return curve;
    }

    /// <summary>
    /// Plays the focus animation - scales up and moves forward.
    /// </summary>
    public void PlayFocusAnimation()
    {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(FocusAnimationCoroutine());
    }

    /// <summary>
    /// Plays the unfocus animation - returns to normal scale and dims.
    /// </summary>
    public void PlayUnfocusAnimation()
    {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(UnfocusAnimationCoroutine());
    }

    /// <summary>
    /// Plays the slide-in animation for when paper is unlocked/added.
    /// </summary>
    public void PlaySlideInAnimation()
    {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(SlideInAnimationCoroutine());
    }

    /// <summary>
    /// Immediately sets the focused state without animation.
    /// </summary>
    public void SetFocusedImmediate()
    {
        StopCurrentAnimation();
        transform.localScale = originalScale * focusScale;
        transform.localPosition = originalPosition + new Vector3(0, 0, focusZOffset);
        SetAlpha(1f);
        SetDimming(1f);
    }

    /// <summary>
    /// Immediately sets the unfocused state without animation.
    /// </summary>
    public void SetUnfocusedImmediate()
    {
        StopCurrentAnimation();
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        SetAlpha(unfocusedAlpha);
        SetDimming(unfocusedAlpha);
    }

    private void StopCurrentAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
    }


    private IEnumerator FocusAnimationCoroutine()
    {
        PlaySound(focusSound);
        
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = originalScale * focusScale;
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = originalPosition + new Vector3(0, 0, focusZOffset);
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : GetCurrentDimming();
        
        float elapsed = 0f;
        while (elapsed < focusDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / focusDuration);
            float curveValue = focusCurve.Evaluate(t);
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, curveValue);
            
            float alpha = Mathf.Lerp(startAlpha, 1f, curveValue);
            SetAlpha(alpha);
            SetDimming(alpha);
            
            yield return null;
        }

        // Ensure final state
        transform.localScale = targetScale;
        transform.localPosition = targetPos;
        SetAlpha(1f);
        SetDimming(1f);

        currentAnimation = null;
        OnFocusComplete?.Invoke();
    }

    private IEnumerator UnfocusAnimationCoroutine()
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = originalScale;
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = originalPosition;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : GetCurrentDimming();
        
        float elapsed = 0f;
        while (elapsed < unfocusDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / unfocusDuration);
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            
            float alpha = Mathf.Lerp(startAlpha, unfocusedAlpha, t);
            SetAlpha(alpha);
            SetDimming(alpha);
            
            yield return null;
        }

        // Ensure final state
        transform.localScale = targetScale;
        transform.localPosition = targetPos;
        SetAlpha(unfocusedAlpha);
        SetDimming(unfocusedAlpha);

        currentAnimation = null;
        OnUnfocusComplete?.Invoke();
    }

    private IEnumerator SlideInAnimationCoroutine()
    {
        PlaySound(paperRustleSound);
        
        // Start from off-screen (to the right)
        Vector3 startPos = originalPosition + new Vector3(slideDistance, 0, 0);
        transform.localPosition = startPos;
        transform.localScale = originalScale;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        gameObject.SetActive(true);
        
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            float curveValue = slideCurve.Evaluate(t);
            
            transform.localPosition = Vector3.Lerp(startPos, originalPosition, curveValue);
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = t;
            }
            
            yield return null;
        }

        // Ensure final state
        transform.localPosition = originalPosition;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        currentAnimation = null;
        OnSlideInComplete?.Invoke();
    }

    private void SetAlpha(float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }

    private void SetDimming(float brightness)
    {
        if (meshRenderers == null || originalColors == null) return;
        
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null && meshRenderers[i].material != null && i < originalColors.Length)
            {
                Color dimmedColor = originalColors[i] * brightness;
                dimmedColor.a = originalColors[i].a;
                meshRenderers[i].material.color = dimmedColor;
            }
        }
    }

    private float GetCurrentDimming()
    {
        if (meshRenderers != null && meshRenderers.Length > 0 && 
            meshRenderers[0] != null && meshRenderers[0].material != null &&
            originalColors != null && originalColors.Length > 0)
        {
            Color current = meshRenderers[0].material.color;
            Color original = originalColors[0];
            if (original.r > 0)
            {
                return current.r / original.r;
            }
        }
        return 1f;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Sets the original position reference (useful if position changes at runtime).
    /// </summary>
    public void SetOriginalPosition(Vector3 position)
    {
        originalPosition = position;
    }

    /// <summary>
    /// Sets the original scale reference (useful if scale changes at runtime).
    /// </summary>
    public void SetOriginalScale(Vector3 scale)
    {
        originalScale = scale;
    }
}
