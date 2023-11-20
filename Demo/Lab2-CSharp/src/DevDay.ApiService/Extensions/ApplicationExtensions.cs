using Azure.AI.OpenAI;
using Azure.Search.Documents.Models;
using Azure.Search.Documents;
using Azure;
using System.Runtime.CompilerServices;
using System.Text;
using DevDay.ApiService.Models;
using DevDay.Contracts;

namespace DevDay.ApiService.Extensions;

internal static class ApplicationExtensions
{
    internal static WebApplication MapApi(this WebApplication app)
    {
        app.MapGroup("api")
            .MapPost("openai/chat", OnPostChatPromptAsync);

        return app;
    }

    private static async IAsyncEnumerable<ChatChunkResponse> OnPostChatPromptAsync(PromptRequest prompt,
        OpenAIClient client, SearchClient searchClient, IConfiguration config,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var searchTerm = await GenerateSearchTermAsync(client, config, prompt.Prompt, cancellationToken);

        var results = await SearchHotelsAsync(client, searchClient, config, searchTerm, cancellationToken);

        StreamingResponse<StreamingChatCompletionsUpdate> response = await GenerateResponseAsync(client, config, prompt, results, cancellationToken);

        await foreach (var message in response.EnumerateValues().WithCancellation(cancellationToken))
        {
            if (message is { ContentUpdate.Length: > 0 })
            {
                var (length, content) = (message.ContentUpdate.Length, message.ContentUpdate);
                yield return new ChatChunkResponse(length, content);
            }
        }
    }

    /// <summary>
    /// Asks ChatGPT to generate a search term, based on a query from an end-user.
    /// Where a user can be very elaborate in describing the question, you only need a couple of keywords for the search to get correct hits.
    /// </summary>
    private static async Task<string> GenerateSearchTermAsync(OpenAIClient client, IConfiguration config, string prompt, CancellationToken cancellationToken)
    {
        var deploymentId = config["AZURE_OPENAI_CHATGPT_DEPLOYMENT"];

        var response = await client.GetChatCompletionsStreamingAsync(new ChatCompletionsOptions(deploymentId, [
            new ChatMessage(ChatRole.System, """
                                             Your task is to create a search term useful for Azure Cognitive Search.
                                             Provide a search term for the following prompt:
                                             """),

            new ChatMessage(ChatRole.User, prompt)
        ]), cancellationToken);

        var builder = new StringBuilder();
        await foreach (var message in response.EnumerateValues().WithCancellation(cancellationToken))
        {
            if (message is { ContentUpdate.Length: > 0 })
            {
                builder.Append(message.ContentUpdate);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Generates a vector for the specified searchTerm, this can be used for the vectorized query in Azure Cognitive Search.
    /// </summary>
    private static async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(OpenAIClient client, IConfiguration config, string searchTerm, CancellationToken cancellationToken)
    {
        var deploymentId = config["AZURE_OPENAI_EMB_DEPLOYMENT"];

        var response =
            await client.GetEmbeddingsAsync(new EmbeddingsOptions(deploymentId, [searchTerm]), cancellationToken);

        return response.Value.Data[0].Embedding;
    }

    /// <summary>
    /// Searches for matches with the specified searchTerm, which is matched to the "TextVector" index that was generated during deployment.
    /// </summary>
    private static async Task<Response<SearchResults<Hotel>>> SearchHotelsAsync(OpenAIClient client, SearchClient searchClient, IConfiguration config, string searchTerm, CancellationToken cancellationToken)
    {
        SearchOptions searchOptions = new()
        {
            VectorSearch = new VectorSearchOptions
            {
                Queries =
                {
                    new VectorizedQuery(await GenerateEmbeddingAsync(client, config, searchTerm, cancellationToken))
                    {
                        KNearestNeighborsCount = 3,
                        Fields = { "TextVector" }
                    }
                }
            },
            Select =
            {
                nameof(Hotel.Id),
                nameof(Hotel.HotelName),
                nameof(Hotel.Description),
                nameof(Hotel.Text),
                nameof(Hotel.Location),
                nameof(Hotel.Features),
                nameof(Hotel.PriceMin),
                nameof(Hotel.PriceMax),
            },
            Size = 5
        };

        Response<SearchResults<Hotel>>? results = await searchClient.SearchAsync<Hotel>(searchTerm, searchOptions, cancellationToken);
        return results;
    }

    /// <summary>
    /// Asks ChatGPT to return a well written answer, based on the data from the search index.
    /// </summary>
    private static async Task<StreamingResponse<StreamingChatCompletionsUpdate>> GenerateResponseAsync(OpenAIClient client, IConfiguration config, PromptRequest prompt,
        Response<SearchResults<Hotel>> results, CancellationToken cancellationToken)
    {
        var builder = new StringBuilder()
            .Append("""
                    Assistant is a large language model designed to help users find hotels.
                    You have access to an Azure Cognitive Search index with hundreds of hotels.
                    This information returned from the search to answer the users question
                    You will always reply with a Markdown formatted response.
                    """);

        await foreach (var result in results.Value.GetResultsAsync())
        {
            builder
                .Append("Hotel: ")
                .AppendLine(result.Document.Text);
        }

        builder
            .Append("""
                    If you don't know the answer, just say that you don't know. Don't try to make up an answer.
                    Give your response as a story, not a list.
                    """);

        var deploymentId = config["AZURE_OPENAI_CHATGPT_DEPLOYMENT"];
        var response = await client.GetChatCompletionsStreamingAsync(new ChatCompletionsOptions(deploymentId, [
            new ChatMessage(ChatRole.System, builder.ToString()),
            new ChatMessage(ChatRole.User, prompt.Prompt)
        ]), cancellationToken);
        return response;
    }
}
