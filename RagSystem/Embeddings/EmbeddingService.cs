using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Embeddings;

namespace RagSystem.Embeddings;

/// <summary>
/// Generates vector embeddings using Azure OpenAI (SDK 2.1.0)
/// </summary>
public class EmbeddingService
{
    private readonly OpenAI.Embeddings.EmbeddingClient _client;

    public EmbeddingService(string endpoint, string apiKey, string deploymentName)
    {
        var openAiClient = new OpenAIClient(
            new Azure.AzureKeyCredential(apiKey));

        // NEW in 2.x: capability-specific client
        _client = openAiClient.GetEmbeddingClient(deploymentName);
    }

    public float[] GenerateEmbedding(string text)
    {
        var response = _client.GenerateEmbedding(text);
        return response.Value.ToFloats().Span.ToArray();
    }

}
