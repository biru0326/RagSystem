using UglyToad.PdfPig;

namespace RagSystem.Extraction;

/// <summary>
/// Extracts text from PDF files.
/// </summary>
public class PdfTextExtractor : ITextExtractor
{
    public bool CanHandle(string filePath)
        => filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(string filePath)
    {
        using var pdf = PdfDocument.Open(filePath);

        // Combine text from all pages
        return string.Join("\n", pdf.GetPages().Select(p => p.Text));
    }
}
