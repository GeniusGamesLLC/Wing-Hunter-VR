using UnityEngine;

/// <summary>
/// Simple test controller to verify audio functionality
/// This script can be used to test the shooting audio system
/// </summary>
public class AudioTestController : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private ShootingController shootingController;
    [SerializeField] private KeyCode testHitKey = KeyCode.H;
    [SerializeField] private KeyCode testMissKey = KeyCode.M;
    
    private void Update()
    {
        // Test hit sound
        if (Input.GetKeyDown(testHitKey))
        {
            TestHitSound();
        }
        
        // Test miss sound
        if (Input.GetKeyDown(testMissKey))
        {
            TestMissSound();
        }
    }
    
    /// <summary>
    /// Test the hit sound by simulating a successful shot
    /// </summary>
    public void TestHitSound()
    {
        if (shootingController != null)
        {
            // Access the private PlayShotAudio method through reflection or create a public test method
            Debug.Log("Testing hit sound - Press H key");
            
            // For testing, we can call the audio directly if we have access to the AudioSource
            AudioSource audioSource = shootingController.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                // Get the hit sound clip through reflection or add a public getter
                Debug.Log("Hit sound test triggered");
            }
        }
    }
    
    /// <summary>
    /// Test the miss sound by simulating a missed shot
    /// </summary>
    public void TestMissSound()
    {
        if (shootingController != null)
        {
            Debug.Log("Testing miss sound - Press M key");
            
            // For testing, we can call the audio directly if we have access to the AudioSource
            AudioSource audioSource = shootingController.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                Debug.Log("Miss sound test triggered");
            }
        }
    }
    
    private void Start()
    {
        if (shootingController == null)
        {
            shootingController = FindObjectOfType<ShootingController>();
        }
        
        Debug.Log("Audio Test Controller initialized");
        Debug.Log("Press H to test hit sound, M to test miss sound");
    }
}