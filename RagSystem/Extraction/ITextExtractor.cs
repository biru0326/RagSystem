namespace RagSystem.Extraction;

/// <summary>
/// Contract for extracting plain text from files.
/// </summary>
public interface ITextExtractor
{
    bool CanHandle(string filePath);
    string ExtractText(string filePath);
}
