using System;
using UnityEngine;

/// <summary>
/// Controller for the Announcement Board - a static fixture displaying menu papers.
/// Manages debug unlock via Konami code.
/// </summary>
public class AnnouncementBoardController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PaperManager paperManager;
    [SerializeField] private KonamiCodeDetector konamiCodeDetector;
    [SerializeField] private MenuPaper settingsPaper;
    [SerializeField] private MenuPaper debugPaper;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;

    /// <summary>
    /// Whether the debug paper has been unlocked via Konami code.
    /// </summary>
    public bool IsDebugUnlocked { get; private set; }

    /// <summary>
    /// Event fired when the debug paper is unlocked via Konami code.
    /// </summary>
    public event Action OnDebugUnlocked;

    private void Awake()
    {
        if (paperManager == null)
        {
            paperManager = GetComponent<PaperManager>();
        }

        if (konamiCodeDetector == null)
        {
            konamiCodeDetector = GetComponent<KonamiCodeDetector>();
        }
    }

    private void OnEnable()
    {
        if (konamiCodeDetector != null)
        {
            konamiCodeDetector.OnKonamiCodeEntered += OnKonamiCodeEntered;
        }
    }

    private void OnDisable()
    {
        if (konamiCodeDetector != null)
        {
            konamiCodeDetector.OnKonamiCodeEntered -= OnKonamiCodeEntered;
        }
    }

    private void Start()
    {
        IsDebugUnlocked = false;

        // Refresh paper visibility to hide locked debug paper
        if (paperManager != null)
        {
            paperManager.RefreshPaperVisibility();
        }

        if (debugMode)
        {
            Debug.Log("[AnnouncementBoardController] Initialized - debug paper locked");
        }
    }

    private void OnKonamiCodeEntered()
    {
        if (!IsDebugUnlocked)
        {
            UnlockDebugPaper();
        }
    }

    /// <summary>
    /// Unlocks the debug paper.
    /// </summary>
    public void UnlockDebugPaper()
    {
        if (IsDebugUnlocked) return;

        IsDebugUnlocked = true;

        if (debugPaper != null)
        {
            debugPaper.Unlock();

            if (debugMode)
            {
                Debug.Log("[AnnouncementBoardController] Debug paper unlocked!");
            }
        }

        if (paperManager != null)
        {
            paperManager.RefreshPaperVisibility();
        }

        OnDebugUnlocked?.Invoke();
    }
}
