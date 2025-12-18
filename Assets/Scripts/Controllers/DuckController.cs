using System;
using System.Collections;
using UnityEngine;
using DuckHunt.Data;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DuckController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float FlightSpeed { get; set; } = 5f;
    public Vector3 TargetPosition { get; set; }
    
    [Header("Components")]
    private Animator animator;
    private CapsuleCollider duckCollider;
    private Renderer[] renderers;
    
    [Header("Effects")]
    public ParticleSystem destructionParticles;
    [SerializeField] private GameObject explosionPrefab;
    
    [Header("Death Animation")]
    [SerializeField] private float deathFallDuration = 0.4f;
    [SerializeField] private float deathFallSpeed = 8f;
    [SerializeField] private float deathSpinSpeed = 720f;
    
    [Header("Spawn Animation")]
    [SerializeField] private float maxSpawnDuration = 0.5f;
    [SerializeField] private float spawnStartScale = 0.1f;
    
    [Header("Escape Animation")]
    [SerializeField] private float maxEscapeDuration = 0.4f;
    
    [Header("Animation Timing")]
    [Tooltip("Minimum time duck must be fully visible and shootable (between spawn and escape)")]
    [SerializeField] private float minFullyVisibleTime = 0.5f;
    [Tooltip("Max percentage of flight time that can be used for spawn+escape animations")]
    [SerializeField] private float maxAnimationTimePercent = 0.4f;
    
    [Header("Path Visualization")]
    [Tooltip("LineRenderer for visualizing the flight path (optional, will be created if needed)")]
    [SerializeField] private LineRenderer pathLineRenderer;
    [Tooltip("Number of points to use for path visualization")]
    [SerializeField] private int pathVisualizationPoints = 50;
    
    // Calculated animation durations for current flight
    private float actualSpawnDuration;
    private float actualEscapeDuration;
    
    // Events
    public event Action<DuckController> OnDestroyed;
    public event Action<DuckController> OnEscaped;
    
    // Private fields
    private Vector3 startPosition;
    private bool isMoving = false;
    private bool isDestroyed = false;
    private bool isPlayingDeathAnimation = false;
    private bool isSpawning = false;
    private bool isEscaping = false;
    private Material[] originalMaterials;
    private Material[] fadeMaterials;
    
    // Spline movement fields
    private FlightPath currentPath;
    private float distanceTraveled;
    private bool useSplineMovement = false;
    
    /// <summary>
    /// The current flight path (null if using legacy straight-line movement)
    /// </summary>
    public FlightPath CurrentPath => currentPath;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        duckCollider = GetComponent<CapsuleCollider>();
        renderers = GetComponentsInChildren<Renderer>();
        CacheMaterials();
        
        // Subscribe to debug settings changes
        if (DebugSettings.Instance != null)
        {
            DebugSettings.Instance.OnSettingsChanged += OnDebugSettingsChanged;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from debug settings
        if (DebugSettings.Instance != null)
        {
            DebugSettings.Instance.OnSettingsChanged -= OnDebugSettingsChanged;
        }
    }
    
    /// <summary>
    /// Cache original materials and create fade-capable copies
    /// </summary>
    private void CacheMaterials()
    {
        if (renderers == null || renderers.Length == 0) return;
        
        // Count total materials
        int totalMaterials = 0;
        foreach (var r in renderers)
        {
            if (r != null) totalMaterials += r.materials.Length;
        }
        
        originalMaterials = new Material[totalMaterials];
        fadeMaterials = new Material[totalMaterials];
        
        int index = 0;
        foreach (var r in renderers)
        {
            if (r == null) continue;
            foreach (var mat in r.materials)
            {
                originalMaterials[index] = mat;
                // Create instance for fading (will be configured when needed)
                fadeMaterials[index] = new Material(mat);
                index++;
            }
        }
    }
    
    private void Start()
    {
        // Ensure the duck starts moving if initialized
        if (isMoving)
        {
            SetAnimationState(true);
        }
    }
    
    private void Update()
    {
        if (isMoving && !isDestroyed && !isPlayingDeathAnimation && !isSpawning && !isEscaping)
        {
            if (useSplineMovement && currentPath != null)
            {
                UpdateSplineMovement();
            }
            else
            {
                MoveDuck();
            }
            CheckIfReachedTarget();
        }
    }
    
    /// <summary>
    /// Initialize the duck with start position, end position, and flight speed
    /// </summary>
    /// <param name="start">Starting position</param>
    /// <param name="end">Target position</param>
    /// <param name="speed">Flight speed</param>
    public void Initialize(Vector3 start, Vector3 end, float speed)
    {
        startPosition = start;
        TargetPosition = end;
        FlightSpeed = speed;
        
        // Set initial position
        transform.position = startPosition;
        
        // Look towards target
        Vector3 direction = (TargetPosition - startPosition).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // Reset state
        isDestroyed = false;
        isEscaping = false;
        
        // Calculate animation durations based on flight time
        CalculateAnimationDurations();
        
        // Disable collider during spawn animation
        if (duckCollider != null)
        {
            duckCollider.enabled = false;
        }
        
        // Start flying animation
        SetAnimationState(true);
        
        // Start spawn animation (will enable movement and collider when done)
        StartCoroutine(PlaySpawnAnimation());
    }
    
    /// <summary>
    /// Initialize the duck with a FlightPath for spline-based movement.
    /// Uses arc-length parameterization for constant-speed movement along curved paths.
    /// </summary>
    /// <param name="path">The flight path to follow</param>
    /// <param name="speed">Flight speed in units per second</param>
    public void Initialize(FlightPath path, float speed)
    {
        if (path == null)
        {
            Debug.LogError("[DuckController] Initialize called with null FlightPath, falling back to straight line");
            // Fall back to straight line if path is null
            Initialize(Vector3.zero, Vector3.forward * 10f, speed);
            return;
        }
        
        currentPath = path;
        useSplineMovement = true;
        distanceTraveled = 0f;
        
        startPosition = path.SpawnPoint;
        TargetPosition = path.TargetPoint;
        FlightSpeed = speed;
        
        // Set initial position
        transform.position = startPosition;
        
        // Look towards initial movement direction (tangent at start)
        Vector3 initialTangent = path.GetTangentAtDistance(0f);
        if (initialTangent != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(initialTangent);
        }
        
        // Reset state
        isDestroyed = false;
        isEscaping = false;
        
        // Calculate animation durations based on flight time
        CalculateAnimationDurationsForPath();
        
        // Disable collider during spawn animation
        if (duckCollider != null)
        {
            duckCollider.enabled = false;
        }
        
        // Start flying animation
        SetAnimationState(true);
        
        // Setup path visualization
        SetupPathVisualization();
        
        // Start spawn animation (will enable movement and collider when done)
        StartCoroutine(PlaySpawnAnimation());
        
        Debug.Log($"[DuckController] Initialized with FlightPath: {path}, Speed: {speed}");
    }
    
    /// <summary>
    /// Calculate animation durations for spline-based flight paths.
    /// Uses the total arc length of the path for accurate timing.
    /// </summary>
    private void CalculateAnimationDurationsForPath()
    {
        if (currentPath == null)
        {
            CalculateAnimationDurations();
            return;
        }
        
        // Use actual arc length for accurate flight time estimation
        float estimatedFlightTime = currentPath.GetEstimatedDuration(FlightSpeed);
        
        // Calculate max time available for animations
        float maxAnimationTime = estimatedFlightTime * maxAnimationTimePercent;
        
        // Ensure we have minimum fully visible time
        float availableAnimationTime = Mathf.Max(0f, estimatedFlightTime - minFullyVisibleTime);
        
        // Use the smaller of the two constraints
        float totalAnimationBudget = Mathf.Min(maxAnimationTime, availableAnimationTime);
        
        // Split budget between spawn and escape (spawn gets slightly more)
        float spawnRatio = 0.55f;
        float escapeRatio = 0.45f;
        
        // Calculate actual durations, capped by max values
        actualSpawnDuration = Mathf.Min(maxSpawnDuration, totalAnimationBudget * spawnRatio);
        actualEscapeDuration = Mathf.Min(maxEscapeDuration, totalAnimationBudget * escapeRatio);
        
        // Ensure minimum animation time if we have any budget
        float minAnimDuration = 0.1f;
        if (totalAnimationBudget > minAnimDuration * 2)
        {
            actualSpawnDuration = Mathf.Max(minAnimDuration, actualSpawnDuration);
            actualEscapeDuration = Mathf.Max(minAnimDuration, actualEscapeDuration);
        }
        else if (totalAnimationBudget <= 0)
        {
            actualSpawnDuration = 0f;
            actualEscapeDuration = 0f;
        }
    }
    
    /// <summary>
    /// Calculate spawn and escape animation durations based on flight time.
    /// Ensures duck has minimum fully-visible time for gameplay.
    /// Future-proof: works with single path or multi-waypoint paths.
    /// </summary>
    private void CalculateAnimationDurations()
    {
        // Estimate total flight time (for single path, this is exact)
        // For future multi-waypoint paths, this would be the first segment time
        float distance = Vector3.Distance(startPosition, TargetPosition);
        float estimatedFlightTime = distance / FlightSpeed;
        
        // Calculate max time available for animations
        float maxAnimationTime = estimatedFlightTime * maxAnimationTimePercent;
        
        // Ensure we have minimum fully visible time
        float availableAnimationTime = Mathf.Max(0f, estimatedFlightTime - minFullyVisibleTime);
        
        // Use the smaller of the two constraints
        float totalAnimationBudget = Mathf.Min(maxAnimationTime, availableAnimationTime);
        
        // Split budget between spawn and escape (spawn gets slightly more)
        float spawnRatio = 0.55f;
        float escapeRatio = 0.45f;
        
        // Calculate actual durations, capped by max values
        actualSpawnDuration = Mathf.Min(maxSpawnDuration, totalAnimationBudget * spawnRatio);
        actualEscapeDuration = Mathf.Min(maxEscapeDuration, totalAnimationBudget * escapeRatio);
        
        // Ensure minimum animation time if we have any budget (avoid jarring instant transitions)
        float minAnimDuration = 0.1f;
        if (totalAnimationBudget > minAnimDuration * 2)
        {
            actualSpawnDuration = Mathf.Max(minAnimDuration, actualSpawnDuration);
            actualEscapeDuration = Mathf.Max(minAnimDuration, actualEscapeDuration);
        }
        else if (totalAnimationBudget <= 0)
        {
            // Very short flight - skip animations entirely
            actualSpawnDuration = 0f;
            actualEscapeDuration = 0f;
        }
    }
    
    /// <summary>
    /// Plays spawn animation - fade in and scale up
    /// </summary>
    private IEnumerator PlaySpawnAnimation()
    {
        isSpawning = true;
        isMoving = false;
        
        // Skip animation if duration is zero (very short flight)
        if (actualSpawnDuration <= 0f)
        {
            transform.localScale = Vector3.one;
            SetAlpha(1f);
            isSpawning = false;
            isMoving = true;
            if (duckCollider != null) duckCollider.enabled = true;
            yield break;
        }
        
        float elapsed = 0f;
        
        // Start small and transparent
        transform.localScale = Vector3.one * spawnStartScale;
        SetAlpha(0f);
        
        while (elapsed < actualSpawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / actualSpawnDuration;
            
            // Ease out curve for smoother feel
            float easedT = 1f - Mathf.Pow(1f - t, 2f);
            
            // Scale up
            float scale = Mathf.Lerp(spawnStartScale, 1f, easedT);
            transform.localScale = Vector3.one * scale;
            
            // Fade in
            SetAlpha(easedT);
            
            // Move while spawning (so it doesn't just sit there)
            if (useSplineMovement && currentPath != null)
            {
                MoveSplineDuringAnimation();
            }
            else
            {
                MoveDuck();
            }
            
            yield return null;
        }
        
        // Ensure final state
        transform.localScale = Vector3.one;
        SetAlpha(1f);
        
        isSpawning = false;
        isMoving = true;
        
        // Enable collider now that spawn is complete
        if (duckCollider != null)
        {
            duckCollider.enabled = true;
        }
    }
    
    /// <summary>
    /// Move the duck towards its target position (legacy straight-line movement)
    /// </summary>
    private void MoveDuck()
    {
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, FlightSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// Update duck position along the spline path using arc-length parameterization.
    /// Maintains constant speed regardless of path curvature.
    /// </summary>
    private void UpdateSplineMovement()
    {
        if (currentPath == null) return;
        
        // Update distance traveled
        distanceTraveled += FlightSpeed * Time.deltaTime;
        
        // Get position and tangent at current distance
        Vector3 newPosition = currentPath.GetPositionAtDistance(distanceTraveled);
        Vector3 tangent = currentPath.GetTangentAtDistance(distanceTraveled);
        
        // Update position
        transform.position = newPosition;
        
        // Update orientation to face movement direction (smooth rotation)
        if (tangent != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(tangent);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
    
    /// <summary>
    /// Move the duck along the spline during spawn animation.
    /// </summary>
    private void MoveSplineDuringAnimation()
    {
        if (currentPath == null || !useSplineMovement) return;
        
        distanceTraveled += FlightSpeed * Time.deltaTime;
        
        Vector3 newPosition = currentPath.GetPositionAtDistance(distanceTraveled);
        Vector3 tangent = currentPath.GetTangentAtDistance(distanceTraveled);
        
        transform.position = newPosition;
        
        if (tangent != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(tangent);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
    
    /// <summary>
    /// Check if the duck should start escaping (approaching target) or has reached target
    /// </summary>
    private void CheckIfReachedTarget()
    {
        if (useSplineMovement && currentPath != null)
        {
            // For spline movement, check distance traveled vs total arc length
            float remainingDistance = currentPath.TotalArcLength - distanceTraveled;
            
            // Calculate distance at which to start escape animation
            float escapeStartDistance = actualEscapeDuration * FlightSpeed;
            
            // Start escape animation when approaching end of path
            if (remainingDistance <= escapeStartDistance && !isEscaping)
            {
                TriggerEscape();
            }
            // Fallback: if past the end and somehow not escaping yet
            else if (currentPath.IsAtEnd(distanceTraveled) && !isEscaping)
            {
                TriggerEscape();
            }
        }
        else
        {
            // Legacy straight-line check
            float distanceToTarget = Vector3.Distance(transform.position, TargetPosition);
            
            // Calculate distance at which to start escape animation
            float escapeStartDistance = actualEscapeDuration * FlightSpeed;
            
            // Start escape animation when approaching target (so it fades while moving)
            if (distanceToTarget <= escapeStartDistance && !isEscaping)
            {
                TriggerEscape();
            }
            // Fallback: if very close and somehow not escaping yet
            else if (distanceToTarget < 0.1f && !isEscaping)
            {
                TriggerEscape();
            }
        }
    }
    
    /// <summary>
    /// Called when the duck is hit by a shot
    /// </summary>
    public void OnHit()
    {
        if (isDestroyed) return; // Prevent multiple hits
        
        isDestroyed = true;
        isMoving = false;
        
        // Disable collider to prevent further hits
        if (duckCollider != null)
        {
            duckCollider.enabled = false;
        }
        
        // Stop flying animation
        SetAnimationState(false);
        
        // Hide path visualization
        HidePathVisualization();
        
        // IMPORTANT: Capture position BEFORE any state changes for particle effects
        Vector3 hitPosition = transform.position;
        
        // Play destruction effects at the captured position
        PlayDestructionEffects(hitPosition);
        
        // Start death animation coroutine
        StartCoroutine(PlayDeathAnimation());
    }
    
    /// <summary>
    /// Plays a brief death animation (fall and spin) before notifying destruction
    /// </summary>
    private IEnumerator PlayDeathAnimation()
    {
        isPlayingDeathAnimation = true;
        
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        
        // Brief fall and spin animation
        while (elapsed < deathFallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / deathFallDuration;
            
            // Fall down with acceleration
            float fallDistance = deathFallSpeed * elapsed * t;
            transform.position = startPos + Vector3.down * fallDistance;
            
            // Spin while falling
            transform.Rotate(Vector3.forward, deathSpinSpeed * Time.deltaTime, Space.Self);
            
            // Fade out (optional - scale down slightly)
            float scale = Mathf.Lerp(1f, 0.5f, t);
            transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        isPlayingDeathAnimation = false;
        
        // Now notify listeners that destruction is complete
        OnDestroyed?.Invoke(this);
    }
    
    /// <summary>
    /// Called when the duck approaches the end of its path without being hit
    /// </summary>
    private void TriggerEscape()
    {
        if (isDestroyed || isEscaping) return; // Already destroyed or escaping
        
        // Don't stop moving - duck continues flying while fading
        // isMoving stays true so escape animation can move the duck
        
        // Start escape animation (will notify when done)
        StartCoroutine(PlayEscapeAnimation());
    }
    
    /// <summary>
    /// Plays escape animation - fade out and scale down while continuing to fly
    /// </summary>
    private IEnumerator PlayEscapeAnimation()
    {
        isEscaping = true;
        
        // Disable collider during escape (can't be shot while fading)
        if (duckCollider != null)
        {
            duckCollider.enabled = false;
        }
        
        // Skip animation if duration is zero (very short flight)
        if (actualEscapeDuration <= 0f)
        {
            transform.localScale = Vector3.one * 0.1f;
            SetAlpha(0f);
            isEscaping = false;
            isMoving = false;
            SetAnimationState(false);
            OnEscaped?.Invoke(this);
            yield break;
        }
        
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        
        while (elapsed < actualEscapeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / actualEscapeDuration;
            
            // Ease in curve for smooth fade out
            float easedT = t * t;
            
            // Scale down
            float scale = Mathf.Lerp(1f, 0.1f, easedT);
            transform.localScale = Vector3.one * scale;
            
            // Fade out
            SetAlpha(1f - easedT);
            
            // Keep moving while escaping (duck flies into the distance while fading)
            if (useSplineMovement && currentPath != null)
            {
                MoveSplineDuringAnimation();
            }
            else
            {
                MoveDuck();
            }
            
            yield return null;
        }
        
        // Ensure final state
        transform.localScale = Vector3.one * 0.1f;
        SetAlpha(0f);
        
        isEscaping = false;
        isMoving = false;
        
        // Stop flying animation
        SetAnimationState(false);
        
        // Hide path visualization when escaping
        HidePathVisualization();
        
        // Notify listeners that duck escaped (SpawnManager will handle returning to pool)
        OnEscaped?.Invoke(this);
    }
    
    /// <summary>
    /// Play destruction particle effects at the specified position
    /// </summary>
    /// <param name="hitPosition">World position where the duck was hit</param>
    private void PlayDestructionEffects(Vector3 hitPosition)
    {
        if (destructionParticles != null)
        {
            // Move attached particle system to hit position and play
            destructionParticles.transform.position = hitPosition;
            destructionParticles.Play();
        }
        else if (explosionPrefab != null && PerformanceManager.Instance != null)
        {
            // Use pooled particle from PerformanceManager at hit position
            PerformanceManager.Instance.GetPooledParticle(explosionPrefab, hitPosition, Quaternion.identity);
        }
        else
        {
            // Create a feather poof effect at the hit position
            CreateFeatherPoofEffect(hitPosition);
        }
    }
    
    /// <summary>
    /// Creates a satisfying feather poof/explosion effect using URP-compatible settings
    /// Optimized for Quest VR performance
    /// </summary>
    /// <param name="position">World position to spawn the effect</param>
    private void CreateFeatherPoofEffect(Vector3 position)
    {
        // Create parent GameObject for the effect
        GameObject effectGO = new GameObject("DuckFeatherPoof");
        effectGO.transform.position = position;
        
        // Create the main feather burst particle system
        ParticleSystem featherBurst = effectGO.AddComponent<ParticleSystem>();
        ConfigureFeatherBurstParticles(featherBurst);
        
        // Create a secondary smoke poof child
        GameObject smokeGO = new GameObject("SmokePoof");
        smokeGO.transform.SetParent(effectGO.transform);
        smokeGO.transform.localPosition = Vector3.zero;
        ParticleSystem smokePoof = smokeGO.AddComponent<ParticleSystem>();
        ConfigureSmokePoofParticles(smokePoof);
        
        // Auto-destroy after effects complete
        Destroy(effectGO, 2f);
        
        // Play both effects
        featherBurst.Play();
        smokePoof.Play();
    }
    
    /// <summary>
    /// Configures the main feather burst particle system
    /// </summary>
    private void ConfigureFeatherBurstParticles(ParticleSystem particles)
    {
        // Main module - feather-like particles
        var main = particles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.loop = false;
        main.maxParticles = 20;
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
        main.gravityModifier = 0.8f;
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        
        // Duck-colored feathers (brown/green/white mix)
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.4f, 0.6f, 0.2f, 1f),  // Green-brown
            new Color(0.9f, 0.85f, 0.7f, 1f)  // Off-white
        );
        
        // Emission - burst of feathers
        var emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { 
            new ParticleSystem.Burst(0.0f, 15, 20) 
        });
        
        // Shape - sphere burst
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;
        
        // Rotation over lifetime - tumbling feathers
        var rotationOverLifetime = particles.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-180f, 180f);
        
        // Size over lifetime - shrink as they fall
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.7f, 0.8f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Color over lifetime - fade out
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0.0f), 
                new GradientColorKey(Color.white, 0.6f),
                new GradientColorKey(Color.white, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(1.0f, 0.5f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;
        
        // Configure renderer for URP compatibility
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        // Use default particle material which is URP compatible
        renderer.material = GetURPParticleMaterial();
    }
    
    /// <summary>
    /// Configures the smoke poof particle system
    /// </summary>
    private void ConfigureSmokePoofParticles(ParticleSystem particles)
    {
        // Main module - quick expanding smoke poof
        var main = particles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.loop = false;
        main.maxParticles = 8;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
        main.gravityModifier = -0.2f; // Rise slightly
        main.startColor = new Color(1f, 0.95f, 0.8f, 0.6f); // Light tan smoke
        
        // Emission - quick burst
        var emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { 
            new ParticleSystem.Burst(0.0f, 6, 8) 
        });
        
        // Shape - small sphere
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;
        
        // Size over lifetime - expand then fade
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.5f);
        sizeCurve.AddKey(0.3f, 1f);
        sizeCurve.AddKey(1f, 1.5f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Color over lifetime - fade out quickly
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.95f, 0.8f), 0.0f), 
                new GradientColorKey(new Color(0.9f, 0.85f, 0.7f), 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.6f, 0.0f), 
                new GradientAlphaKey(0.3f, 0.5f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;
        
        // Configure renderer for URP compatibility
        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = GetURPParticleMaterial();
    }
    
    /// <summary>
    /// Gets a URP-compatible particle material
    /// </summary>
    private Material GetURPParticleMaterial()
    {
        // Try to find the URP default particle material
        Material urpParticleMat = Resources.Load<Material>("Default-Particle");
        
        if (urpParticleMat == null)
        {
            // Create a simple unlit material that works with URP
            // Using the built-in Particles/Standard Unlit shader which is URP compatible
            Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particleShader == null)
            {
                // Fallback to legacy shader if URP shader not found
                particleShader = Shader.Find("Particles/Standard Unlit");
            }
            if (particleShader == null)
            {
                // Last resort fallback
                particleShader = Shader.Find("Sprites/Default");
            }
            
            if (particleShader != null)
            {
                urpParticleMat = new Material(particleShader);
                urpParticleMat.SetFloat("_Surface", 1); // Transparent
                urpParticleMat.SetFloat("_Blend", 0); // Alpha blend
            }
        }
        
        return urpParticleMat;
    }
    
    /// <summary>
    /// Set the animation state for flying
    /// </summary>
    /// <param name="isFlying">Whether the duck should be in flying animation state</param>
    private void SetAnimationState(bool isFlying)
    {
        if (animator != null)
        {
            animator.enabled = isFlying;
        }
    }
    
    /// <summary>
    /// Set the alpha/opacity of all duck renderers
    /// </summary>
    /// <param name="alpha">Alpha value from 0 (transparent) to 1 (opaque)</param>
    private void SetAlpha(float alpha)
    {
        if (renderers == null) return;
        
        foreach (var r in renderers)
        {
            if (r == null) continue;
            
            foreach (var mat in r.materials)
            {
                if (mat == null) continue;
                
                // Set rendering mode to transparent if fading
                if (alpha < 1f)
                {
                    // Enable transparency mode
                    mat.SetFloat("_Surface", 1); // 0 = Opaque, 1 = Transparent
                    mat.SetFloat("_Blend", 0); // 0 = Alpha, 1 = Premultiply, 2 = Additive, 3 = Multiply
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.renderQueue = 3000;
                }
                else
                {
                    // Restore opaque mode
                    mat.SetFloat("_Surface", 0);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    mat.SetInt("_ZWrite", 1);
                    mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    mat.renderQueue = -1;
                }
                
                // Set the alpha value
                if (mat.HasProperty("_BaseColor"))
                {
                    Color color = mat.GetColor("_BaseColor");
                    color.a = alpha;
                    mat.SetColor("_BaseColor", color);
                }
                else if (mat.HasProperty("_Color"))
                {
                    Color color = mat.GetColor("_Color");
                    color.a = alpha;
                    mat.SetColor("_Color", color);
                }
            }
        }
    }
    
    /// <summary>
    /// Get the current movement status
    /// </summary>
    /// <returns>True if the duck is currently moving</returns>
    public bool IsMoving()
    {
        return isMoving && !isDestroyed;
    }
    
    /// <summary>
    /// Get the current destroyed status
    /// </summary>
    /// <returns>True if the duck has been destroyed</returns>
    public bool IsDestroyed()
    {
        return isDestroyed;
    }
    
    /// <summary>
    /// Get the distance remaining to target
    /// </summary>
    /// <returns>Distance to target position</returns>
    public float GetDistanceToTarget()
    {
        return Vector3.Distance(transform.position, TargetPosition);
    }
    
    #region Path Visualization
    
    /// <summary>
    /// Sets up the path visualization LineRenderer if debug settings allow.
    /// </summary>
    private void SetupPathVisualization()
    {
        if (currentPath == null || !useSplineMovement) return;
        
        // Check if visualization should be shown
        bool showPath = DebugSettings.Instance != null && DebugSettings.Instance.ShowSplinePaths;
        
        if (!showPath)
        {
            HidePathVisualization();
            return;
        }
        
        // Create LineRenderer if needed
        if (pathLineRenderer == null)
        {
            pathLineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        // Configure LineRenderer
        ConfigurePathLineRenderer();
        
        // Generate and set path points
        UpdatePathVisualization();
    }
    
    /// <summary>
    /// Configures the LineRenderer with appropriate settings for path visualization.
    /// </summary>
    private void ConfigurePathLineRenderer()
    {
        if (pathLineRenderer == null) return;
        
        // Get visualization settings from FlightPathConfig if available
        Color pathColor = Color.cyan;
        float pathWidth = 0.05f;
        
        // Try to find FlightPathConfig for settings
        FlightPathConfig config = Resources.Load<FlightPathConfig>("FlightPathConfig");
        if (config == null)
        {
            // Try to find FlightPathGenerator in scene and get its config
            FlightPathGenerator generator = FindObjectOfType<FlightPathGenerator>();
            if (generator != null)
            {
                config = generator.Config;
            }
        }
        
        if (config != null)
        {
            pathColor = config.SplinePathColor;
            pathWidth = config.SplinePathWidth;
        }
        
        pathLineRenderer.startWidth = pathWidth;
        pathLineRenderer.endWidth = pathWidth;
        pathLineRenderer.startColor = pathColor;
        pathLineRenderer.endColor = pathColor;
        pathLineRenderer.useWorldSpace = true;
        pathLineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        pathLineRenderer.receiveShadows = false;
        
        // Create or get a simple unlit material
        Material lineMaterial = GetPathVisualizationMaterial(pathColor);
        if (lineMaterial != null)
        {
            pathLineRenderer.material = lineMaterial;
        }
    }
    
    /// <summary>
    /// Gets or creates a material for path visualization.
    /// </summary>
    private Material GetPathVisualizationMaterial(Color color)
    {
        // Try to find URP unlit shader
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }
        
        if (shader != null)
        {
            Material mat = new Material(shader);
            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            return mat;
        }
        
        return null;
    }
    
    /// <summary>
    /// Updates the path visualization with current path points.
    /// </summary>
    private void UpdatePathVisualization()
    {
        if (pathLineRenderer == null || currentPath == null) return;
        
        // Generate visualization points
        Vector3[] points = currentPath.GenerateVisualizationPoints(pathVisualizationPoints);
        
        if (points != null && points.Length > 0)
        {
            pathLineRenderer.positionCount = points.Length;
            pathLineRenderer.SetPositions(points);
            pathLineRenderer.enabled = true;
        }
    }
    
    /// <summary>
    /// Hides the path visualization.
    /// </summary>
    private void HidePathVisualization()
    {
        if (pathLineRenderer != null)
        {
            pathLineRenderer.enabled = false;
        }
    }
    
    /// <summary>
    /// Called when debug settings change. Updates visualization state.
    /// </summary>
    private void OnDebugSettingsChanged()
    {
        if (DebugSettings.Instance == null) return;
        
        bool showPath = DebugSettings.Instance.ShowSplinePaths;
        
        if (showPath && currentPath != null && useSplineMovement && !isDestroyed)
        {
            SetupPathVisualization();
        }
        else
        {
            HidePathVisualization();
        }
    }
    
    #endregion
    
    /// <summary>
    /// Reset the duck to a clean state for reuse from object pool
    /// </summary>
    public void ResetForReuse()
    {
        // Stop any running coroutines (death animation, spawn, escape)
        StopAllCoroutines();
        
        // Reset state flags
        isMoving = false;
        isDestroyed = false;
        isPlayingDeathAnimation = false;
        isSpawning = false;
        isEscaping = false;
        
        // Reset spline movement state
        currentPath = null;
        distanceTraveled = 0f;
        useSplineMovement = false;
        
        // Hide path visualization
        HidePathVisualization();
        
        // Reset properties
        FlightSpeed = 5f;
        TargetPosition = Vector3.zero;
        
        // Reset transform scale (may have been modified during animations)
        transform.localScale = Vector3.one;
        
        // Reset alpha to fully opaque
        SetAlpha(1f);
        
        // Enable collider
        if (duckCollider != null)
        {
            duckCollider.enabled = true;
        }
        
        // Reset animation
        SetAnimationState(false);
        
        // Clear events (will be reassigned by SpawnManager)
        OnDestroyed = null;
        OnEscaped = null;
    }
    
    /// <summary>
    /// Gets the distance remaining to the end of the path.
    /// Works for both spline and straight-line movement.
    /// </summary>
    /// <returns>Distance remaining in world units</returns>
    public float GetDistanceRemaining()
    {
        if (useSplineMovement && currentPath != null)
        {
            return Mathf.Max(0f, currentPath.TotalArcLength - distanceTraveled);
        }
        return Vector3.Distance(transform.position, TargetPosition);
    }
    
    /// <summary>
    /// Gets the total path length.
    /// Works for both spline and straight-line movement.
    /// </summary>
    /// <returns>Total path length in world units</returns>
    public float GetTotalPathLength()
    {
        if (useSplineMovement && currentPath != null)
        {
            return currentPath.TotalArcLength;
        }
        return Vector3.Distance(startPosition, TargetPosition);
    }
    
    /// <summary>
    /// Gets the normalized progress along the path (0 to 1).
    /// </summary>
    /// <returns>Progress value from 0 (start) to 1 (end)</returns>
    public float GetPathProgress()
    {
        float totalLength = GetTotalPathLength();
        if (totalLength <= 0f) return 1f;
        
        if (useSplineMovement && currentPath != null)
        {
            return Mathf.Clamp01(distanceTraveled / totalLength);
        }
        
        float traveled = Vector3.Distance(startPosition, transform.position);
        return Mathf.Clamp01(traveled / totalLength);
    }
}