using UnityEngine;
using TMPro;

/// <summary>
/// Reusable world-space debug text overlay for VR.
/// Follows a target transform and faces the camera.
/// </summary>
public class VRDebugOverlay : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private float fontSize = 18f;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField] private float outlineWidth = 0.15f;
    [SerializeField] private float worldScale = 0.01f;
    
    [Header("Position")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 offset = new Vector3(0, 0.2f, 0);
    
    private TextMeshPro textMesh;
    private Camera mainCam;
    
    private static VRDebugOverlay _instance;
    public static VRDebugOverlay Instance => _instance;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }
    
    private void Start()
    {
        mainCam = Camera.main;
        CreateTextMesh();
    }

    private void CreateTextMesh()
    {
        textMesh = gameObject.AddComponent<TextMeshPro>();
        textMesh.fontSize = fontSize;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.color = textColor;
        textMesh.outlineColor = outlineColor;
        textMesh.outlineWidth = outlineWidth;
        transform.localScale = Vector3.one * worldScale;
    }
    
    private void LateUpdate()
    {
        if (followTarget != null)
        {
            transform.position = followTarget.position + offset;
        }
        
        if (mainCam != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
        }
    }
    
    public void SetText(string text)
    {
        if (textMesh != null)
            textMesh.text = text;
    }
    
    public void SetFollowTarget(Transform target, Vector3? customOffset = null)
    {
        followTarget = target;
        if (customOffset.HasValue)
            offset = customOffset.Value;
    }
    
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
    
    public static VRDebugOverlay Create(string name = "VRDebugOverlay")
    {
        var go = new GameObject(name);
        return go.AddComponent<VRDebugOverlay>();
    }
}
