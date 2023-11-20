namespace DevDay.ApiService.Models;

public record Hotel(
    string Id,
    string HotelName,
    string Description,
    string Text,
    string Location,
    string Features,
    int PriceMin,
    int PriceMax);
