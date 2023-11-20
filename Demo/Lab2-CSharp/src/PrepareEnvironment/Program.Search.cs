using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents;
using PrepareEnvironment.Models;
using Azure;
using PrepareEnvironment;

internal static partial class Program
{
    private static SearchIndexClient GetSearchIndexClient(Options options)
    {
        Uri endpoint = new Uri(options.SearchService);

        if (string.IsNullOrEmpty(options.SearchKey))
        {
            return new SearchIndexClient(endpoint, DefaultCredential);
        }

        return new SearchIndexClient(endpoint, new AzureKeyCredential(options.SearchKey));
    }

    private static SearchClient GetSearchClient(Options options)
    {
        Uri endpoint = new Uri(options.SearchService);

        if (string.IsNullOrEmpty(options.SearchKey))
        {
            return new SearchClient(endpoint, options.Index, DefaultCredential);
        }

        return new SearchClient(endpoint, options.Index, new AzureKeyCredential(options.SearchKey));
    }

    private static async Task CreateIndexAsync(Options options, Hotel[] hotels)
    {
        try
        {
            SearchIndexClient client = GetSearchIndexClient(options);
            SearchIndex searchIndex = GetIndex(options.Index);


            await client.DeleteIndexAsync(searchIndex);
            await client.CreateIndexAsync(searchIndex);

            SearchClient searchClient = GetSearchClient(options);
            await searchClient.UploadDocumentsAsync(hotels);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static SearchIndex GetIndex(string name)
    {
        IList<SearchField> fields = new FieldBuilder()
            .Build(typeof(Hotel));

        return new SearchIndex(name, fields)
        {
            CorsOptions = new CorsOptions(["*"]) { MaxAgeInSeconds = 60 },
            VectorSearch = new VectorSearch
            {
                Algorithms = { new HnswAlgorithmConfiguration("my-vector-config") },
                Profiles = { new VectorSearchProfile(Hotel.VECTOR_SEARCH_PROFILE_NAME, "my-vector-config") }
            }
        };
    }
}
