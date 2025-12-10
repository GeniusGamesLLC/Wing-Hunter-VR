using UnityEngine;

/// <summary>
/// Utility script to set up audio clips for the ShootingController
/// This can be used to assign audio clips programmatically or as a reference for manual setup
/// </summary>
public class AudioSetup : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip missSound;
    
    [Header("Target Controller")]
    [SerializeField] private ShootingController shootingController;
    
    private void Start()
    {
        SetupAudioClips();
    }
    
    /// <summary>
    /// Configure the ShootingController with the assigned audio clips
    /// </summary>
    public void SetupAudioClips()
    {
        if (shootingController != null)
        {
            shootingController.SetAudioClips(hitSound, missSound);
            Debug.Log("Audio clips configured for ShootingController");
        }
        else
        {
            Debug.LogWarning("ShootingController reference not set in AudioSetup");
        }
    }
    
    /// <summary>
    /// Load default audio clips from Resources folder
    /// Note: Audio clips should be placed in a Resources folder to use this method
    /// Alternatively, assign clips directly in the Inspector
    /// </summary>
    [ContextMenu("Load Default Audio Clips")]
    public void LoadDefaultAudioClips()
    {
        // Note: For this to work, audio clips need to be in a Resources folder
        // For now, clips should be assigned manually in the Inspector
        
        Debug.Log("To use audio clips:");
        Debug.Log("1. Assign hitSound and missSound in the Inspector");
        Debug.Log("2. Recommended clips: Goal.wav for hits, ButtonClick.wav for misses");
        Debug.Log("3. These can be found in Assets/MRTemplateAssets/Audio/");
        
        // Apply the clips to the shooting controller if they're already assigned
        if (hitSound != null && missSound != null)
        {
            SetupAudioClips();
        }
    }
    
    /// <summary>
    /// Validate that all required components and clips are assigned
    /// </summary>
    public bool ValidateSetup()
    {
        bool isValid = true;
        
        if (shootingController == null)
        {
            Debug.LogError("ShootingController reference is missing");
            isValid = false;
        }
        
        if (hitSound == null)
        {
            Debug.LogWarning("Hit sound clip is not assigned");
            isValid = false;
        }
        
        if (missSound == null)
        {
            Debug.LogWarning("Miss sound clip is not assigned");
            isValid = false;
        }
        
        return isValid;
    }
}