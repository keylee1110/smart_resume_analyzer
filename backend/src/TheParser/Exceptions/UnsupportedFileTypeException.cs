namespace TheParser.Exceptions;

/// <summary>
/// Exception thrown when a file with an unsupported extension is uploaded
/// </summary>
public class UnsupportedFileTypeException : ParserException
{
    /// <summary>
    /// The unsupported file extension
    /// </summary>
    public string Extension { get; }
    
    /// <summary>
    /// Creates a new UnsupportedFileTypeException
    /// </summary>
    /// <param name="extension">The unsupported file extension</param>
    public UnsupportedFileTypeException(string extension)
        : base($"File extension '{extension}' is not supported. Only .pdf and .docx files are allowed.", "UNSUPPORTED_FILE_TYPE")
    {
        Extension = extension;
    }
}
