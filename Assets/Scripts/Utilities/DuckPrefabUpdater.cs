using UnityEngine;

public class DuckPrefabUpdater : MonoBehaviour
{
    [Header("Duck Prefab Integration")]
    [SerializeField] private GameObject duckPrefab;
    [SerializeField] private bool updateOnStart = true;
    
    private void Start()
    {
        if (updateOnStart)
        {
            UpdateDuckPrefab();
            
            // Destroy this GameObject after updating
            Destroy(gameObject, 1f);
        }
    }
    
    public void UpdateDuckPrefab()
    {
        if (duckPrefab == null)
        {
            // Try to find the duck prefab
            duckPrefab = Resources.Load<GameObject>("Duck");
            if (duckPrefab == null)
            {
                Debug.LogWarning("Duck prefab not found. Please assign it manually.");
                return;
            }
        }
        
        // Instantiate the duck prefab to modify it
        GameObject duckInstance = Instantiate(duckPrefab);
        
        // Add destruction particle system if it doesn't exist
        ParticleSystem existingParticles = duckInstance.GetComponent<ParticleSystem>();
        if (existingParticles == null)
        {
            // Create destruction particle system
            ParticleSystem destructionParticles = duckInstance.AddComponent<ParticleSystem>();
            
            // Configure main module
            var main = destructionParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.playOnAwake = false;
            main.loop = false;
            main.maxParticles = 30;
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
            main.startColor = new Color(1f, 1f, 0.2f, 1f); // Yellow explosion
            main.gravityModifier = 0.5f;
            
            // Configure emission module
            var emission = destructionParticles.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[]
            {
                new ParticleSystem.Burst(0.0f, 30)
            });
            
            // Configure shape module
            var shape = destructionParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;
            
            // Configure velocity over lifetime for spread
            var velocityOverLifetime = destructionParticles.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
            velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(2f);
            
            // Configure size over lifetime
            var sizeOverLifetime = destructionParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(0.3f, 1.2f);
            sizeCurve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            
            // Configure color over lifetime for fade out
            var colorOverLifetime = destructionParticles.colorOverLifetime;
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
            
            Debug.Log("Added destruction particle system to duck prefab");
        }
        
        // Update the DuckController to reference the particle system
        DuckController duckController = duckInstance.GetComponent<DuckController>();
        if (duckController != null)
        {
            ParticleSystem particles = duckInstance.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                // Use reflection to set the destructionParticles field
                var field = typeof(DuckController).GetField("destructionParticles");
                if (field != null)
                {
                    field.SetValue(duckController, particles);
                    Debug.Log("Updated DuckController to reference destruction particles");
                }
            }
        }
        
        Debug.Log("Duck prefab updated successfully");
        
        // Clean up the instance
        Destroy(duckInstance, 0.1f);
    }
}