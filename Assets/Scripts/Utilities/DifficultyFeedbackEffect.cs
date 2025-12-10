using UnityEngine;

/// <summary>
/// Provides visual particle effects for difficulty level changes
/// </summary>
public class DifficultyFeedbackEffect : MonoBehaviour
{
    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem difficultyParticles;
    
    [Header("Effect Configuration")]
    [SerializeField] private Color[] difficultyColors = {
        Color.green,    // Level 1
        Color.yellow,   // Level 2
        Color.orange,   // Level 3
        Color.red,      // Level 4
        Color.magenta   // Level 5+
    };
    
    [SerializeField] private float effectDuration = 1.5f;
    [SerializeField] private int particleCount = 50;
    
    private void Awake()
    {
        // Create particle system if not assigned
        if (difficultyParticles == null)
        {
            CreateParticleSystem();
        }
    }
    
    /// <summary>
    /// Triggers the difficulty change effect
    /// </summary>
    /// <param name="difficultyLevel">The new difficulty level</param>
    public void TriggerDifficultyEffect(int difficultyLevel)
    {
        if (difficultyParticles == null)
        {
            Debug.LogWarning("DifficultyFeedbackEffect: No particle system available");
            return;
        }
        
        // Configure particle system for this difficulty level
        ConfigureParticleSystem(difficultyLevel);
        
        // Play the effect
        difficultyParticles.Play();
        
        Debug.Log($"DifficultyFeedbackEffect: Triggered effect for difficulty level {difficultyLevel}");
    }
    
    /// <summary>
    /// Creates a basic particle system if none is assigned
    /// </summary>
    private void CreateParticleSystem()
    {
        GameObject particleObject = new GameObject("DifficultyParticles");
        particleObject.transform.SetParent(transform);
        particleObject.transform.localPosition = Vector3.zero;
        
        difficultyParticles = particleObject.AddComponent<ParticleSystem>();
        
        // Configure basic particle system settings
        var main = difficultyParticles.main;
        main.startLifetime = effectDuration;
        main.startSpeed = 2f;
        main.maxParticles = particleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        
        var emission = difficultyParticles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0; // We'll use bursts instead
        
        var burst = new ParticleSystem.Burst(0.0f, particleCount);
        emission.SetBursts(new ParticleSystem.Burst[] { burst });
        
        var shape = difficultyParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f;
        
        Debug.Log("DifficultyFeedbackEffect: Created default particle system");
    }
    
    /// <summary>
    /// Configures the particle system based on difficulty level
    /// </summary>
    /// <param name="difficultyLevel">The difficulty level to configure for</param>
    private void ConfigureParticleSystem(int difficultyLevel)
    {
        var main = difficultyParticles.main;
        
        // Set color based on difficulty level
        int colorIndex = Mathf.Clamp(difficultyLevel - 1, 0, difficultyColors.Length - 1);
        main.startColor = difficultyColors[colorIndex];
        
        // Increase intensity with higher difficulty
        main.startSize = 0.1f + (difficultyLevel * 0.05f);
        main.startSpeed = 1f + (difficultyLevel * 0.5f);
        
        // Adjust particle count based on difficulty
        main.maxParticles = particleCount + (difficultyLevel * 10);
        
        var emission = difficultyParticles.emission;
        var bursts = new ParticleSystem.Burst[1];
        bursts[0] = new ParticleSystem.Burst(0.0f, main.maxParticles);
        emission.SetBursts(bursts);
    }
    
    /// <summary>
    /// Sets custom colors for difficulty levels
    /// </summary>
    /// <param name="colors">Array of colors for each difficulty level</param>
    public void SetDifficultyColors(Color[] colors)
    {
        if (colors != null && colors.Length > 0)
        {
            difficultyColors = colors;
        }
    }
    
    /// <summary>
    /// Sets the particle system to use for effects
    /// </summary>
    /// <param name="particles">The particle system to use</param>
    public void SetParticleSystem(ParticleSystem particles)
    {
        difficultyParticles = particles;
    }
    
    #if UNITY_EDITOR
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnValidate()
    {
        // Ensure effect duration is positive
        if (effectDuration <= 0)
        {
            effectDuration = 1.5f;
        }
        
        // Ensure particle count is positive
        if (particleCount <= 0)
        {
            particleCount = 50;
        }
        
        // Ensure we have at least one color
        if (difficultyColors == null || difficultyColors.Length == 0)
        {
            difficultyColors = new Color[] { Color.white };
        }
    }
    #endif
}