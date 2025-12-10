using UnityEngine;

[System.Serializable]
public class GunData
{
    [Header("Gun Information")]
    public string gunName;
    public string description;
    public GameObject gunPrefab;
    
    [Header("Gun Properties")]
    public float fireRate = 1.0f;
    public float hapticIntensity = 0.5f;
    public float muzzleFlashScale = 1.0f;
    
    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    
    [Header("Visual Effects")]
    public GameObject muzzleFlashPrefab;
    public Transform muzzlePoint; // Will be set at runtime from the gun prefab
    
    [Header("UI")]
    public Sprite gunIcon;
    public Texture2D gunPreview;
    
    public bool IsValid()
    {
        return gunPrefab != null && !string.IsNullOrEmpty(gunName);
    }
}