using System;
using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles the debug unlock animation sequence including paper slide-in,
/// pin stick sound, success chime, and "Debug Mode Unlocked" message.
/// Requirements: 8.3 - Debug unlock confirmation with animation and audio
/// </summary>
public class DebugUnlockAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AnnouncementBoardController boardController;
    [SerializeField] private MenuPaper debugPaper;
    [SerializeField] private PaperAnimator debugPaperAnimator;
    
    [Header("Message Display")]
    [SerializeField] private GameObject unlockMessagePrefab;
    [SerializeField] private Transform messageSpawnPoint;
    [SerializeField] private float messageDuration = 3f;
    [SerializeField] private string unlockMessage = "Debug Mode Unlocked!";
    
    [Header("Animation Timing")]
    [SerializeField] private float slideInDelay = 0.1f;
    [SerializeField] private float pinSoundDelay = 0.3f;
    [SerializeField] private float successSoundDelay = 0.5f;
    [SerializeField] private float messageDelay = 0.4f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pinStickSound;
    [SerializeField] private AudioClip successChimeSound;
    [SerializeField] private AudioClip konamiSuccessSound;
    
    private GameObject currentMessage;
    private Coroutine unlockSequence;
    
    /// <summary>
    /// Event fired when the unlock animation sequence completes.
    /// </summary>
    public event Action OnUnlockSequenceComplete;

    private void Awake()
    {
        if (boardController == null)
        {
            boardController = GetComponent<AnnouncementBoardController>();
        }
        
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D sound
            }
        }
    }


    private void OnEnable()
    {
        if (boardController != null)
        {
            boardController.OnDebugUnlocked += OnDebugUnlocked;
        }
    }

    private void OnDisable()
    {
        if (boardController != null)
        {
            boardController.OnDebugUnlocked -= OnDebugUnlocked;
        }
        
        StopUnlockSequence();
    }

    private void OnDebugUnlocked()
    {
        PlayUnlockSequence();
    }

    /// <summary>
    /// Plays the full debug unlock animation sequence.
    /// </summary>
    public void PlayUnlockSequence()
    {
        StopUnlockSequence();
        unlockSequence = StartCoroutine(UnlockSequenceCoroutine());
    }

    /// <summary>
    /// Stops the current unlock sequence if running.
    /// </summary>
    public void StopUnlockSequence()
    {
        if (unlockSequence != null)
        {
            StopCoroutine(unlockSequence);
            unlockSequence = null;
        }
        
        if (currentMessage != null)
        {
            Destroy(currentMessage);
            currentMessage = null;
        }
    }

    private IEnumerator UnlockSequenceCoroutine()
    {
        // Play Konami success sound immediately
        PlaySound(konamiSuccessSound);
        
        // Wait before starting slide-in
        yield return new WaitForSeconds(slideInDelay);
        
        // Activate and animate the debug paper sliding in
        if (debugPaper != null)
        {
            debugPaper.gameObject.SetActive(true);
            
            if (debugPaperAnimator != null)
            {
                debugPaperAnimator.PlaySlideInAnimation();
            }
            else
            {
                // Try to get animator from paper
                var animator = debugPaper.GetComponent<PaperAnimator>();
                if (animator != null)
                {
                    animator.PlaySlideInAnimation();
                }
            }
        }
        
        // Play pin stick sound after delay
        yield return new WaitForSeconds(pinSoundDelay);
        PlaySound(pinStickSound);
        
        // Play success chime after delay
        yield return new WaitForSeconds(successSoundDelay - pinSoundDelay);
        PlaySound(successChimeSound);
        
        // Show unlock message after delay
        yield return new WaitForSeconds(messageDelay);
        ShowUnlockMessage();
        
        // Wait for message duration
        yield return new WaitForSeconds(messageDuration);
        
        // Hide message
        HideUnlockMessage();
        
        unlockSequence = null;
        OnUnlockSequenceComplete?.Invoke();
    }

    private void ShowUnlockMessage()
    {
        if (currentMessage != null)
        {
            Destroy(currentMessage);
        }

        if (unlockMessagePrefab != null && messageSpawnPoint != null)
        {
            currentMessage = Instantiate(unlockMessagePrefab, messageSpawnPoint);
            
            // Try to set the message text
            var textComponent = currentMessage.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = unlockMessage;
            }
            else
            {
                var textMesh = currentMessage.GetComponentInChildren<TextMeshPro>();
                if (textMesh != null)
                {
                    textMesh.text = unlockMessage;
                }
            }
        }
        else
        {
            // Create a simple floating text message
            CreateDefaultUnlockMessage();
        }
    }

    private void CreateDefaultUnlockMessage()
    {
        Transform spawnPoint = messageSpawnPoint != null ? messageSpawnPoint : transform;
        
        currentMessage = new GameObject("UnlockMessage");
        currentMessage.transform.SetParent(spawnPoint, false);
        currentMessage.transform.localPosition = new Vector3(0, 0.3f, -0.1f);
        
        // Add TextMeshPro component
        var textMesh = currentMessage.AddComponent<TextMeshPro>();
        textMesh.text = unlockMessage;
        textMesh.fontSize = 0.15f;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Green color
        textMesh.fontStyle = FontStyles.Bold;
        
        // Add a simple fade-in animation
        StartCoroutine(FadeInMessage(textMesh));
    }

    private IEnumerator FadeInMessage(TextMeshPro textMesh)
    {
        if (textMesh == null) yield break;
        
        Color startColor = textMesh.color;
        startColor.a = 0f;
        textMesh.color = startColor;
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            
            Color color = textMesh.color;
            color.a = t;
            textMesh.color = color;
            
            yield return null;
        }
        
        Color finalColor = textMesh.color;
        finalColor.a = 1f;
        textMesh.color = finalColor;
    }

    private void HideUnlockMessage()
    {
        if (currentMessage != null)
        {
            // Start fade out then destroy
            StartCoroutine(FadeOutAndDestroy(currentMessage));
            currentMessage = null;
        }
    }

    private IEnumerator FadeOutAndDestroy(GameObject messageObj)
    {
        if (messageObj == null) yield break;
        
        var textMesh = messageObj.GetComponentInChildren<TextMeshPro>();
        var textMeshUI = messageObj.GetComponentInChildren<TextMeshProUGUI>();
        
        float duration = 0.3f;
        float elapsed = 0f;
        
        while (elapsed < duration && messageObj != null)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Clamp01(elapsed / duration);
            
            if (textMesh != null)
            {
                Color color = textMesh.color;
                color.a = t;
                textMesh.color = color;
            }
            
            if (textMeshUI != null)
            {
                Color color = textMeshUI.color;
                color.a = t;
                textMeshUI.color = color;
            }
            
            yield return null;
        }
        
        if (messageObj != null)
        {
            Destroy(messageObj);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Sets the debug paper reference.
    /// </summary>
    public void SetDebugPaper(MenuPaper paper)
    {
        debugPaper = paper;
        if (paper != null)
        {
            debugPaperAnimator = paper.GetComponent<PaperAnimator>();
        }
    }
}
