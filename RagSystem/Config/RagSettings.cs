namespace RagSystem.Config;

/// <summary>
/// Central configuration for the RAG system.
/// Keeps secrets OUT of code.
/// </summary>
public class RagSettings
{
    public string AzureOpenAiEndpoint { get; set; }
    public string AzureOpenAiKey { get; set; }

    public string ChatDeployment { get; set; }
    public string EmbeddingDeployment { get; set; }

    public int ChunkSize { get; set; } = 500;
    public int ChunkOverlap { get; set; } = 50;
}
