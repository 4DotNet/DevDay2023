using Azure.AI.OpenAI;
using PrepareEnvironment.Models;
using System.Text.Json;
using System.Text;
using PrepareEnvironment;
using PrepareEnvironment.Extensions;
using Azure;

internal static partial class Program
{
    private static OpenAIClient GetOpenAIClient(Options options)
    {
        if (string.Equals(options.OpenAIHost, "azure", StringComparison.InvariantCultureIgnoreCase))
        {
            Uri endpoint = new Uri(options.OpenAIService);
            if (string.IsNullOrEmpty(options.OpenAIKey))
            {
                return new OpenAIClient(endpoint, DefaultCredential);
            }

            return new OpenAIClient(endpoint, new AzureKeyCredential(options.OpenAIKey));
        }

        return new OpenAIClient(options.OpenAIKey);
    }

    private static async Task<Hotel[]> LoadHotelsAsync(Options options)
    {
        Console.WriteLine(
            $"Creating embeddings for hotels using model {options.OpenAIModelName} and openai service {options.OpenAIService}");

        OpenAIClient client = GetOpenAIClient(options);

        await using FileStream file = File.OpenRead(options.Filename);

        JsonSerializerOptions serializerOptions = new() { PropertyNameCaseInsensitive = true };
        Hotel[] hotels = await JsonSerializer.DeserializeAsync<Hotel[]>(file, serializerOptions) ??
                         throw new InvalidOperationException($"Failed to parse file: {options.Filename}");

        List<string> input = new();
        List<string> features = new();
        foreach (var hotel in hotels)
        {
            // Creating the text for the embedding
            StringBuilder textForEmbedding = new();
            textForEmbedding.Append("Name:").Append(hotel.HotelName).Append(' ')
                .Append("Price min: ").Append(hotel.PriceMin).Append(" euro ")
                .Append("max: ").Append(hotel.PriceMax).Append(" euro ")
                .Append("Location: City: ").Append(hotel.City).Append(' ')
                .Append("Country: ").Append(hotel.Country).Append(' ')
                .Append("Features of the hotel: ").Append(hotel.Features).Append(' ')
                .Append("Description: ").Append(hotel.Description);

            hotel.Text = textForEmbedding.ToString();

            input.Add(hotel.Text);
            features.Add(hotel.Features);
        }

        // Creating the embeddings for the text and the features
        EmbeddingItem[] textVectors = await client.GetEmbeddingsBatchedAsync(options.OpenAIModelName, input);
        EmbeddingItem[] featuresVectors = await client.GetEmbeddingsBatchedAsync(options.OpenAIModelName, features);

        foreach ((Hotel hotel, EmbeddingItem textVector, EmbeddingItem featuresVector) in hotels.Zip(textVectors, featuresVectors))
        {
            hotel.TextVector = textVector.Embedding.ToArray();
            hotel.FeaturesVector = featuresVector.Embedding.ToArray();
        }

        return hotels;
    }
}
