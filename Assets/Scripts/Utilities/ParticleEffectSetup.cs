using UnityEngine;

public class ParticleEffectSetup : MonoBehaviour
{
    private void Start()
    {
        CreateParticleEffects();
    }
    
    public void CreateParticleEffects()
    {
        CreateMuzzleFlashEffect();
        CreateDestructionEffect();
    }
    
    private void CreateMuzzleFlashEffect()
    {
        // Create muzzle flash GameObject
        GameObject muzzleFlashGO = new GameObject("MuzzleFlashEffect");
        
        // Add ParticleSystem component
        ParticleSystem muzzleFlash = muzzleFlashGO.AddComponent<ParticleSystem>();
        
        // Configure main module
        var main = muzzleFlash.main;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.playOnAwake = false;
        main.loop = false;
        main.maxParticles = 50;
        main.startSpeed = 8f;
        main.startSize = 0.3f;
        main.startLifetime = 0.1f;
        main.startColor = new Color(1f, 0.8f, 0.2f, 1f); // Orange-yellow flash
        
        // Configure emission module
        var emission = muzzleFlash.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f; // No continuous emission
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 50) // Burst of 50 particles at start
        });
        
        // Configure shape module
        var shape = muzzleFlash.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.1f;
        shape.length = 0.5f;
        
        // Configure velocity over lifetime
        var velocityOverLifetime = muzzleFlash.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(10f); // Forward velocity
        
        // Configure size over lifetime
        var sizeOverLifetime = muzzleFlash.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0f);
        sizeCurve.AddKey(0.1f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Configure color over lifetime for fade out
        var colorOverLifetime = muzzleFlash.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
        );
        colorOverLifetime.color = gradient;
        
        Debug.Log("Muzzle flash effect created successfully");
        
        // Destroy this setup object after creating effects
        Destroy(gameObject, 1f);
    }
    
    private void CreateDestructionEffect()
    {
        // Create destruction effect GameObject
        GameObject destructionGO = new GameObject("DestructionEffect");
        
        // Add ParticleSystem component
        ParticleSystem destruction = destructionGO.AddComponent<ParticleSystem>();
        
        // Configure main module
        var main = destruction.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.loop = false;
        main.maxParticles = 30;
        main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startColor = new Color(1f, 1f, 0.2f, 1f); // Yellow explosion
        main.gravityModifier = 0.5f; // Some gravity for realistic fall
        
        // Configure emission module
        var emission = destruction.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f; // No continuous emission
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0.0f, 30) // Burst of 30 particles at start
        });
        
        // Configure shape module
        var shape = destruction.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;
        
        // Configure velocity over lifetime for spread
        var velocityOverLifetime = destruction.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(2f);
        
        // Configure size over lifetime
        var sizeOverLifetime = destruction.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.3f, 1.2f);
        sizeCurve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        
        // Configure color over lifetime for fade out
        var colorOverLifetime = destruction.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.yellow, 0.0f), 
                new GradientColorKey(Color.red, 0.5f),
                new GradientColorKey(Color.gray, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        colorOverLifetime.color = gradient;
        
        Debug.Log("Destruction effect created successfully");
    }
}