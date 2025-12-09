namespace TheChatbot.Models; // Namespace adjusted

/// <summary>
/// Represents entities extracted from CV text
/// </summary>
public class ExtractedEntities
{
    /// <summary>
    /// Candidate's name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Email address
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Phone number
    /// </summary>
    public string? Phone { get; set; }
    
    /// <summary>
    /// List of skills
    /// </summary>
    public List<string> Skills { get; set; } = new();
    
    /// <summary>
    /// Extraction method used: "Comprehend" or "Regex"
    /// </summary>
    public string Method { get; set; } = string.Empty;
    
    /// <summary>
    /// Total number of entities found
    /// </summary>
    public int TotalEntitiesFound { get; set; }
}
