using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles animations for the Announcement Board including show/hide with scale and bounce effects.
/// Requirements: 1.1 - Board toggle visibility with animations
/// </summary>
public class BoardAnimator : MonoBehaviour
{
    [Header("Show Animation")]
    [SerializeField] private float showDuration = 0.4f;
    [SerializeField] private float bounceOvershoot = 1.15f;
    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Hide Animation")]
    [SerializeField] private float hideDuration = 0.25f;
    [SerializeField] private AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip showSound;
    [SerializeField] private AudioClip hideSound;
    
    private Vector3 originalScale;
    private Coroutine currentAnimation;
    private CanvasGroup canvasGroup;
    
    /// <summary>
    /// Whether an animation is currently playing.
    /// </summary>
    public bool IsAnimating => currentAnimation != null;
    
    /// <summary>
    /// Event fired when show animation completes.
    /// </summary>
    public event Action OnShowComplete;
    
    /// <summary>
    /// Event fired when hide animation completes.
    /// </summary>
    public event Action OnHideComplete;

    private void Awake()
    {
        originalScale = transform.localScale;
        canvasGroup = GetComponent<CanvasGroup>();
        
        // Create default animation curves if not set
        if (showCurve == null || showCurve.length == 0)
        {
            showCurve = CreateBounceInCurve();
        }
        
        if (hideCurve == null || hideCurve.length == 0)
        {
            hideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        }
    }


    /// <summary>
    /// Creates a bounce-in animation curve with overshoot.
    /// </summary>
    private AnimationCurve CreateBounceInCurve()
    {
        var curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0f, 0f, 0f, 2f));
        curve.AddKey(new Keyframe(0.6f, bounceOvershoot, 0f, 0f));
        curve.AddKey(new Keyframe(0.8f, 0.95f, 0f, 0f));
        curve.AddKey(new Keyframe(1f, 1f, 0f, 0f));
        return curve;
    }

    /// <summary>
    /// Plays the show animation with scale bounce effect.
    /// </summary>
    public void PlayShowAnimation()
    {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(ShowAnimationCoroutine());
    }

    /// <summary>
    /// Plays the hide animation with scale down and fade.
    /// </summary>
    public void PlayHideAnimation()
    {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(HideAnimationCoroutine());
    }

    /// <summary>
    /// Immediately shows the board without animation.
    /// </summary>
    public void ShowImmediate()
    {
        StopCurrentAnimation();
        transform.localScale = originalScale;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Immediately hides the board without animation.
    /// </summary>
    public void HideImmediate()
    {
        StopCurrentAnimation();
        transform.localScale = Vector3.zero;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        gameObject.SetActive(false);
    }

    private void StopCurrentAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
    }

    private IEnumerator ShowAnimationCoroutine()
    {
        gameObject.SetActive(true);
        
        // Play show sound
        PlaySound(showSound);
        
        // Start from zero scale
        transform.localScale = Vector3.zero;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        float elapsed = 0f;
        while (elapsed < showDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / showDuration);
            float curveValue = showCurve.Evaluate(t);
            
            transform.localScale = originalScale * curveValue;
            
            if (canvasGroup != null)
            {
                // Fade in faster than scale
                canvasGroup.alpha = Mathf.Clamp01(t * 2f);
            }
            
            yield return null;
        }

        // Ensure final state
        transform.localScale = originalScale;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        currentAnimation = null;
        OnShowComplete?.Invoke();
    }

    private IEnumerator HideAnimationCoroutine()
    {
        // Play hide sound
        PlaySound(hideSound);
        
        Vector3 startScale = transform.localScale;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;

        float elapsed = 0f;
        while (elapsed < hideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / hideDuration);
            float curveValue = hideCurve.Evaluate(t);
            
            transform.localScale = startScale * curveValue;
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = startAlpha * curveValue;
            }
            
            yield return null;
        }

        // Ensure final state
        transform.localScale = Vector3.zero;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        gameObject.SetActive(false);

        currentAnimation = null;
        OnHideComplete?.Invoke();
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Sets the original scale reference (useful if scale changes at runtime).
    /// </summary>
    public void SetOriginalScale(Vector3 scale)
    {
        originalScale = scale;
    }
}
