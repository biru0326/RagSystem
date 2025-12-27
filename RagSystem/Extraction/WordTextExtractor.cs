using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace RagSystem.Extraction;

/// <summary>
/// Extracts plain text from Word (.docx) files.
/// Why this approach:
/// - No dependency on MS Word
/// - Safe for server / AKS environments
/// - Reads document structure directly
/// </summary>
public class WordTextExtractor : ITextExtractor
{
    public bool CanHandle(string filePath)
        => filePath.EndsWith(".docx", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(string filePath)
    {
        using var document = WordprocessingDocument.Open(filePath, false);

        var body = document.MainDocumentPart?.Document?.Body;
        if (body == null)
            return string.Empty;

        // Combine all paragraph texts
        return string.Join(
            Environment.NewLine,
            body.Descendants<Text>().Select(t => t.Text)
        );
    }
}
