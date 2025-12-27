using RagSystem.Chunking;
using RagSystem.Embeddings;
using RagSystem.Llm;
using RagSystem.VectorStore;

namespace RagSystem.Rag;

/// <summary>
/// Coordinates retrieval + generation.
/// Includes an optional critique-and-revise loop for higher-quality answers.
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

    /// <summary>
    /// Indexes raw text by chunking and embedding.
    /// </summary>
    public void Index(string text)
    {
        foreach (var chunk in _chunker.Chunk(text))
        {
            var vector = _embedding.GenerateEmbedding(chunk);
            _store.Add(chunk, vector);
        }
    }

    /// <summary>
    /// Basic RAG query (retrieve → answer).
    /// </summary>
    public string Query(string question)
    {
        var context = RetrieveContext(question);
        return _llm.Ask(context, question);
    }

    /// <summary>
    /// RAG query with a single critique + revision pass.
    /// This improves factual alignment and reduces hallucination.
    /// </summary>
    public string QueryWithCritique(string question)
    {
        var context = RetrieveContext(question);

        // 1. Initial answer
        var initialAnswer = _llm.AskWithInstructions(
            systemInstruction: "Answer ONLY using the provided context.",
            userMessage: BuildQaPrompt(context, question)
        );

        // 2. Critique
        var critique = _llm.AskWithInstructions(
            systemInstruction: "You are a strict reviewer.",
            userMessage: $"""
            Context:
            {context}

            Question:
            {question}

            Answer:
            {initialAnswer}

            Critique the answer:
            - Is it fully supported by the context?
            - Is anything missing, unclear, or incorrect?
            - Does it make assumptions beyond the context?
            """
        );

        // 3. Revision
        var revisedAnswer = _llm.AskWithInstructions(
            systemInstruction: "Revise the answer using the critique. Do NOT add new information.",
            userMessage: $"""
            Context:
            {context}

            Original Answer:
            {initialAnswer}

            Critique:
            {critique}

            Provide the improved final answer:
            """
        );

        return revisedAnswer;
    }

    /// <summary>
    /// RAG query with multiple critique/revision iterations.
    /// </summary>
    public string QueryIterative(string question, int maxIterations = 2)
    {
        var context = RetrieveContext(question);
        var answer = _llm.Ask(context, question);

        for (int i = 0; i < maxIterations; i++)
        {
            var critique = _llm.AskWithInstructions(
                systemInstruction: "You are a strict reviewer. Identify weaknesses only.",
                userMessage: $"""
                Context:
                {context}

                Question:
                {question}

                Answer:
                {answer}
                """
            );

            answer = _llm.AskWithInstructions(
                systemInstruction: "Improve the answer using the critique. Stay strictly within the context.",
                userMessage: $"""
                Context:
                {context}

                Previous Answer:
                {answer}

                Critique:
                {critique}
                """
            );
        }

        return answer;
    }

    // ----------------- Helper Methods -----------------

    private string RetrieveContext(string question, int topK = 5)
    {
        var queryVector = _embedding.GenerateEmbedding(question);
        var relevantChunks = _store.Search(queryVector, topK);
        return string.Join("\n\n", relevantChunks);
    }

    private static string BuildQaPrompt(string context, string question)
    {
        return $"""
        Context:
        {context}

        Question:
        {question}
        """;
    }
}
