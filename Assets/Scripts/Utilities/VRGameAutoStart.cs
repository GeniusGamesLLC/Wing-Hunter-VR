using UnityEngine;

/// <summary>
/// Automatically starts the VR Duck Hunt game after a short delay.
/// Attach this to any GameObject in the scene.
/// </summary>
public class VRGameAutoStart : MonoBehaviour
{
    [Header("Auto Start Settings")]
    [Tooltip("Whether to automatically start the game on scene load")]
    public bool autoStartGame = true;
    
    [Tooltip("Delay in seconds before starting the game")]
    public float startDelay = 2f;
    
    void Start()
    {
        if (autoStartGame)
        {
            Invoke(nameof(StartGame), startDelay);
        }
    }
    
    void StartGame()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.StartGame();
            Debug.Log("VRGameAutoStart: Game started!");
        }
        else
        {
            Debug.LogWarning("VRGameAutoStart: GameManager not found!");
        }
    }
}
