using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;

namespace RagSystem.Llm;

/// <summary>
/// Handles chat completions using the OpenAI SDK with an Azure endpoint override.
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

        _client = openAiClient.GetChatClient(deploymentName);
    }

    /// <summary>
    /// Basic question answering using provided context.
    /// </summary>
    public string Ask(string context, string question)
    {
        var messages = new ChatMessage[]
        {
            ChatMessage.CreateSystemMessage(
                "Answer ONLY using the provided context."
            ),
            ChatMessage.CreateUserMessage(
                BuildPrompt(context, question)
            )
        };

        var response = _client.CompleteChat(messages);
        return response.Value.Content[0].Text;
    }

    /// <summary>
    /// Executes a chat request with a custom system instruction.
    /// Used for critique, revision, or controlled reasoning.
    /// </summary>
    public string AskWithInstructions(string systemInstruction, string userMessage)
    {
        var messages = new ChatMessage[]
        {
            ChatMessage.CreateSystemMessage(systemInstruction),
            ChatMessage.CreateUserMessage(userMessage)
        };

        var response = _client.CompleteChat(messages);
        return response.Value.Content[0].Text;
    }

    // ----------------- Helpers -----------------

    private static string BuildPrompt(string context, string question)
    {
        return $"""
        Context:
        {context}

        Question:
        {question}
        """;
    }
}
