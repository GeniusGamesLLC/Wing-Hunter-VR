using UnityEngine;

[CreateAssetMenu(fileName = "CreditsData", menuName = "Game/Credits Data")]
public class CreditsData : ScriptableObject
{
    [Header("Project Credits")]
    [SerializeField] private AssetCredit[] assetCredits = new AssetCredit[0];
    
    public AssetCredit[] AssetCredits => assetCredits;
    
    /// <summary>
    /// Get formatted credits text for display
    /// </summary>
    public string GetFormattedCreditsText()
    {
        if (assetCredits == null || assetCredits.Length == 0)
        {
            return "No credits available.";
        }
        
        System.Text.StringBuilder creditsText = new System.Text.StringBuilder();
        creditsText.AppendLine("CREDITS\n");
        
        foreach (var credit in assetCredits)
        {
            if (!string.IsNullOrEmpty(credit.assetName))
            {
                creditsText.AppendLine($"Asset: {credit.assetName}");
                
                if (!string.IsNullOrEmpty(credit.author))
                    creditsText.AppendLine($"Author: {credit.author}");
                    
                if (!string.IsNullOrEmpty(credit.license))
                    creditsText.AppendLine($"License: {credit.license}");
                    
                if (!string.IsNullOrEmpty(credit.source))
                    creditsText.AppendLine($"Source: {credit.source}");
                    
                if (!string.IsNullOrEmpty(credit.attributionText))
                    creditsText.AppendLine($"Attribution: {credit.attributionText}");
                    
                creditsText.AppendLine(); // Empty line between credits
            }
        }
        
        return creditsText.ToString();
    }
}