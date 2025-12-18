using System;

namespace DuckHunt.Data
{
    /// <summary>
    /// Attribute for categorizing debug properties in DebugSettings.
    /// Used by the Debug Paper UI to group related toggles under category headers.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DebugCategoryAttribute : Attribute
    {
        /// <summary>
        /// The category name for grouping this debug property
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// Creates a new DebugCategoryAttribute with the specified category name
        /// </summary>
        /// <param name="category">The category name (e.g., "Path Visualization", "Point Indicators")</param>
        public DebugCategoryAttribute(string category)
        {
            Category = category ?? throw new ArgumentNullException(nameof(category));
        }
    }
}
