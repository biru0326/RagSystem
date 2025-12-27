using ExcelDataReader;
using System.Text;

namespace RagSystem.Extraction;

/// <summary>
/// Extracts text from Excel (.xlsx) files.
/// Why this approach:
/// - Reads cell values safely
/// - No Excel dependency
/// - Handles large sheets efficiently
/// </summary>
public class ExcelTextExtractor : ITextExtractor
{
    public bool CanHandle(string filePath)
        => filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
        || filePath.EndsWith(".xls", StringComparison.OrdinalIgnoreCase);

    public string ExtractText(string filePath)
    {
        // Required for ExcelDataReader on .NET Core / .NET 8
        System.Text.Encoding.RegisterProvider(
            System.Text.CodePagesEncodingProvider.Instance);

        var sb = new StringBuilder();

        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        do
        {
            while (reader.Read()) // Row
            {
                for (int col = 0; col < reader.FieldCount; col++)
                {
                    var value = reader.GetValue(col);
                    if (value != null)
                        sb.Append(value).Append(' ');
                }
                sb.AppendLine();
            }
        }
        while (reader.NextResult()); // Next sheet

        return sb.ToString();
    }
}
