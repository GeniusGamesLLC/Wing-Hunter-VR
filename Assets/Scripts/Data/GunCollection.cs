using UnityEngine;

[CreateAssetMenu(fileName = "GunCollection", menuName = "Game/Gun Collection")]
public class GunCollection : ScriptableObject
{
    [Header("Available Guns")]
    [SerializeField] private GunData[] availableGuns = new GunData[0];
    
    [Header("Default Selection")]
    [SerializeField] private int defaultGunIndex = 0;
    
    public GunData[] AvailableGuns => availableGuns;
    public int DefaultGunIndex => Mathf.Clamp(defaultGunIndex, 0, availableGuns.Length - 1);
    
    /// <summary>
    /// Get gun data by index
    /// </summary>
    public GunData GetGun(int index)
    {
        if (index >= 0 && index < availableGuns.Length)
        {
            return availableGuns[index];
        }
        return null;
    }
    
    /// <summary>
    /// Get gun data by name
    /// </summary>
    public GunData GetGun(string gunName)
    {
        foreach (var gun in availableGuns)
        {
            if (gun.gunName.Equals(gunName, System.StringComparison.OrdinalIgnoreCase))
            {
                return gun;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Get the index of a gun by name
    /// </summary>
    public int GetGunIndex(string gunName)
    {
        for (int i = 0; i < availableGuns.Length; i++)
        {
            if (availableGuns[i].gunName.Equals(gunName, System.StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// Get default gun data
    /// </summary>
    public GunData GetDefaultGun()
    {
        return GetGun(DefaultGunIndex);
    }
    
    /// <summary>
    /// Validate all guns in the collection
    /// </summary>
    public bool ValidateCollection()
    {
        if (availableGuns == null || availableGuns.Length == 0)
        {
            Debug.LogWarning("Gun collection is empty.");
            return false;
        }
        
        bool allValid = true;
        for (int i = 0; i < availableGuns.Length; i++)
        {
            if (availableGuns[i] == null || !availableGuns[i].IsValid())
            {
                Debug.LogWarning($"Gun at index {i} is invalid or missing required data.");
                allValid = false;
            }
        }
        
        return allValid;
    }
    
    /// <summary>
    /// Get gun names for UI display
    /// </summary>
    public string[] GetGunNames()
    {
        string[] names = new string[availableGuns.Length];
        for (int i = 0; i < availableGuns.Length; i++)
        {
            names[i] = availableGuns[i]?.gunName ?? $"Gun {i + 1}";
        }
        return names;
    }
}