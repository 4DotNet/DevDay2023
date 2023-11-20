using Azure.Identity;
using CommandLine;
using PrepareEnvironment;
using PrepareEnvironment.Models;
using Azure.Core;

await Parser.Default
    .ParseArguments<Options>(args)
    .WithParsedAsync(async options =>
    {
        // Load the Hotel data and generate embeddings
        Hotel[] hotels = await LoadHotelsAsync(options);

        // Builds a search index, based on the Hotel model and uploads the loaded hotels data.
        await CreateIndexAsync(options, hotels);
    });

internal static partial class Program
{
    private static TokenCredential DefaultCredential { get; } = new DefaultAzureCredential();
}
