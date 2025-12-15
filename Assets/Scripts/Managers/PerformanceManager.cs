using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages performance monitoring and optimization for VR gameplay.
/// Targets 72 FPS for Meta Quest devices with dynamic adjustments.
/// </summary>
public class PerformanceManager : MonoBehaviour
{
    [Header("Performance Targets")]
    [Tooltip("Target frame rate for VR (72 FPS for Quest)")]
    [SerializeField] private int targetFrameRate = 72;
    
    [Tooltip("Minimum acceptable frame rate before optimizations kick in")]
    [SerializeField] private float minAcceptableFrameRate = 50f;
    
    [Tooltip("Frame rate threshold to restore quality")]
    [SerializeField] private float qualityRestoreThreshold = 60f;
    
    [Header("Auto Optimization")]
    [Tooltip("Enable automatic performance optimization (disable for manual control)")]
    [SerializeField] private bool enableAutoOptimization = false;
    
    [Header("Monitoring Settings")]
    [Tooltip("How often to sample frame rate (in seconds)")]
    [SerializeField] private float sampleInterval = 0.5f;
    
    [Tooltip("Number of samples to average for FPS calculation")]
    [SerializeField] private int sampleCount = 10;
    
    [Header("Duck Optimization")]
    [Tooltip("Maximum concurrent ducks allowed")]
    [SerializeField] private int maxConcurrentDucks = 15;
    
    [Tooltip("Minimum concurrent ducks (won't go below this)")]
    [SerializeField] private int minConcurrentDucks = 3;
    
    [Tooltip("Current duck limit (adjusted dynamically)")]
    [SerializeField] private int currentDuckLimit = 10;
    
    [Header("Spawn Rate Optimization")]
    [Tooltip("Minimum spawn interval multiplier")]
    [SerializeField] private float minSpawnMultiplier = 0.5f;
    
    [Tooltip("Maximum spawn interval multiplier (slower spawning)")]
    [SerializeField] private float maxSpawnMultiplier = 2.0f;
    
    [Tooltip("Current spawn rate multiplier")]
    [SerializeField] private float currentSpawnMultiplier = 1.0f;
    
    [Header("Particle Optimization")]
    [Tooltip("Enable particle effect pooling")]
    [SerializeField] private bool useParticlePooling = true;
    
    [Tooltip("Maximum particle systems active at once")]
    [SerializeField] private int maxActiveParticles = 5;
    
    [Header("LOD Settings")]
    [Tooltip("Enable LOD for duck models")]
    [SerializeField] private bool enableLOD = false;
    
    [Tooltip("LOD bias (lower = more aggressive LOD)")]
    [Range(0.1f, 2.0f)]
    [SerializeField] private float lodBias = 1.0f;
    
    [Header("Debug Display")]
    [Tooltip("Show FPS counter in VR")]
    [SerializeField] private bool showFPSCounter = false;
    
    [Tooltip("Show performance stats")]
    [SerializeField] private bool showPerformanceStats = false;
    
    // Performance tracking
    private Queue<float> frameTimes = new Queue<float>();
    private float lastSampleTime;
    private float currentFPS;
    private float averageFPS;
    private int frameCount;
    private float frameTimeAccumulator;
    
    // References
    private SpawnManager spawnManager;
    private DuckPool duckPool;
    
    // Optimization state
    private int optimizationLevel = 0; // 0 = normal, higher = more aggressive
    private float lastOptimizationTime;
    private const float OPTIMIZATION_COOLDOWN = 2.0f;
    
    // Particle pool
    private Queue<ParticleSystem> particlePool = new Queue<ParticleSystem>();
    private List<ParticleSystem> activeParticles = new List<ParticleSystem>();
    
    // Singleton pattern for easy access
    public static PerformanceManager Instance { get; private set; }
    
    // Public properties
    public float CurrentFPS => currentFPS;
    public float AverageFPS => averageFPS;
    public int CurrentDuckLimit => currentDuckLimit;
    public float CurrentSpawnMultiplier => currentSpawnMultiplier;
    public int OptimizationLevel => optimizationLevel;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Set target frame rate
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0; // Disable VSync for VR
        
        // Apply LOD bias
        if (enableLOD)
        {
            QualitySettings.lodBias = lodBias;
        }
    }
    
    private void Start()
    {
        // Find references
        spawnManager = FindObjectOfType<SpawnManager>();
        duckPool = FindObjectOfType<DuckPool>();
        
        // Try to load settings from game config
        LoadSettingsFromConfig();
        
        lastSampleTime = Time.realtimeSinceStartup;
        lastOptimizationTime = Time.realtimeSinceStartup;
        
        Debug.Log($"PerformanceManager: Initialized with target {targetFrameRate} FPS, max ducks: {maxConcurrentDucks}");
    }
    
    /// <summary>
    /// Load performance settings from DuckHuntConfig if available
    /// </summary>
    private void LoadSettingsFromConfig()
    {
        // Try to find GameManager and get config
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            DuckHuntConfig config = gameManager.GetGameConfig();
            if (config != null)
            {
                maxConcurrentDucks = config.MaxConcurrentDucks;
                currentDuckLimit = maxConcurrentDucks;
                targetFrameRate = config.TargetFrameRate;
                Application.targetFrameRate = targetFrameRate;
                
                Debug.Log($"PerformanceManager: Loaded settings from config - Max ducks: {maxConcurrentDucks}, Target FPS: {targetFrameRate}");
            }
        }
    }

    private void Update()
    {
        // Track frame time
        TrackFrameTime();
        
        // Sample FPS at intervals
        if (Time.realtimeSinceStartup - lastSampleTime >= sampleInterval)
        {
            CalculateFPS();
            lastSampleTime = Time.realtimeSinceStartup;
            
            // Check if optimization is needed
            CheckPerformance();
        }
        
        // Clean up finished particles
        if (useParticlePooling)
        {
            CleanupParticles();
        }
    }
    
    /// <summary>
    /// Track frame time for FPS calculation
    /// </summary>
    private void TrackFrameTime()
    {
        frameCount++;
        frameTimeAccumulator += Time.unscaledDeltaTime;
    }
    
    /// <summary>
    /// Calculate current and average FPS
    /// </summary>
    private void CalculateFPS()
    {
        if (frameCount > 0)
        {
            currentFPS = frameCount / frameTimeAccumulator;
            
            // Add to sample queue
            frameTimes.Enqueue(currentFPS);
            while (frameTimes.Count > sampleCount)
            {
                frameTimes.Dequeue();
            }
            
            // Calculate average
            float sum = 0f;
            foreach (float fps in frameTimes)
            {
                sum += fps;
            }
            averageFPS = sum / frameTimes.Count;
            
            // Reset counters
            frameCount = 0;
            frameTimeAccumulator = 0f;
        }
    }
    
    /// <summary>
    /// Check performance and apply optimizations if needed
    /// </summary>
    private void CheckPerformance()
    {
        // Skip if auto optimization is disabled
        if (!enableAutoOptimization)
        {
            return;
        }
        
        // Don't optimize too frequently
        if (Time.realtimeSinceStartup - lastOptimizationTime < OPTIMIZATION_COOLDOWN)
        {
            return;
        }
        
        if (averageFPS < minAcceptableFrameRate)
        {
            // Performance is below acceptable - increase optimization
            IncreaseOptimization();
            lastOptimizationTime = Time.realtimeSinceStartup;
        }
        else if (averageFPS > qualityRestoreThreshold && optimizationLevel > 0)
        {
            // Performance is good - try to restore quality
            DecreaseOptimization();
            lastOptimizationTime = Time.realtimeSinceStartup;
        }
    }
    
    /// <summary>
    /// Increase optimization level to improve performance
    /// </summary>
    private void IncreaseOptimization()
    {
        optimizationLevel++;
        
        switch (optimizationLevel)
        {
            case 1:
                // Level 1: Reduce duck limit slightly
                currentDuckLimit = Mathf.Max(minConcurrentDucks, currentDuckLimit - 2);
                Debug.Log($"PerformanceManager: Optimization Level 1 - Duck limit: {currentDuckLimit}");
                break;
                
            case 2:
                // Level 2: Slow down spawn rate
                currentSpawnMultiplier = Mathf.Min(maxSpawnMultiplier, currentSpawnMultiplier + 0.25f);
                Debug.Log($"PerformanceManager: Optimization Level 2 - Spawn multiplier: {currentSpawnMultiplier}");
                break;
                
            case 3:
                // Level 3: Further reduce ducks and increase LOD aggressiveness
                currentDuckLimit = Mathf.Max(minConcurrentDucks, currentDuckLimit - 2);
                if (enableLOD)
                {
                    QualitySettings.lodBias = Mathf.Max(0.3f, lodBias - 0.3f);
                }
                Debug.Log($"PerformanceManager: Optimization Level 3 - Duck limit: {currentDuckLimit}, LOD bias: {QualitySettings.lodBias}");
                break;
                
            case 4:
                // Level 4: Reduce particle effects
                maxActiveParticles = Mathf.Max(2, maxActiveParticles - 2);
                Debug.Log($"PerformanceManager: Optimization Level 4 - Max particles: {maxActiveParticles}");
                break;
                
            default:
                // Maximum optimization reached
                currentDuckLimit = minConcurrentDucks;
                currentSpawnMultiplier = maxSpawnMultiplier;
                Debug.LogWarning("PerformanceManager: Maximum optimization level reached");
                break;
        }
        
        ApplyOptimizations();
    }
    
    /// <summary>
    /// Decrease optimization level to restore quality
    /// </summary>
    private void DecreaseOptimization()
    {
        if (optimizationLevel <= 0) return;
        
        optimizationLevel--;
        
        switch (optimizationLevel)
        {
            case 0:
                // Restore to defaults
                currentDuckLimit = maxConcurrentDucks;
                currentSpawnMultiplier = 1.0f;
                maxActiveParticles = 5;
                if (enableLOD)
                {
                    QualitySettings.lodBias = lodBias;
                }
                Debug.Log("PerformanceManager: Restored to default settings");
                break;
                
            case 1:
                currentSpawnMultiplier = Mathf.Max(1.0f, currentSpawnMultiplier - 0.25f);
                Debug.Log($"PerformanceManager: Restored spawn multiplier: {currentSpawnMultiplier}");
                break;
                
            case 2:
                currentDuckLimit = Mathf.Min(maxConcurrentDucks, currentDuckLimit + 2);
                Debug.Log($"PerformanceManager: Restored duck limit: {currentDuckLimit}");
                break;
                
            case 3:
                if (enableLOD)
                {
                    QualitySettings.lodBias = Mathf.Min(lodBias, QualitySettings.lodBias + 0.3f);
                }
                Debug.Log($"PerformanceManager: Restored LOD bias: {QualitySettings.lodBias}");
                break;
        }
        
        ApplyOptimizations();
    }
    
    /// <summary>
    /// Apply current optimization settings to game systems
    /// </summary>
    private void ApplyOptimizations()
    {
        // Update duck pool max size
        if (duckPool != null)
        {
            duckPool.maxPoolSize = currentDuckLimit;
        }
        
        // Note: SpawnManager will check CanSpawnDuck() before spawning
    }
    
    /// <summary>
    /// Check if a new duck can be spawned based on current limits
    /// </summary>
    public bool CanSpawnDuck(int currentActiveDucks)
    {
        return currentActiveDucks < currentDuckLimit;
    }
    
    /// <summary>
    /// Get the adjusted spawn interval based on current optimization
    /// </summary>
    public float GetAdjustedSpawnInterval(float baseInterval)
    {
        return baseInterval * currentSpawnMultiplier;
    }

    #region Particle Pooling
    
    /// <summary>
    /// Get a particle system from the pool or create a new one
    /// </summary>
    public ParticleSystem GetPooledParticle(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!useParticlePooling || prefab == null)
        {
            // Fallback to instantiation
            GameObject instance = Instantiate(prefab, position, rotation);
            return instance.GetComponent<ParticleSystem>();
        }
        
        // Check if we're at the limit
        if (activeParticles.Count >= maxActiveParticles)
        {
            // Remove oldest particle
            if (activeParticles.Count > 0)
            {
                ParticleSystem oldest = activeParticles[0];
                activeParticles.RemoveAt(0);
                if (oldest != null)
                {
                    oldest.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    oldest.gameObject.SetActive(false);
                    particlePool.Enqueue(oldest);
                }
            }
        }
        
        ParticleSystem particle = null;
        
        // Try to get from pool
        while (particlePool.Count > 0 && particle == null)
        {
            particle = particlePool.Dequeue();
            if (particle == null || particle.gameObject == null)
            {
                particle = null;
            }
        }
        
        // Create new if pool is empty
        if (particle == null)
        {
            GameObject instance = Instantiate(prefab, position, rotation);
            particle = instance.GetComponent<ParticleSystem>();
        }
        else
        {
            // Reuse pooled particle
            particle.transform.position = position;
            particle.transform.rotation = rotation;
            particle.gameObject.SetActive(true);
        }
        
        if (particle != null)
        {
            activeParticles.Add(particle);
            particle.Play();
        }
        
        return particle;
    }
    
    /// <summary>
    /// Return a particle system to the pool
    /// </summary>
    public void ReturnParticle(ParticleSystem particle)
    {
        if (particle == null) return;
        
        activeParticles.Remove(particle);
        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particle.gameObject.SetActive(false);
        particlePool.Enqueue(particle);
    }
    
    /// <summary>
    /// Clean up finished particles
    /// </summary>
    private void CleanupParticles()
    {
        for (int i = activeParticles.Count - 1; i >= 0; i--)
        {
            ParticleSystem particle = activeParticles[i];
            if (particle == null || !particle.IsAlive())
            {
                if (particle != null)
                {
                    particle.gameObject.SetActive(false);
                    particlePool.Enqueue(particle);
                }
                activeParticles.RemoveAt(i);
            }
        }
    }
    
    #endregion
    
    #region LOD Management
    
    /// <summary>
    /// Setup LOD for a duck GameObject
    /// </summary>
    public void SetupDuckLOD(GameObject duck)
    {
        if (!enableLOD || duck == null) return;
        
        // Check if LOD group already exists
        LODGroup existingLOD = duck.GetComponent<LODGroup>();
        if (existingLOD != null) return;
        
        // Get all renderers
        Renderer[] renderers = duck.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;
        
        // Create LOD group
        LODGroup lodGroup = duck.AddComponent<LODGroup>();
        
        // Create LOD levels
        // LOD 0: Full quality (0-50% screen height)
        // LOD 1: Reduced quality (50-25% screen height) - disable shadows
        // LOD 2: Culled (below 25% screen height)
        
        LOD[] lods = new LOD[3];
        
        // LOD 0 - Full quality
        lods[0] = new LOD(0.5f, renderers);
        
        // LOD 1 - Reduced (same renderers but we'll disable shadows at runtime)
        lods[1] = new LOD(0.25f, renderers);
        
        // LOD 2 - Culled (no renderers)
        lods[2] = new LOD(0.01f, new Renderer[0]);
        
        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();
    }
    
    /// <summary>
    /// Optimize duck rendering based on distance
    /// </summary>
    public void OptimizeDuckRendering(GameObject duck, float distanceToPlayer)
    {
        if (duck == null) return;
        
        Renderer[] renderers = duck.GetComponentsInChildren<Renderer>();
        
        // Disable shadows for distant ducks
        bool enableShadows = distanceToPlayer < 15f;
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.shadowCastingMode = enableShadows 
                    ? UnityEngine.Rendering.ShadowCastingMode.On 
                    : UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
    }
    
    #endregion
    
    #region Debug Display
    
    private void OnGUI()
    {
        if (!showFPSCounter && !showPerformanceStats) return;
        
        // Only show in editor or development builds
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = GetFPSColor();
        
        float yOffset = 10f;
        
        if (showFPSCounter)
        {
            GUI.Label(new Rect(10, yOffset, 200, 30), $"FPS: {currentFPS:F1}", style);
            yOffset += 30f;
            GUI.Label(new Rect(10, yOffset, 200, 30), $"Avg: {averageFPS:F1}", style);
            yOffset += 30f;
        }
        
        if (showPerformanceStats)
        {
            style.fontSize = 18;
            style.normal.textColor = Color.white;
            
            GUI.Label(new Rect(10, yOffset, 300, 25), $"Optimization Level: {optimizationLevel}", style);
            yOffset += 25f;
            GUI.Label(new Rect(10, yOffset, 300, 25), $"Duck Limit: {currentDuckLimit}", style);
            yOffset += 25f;
            GUI.Label(new Rect(10, yOffset, 300, 25), $"Spawn Multiplier: {currentSpawnMultiplier:F2}x", style);
            yOffset += 25f;
            GUI.Label(new Rect(10, yOffset, 300, 25), $"Active Particles: {activeParticles.Count}/{maxActiveParticles}", style);
            yOffset += 25f;
            GUI.Label(new Rect(10, yOffset, 300, 25), $"LOD Bias: {QualitySettings.lodBias:F2}", style);
        }
        
        #endif
    }
    
    private Color GetFPSColor()
    {
        if (currentFPS >= targetFrameRate - 2)
            return Color.green;
        else if (currentFPS >= minAcceptableFrameRate)
            return Color.yellow;
        else
            return Color.red;
    }
    
    #endregion
    
    #region Public API
    
    /// <summary>
    /// Force a specific optimization level
    /// </summary>
    public void SetOptimizationLevel(int level)
    {
        optimizationLevel = Mathf.Clamp(level, 0, 5);
        ApplyOptimizations();
    }
    
    /// <summary>
    /// Reset all optimizations to default
    /// </summary>
    public void ResetOptimizations()
    {
        optimizationLevel = 0;
        currentDuckLimit = maxConcurrentDucks;
        currentSpawnMultiplier = 1.0f;
        maxActiveParticles = 5;
        
        if (enableLOD)
        {
            QualitySettings.lodBias = lodBias;
        }
        
        ApplyOptimizations();
        Debug.Log("PerformanceManager: All optimizations reset to default");
    }
    
    /// <summary>
    /// Get performance report as string
    /// </summary>
    public string GetPerformanceReport()
    {
        return $"Performance Report:\n" +
               $"  Current FPS: {currentFPS:F1}\n" +
               $"  Average FPS: {averageFPS:F1}\n" +
               $"  Target FPS: {targetFrameRate}\n" +
               $"  Optimization Level: {optimizationLevel}\n" +
               $"  Duck Limit: {currentDuckLimit}\n" +
               $"  Spawn Multiplier: {currentSpawnMultiplier:F2}x\n" +
               $"  Active Particles: {activeParticles.Count}\n" +
               $"  LOD Bias: {QualitySettings.lodBias:F2}";
    }
    
    #endregion
    
    private void OnDestroy()
    {
        // Clean up particle pool
        foreach (var particle in particlePool)
        {
            if (particle != null)
            {
                Destroy(particle.gameObject);
            }
        }
        particlePool.Clear();
        
        foreach (var particle in activeParticles)
        {
            if (particle != null)
            {
                Destroy(particle.gameObject);
            }
        }
        activeParticles.Clear();
        
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
