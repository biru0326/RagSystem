namespace RagSystem.VectorStore;

/// <summary>
/// Simple vector store for understanding.
/// Replace with Azure AI Search later.
/// </summary>
public class InMemoryVectorStore
{
    private readonly List<(string Text, float[] Vector)> _items = new();

    public void Add(string text, float[] vector)
    {
        _items.Add((text, vector));
    }

    public IEnumerable<string> Search(float[] queryVector, int topK)
    {
        return _items
            .Select(i => new
            {
                i.Text,
                Score = CosineSimilarity(i.Vector, queryVector)
            })
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => x.Text);
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        float dot = 0, magA = 0, magB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        return dot / ((float)Math.Sqrt(magA) * (float)Math.Sqrt(magB));
    }
}
