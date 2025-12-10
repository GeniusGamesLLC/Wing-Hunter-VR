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
    /// Load recommended audio clips from Weapons of Choice pack
    /// </summary>
    [ContextMenu("Load Recommended Audio Clips")]
    public void LoadRecommendedAudioClips()
    {
        Debug.Log("Recommended Audio Clips for VR Duck Hunt:");
        Debug.Log("Hit Sound: Assets/Weapons of Choice FREE - Komposite Sound/BULLETS/Shell/Shell_Short_01_SFX.wav");
        Debug.Log("Miss Sound: Assets/Weapons of Choice FREE - Komposite Sound/BULLETS/Ricochets/Ricochet_01_SFX.wav");
        Debug.Log("");
        Debug.Log("To assign these clips:");
        Debug.Log("1. Select the ShootingController GameObject");
        Debug.Log("2. In the Inspector, find the ShootingController component");
        Debug.Log("3. Drag the recommended audio files to Hit Sound and Miss Sound fields");
        Debug.Log("4. The AudioSetup component will automatically configure them on Start");
        
        // Apply the clips to the shooting controller if they're already assigned
        if (hitSound != null && missSound != null)
        {
            SetupAudioClips();
        }
    }
    
    /// <summary>
    /// Load fallback audio clips from MRTemplateAssets folder
    /// </summary>
    [ContextMenu("Load Fallback Audio Clips")]
    public void LoadFallbackAudioClips()
    {
        Debug.Log("Fallback Audio Clips (if Weapons of Choice pack not available):");
        Debug.Log("Hit Sound: Assets/MRTemplateAssets/Audio/Goal.wav");
        Debug.Log("Miss Sound: Assets/MRTemplateAssets/Audio/ButtonClick.wav");
        Debug.Log("");
        Debug.Log("Note: The Weapons of Choice pack provides much better audio quality");
        
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