using CommandLine;
// ReSharper disable StringLiteralTypo

namespace PrepareEnvironment;

public class Options
{
    [Value(0, Required = true, MetaName = "Input file to be processed")]
    public required string Filename { get; init; }

    [Option("openaihost", Required = true, Default = "azure", HelpText = "Host of the API used to compute embeddings ('azure' or 'openai')")]
    public required string OpenAIHost { get; init; }

    [Option("openaiservice", HelpText = "Name of the Azure OpenAI service used to compute embeddings")]
    public required string OpenAIService { get; init; }

    [Option("openaikey", HelpText = "Optional. Use this Azure OpenAI account key instead of the current user identity to login (use az login to set current user for Azure). This is required only when using non-Azure endpoints.")]
    public required string OpenAIKey { get; init; }

    [Option("openaimodelname", Default = "text-embedding-ada-002", HelpText = "Name of the Azure OpenAI embedding model ('text-embedding-ada-002' recommended)")]
    public required string OpenAIModelName { get; init; }

    [Option("index", Required = true, HelpText = "Name of the Azure Cognitive Search index where content should be indexed (will be created if it doesn't exist)")]
    public required string Index { get; init; }

    [Option("searchservice", Required = true, HelpText = "Name of the Azure AI Search service where content should be indexed (must exist already)")]
    public required string SearchService { get; init; }

    [Option("searchkey", HelpText = "Optional. Use this Azure AI Search account key instead of the current user identity to login (use az login to set current user for Azure)")]
    public required string SearchKey { get; init; }
}