using UnityEngine;

public class GunRayVisualizer : MonoBehaviour
{
    [Header("Ray Settings")]
    [SerializeField] private float rayLength = 50f;
    [SerializeField] private float rayWidth = 0.005f;
    [SerializeField] private Color rayColor = Color.red;
    [SerializeField] private bool showRay = true;
    
    [Header("References")]
    [SerializeField] private GunSelectionManager gunSelectionManager;
    [SerializeField] private ShootingController shootingController;
    
    private LineRenderer lineRenderer;
    private Transform currentMuzzlePoint;
    
    private void Awake()
    {
        SetupLineRenderer();
        
        if (gunSelectionManager == null)
            gunSelectionManager = FindObjectOfType<GunSelectionManager>();
        
        if (shootingController == null)
            shootingController = FindObjectOfType<ShootingController>();
    }
    
    private void Start()
    {
        if (gunSelectionManager != null)
        {
            gunSelectionManager.OnGunChanged.AddListener(OnGunChanged);
            
            if (gunSelectionManager.CurrentGun != null)
                OnGunChanged(gunSelectionManager.CurrentGun);
        }
    }
    
    private void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = rayWidth;
        lineRenderer.endWidth = rayWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = rayColor;
        lineRenderer.endColor = rayColor;
        lineRenderer.useWorldSpace = true;
        lineRenderer.enabled = showRay;
    }
    
    private void OnGunChanged(GunData gunData)
    {
        if (gunData != null && gunData.muzzlePoint != null)
        {
            currentMuzzlePoint = gunData.muzzlePoint;
            Debug.Log($"GunRayVisualizer: Updated muzzle point for {gunData.gunName}");
        }
    }
    
    private void Update()
    {
        if (!showRay || lineRenderer == null)
        {
            if (lineRenderer != null)
                lineRenderer.enabled = false;
            return;
        }
        
        UpdateMuzzlePointReference();
        
        if (currentMuzzlePoint != null)
        {
            lineRenderer.enabled = true;
            Vector3 startPos = currentMuzzlePoint.position;
            Vector3 endPos = startPos + currentMuzzlePoint.forward * rayLength;
            
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }
    
    private void UpdateMuzzlePointReference()
    {
        if (currentMuzzlePoint == null && gunSelectionManager != null)
        {
            GunData currentGun = gunSelectionManager.CurrentGun;
            if (currentGun != null && currentGun.muzzlePoint != null)
                currentMuzzlePoint = currentGun.muzzlePoint;
        }
    }
    
    public void SetShowRay(bool show)
    {
        showRay = show;
        if (lineRenderer != null)
            lineRenderer.enabled = show;
    }
    
    public void SetRayColor(Color color)
    {
        rayColor = color;
        if (lineRenderer != null)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }
    }
    
    public void SetRayWidth(float width)
    {
        rayWidth = width;
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
        }
    }
    
    public void SetRayLength(float length)
    {
        rayLength = length;
    }
    
    private void OnDestroy()
    {
        if (gunSelectionManager != null)
            gunSelectionManager.OnGunChanged.RemoveListener(OnGunChanged);
    }
}
