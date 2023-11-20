using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using Azure.Search.Documents;

namespace DevDay.ApiService.Extensions;

internal static class ServiceCollectionExtensions
{
    private static readonly TokenCredential s_azureCredential = new DefaultAzureCredential();

    internal static IServiceCollection AddAzureServices(this IServiceCollection services)
    {
        services.AddSingleton<SearchClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var (azureSearchService, azureSearchIndex) =
                (config["AZURE_SEARCH_SERVICE"], config["AZURE_SEARCH_INDEX"]);

            ArgumentException.ThrowIfNullOrEmpty(azureSearchService);

            Uri azureSearchServiceEndpoint = new(azureSearchService);
            var searchClient = new SearchClient(azureSearchServiceEndpoint, azureSearchIndex, s_azureCredential);

            return searchClient;
        });

        services.AddSingleton<OpenAIClient>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var (openAiHost, openAiKey, azureOpenAiService) =
                (config["OPENAI_HOST"], config["OPENAI_API_KEY"], config["AZURE_OPENAI_SERVICE"]);

            if (openAiHost == "azure")
            {
                ArgumentException.ThrowIfNullOrEmpty(azureOpenAiService);
                Uri azureOpenAiServiceEndpoint = new(azureOpenAiService);

                return new OpenAIClient(azureOpenAiServiceEndpoint, s_azureCredential);
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(openAiKey);
            return new OpenAIClient(openAiKey);
        });

        return services;
    }
}
