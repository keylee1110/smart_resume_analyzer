namespace TheParser.Models;

/// <summary>
/// Enumeration of supported file types for resume processing
/// </summary>
public enum FileType
{
    /// <summary>
    /// Unknown or unsupported file type
    /// </summary>
    Unknown,
    
    /// <summary>
    /// PDF document format
    /// </summary>
    Pdf,
    
    /// <summary>
    /// Microsoft Word DOCX format
    /// </summary>
    Docx
}
