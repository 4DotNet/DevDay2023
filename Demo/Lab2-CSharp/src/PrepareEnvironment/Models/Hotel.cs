using Azure.Search.Documents.Indexes;

namespace PrepareEnvironment.Models;

public record Hotel
{
    public const string VECTOR_SEARCH_PROFILE_NAME = "hotel-vector-profile";

    [SimpleField(IsKey = true, IsFilterable = true)]
    public required string Id { get; init; }

    [SearchableField]
    public required string HotelName { get; init; }

    [SearchableField]
    public required string City { get; init; }

    [SearchableField]
    public required string Country { get; init; }

    [SearchableField]
    public required string Location { get; init; }

    [SimpleField(IsFilterable = true)]
    public required int PriceMin { get; init; }

    [SimpleField(IsFilterable = true)]
    public required int PriceMax { get; init; }

    [SearchableField]
    public required string Features { get; init; }

    [SearchableField]
    public required string Description { get; init; }

    [SearchableField]
    public string? Text { get; set; }

    [VectorSearchField(VectorSearchDimensions = 1536, VectorSearchProfileName = VECTOR_SEARCH_PROFILE_NAME)]
    public float[]? TextVector { get; set; }

    [VectorSearchField(VectorSearchDimensions = 1536, VectorSearchProfileName = VECTOR_SEARCH_PROFILE_NAME)]
    public float[]? FeaturesVector { get; set; }
}
