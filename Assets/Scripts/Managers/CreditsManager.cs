using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreditsManager : MonoBehaviour
{
    [Header("Credits Configuration")]
    [SerializeField] private CreditsData creditsData;
    
    [Header("UI References")]
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button closeCreditsButton;
    [SerializeField] private ScrollRect creditsScrollRect;
    
    private void Start()
    {
        InitializeCreditsSystem();
    }
    
    private void InitializeCreditsSystem()
    {
        // Ensure credits panel is initially hidden
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
        
        // Wire up button events
        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(ShowCredits);
        }
        
        if (closeCreditsButton != null)
        {
            closeCreditsButton.onClick.AddListener(HideCredits);
        }
        
        // Load credits text if data is available
        LoadCreditsText();
    }
    
    private void LoadCreditsText()
    {
        if (creditsData != null && creditsText != null)
        {
            creditsText.text = creditsData.GetFormattedCreditsText();
        }
        else if (creditsText != null)
        {
            creditsText.text = "Credits data not available.";
        }
    }
    
    /// <summary>
    /// Show the credits panel
    /// </summary>
    public void ShowCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
            
            // Reset scroll position to top
            if (creditsScrollRect != null)
            {
                creditsScrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }
    
    /// <summary>
    /// Hide the credits panel
    /// </summary>
    public void HideCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Toggle credits panel visibility
    /// </summary>
    public void ToggleCredits()
    {
        if (creditsPanel != null)
        {
            if (creditsPanel.activeInHierarchy)
            {
                HideCredits();
            }
            else
            {
                ShowCredits();
            }
        }
    }
    
    /// <summary>
    /// Update credits data at runtime
    /// </summary>
    public void SetCreditsData(CreditsData newCreditsData)
    {
        creditsData = newCreditsData;
        LoadCreditsText();
    }
    
    private void OnDestroy()
    {
        // Clean up button listeners
        if (creditsButton != null)
        {
            creditsButton.onClick.RemoveListener(ShowCredits);
        }
        
        if (closeCreditsButton != null)
        {
            closeCreditsButton.onClick.RemoveListener(HideCredits);
        }
    }
}