using UnityEngine;

public class AutoDestroyParticleSystem : MonoBehaviour
{
    [Header("Auto Destroy Settings")]
    public float destroyDelay = 2f;
    
    private ParticleSystem particles;
    private bool hasStarted = false;
    
    private void Awake()
    {
        particles = GetComponent<ParticleSystem>();
    }
    
    private void Start()
    {
        if (particles != null && particles.isPlaying)
        {
            StartDestroyTimer();
        }
    }
    
    private void Update()
    {
        if (particles != null && !hasStarted && particles.isPlaying)
        {
            StartDestroyTimer();
        }
        
        // Also check if particle system has stopped playing
        if (particles != null && hasStarted && !particles.isPlaying && !particles.IsAlive())
        {
            Destroy(gameObject);
        }
    }
    
    private void StartDestroyTimer()
    {
        if (!hasStarted)
        {
            hasStarted = true;
            Destroy(gameObject, destroyDelay);
        }
    }
    
    /// <summary>
    /// Manually trigger the destruction timer
    /// </summary>
    public void TriggerDestroy()
    {
        StartDestroyTimer();
    }
}