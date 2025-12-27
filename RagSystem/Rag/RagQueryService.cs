using RagSystem.Chunking;
using RagSystem.Embeddings;
using RagSystem.Llm;
using RagSystem.VectorStore;

namespace RagSystem.Rag;

/// <summary>
/// Coordinates retrieval + generation.
/// This is where RAG actually happens.
/// </summary>
public class RagQueryService
{
    private readonly TextChunker _chunker;
    private readonly EmbeddingService _embedding;
    private readonly InMemoryVectorStore _store;
    private readonly LlmService _llm;

    public RagQueryService(
        TextChunker chunker,
        EmbeddingService embedding,
        InMemoryVectorStore store,
        LlmService llm)
    {
        _chunker = chunker;
        _embedding = embedding;
        _store = store;
        _llm = llm;
    }

    public void Index(string text)
    {
        foreach (var chunk in _chunker.Chunk(text))
        {
            var vector = _embedding.GenerateEmbedding(chunk);
            _store.Add(chunk, vector);
        }
    }

    public string Query(string question)
    {
        var queryVector = _embedding.GenerateEmbedding(question);
        var relevantChunks = _store.Search(queryVector, topK: 5);

        var context = string.Join("\n\n", relevantChunks);
        return _llm.Ask(context, question);
    }
}
