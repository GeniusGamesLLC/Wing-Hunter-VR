using System;
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
    
    [Header("Effects")]
    public ParticleSystem destructionParticles;
    [SerializeField] private GameObject explosionPrefab;
    
    // Events
    public event Action<DuckController> OnDestroyed;
    public event Action<DuckController> OnEscaped;
    
    // Private fields
    private Vector3 startPosition;
    private bool isMoving = false;
    private bool isDestroyed = false;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        duckCollider = GetComponent<CapsuleCollider>();
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
        if (isMoving && !isDestroyed)
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
        
        // Play destruction effects
        PlayDestructionEffects();
        
        // Notify listeners (SpawnManager will handle returning to pool)
        OnDestroyed?.Invoke(this);
        
        // Note: No longer destroying the GameObject - it will be returned to pool by SpawnManager
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
    /// Play destruction particle effects
    /// </summary>
    private void PlayDestructionEffects()
    {
        if (destructionParticles != null)
        {
            destructionParticles.Play();
        }
        else if (explosionPrefab != null && PerformanceManager.Instance != null)
        {
            // Use pooled particle from PerformanceManager
            PerformanceManager.Instance.GetPooledParticle(explosionPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            // Create a simple particle effect if none assigned
            CreateSimpleDestructionEffect();
        }
    }
    
    /// <summary>
    /// Create a destruction effect using the assigned explosion prefab or fallback
    /// </summary>
    private void CreateSimpleDestructionEffect()
    {
        // For now, always use the fallback effect since Unity Particle Pack materials 
        // are not compatible with URP and cause pink particles
        CreateFallbackDestructionEffect();
    }
    
    /// <summary>
    /// Fallback method to create a simple particle effect that works with URP
    /// Optimized for VR performance with reduced particle count
    /// </summary>
    private void CreateFallbackDestructionEffect()
    {
        // Create a simple explosion effect using a temporary particle system
        GameObject effectGO = new GameObject("DestructionEffect");
        effectGO.transform.position = transform.position;
        
        ParticleSystem particles = effectGO.AddComponent<ParticleSystem>();
        
        // Configure main module for a nice explosion (optimized for VR)
        var main = particles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.loop = false;
        main.maxParticles = 15; // Reduced from 30 for better performance
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 6f); // Slightly reduced
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 1.0f); // Shorter lifetime
        main.startColor = new Color(1f, 1f, 0.2f, 1f); // Bright yellow explosion
        main.gravityModifier = 0.5f; // More gravity for faster cleanup
        
        // Configure emission for burst (reduced count)
        var emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, 15) }); // Reduced from 30
        
        // Configure shape for spherical explosion
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.15f; // Slightly smaller
        
        // Configure velocity over lifetime for outward explosion
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(2f);
        
        // Configure size over lifetime for nice scaling (simplified curve)
        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Configure color over lifetime for fade out (simplified gradient)
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.yellow, 0.0f), 
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f), // Orange
                new GradientColorKey(Color.gray, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.5f, 0.5f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;
        
        // Auto-destroy (shorter time)
        Destroy(effectGO, 1.5f);
        particles.Play();
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
        // Reset state flags
        isMoving = false;
        isDestroyed = false;
        
        // Reset properties
        FlightSpeed = 5f;
        TargetPosition = Vector3.zero;
        
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