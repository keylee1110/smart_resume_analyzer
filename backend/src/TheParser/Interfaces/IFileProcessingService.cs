namespace TheParser.Interfaces;

using TheParser.Models;

/// <summary>
/// Service for processing uploaded files and extracting text
/// </summary>
public interface IFileProcessingService
{
    /// <summary>
    /// Processes a file from S3 and extracts text based on file type
    /// </summary>
    /// <param name="bucketName">S3 bucket name</param>
    /// <param name="objectKey">S3 object key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processing result with extracted text and metadata</returns>
    Task<FileProcessingResult> ProcessFileAsync(
        string bucketName, 
        string objectKey, 
        CancellationToken cancellationToken = default);
}
