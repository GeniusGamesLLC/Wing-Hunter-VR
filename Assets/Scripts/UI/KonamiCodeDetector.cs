using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Input types for the Konami code sequence.
/// </summary>
public enum KonamiInput
{
    Up,
    Down,
    Left,
    Right,
    A,
    B
}

/// <summary>
/// Detects the VR Konami code input sequence using either thumbstick.
/// Sequence: Up, Up, Down, Down, Left, Right, Left, Right, B, A
/// </summary>
public class KonamiCodeDetector : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private float inputTimeout = 2f;
    [SerializeField] private float thumbstickThreshold = 0.7f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    /// <summary>
    /// Fired when the complete Konami code sequence is entered successfully.
    /// </summary>
    public event Action OnKonamiCodeEntered;
    
    // The classic Konami code sequence
    private readonly KonamiInput[] sequence = {
        KonamiInput.Up, KonamiInput.Up,
        KonamiInput.Down, KonamiInput.Down,
        KonamiInput.Left, KonamiInput.Right,
        KonamiInput.Left, KonamiInput.Right,
        KonamiInput.B, KonamiInput.A
    };
    
    private int currentIndex = 0;
    private float lastInputTime = 0f;
    
    // XR Input devices
    private UnityEngine.XR.InputDevice leftController;
    private UnityEngine.XR.InputDevice rightController;

    
    // Track previous thumbstick state to detect discrete inputs
    private Vector2 previousLeftThumbstick = Vector2.zero;
    private Vector2 previousRightThumbstick = Vector2.zero;
    private bool previousLeftAButton = false;
    private bool previousRightAButton = false;
    private bool previousLeftBButton = false;
    private bool previousRightBButton = false;
    
    private void Start()
    {
        InitializeXRDevices();
    }
    
    private void InitializeXRDevices()
    {
        var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftHandDevices);
        if (leftHandDevices.Count > 0)
        {
            leftController = leftHandDevices[0];
            if (debugMode)
                Debug.Log($"KonamiCodeDetector: Found left controller: {leftController.name}");
        }
        
        var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);
        if (rightHandDevices.Count > 0)
        {
            rightController = rightHandDevices[0];
            if (debugMode)
                Debug.Log($"KonamiCodeDetector: Found right controller: {rightController.name}");
        }
    }
    
    private void Update()
    {
        // Check for timeout - reset sequence if too much time has passed
        if (currentIndex > 0 && Time.time - lastInputTime > inputTimeout)
        {
            if (debugMode)
                Debug.Log("KonamiCodeDetector: Sequence timed out, resetting");
            ResetSequence();
        }
        
        // Try to find controllers if not valid
        if (!leftController.isValid || !rightController.isValid)
        {
            InitializeXRDevices();
        }
        
        // Check for inputs from either controller
        CheckThumbstickInput();
        CheckButtonInput();
    }

    
    private void CheckThumbstickInput()
    {
        // Check left thumbstick
        if (leftController.isValid)
        {
            Vector2 leftThumbstick = Vector2.zero;
            if (leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out leftThumbstick))
            {
                KonamiInput? input = GetThumbstickDirection(leftThumbstick, previousLeftThumbstick);
                if (input.HasValue)
                {
                    ProcessInput(input.Value);
                }
                previousLeftThumbstick = leftThumbstick;
            }
        }
        
        // Check right thumbstick
        if (rightController.isValid)
        {
            Vector2 rightThumbstick = Vector2.zero;
            if (rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out rightThumbstick))
            {
                KonamiInput? input = GetThumbstickDirection(rightThumbstick, previousRightThumbstick);
                if (input.HasValue)
                {
                    ProcessInput(input.Value);
                }
                previousRightThumbstick = rightThumbstick;
            }
        }
    }
    
    private KonamiInput? GetThumbstickDirection(Vector2 current, Vector2 previous)
    {
        // Only register input when crossing the threshold (edge detection)
        bool wasAboveThreshold = Mathf.Abs(previous.x) >= thumbstickThreshold || 
                                  Mathf.Abs(previous.y) >= thumbstickThreshold;
        
        // Check vertical (Up/Down) - prioritize vertical over horizontal
        if (current.y >= thumbstickThreshold && previous.y < thumbstickThreshold)
        {
            return KonamiInput.Up;
        }
        if (current.y <= -thumbstickThreshold && previous.y > -thumbstickThreshold)
        {
            return KonamiInput.Down;
        }
        
        // Check horizontal (Left/Right)
        if (current.x >= thumbstickThreshold && previous.x < thumbstickThreshold)
        {
            return KonamiInput.Right;
        }
        if (current.x <= -thumbstickThreshold && previous.x > -thumbstickThreshold)
        {
            return KonamiInput.Left;
        }
        
        return null;
    }

    
    private void CheckButtonInput()
    {
        // Check A button (primaryButton) from either controller
        bool leftAPressed = false;
        bool rightAPressed = false;
        
        if (leftController.isValid)
        {
            leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out leftAPressed);
        }
        if (rightController.isValid)
        {
            rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out rightAPressed);
        }
        
        // Detect A button press (edge detection)
        if ((leftAPressed && !previousLeftAButton) || (rightAPressed && !previousRightAButton))
        {
            ProcessInput(KonamiInput.A);
        }
        previousLeftAButton = leftAPressed;
        previousRightAButton = rightAPressed;
        
        // Check B button (secondaryButton) from either controller
        bool leftBPressed = false;
        bool rightBPressed = false;
        
        if (leftController.isValid)
        {
            leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out leftBPressed);
        }
        if (rightController.isValid)
        {
            rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out rightBPressed);
        }
        
        // Detect B button press (edge detection)
        if ((leftBPressed && !previousLeftBButton) || (rightBPressed && !previousRightBButton))
        {
            ProcessInput(KonamiInput.B);
        }
        previousLeftBButton = leftBPressed;
        previousRightBButton = rightBPressed;
    }
    
    private void ProcessInput(KonamiInput input)
    {
        if (debugMode)
            Debug.Log($"KonamiCodeDetector: Received input {input}, expecting {sequence[currentIndex]} at index {currentIndex}");
        
        // Check if this input matches the expected input in the sequence
        if (input == sequence[currentIndex])
        {
            currentIndex++;
            lastInputTime = Time.time;
            
            if (debugMode)
                Debug.Log($"KonamiCodeDetector: Correct! Progress: {currentIndex}/{sequence.Length}");
            
            // Check if sequence is complete
            if (currentIndex >= sequence.Length)
            {
                if (debugMode)
                    Debug.Log("KonamiCodeDetector: Konami code entered successfully!");
                
                OnKonamiCodeEntered?.Invoke();
                ResetSequence();
            }
        }
        else
        {
            // Wrong input - reset sequence
            if (debugMode)
                Debug.Log($"KonamiCodeDetector: Wrong input! Expected {sequence[currentIndex]}, got {input}. Resetting.");
            
            ResetSequence();
            
            // Check if this wrong input could be the start of a new sequence
            if (input == sequence[0])
            {
                currentIndex = 1;
                lastInputTime = Time.time;
            }
        }
    }

    
    /// <summary>
    /// Resets the sequence progress to the beginning.
    /// </summary>
    public void ResetSequence()
    {
        currentIndex = 0;
        lastInputTime = 0f;
    }
    
    /// <summary>
    /// Gets the current progress through the sequence (0 to sequence length).
    /// </summary>
    public int GetCurrentProgress()
    {
        return currentIndex;
    }
    
    /// <summary>
    /// Gets the total length of the Konami code sequence.
    /// </summary>
    public int GetSequenceLength()
    {
        return sequence.Length;
    }
    
    /// <summary>
    /// Gets the input timeout duration in seconds.
    /// </summary>
    public float GetInputTimeout()
    {
        return inputTimeout;
    }
    
    /// <summary>
    /// Gets the thumbstick threshold for directional input detection.
    /// </summary>
    public float GetThumbstickThreshold()
    {
        return thumbstickThreshold;
    }
    
    // ============================================
    // Test Support Methods (for property-based testing)
    // ============================================
    
    /// <summary>
    /// Simulates an input for testing purposes.
    /// </summary>
    /// <param name="input">The input to simulate.</param>
    public void SimulateInput(KonamiInput input)
    {
        ProcessInput(input);
    }
    
    /// <summary>
    /// Simulates a sequence of inputs for testing purposes.
    /// </summary>
    /// <param name="inputs">The inputs to simulate in order.</param>
    public void SimulateInputSequence(KonamiInput[] inputs)
    {
        foreach (var input in inputs)
        {
            ProcessInput(input);
        }
    }
    
    /// <summary>
    /// Gets the expected sequence for testing purposes.
    /// </summary>
    public KonamiInput[] GetExpectedSequence()
    {
        return (KonamiInput[])sequence.Clone();
    }
    
    /// <summary>
    /// Simulates a timeout for testing purposes.
    /// </summary>
    public void SimulateTimeout()
    {
        if (currentIndex > 0)
        {
            lastInputTime = Time.time - inputTimeout - 1f;
        }
    }
}
