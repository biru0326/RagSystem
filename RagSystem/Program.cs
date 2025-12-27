using RagSystem.Chunking;
using RagSystem.Config;
using RagSystem.Embeddings;
using RagSystem.Extraction;
using RagSystem.Llm;
using RagSystem.Rag;
using RagSystem.VectorStore;

var settings = new RagSettings
{
    AzureOpenAiEndpoint = "https://YOUR-RESOURCE.openai.azure.com/",
    AzureOpenAiKey = "YOUR-KEY",

    ChatDeployment = "chat-deploy",
    EmbeddingDeployment = "embed-deploy"
};

// Infrastructure
var ragService = new RagQueryService(
    new TextChunker(settings.ChunkSize, settings.ChunkOverlap),
    new EmbeddingService(
        settings.AzureOpenAiEndpoint,
        settings.AzureOpenAiKey,
        settings.EmbeddingDeployment),
    new InMemoryVectorStore(),
    new LlmService(
        settings.AzureOpenAiEndpoint,
        settings.AzureOpenAiKey,
        settings.ChatDeployment)
);

// Extraction
var extractionService = new TextExtractionService();

// Index confidential document
string text = extractionService.Extract("confidential.docx");
ragService.Index(text);

// Query
string answer = ragService.Query("Why was job marked poison pill?");
Console.WriteLine(answer);
