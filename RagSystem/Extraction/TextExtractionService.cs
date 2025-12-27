namespace RagSystem.Extraction;

/// <summary>
/// Central service that selects the correct text extractor
/// based on file type.
/// 
/// Why this exists:
/// - Avoids if/else chains
/// - Open for extension (add new extractors)
/// - Closed for modification (RAG code never changes)
/// </summary>
public class TextExtractionService
{
    private readonly List<ITextExtractor> _extractors;

    public TextExtractionService()
    {
        // Register all supported extractors here
        _extractors = new List<ITextExtractor>
        {
            new PdfTextExtractor(),
            new WordTextExtractor(),
            new ExcelTextExtractor()
        };
    }

    /// <summary>
    /// Extracts text from any supported document type.
    /// Throws if unsupported file is provided.
    /// </summary>
    public string Extract(string filePath)
    {
        var extractor = _extractors
            .FirstOrDefault(e => e.CanHandle(filePath));

        if (extractor == null)
            throw new NotSupportedException(
                $"No extractor registered for file: {filePath}");

        return extractor.ExtractText(filePath);
    }
}
