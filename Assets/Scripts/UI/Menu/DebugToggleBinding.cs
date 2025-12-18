using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using DuckHunt.Data;

/// <summary>
/// Runtime binding between a UI Toggle and a DebugSettings boolean property.
/// Handles bidirectional synchronization between the toggle state and the property value.
/// </summary>
public class DebugToggleBinding
{
    /// <summary>
    /// The PropertyInfo for the bound DebugSettings property.
    /// </summary>
    public PropertyInfo Property { get; private set; }

    /// <summary>
    /// Reference to the UI Toggle component.
    /// </summary>
    public Toggle UIToggle { get; private set; }

    /// <summary>
    /// The category this toggle belongs to (from DebugCategoryAttribute).
    /// </summary>
    public string Category { get; private set; }

    /// <summary>
    /// The display name shown on the toggle label.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Optional tooltip text (from TooltipAttribute).
    /// </summary>
    public string Tooltip { get; private set; }

    /// <summary>
    /// Creates a new DebugToggleBinding.
    /// </summary>
    /// <param name="property">The PropertyInfo for the DebugSettings property</param>
    /// <param name="toggle">The UI Toggle component</param>
    /// <param name="category">The category name</param>
    /// <param name="displayName">The display name for the toggle</param>
    /// <param name="tooltip">Optional tooltip text</param>
    public DebugToggleBinding(PropertyInfo property, Toggle toggle, string category, string displayName, string tooltip = null)
    {
        Property = property ?? throw new ArgumentNullException(nameof(property));
        UIToggle = toggle ?? throw new ArgumentNullException(nameof(toggle));
        Category = category ?? "Uncategorized";
        DisplayName = displayName ?? property.Name;
        Tooltip = tooltip;
    }

    /// <summary>
    /// Synchronizes the toggle state FROM the DebugSettings property value.
    /// Call this when DebugSettings changes externally.
    /// </summary>
    public void SyncFromSettings()
    {
        if (UIToggle == null || Property == null)
        {
            Debug.LogWarning("[DebugToggleBinding] Cannot sync - toggle or property is null");
            return;
        }

        try
        {
            var settings = DebugSettings.Instance;
            if (settings == null)
            {
                Debug.LogWarning("[DebugToggleBinding] DebugSettings instance is null");
                return;
            }

            bool currentValue = (bool)Property.GetValue(settings);
            
            // Only update if different to avoid triggering unnecessary events
            if (UIToggle.isOn != currentValue)
            {
                // Temporarily remove listener to avoid feedback loop
                UIToggle.SetIsOnWithoutNotify(currentValue);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DebugToggleBinding] Error syncing from settings for '{DisplayName}': {ex.Message}");
        }
    }

    /// <summary>
    /// Synchronizes the DebugSettings property TO match the toggle state.
    /// Call this when the toggle value changes via UI interaction.
    /// </summary>
    public void SyncToSettings()
    {
        if (UIToggle == null || Property == null)
        {
            Debug.LogWarning("[DebugToggleBinding] Cannot sync - toggle or property is null");
            return;
        }

        try
        {
            var settings = DebugSettings.Instance;
            if (settings == null)
            {
                Debug.LogWarning("[DebugToggleBinding] DebugSettings instance is null");
                return;
            }

            bool toggleValue = UIToggle.isOn;
            bool currentValue = (bool)Property.GetValue(settings);

            // Only update if different to avoid triggering unnecessary events
            if (currentValue != toggleValue)
            {
                Property.SetValue(settings, toggleValue);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DebugToggleBinding] Error syncing to settings for '{DisplayName}': {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the current value from DebugSettings.
    /// </summary>
    /// <returns>The current boolean value of the property</returns>
    public bool GetCurrentValue()
    {
        try
        {
            var settings = DebugSettings.Instance;
            if (settings != null && Property != null)
            {
                return (bool)Property.GetValue(settings);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DebugToggleBinding] Error getting value for '{DisplayName}': {ex.Message}");
        }
        return false;
    }

    /// <summary>
    /// Sets the value in DebugSettings directly.
    /// </summary>
    /// <param name="value">The value to set</param>
    public void SetValue(bool value)
    {
        try
        {
            var settings = DebugSettings.Instance;
            if (settings != null && Property != null)
            {
                Property.SetValue(settings, value);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[DebugToggleBinding] Error setting value for '{DisplayName}': {ex.Message}");
        }
    }
}
