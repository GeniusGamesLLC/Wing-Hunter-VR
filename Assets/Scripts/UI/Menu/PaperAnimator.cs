using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles animations for menu papers including focus/unfocus effects.
/// Requirements: 2.2, 2.3 - Paper focus with visual highlighting and dimming
/// Requirements: 10.2, 10.4 - Compact/expanded state transitions
/// Requirements: 1.3, 10.2 - Paper-to-player movement on focus
/// </summary>
public class PaperAnimator : MonoBehaviour
{
    [Header("Focus Animation (Legacy)")]
    [SerializeField] private float focusScale = 1.1f;
    [SerializeField] private float focusZOffset = -0.05f;
    [SerializeField] private float focusDuration = 0.25f;
    [SerializeField] private AnimationCurve focusCurve;
    
    [Header("Paper-to-Player Animation")]
    [Tooltip("Duration of animation when paper moves to player")]
    [SerializeField] private float moveToPlayerDuration = 0.35f;
    [Tooltip("Duration of animation when paper returns to board")]
    [SerializeField] private float returnToBoardDuration = 0.3f;
    [Tooltip("Curve for paper-to-player animation")]
    [SerializeField] private AnimationCurve moveToPlayerCurve;
    
    [Header("Unfocus Animation (Legacy)")]
    [SerializeField] private float unfocusedAlpha = 0.6f;
    [SerializeField] private float unfocusDuration = 0.2f;
    
    [Header("Focus Animation (Normal to Focused)")]
    [Tooltip("Duration of focus animation in seconds")]
    [SerializeField] private float focusAnimDuration = 0.3f;
    [Tooltip("Ease-out curve for focus animation")]
    [SerializeField] private AnimationCurve focusAnimCurve;
    
    [Header("Unfocus Animation (Focused to Normal)")]
    [Tooltip("Duration of unfocus animation in seconds")]
    [SerializeField] private float unfocusAnimDuration = 0.2f;
    [Tooltip("Ease-in curve for unfocus animation")]
    [SerializeField] private AnimationCurve unfocusAnimCurve;
    
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
    private Quaternion originalRotation;
    private Vector3 homePositionWorld;
    private Quaternion homeRotationWorld;
    private bool hasStoredHomePosition;
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
    
    /// <summary>
    /// Event fired when focus animation (new style) completes.
    /// </summary>
    public event Action OnFocusAnimComplete;
    
    /// <summary>
    /// Event fired when unfocus animation (new style) completes.
    /// </summary>
    public event Action OnUnfocusAnimComplete;
    
    /// <summary>
    /// Event fired when paper-to-player animation completes.
    /// </summary>
    public event Action OnMoveToPlayerComplete;
    
    /// <summary>
    /// Event fired when paper-return-to-board animation completes.
    /// </summary>
    public event Action OnReturnToBoardComplete;


    private void Awake()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
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
                    // Check if material has _Color property (skip TextMeshPro materials)
                    if (meshRenderers[i].material.HasProperty("_Color"))
                    {
                        originalColors[i] = meshRenderers[i].material.color;
                    }
                    else
                    {
                        originalColors[i] = Color.white;
                    }
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
        
        // Create ease-out curve for focus animation (fast start, slow end)
        if (focusAnimCurve == null || focusAnimCurve.length == 0)
        {
            focusAnimCurve = CreateEaseOutCurve();
        }
        
        // Create ease-in curve for unfocus animation (slow start, fast end)
        if (unfocusAnimCurve == null || unfocusAnimCurve.length == 0)
        {
            unfocusAnimCurve = CreateEaseInCurve();
        }
        
        // Create curve for paper-to-player animation (ease-out with slight overshoot)
        if (moveToPlayerCurve == null || moveToPlayerCurve.length == 0)
        {
            moveToPlayerCurve = CreateMoveToPlayerCurve();
        }
    }
    
    private AnimationCurve CreateMoveToPlayerCurve()
    {
        // Ease-out with slight overshoot for natural feel
        var curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0f, 0f, 0f, 2f));
        curve.AddKey(new Keyframe(0.8f, 1.02f, 0f, 0f));
        curve.AddKey(new Keyframe(1f, 1f, 0f, 0f));
        return curve;
    }
    
    private AnimationCurve CreateEaseOutCurve()
    {
        // Ease-out: fast start, slow end
        var curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0f, 0f, 0f, 2f));
        curve.AddKey(new Keyframe(1f, 1f, 0f, 0f));
        return curve;
    }
    
    private AnimationCurve CreateEaseInCurve()
    {
        // Ease-in: slow start, fast end
        var curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0f, 0f, 0f, 0f));
        curve.AddKey(new Keyframe(1f, 1f, 2f, 0f));
        return curve;
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
    /// Plays the focus animation - scales up slightly, moves forward.
    /// Requirements: 10.2 - Animate to larger focused size (0.3s ease-out)
    /// </summary>
    /// <param name="targetScale">The target focused scale.</param>
    /// <param name="targetZOffset">The target Z offset (negative moves toward user).</param>
    public void PlayFocusAnimation(Vector3 targetScale, float targetZOffset)
    {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(FocusAnimCoroutine(targetScale, targetZOffset));
    }
    
    /// <summary>
    /// Plays the unfocus animation - returns to normal size, moves back.
    /// Requirements: 10.4 - Animate back to normal unfocused state (0.2s ease-in)
    /// Papers stay at readable size (1.0 scale), no dimming.
    /// </summary>
    /// <param name="targetScale">The target unfocused scale (normal size).</param>
    public void PlayUnfocusAnimation(Vector3 targetScale)
    {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(UnfocusAnimCoroutine(targetScale));
    }
    
    /// <summary>
    /// Immediately sets the focused state without animation.
    /// </summary>
    /// <param name="targetScale">The target focused scale.</param>
    /// <param name="targetZOffset">The target Z offset.</param>
    public void SetFocusedStateImmediate(Vector3 targetScale, float targetZOffset)
    {
        StopCurrentAnimation();
        transform.localScale = targetScale;
        Vector3 pos = originalPosition;
        pos.z += targetZOffset;
        transform.localPosition = pos;
        SetAlpha(1f);
    }
    
    /// <summary>
    /// Immediately sets the unfocused state without animation.
    /// Papers stay at normal readable size, no dimming.
    /// </summary>
    /// <param name="targetScale">The target unfocused scale (normal size).</param>
    public void SetUnfocusedStateImmediate(Vector3 targetScale)
    {
        StopCurrentAnimation();
        transform.localScale = targetScale;
        // Use same Z offset as label tab (-0.005) so paper sits at same depth
        transform.localPosition = new Vector3(originalPosition.x, originalPosition.y, -0.005f);
        SetAlpha(1f);
    }

    /// <summary>
    /// Immediately sets the focused state without animation (legacy method).
    /// </summary>
    public void SetFocusedImmediate()
    {
        StopCurrentAnimation();
        transform.localScale = originalScale * focusScale;
        transform.localPosition = originalPosition + new Vector3(0, 0, focusZOffset);
        SetAlpha(1f);
        // No dimming - papers maintain full brightness
    }

    /// <summary>
    /// Immediately sets the unfocused state without animation (legacy method).
    /// Papers stay at normal size, no dimming.
    /// </summary>
    public void SetUnfocusedImmediate()
    {
        StopCurrentAnimation();
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        SetAlpha(1f);
        // No dimming - papers maintain full brightness
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
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        
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
            // No dimming - papers maintain full brightness
            
            yield return null;
        }

        // Ensure final state
        transform.localScale = targetScale;
        transform.localPosition = targetPos;
        SetAlpha(1f);

        currentAnimation = null;
        OnFocusComplete?.Invoke();
    }

    private IEnumerator UnfocusAnimationCoroutine()
    {
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = originalScale;
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = originalPosition;
        
        float elapsed = 0f;
        while (elapsed < unfocusDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / unfocusDuration);
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            // No dimming - papers maintain full brightness
            
            yield return null;
        }

        // Ensure final state - papers stay at normal size, no dimming
        transform.localScale = targetScale;
        transform.localPosition = targetPos;
        SetAlpha(1f);

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
    
    private IEnumerator FocusAnimCoroutine(Vector3 targetScale, float targetZOffset)
    {
        PlaySound(focusSound);
        
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = originalPosition + new Vector3(0, 0, targetZOffset);
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        
        float elapsed = 0f;
        while (elapsed < focusAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / focusAnimDuration);
            float curveValue = focusAnimCurve.Evaluate(t);
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, curveValue);
            
            float alpha = Mathf.Lerp(startAlpha, 1f, curveValue);
            SetAlpha(alpha);
            // No dimming - papers maintain full brightness
            
            yield return null;
        }
        
        // Ensure final state
        transform.localScale = targetScale;
        transform.localPosition = targetPos;
        SetAlpha(1f);
        
        currentAnimation = null;
        OnFocusAnimComplete?.Invoke();
    }
    
    private IEnumerator UnfocusAnimCoroutine(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.localPosition;
        // Use same Z offset as label tab (-0.005) so paper sits at same depth
        Vector3 targetPos = new Vector3(originalPosition.x, originalPosition.y, -0.005f);
        
        float elapsed = 0f;
        while (elapsed < unfocusAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / unfocusAnimDuration);
            float curveValue = unfocusAnimCurve.Evaluate(t);
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            transform.localPosition = Vector3.Lerp(startPos, targetPos, curveValue);
            // No dimming - papers maintain full brightness
            
            yield return null;
        }
        
        // Ensure final state - papers stay at normal readable size, no dimming
        transform.localScale = targetScale;
        transform.localPosition = targetPos;
        SetAlpha(1f);
        
        currentAnimation = null;
        OnUnfocusAnimComplete?.Invoke();
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
                // Skip materials without _Color property (like TextMeshPro)
                if (!meshRenderers[i].material.HasProperty("_Color")) continue;
                
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
            // Skip materials without _Color property (like TextMeshPro)
            if (!meshRenderers[0].material.HasProperty("_Color")) return 1f;
            
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
    
    /// <summary>
    /// Stores the current world position and rotation as the "home" position on the board.
    /// Call this before animating the paper to the player.
    /// </summary>
    public void StoreHomePosition()
    {
        homePositionWorld = transform.position;
        homeRotationWorld = transform.rotation;
        hasStoredHomePosition = true;
    }
    
    /// <summary>
    /// Gets the stored home position in world space.
    /// </summary>
    public Vector3 HomePositionWorld => homePositionWorld;
    
    /// <summary>
    /// Gets the stored home rotation in world space.
    /// </summary>
    public Quaternion HomeRotationWorld => homeRotationWorld;
    
    /// <summary>
    /// Whether a home position has been stored.
    /// </summary>
    public bool HasStoredHomePosition => hasStoredHomePosition;
    
    /// <summary>
    /// Animates the paper from its current position to in front of the player.
    /// Requirements: 1.3, 10.2 - Paper moves to player on focus
    /// </summary>
    /// <param name="targetPosition">World position in front of player</param>
    /// <param name="targetRotation">World rotation facing the player</param>
    /// <param name="targetScale">Target scale when focused</param>
    public void PlayMoveToPlayerAnimation(Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale)
    {
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(MoveToPlayerCoroutine(targetPosition, targetRotation, targetScale));
    }
    
    /// <summary>
    /// Animates the paper from its current position back to its home position on the board.
    /// Requirements: 10.4 - Paper returns to board on unfocus
    /// </summary>
    /// <param name="targetScale">Target scale when unfocused (normal size)</param>
    public void PlayReturnToBoardAnimation(Vector3 targetScale)
    {
        if (!hasStoredHomePosition)
        {
            Debug.LogWarning("[PaperAnimator] No home position stored, cannot return to board");
            return;
        }
        
        StopCurrentAnimation();
        currentAnimation = StartCoroutine(ReturnToBoardCoroutine(targetScale));
    }
    
    private IEnumerator MoveToPlayerCoroutine(Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale)
    {
        PlaySound(focusSound);
        
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 startScale = transform.localScale;
        
        float elapsed = 0f;
        while (elapsed < moveToPlayerDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveToPlayerDuration);
            float curveValue = moveToPlayerCurve.Evaluate(t);
            
            transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, curveValue);
            transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            
            yield return null;
        }
        
        // Ensure final state
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        transform.localScale = targetScale;
        
        currentAnimation = null;
        OnMoveToPlayerComplete?.Invoke();
    }
    
    private IEnumerator ReturnToBoardCoroutine(Vector3 targetScale)
    {
        PlaySound(paperRustleSound);
        
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 startScale = transform.localScale;
        
        float elapsed = 0f;
        while (elapsed < returnToBoardDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / returnToBoardDuration);
            // Use ease-in curve for return (slow start, fast end)
            float curveValue = unfocusAnimCurve.Evaluate(t);
            
            transform.position = Vector3.Lerp(startPosition, homePositionWorld, curveValue);
            transform.rotation = Quaternion.Slerp(startRotation, homeRotationWorld, curveValue);
            transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            
            yield return null;
        }
        
        // Ensure final state
        transform.position = homePositionWorld;
        transform.rotation = homeRotationWorld;
        transform.localScale = targetScale;
        
        currentAnimation = null;
        OnReturnToBoardComplete?.Invoke();
    }
    
    /// <summary>
    /// Immediately sets the paper to the player-facing position without animation.
    /// </summary>
    public void SetPlayerPositionImmediate(Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale)
    {
        StopCurrentAnimation();
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        transform.localScale = targetScale;
    }
    
    /// <summary>
    /// Immediately returns the paper to its home position without animation.
    /// </summary>
    public void SetHomePositionImmediate(Vector3 targetScale)
    {
        if (!hasStoredHomePosition) return;
        
        StopCurrentAnimation();
        transform.position = homePositionWorld;
        transform.rotation = homeRotationWorld;
        transform.localScale = targetScale;
    }
}
