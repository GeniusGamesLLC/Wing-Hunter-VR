using UnityEngine;

/// <summary>
/// Utility script to help set up gun collection with the imported gun assets
/// </summary>
public class GunCollectionSetup : MonoBehaviour
{
    [Header("Gun Collection Setup")]
    [SerializeField] private bool createGunCollection = false;
    [SerializeField] private string collectionAssetName = "VRDuckHuntGuns";
    
    [Header("Gun Prefab References")]
    [SerializeField] private GameObject nZapGunPrefab;
    [SerializeField] private GameObject zapperGunPrefab;
    
    [Header("Audio References")]
    [SerializeField] private AudioClip defaultFireSound;
    [SerializeField] private AudioClip defaultReloadSound;
    
    private void Start()
    {
        if (createGunCollection)
        {
            CreateGunCollectionAsset();
        }
    }
    
    /// <summary>
    /// Create a gun collection asset with the imported gun models
    /// </summary>
    public void CreateGunCollectionAsset()
    {
        // Load gun prefabs if not assigned
        if (nZapGunPrefab == null)
        {
            nZapGunPrefab = Resources.Load<GameObject>("Assets/Import/N-ZAP_85/N-ZAP_85.prefab");
        }
        
        if (zapperGunPrefab == null)
        {
            zapperGunPrefab = Resources.Load<GameObject>("Assets/Import/Nintendo_Zapper_Light_Gun/Nintendo_Zapper_Light_Gun.prefab");
        }
        
        // Create gun collection
        GunCollection gunCollection = ScriptableObject.CreateInstance<GunCollection>();
        
        // Create gun data array
        GunData[] guns = new GunData[2];
        
        // N-ZAP 85 Gun
        guns[0] = new GunData
        {
            gunName = "N-ZAP 85",
            description = "A futuristic water gun design with sleek aesthetics. Perfect for VR duck hunting with its ergonomic grip and modern styling.",
            gunPrefab = nZapGunPrefab,
            fireRate = 1.2f,
            hapticIntensity = 0.6f,
            muzzleFlashScale = 1.0f,
            fireSound = defaultFireSound,
            reloadSound = defaultReloadSound
        };
        
        // Nintendo Zapper Light Gun
        guns[1] = new GunData
        {
            gunName = "Nintendo Zapper",
            description = "The classic Nintendo Zapper light gun. Nostalgic design that brings back memories of the original Duck Hunt game.",
            gunPrefab = zapperGunPrefab,
            fireRate = 1.0f,
            hapticIntensity = 0.5f,
            muzzleFlashScale = 0.8f,
            fireSound = defaultFireSound,
            reloadSound = defaultReloadSound
        };
        
        // Use reflection to set the private array (for setup purposes)
        var gunCollectionType = typeof(GunCollection);
        var availableGunsField = gunCollectionType.GetField("availableGuns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var defaultGunIndexField = gunCollectionType.GetField("defaultGunIndex", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        availableGunsField?.SetValue(gunCollection, guns);
        defaultGunIndexField?.SetValue(gunCollection, 1); // Default to Nintendo Zapper for nostalgia
        
#if UNITY_EDITOR
        // Save as asset in the editor
        string assetPath = $"Assets/Data/{collectionAssetName}.asset";
        UnityEditor.AssetDatabase.CreateAsset(gunCollection, assetPath);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log($"Gun collection asset created at {assetPath}");
        
        // Also create a sample CreditsData with gun attributions
        CreateGunCreditsData();
#else
        Debug.Log("Gun collection created (runtime). To save as asset, run this in the Unity Editor.");
#endif
    }
    
    /// <summary>
    /// Create or update credits data with gun asset attributions
    /// </summary>
    private void CreateGunCreditsData()
    {
#if UNITY_EDITOR
        // Try to load existing credits data
        CreditsData creditsData = UnityEditor.AssetDatabase.LoadAssetAtPath<CreditsData>("Assets/Data/VRDuckHuntCredits.asset");
        
        if (creditsData == null)
        {
            creditsData = ScriptableObject.CreateInstance<CreditsData>();
        }
        
        // Get existing credits
        var existingCredits = creditsData.AssetCredits;
        var creditsList = new System.Collections.Generic.List<AssetCredit>();
        
        if (existingCredits != null)
        {
            creditsList.AddRange(existingCredits);
        }
        
        // Add gun credits
        AssetCredit nZapCredit = new AssetCredit(
            "N-ZAP 85",
            "ThePinguFan2006",
            "CC-BY-4.0",
            "https://sketchfab.com/3d-models/n-zap-85-72e1be01b71348239a603f6c9116940f",
            "This work is based on \"N-ZAP 85\" by ThePinguFan2006 licensed under CC-BY-4.0"
        );
        
        AssetCredit zapperCredit = new AssetCredit(
            "Nintendo Zapper Light Gun",
            "Matt Wilson",
            "CC-BY-4.0",
            "https://sketchfab.com/3d-models/nintendo-zapper-light-gun-44dceff49a8b4397ac940a8fd470d471",
            "This work is based on \"Nintendo Zapper Light Gun\" by Matt Wilson licensed under CC-BY-4.0"
        );
        
        // Check if credits already exist to avoid duplicates
        bool nZapExists = false;
        bool zapperExists = false;
        
        foreach (var credit in creditsList)
        {
            if (credit.assetName == "N-ZAP 85") nZapExists = true;
            if (credit.assetName == "Nintendo Zapper Light Gun") zapperExists = true;
        }
        
        if (!nZapExists) creditsList.Add(nZapCredit);
        if (!zapperExists) creditsList.Add(zapperCredit);
        
        // Update credits data
        var creditsDataType = typeof(CreditsData);
        var assetCreditsField = creditsDataType.GetField("assetCredits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        assetCreditsField?.SetValue(creditsData, creditsList.ToArray());
        
        // Save credits data
        string creditsPath = "Assets/Data/VRDuckHuntCredits.asset";
        if (UnityEditor.AssetDatabase.LoadAssetAtPath<CreditsData>(creditsPath) == null)
        {
            UnityEditor.AssetDatabase.CreateAsset(creditsData, creditsPath);
        }
        else
        {
            UnityEditor.EditorUtility.SetDirty(creditsData);
        }
        
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log($"Credits data updated with gun attributions at {creditsPath}");
#endif
    }
    
    /// <summary>
    /// Validate gun prefab references
    /// </summary>
    public void ValidateGunPrefabs()
    {
        Debug.Log("=== Gun Prefab Validation ===");
        
        // Check N-ZAP 85
        if (nZapGunPrefab != null)
        {
            Debug.Log($"✓ N-ZAP 85 prefab found: {nZapGunPrefab.name}");
        }
        else
        {
            Debug.LogWarning("✗ N-ZAP 85 prefab not assigned or found");
        }
        
        // Check Nintendo Zapper
        if (zapperGunPrefab != null)
        {
            Debug.Log($"✓ Nintendo Zapper prefab found: {zapperGunPrefab.name}");
        }
        else
        {
            Debug.LogWarning("✗ Nintendo Zapper prefab not assigned or found");
        }
        
        // Try to find prefabs by path
        if (nZapGunPrefab == null || zapperGunPrefab == null)
        {
            Debug.Log("Attempting to find prefabs by path...");
            
#if UNITY_EDITOR
            if (nZapGunPrefab == null)
            {
                string nZapPath = "Assets/Import/N-ZAP_85/N-ZAP_85.prefab";
                nZapGunPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(nZapPath);
                if (nZapGunPrefab != null)
                {
                    Debug.Log($"✓ Found N-ZAP 85 at: {nZapPath}");
                }
            }
            
            if (zapperGunPrefab == null)
            {
                string zapperPath = "Assets/Import/Nintendo_Zapper_Light_Gun/Nintendo_Zapper_Light_Gun.prefab";
                zapperGunPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(zapperPath);
                if (zapperGunPrefab != null)
                {
                    Debug.Log($"✓ Found Nintendo Zapper at: {zapperPath}");
                }
            }
#endif
        }
        
        Debug.Log("=== Validation Complete ===");
    }
}