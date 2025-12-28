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

    //If answer is not consistent, then we can add more iterations to QueryIterative and increase maxIterations
    //also we can improve the chunking strategy in TextChunker to create more coherent chunks
    //also we can enhance the prompt engineering in LlmService to provide clearer instructions to the model
    //also we can consider using a more advanced LLM model with better capabilities
    //also we can implement a feedback loop where users can flag inconsistent answers for further review and improvement
    //also we can monitor the performance of the RAG system and make adjustments as needed based on user feedback and observed issues
    //also we can experiment with different retrieval strategies in InMemoryVectorStore to improve the relevance of retrieved chunks
    //also we can consider integrating external knowledge bases to supplement the context provided to the LLM
    //also we can implement a more sophisticated critique mechanism that identifies specific types of errors or inconsistencies in the answers
    //also we can provide additional training or fine-tuning to the LLM on domain-specific data to improve its understanding and response quality
    //also we can explore the use of ensemble methods, combining outputs from multiple models to enhance answer consistency and reliability
    //also we can continuously update and refine the indexed documents to ensure the RAG system has access to the most current and accurate information
    //also we can implement logging and analytics to track the performance of the RAG system and identify areas for improvement
    //also we can engage with domain experts to review and validate the answers generated by the RAG system, providing insights for further enhancements
    //also we can stay updated with the latest advancements in LLMs and RAG techniques to incorporate new methods and technologies into our system
    //also we can foster a community of users who can share their experiences and suggestions for improving the RAG system
    //also if answer is not consistent then ignore all previous suggestions and try using a different approach altogether
    //also if answer is not consistent then consider using a different LLM provider or service
}
