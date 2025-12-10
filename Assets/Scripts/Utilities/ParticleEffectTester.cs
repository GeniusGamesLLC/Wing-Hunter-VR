using UnityEngine;

public class ParticleEffectTester : MonoBehaviour
{
    [Header("Testing")]
    [SerializeField] private KeyCode testMuzzleFlashKey = KeyCode.M;
    [SerializeField] private KeyCode testDestructionKey = KeyCode.D;
    
    private void Update()
    {
        if (Input.GetKeyDown(testMuzzleFlashKey))
        {
            TestMuzzleFlash();
        }
        
        if (Input.GetKeyDown(testDestructionKey))
        {
            TestDestructionEffect();
        }
    }
    
    public void TestMuzzleFlash()
    {
        // Create a simple muzzle flash effect
        GameObject flashGO = new GameObject("TestMuzzleFlash");
        flashGO.transform.position = transform.position;
        flashGO.transform.rotation = transform.rotation;
        
        ParticleSystem particles = flashGO.AddComponent<ParticleSystem>();
        
        // Configure main module
        var main = particles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.playOnAwake = true;
        main.loop = false;
        main.maxParticles = 50;
        main.startSpeed = 8f;
        main.startSize = 0.3f;
        main.startLifetime = 0.1f;
        main.startColor = new Color(1f, 0.8f, 0.2f, 1f); // Orange-yellow flash
        
        // Configure emission module
        var emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f; // No continuous emission
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 50) // Burst of 50 particles at start
        });
        
        // Configure shape module
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.1f;
        shape.length = 0.5f;
        
        // Configure velocity over lifetime
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(10f); // Forward velocity
        
        // Add auto-destroy component
        AutoDestroyParticleSystem autoDestroy = flashGO.AddComponent<AutoDestroyParticleSystem>();
        autoDestroy.destroyDelay = 1f;
        
        particles.Play();
        
        Debug.Log("Muzzle flash test effect created");
    }
    
    public void TestDestructionEffect()
    {
        // Create a simple destruction effect
        GameObject effectGO = new GameObject("TestDestructionEffect");
        effectGO.transform.position = transform.position;
        
        ParticleSystem particles = effectGO.AddComponent<ParticleSystem>();
        
        // Configure main module
        var main = particles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.loop = false;
        main.maxParticles = 30;
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startColor = new Color(1f, 1f, 0.2f, 1f); // Yellow explosion
        main.gravityModifier = 0.5f; // Some gravity for realistic fall
        
        // Configure emission module
        var emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f; // No continuous emission
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 30) // Burst of 30 particles at start
        });
        
        // Configure shape module
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;
        
        // Configure velocity over lifetime for spread
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(2f);
        
        // Add auto-destroy component
        AutoDestroyParticleSystem autoDestroy = effectGO.AddComponent<AutoDestroyParticleSystem>();
        autoDestroy.destroyDelay = 2f;
        
        particles.Play();
        
        Debug.Log("Destruction test effect created");
    }
}