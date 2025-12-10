using UnityEngine;

/// <summary>
/// Helper script for credits management in the editor
/// Provides utilities for creating and managing credits data
/// </summary>
public class CreditsEditorHelper : MonoBehaviour
{
    [Header("Credits Management")]
    [SerializeField] private CreditsData creditsDataAsset;
    
    [Header("Export Options")]
    [SerializeField] private bool exportToMarkdown = false;
    [SerializeField] private string markdownFilePath = "CREDITS_Export.md";
    
    private void Start()
    {
        if (exportToMarkdown && creditsDataAsset != null)
        {
            ExportCreditsToMarkdown();
        }
    }
    
    /// <summary>
    /// Export credits data to markdown format
    /// </summary>
    public void ExportCreditsToMarkdown()
    {
        if (creditsDataAsset == null)
        {
            Debug.LogWarning("No CreditsData asset assigned for export.");
            return;
        }
        
        string markdownContent = GenerateMarkdownContent();
        
        try
        {
            System.IO.File.WriteAllText(markdownFilePath, markdownContent);
            Debug.Log($"Credits exported to {markdownFilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to export credits: {e.Message}");
        }
    }
    
    private string GenerateMarkdownContent()
    {
        System.Text.StringBuilder markdown = new System.Text.StringBuilder();
        
        markdown.AppendLine("# VR Duck Hunt - Asset Credits");
        markdown.AppendLine();
        markdown.AppendLine("This document contains attribution information for all third-party assets used in the VR Duck Hunt project.");
        markdown.AppendLine();
        
        var credits = creditsDataAsset.AssetCredits;
        
        if (credits != null && credits.Length > 0)
        {
            foreach (var credit in credits)
            {
                if (!string.IsNullOrEmpty(credit.assetName))
                {
                    markdown.AppendLine($"## {credit.assetName}");
                    markdown.AppendLine();
                    
                    if (!string.IsNullOrEmpty(credit.author))
                        markdown.AppendLine($"- **Author**: {credit.author}");
                        
                    if (!string.IsNullOrEmpty(credit.license))
                        markdown.AppendLine($"- **License**: {credit.license}");
                        
                    if (!string.IsNullOrEmpty(credit.source))
                        markdown.AppendLine($"- **Source**: {credit.source}");
                        
                    if (!string.IsNullOrEmpty(credit.attributionText))
                    {
                        markdown.AppendLine($"- **Attribution**: {credit.attributionText}");
                    }
                    
                    markdown.AppendLine();
                }
            }
        }
        else
        {
            markdown.AppendLine("No credits data available.");
            markdown.AppendLine();
        }
        
        markdown.AppendLine("---");
        markdown.AppendLine();
        markdown.AppendLine("*This file was automatically generated from the CreditsData ScriptableObject.*");
        
        return markdown.ToString();
    }
    
    /// <summary>
    /// Validate credits data for completeness
    /// </summary>
    public void ValidateCreditsData()
    {
        if (creditsDataAsset == null)
        {
            Debug.LogWarning("No CreditsData asset assigned for validation.");
            return;
        }
        
        var credits = creditsDataAsset.AssetCredits;
        int validCredits = 0;
        int incompleteCredits = 0;
        
        if (credits != null)
        {
            foreach (var credit in credits)
            {
                bool isComplete = !string.IsNullOrEmpty(credit.assetName) &&
                                !string.IsNullOrEmpty(credit.author) &&
                                !string.IsNullOrEmpty(credit.license);
                
                if (isComplete)
                {
                    validCredits++;
                }
                else
                {
                    incompleteCredits++;
                    Debug.LogWarning($"Incomplete credit entry: {credit.assetName}");
                }
            }
        }
        
        Debug.Log($"Credits validation complete: {validCredits} valid, {incompleteCredits} incomplete entries.");
    }
    
    /// <summary>
    /// Get credits statistics
    /// </summary>
    public void GetCreditsStatistics()
    {
        if (creditsDataAsset == null)
        {
            Debug.LogWarning("No CreditsData asset assigned.");
            return;
        }
        
        var credits = creditsDataAsset.AssetCredits;
        
        if (credits == null)
        {
            Debug.Log("No credits data found.");
            return;
        }
        
        Debug.Log($"Total credits entries: {credits.Length}");
        Debug.Log($"Formatted text length: {creditsDataAsset.GetFormattedCreditsText().Length} characters");
    }
}