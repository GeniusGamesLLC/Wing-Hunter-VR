using System;
using System.Collections;
using UnityEngine;
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
    
    // Events
    public event Action<DuckController> OnDestroyed;
    public event Action<DuckController> OnEscaped;
    
    // Private fields
    private Vector3 startPosition;
    private bool isMoving = false;
    private bool isDestroyed = false;
    private bool isPlayingDeathAnimation = false;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        duckCollider = GetComponent<CapsuleCollider>();
        renderers = GetComponentsInChildren<Renderer>();
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
        if (isMoving && !isDestroyed && !isPlayingDeathAnimation)
        {
            MoveDuck();
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
        
        // Start moving
        isMoving = true;
        isDestroyed = false;
        
        // Enable collider
        if (duckCollider != null)
        {
            duckCollider.enabled = true;
        }
        
        // Start flying animation
        SetAnimationState(true);
    }
    
    /// <summary>
    /// Move the duck towards its target position
    /// </summary>
    private void MoveDuck()
    {
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, FlightSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// Check if the duck has reached its target position and trigger escape
    /// </summary>
    private void CheckIfReachedTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position, TargetPosition);
        
        // If very close to target (within 0.1 units), consider it reached
        if (distanceToTarget < 0.1f)
        {
            TriggerEscape();
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
    /// Called when the duck reaches the end of its path without being hit
    /// </summary>
    private void TriggerEscape()
    {
        if (isDestroyed) return; // Already destroyed
        
        isMoving = false;
        
        // Stop flying animation
        SetAnimationState(false);
        
        // Notify listeners that duck escaped (SpawnManager will handle returning to pool)
        OnEscaped?.Invoke(this);
        
        // Note: No longer destroying the GameObject - it will be returned to pool by SpawnManager
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
    
    /// <summary>
    /// Reset the duck to a clean state for reuse from object pool
    /// </summary>
    public void ResetForReuse()
    {
        // Stop any running coroutines (death animation)
        StopAllCoroutines();
        
        // Reset state flags
        isMoving = false;
        isDestroyed = false;
        isPlayingDeathAnimation = false;
        
        // Reset properties
        FlightSpeed = 5f;
        TargetPosition = Vector3.zero;
        
        // Reset transform scale (may have been modified during death animation)
        transform.localScale = Vector3.one;
        
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
}