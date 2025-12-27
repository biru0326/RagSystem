namespace RagSystem.Chunking;

/// <summary>
/// Splits large text into overlapping chunks.
/// Why?
/// LLMs have token limits and embeddings work better on small text.
/// </summary>
public class TextChunker
{
    private readonly int _chunkSize;
    private readonly int _overlap;

    public TextChunker(int chunkSize, int overlap)
    {
        _chunkSize = chunkSize;
        _overlap = overlap;
    }

    public IEnumerable<string> Chunk(string text)
    {
        for (int i = 0; i < text.Length; i += _chunkSize - _overlap)
        {
            yield return text.Substring(
                i,
                Math.Min(_chunkSize, text.Length - i));

            if (i + _chunkSize >= text.Length)
                yield break;
        }
    }
}
