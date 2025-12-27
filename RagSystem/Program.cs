using RagSystem.Chunking;
using RagSystem.Config;
using RagSystem.Embeddings;
using RagSystem.Extraction;
using RagSystem.Llm;
using RagSystem.Rag;
using RagSystem.VectorStore;

// ---------------- Configuration ----------------

var settings = new RagSettings
{
    AzureOpenAiEndpoint = "https://YOUR-RESOURCE.openai.azure.com/",
    AzureOpenAiKey = "YOUR-KEY",

    ChatDeployment = "chat-deploy",
    EmbeddingDeployment = "embed-deploy",

    ChunkSize = 500,
    ChunkOverlap = 50
};

// ---------------- Infrastructure ----------------

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

// ---------------- Extraction ----------------

var extractionService = new TextExtractionService();

// Index document
string documentText = extractionService.Extract("sample.docx");
ragService.Index(documentText);

// ---------------- Queries ----------------

string question = "Why was the job marked as a poison pill?";

// 1️⃣ Basic RAG (fastest)
string basicAnswer = ragService.Query(question);
Console.WriteLine("=== BASIC ANSWER ===");
Console.WriteLine(basicAnswer);
Console.WriteLine();

// 2️⃣ RAG with critique + revision (recommended default)
string improvedAnswer = ragService.QueryWithCritique(question);
Console.WriteLine("=== CRITIQUE + REVISED ANSWER ===");
Console.WriteLine(improvedAnswer);
Console.WriteLine();

// 3️⃣ Iterative RAG (highest quality, slower)
string iterativeAnswer = ragService.QueryIterative(question, maxIterations: 2);
Console.WriteLine("=== ITERATIVE ANSWER ===");
Console.WriteLine(iterativeAnswer);
