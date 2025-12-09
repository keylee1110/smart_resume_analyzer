namespace TheParser.Interfaces;

/// <summary>
/// Service for extracting text from DOCX files
/// </summary>
public interface IDocxExtractor
{
    /// <summary>
    /// Extracts text from a DOCX file in S3
    /// </summary>
    /// <param name="bucketName">S3 bucket name</param>
    /// <param name="objectKey">S3 object key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Extracted text content</returns>
    Task<string> ExtractTextFromDocxAsync(
        string bucketName, 
        string objectKey, 
        CancellationToken cancellationToken = default);
}
