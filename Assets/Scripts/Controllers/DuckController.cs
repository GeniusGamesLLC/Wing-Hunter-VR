using System;
using UnityEngine;

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
        
        // Notify listeners
        OnDestroyed?.Invoke(this);
        
        // Destroy the duck after a short delay to allow effects to play
        Destroy(gameObject, 0.5f);
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
        
        // Notify listeners that duck escaped
        OnEscaped?.Invoke(this);
        
        // Remove the duck from scene
        Destroy(gameObject, 0.1f);
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
        else
        {
            // Create a simple particle effect if none assigned
            CreateSimpleDestructionEffect();
        }
    }
    
    /// <summary>
    /// Create a simple destruction effect if no particle system is assigned
    /// </summary>
    private void CreateSimpleDestructionEffect()
    {
        // Create a simple explosion effect using a temporary particle system
        GameObject effectGO = new GameObject("DestructionEffect");
        effectGO.transform.position = transform.position;
        
        ParticleSystem particles = effectGO.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 5f;
        main.startSize = 0.1f;
        main.startColor = Color.yellow;
        main.maxParticles = 20;
        
        var emission = particles.emission;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 20)
        });
        
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        particles.Play();
        
        // Destroy the effect after it finishes
        Destroy(effectGO, 1f);
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
}