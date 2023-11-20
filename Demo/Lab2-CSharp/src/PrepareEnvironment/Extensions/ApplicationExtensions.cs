using Azure.AI.OpenAI;

namespace PrepareEnvironment.Extensions;

internal static class ApplicationExtensions
{
    internal static async Task<EmbeddingItem[]> GetEmbeddingsBatchedAsync(this OpenAIClient client, string deploymentName, IEnumerable<string> input)
    {
        if (string.IsNullOrEmpty(deploymentName))
        {
            deploymentName = "text-embedding-ada-002";
        }

        var chunks = await Task.WhenAll(input
            .Chunk(16)
            .Select(chunk => client.GetEmbeddingsAsync(new EmbeddingsOptions(deploymentName, chunk))));

        return chunks
            .SelectMany(static result => result.Value.Data)
            .ToArray();
    }
}