namespace TheParser.Models;

/// <summary>
/// Result of file processing operation containing extracted text and metadata
/// </summary>
public class FileProcessingResult
{
    /// <summary>
    /// The extracted and normalized text content from the document
    /// </summary>
    public string ExtractedText { get; set; } = string.Empty;
    
    /// <summary>
    /// The detected file type (PDF or DOCX)
    /// </summary>
    public FileType FileType { get; set; }
    
    /// <summary>
    /// S3 bucket name where the file is stored
    /// </summary>
    public string BucketName { get; set; } = string.Empty;
    
    /// <summary>
    /// S3 object key of the file
    /// </summary>
    public string ObjectKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Timestamp when the file was processed
    /// </summary>
    public DateTime ProcessedAt { get; set; }
    
    /// <summary>
    /// Number of pages in the document (for PDFs)
    /// </summary>
    public int PageCount { get; set; }
    
    /// <summary>
    /// Indicates whether the processing was successful
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Error message if processing failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
