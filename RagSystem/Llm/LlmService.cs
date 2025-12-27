using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;

namespace RagSystem.Llm;

/// <summary>
/// Handles chat completions using Azure OpenAI (SDK 2.1.0)
/// </summary>
public class LlmService
{
    private readonly ChatClient _client;

    public LlmService(string endpoint, string apiKey, string deploymentName)
    {
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(endpoint)
        };

        var openAiClient = new OpenAIClient(
            new Azure.AzureKeyCredential(apiKey),
            options);

        // NEW in 2.x
        _client = openAiClient.GetChatClient(deploymentName);
    }

    public string Ask(string context, string question)
    {
        var messages = new ChatMessage[]
        {
            ChatMessage.CreateSystemMessage(
                "Answer ONLY from the provided context."
            ),

            ChatMessage.CreateUserMessage(
                $"Context:\n{context}\n\nQuestion:\n{question}"
            )
        };

        var response = _client.CompleteChat(messages);

        return response.Value.Content[0].Text;
    }
}
