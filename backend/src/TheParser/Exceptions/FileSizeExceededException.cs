namespace TheParser.Exceptions;

/// <summary>
/// Exception thrown when an uploaded file exceeds the maximum allowed size
/// </summary>
public class FileSizeExceededException : ParserException
{
    /// <summary>
    /// The actual file size in bytes
    /// </summary>
    public long FileSize { get; }
    
    /// <summary>
    /// The maximum allowed file size in bytes
    /// </summary>
    public long MaxSize { get; }
    
    /// <summary>
    /// Creates a new FileSizeExceededException
    /// </summary>
    /// <param name="fileSize">Actual file size in bytes</param>
    /// <param name="maxSize">Maximum allowed file size in bytes</param>
    public FileSizeExceededException(long fileSize, long maxSize)
        : base($"File size {fileSize} bytes exceeds maximum {maxSize} bytes", "FILE_TOO_LARGE")
    {
        FileSize = fileSize;
        MaxSize = maxSize;
    }
}
