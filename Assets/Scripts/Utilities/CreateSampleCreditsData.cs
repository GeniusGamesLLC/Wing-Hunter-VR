using UnityEngine;

/// <summary>
/// Utility script to create a sample CreditsData ScriptableObject asset
/// This can be run in the editor to generate a sample credits configuration
/// </summary>
public class CreateSampleCreditsData : MonoBehaviour
{
    [Header("Sample Credits Creation")]
    [SerializeField] private bool createSampleCredits = false;
    
    private void Start()
    {
        if (createSampleCredits)
        {
            CreateSampleCreditsAsset();
        }
    }
    
    /// <summary>
    /// Creates a sample CreditsData asset with common attributions
    /// </summary>
    public void CreateSampleCreditsAsset()
    {
        CreditsData creditsData = ScriptableObject.CreateInstance<CreditsData>();
        
        // Create sample credits array
        AssetCredit[] sampleCredits = new AssetCredit[]
        {
            new AssetCredit(
                "Weapons of Choice FREE",
                "Komposite Sound",
                "Free for commercial and non-commercial use",
                "Unity Asset Store",
                "Gun sound effects provided by Komposite Sound - Weapons of Choice FREE package"
            ),
            new AssetCredit(
                "XR Interaction Toolkit",
                "Unity Technologies",
                "Unity Package License",
                "Unity Package Manager",
                "VR functionality powered by Unity XR Interaction Toolkit"
            ),
            new AssetCredit(
                "TextMeshPro",
                "Unity Technologies", 
                "Unity Package License",
                "Unity Package Manager",
                "Text rendering using Unity TextMeshPro"
            ),
            new AssetCredit(
                "Unity Engine",
                "Unity Technologies",
                "Unity Personal/Pro License",
                "Unity Technologies",
                "Built with Unity Engine"
            )
        };
        
        // Use reflection to set the private array (for setup purposes)
        var creditsDataType = typeof(CreditsData);
        var assetCreditsField = creditsDataType.GetField("assetCredits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        assetCreditsField?.SetValue(creditsData, sampleCredits);
        
#if UNITY_EDITOR
        // Save as asset in the editor
        UnityEditor.AssetDatabase.CreateAsset(creditsData, "Assets/Data/SampleCreditsData.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log("Sample CreditsData asset created at Assets/Data/SampleCreditsData.asset");
#else
        Debug.Log("Sample credits data created (runtime). To save as asset, run this in the Unity Editor.");
#endif
    }
}