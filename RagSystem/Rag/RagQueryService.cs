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


    //Example of enriching chunks with context before indexing

    //public void Index(string text)
    //{
    //    foreach (var chunk in _chunker.Chunk(text))
    //    {
    //        // Optional: add neutral context to improve semantic consistency
    //        var enrichedChunk = $"""
    //    Document type: general text
    //    Content:
    //    {chunk}
    //    """;

    //        var vector = _embedding.GenerateEmbedding(enrichedChunk);

    //        _store.Add(new VectorRecord
    //        {
    //            Text = chunk,
    //            Vector = vector,
    //            Metadata = new Dictionary<string, string>
    //            {
    //                ["source"] = "sample",
    //                ["type"] = "text"
    //            }
    //        });
    //    }
    //}


    public string Query(string question)
    {
        var queryVector = _embedding.GenerateEmbedding(question);
        var relevantChunks = _store.Search(queryVector, topK: 5);

        var context = string.Join("\n\n", relevantChunks);
        return _llm.Ask(context, question);
    }
}
