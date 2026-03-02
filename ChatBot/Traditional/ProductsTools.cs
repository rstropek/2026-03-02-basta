using ChatBotDb;
using Microsoft.EntityFrameworkCore;
using OpenAI.Responses;

namespace ChatBot.Traditional;

public static class ProductsTools
{
    public static readonly FunctionTool GetAvailableColorsForFlowerTool = ResponseTool.CreateFunctionTool(
        functionName: nameof(GetAvailableColorsForFlower),
        functionDescription: "Gets a list of available colors for a specific flower",
        functionParameters: FunctionHelpers.ToJsonSchema<GetAvailableColorsForFlowerRequest>(),
        strictModeEnabled: false
    );

    public static readonly FunctionTool GetBouquetSizesTool = ResponseTool.CreateFunctionTool(
        functionName: nameof(GetBouquetSizes),
        functionDescription: "Gets the list of available bouquet sizes (e.g. Small, Medium, Large)",
        functionParameters: BinaryData.FromString("{}"),
        strictModeEnabled: false
    );

    public static readonly FunctionTool GetBouquetPriceTool = ResponseTool.CreateFunctionTool(
        functionName: nameof(GetBouquetPrice),
        functionDescription: "Gets the price, number of flowers, and description for a specific bouquet size",
        functionParameters: FunctionHelpers.ToJsonSchema<GetBouquetPriceRequest>(),
        strictModeEnabled: false
    );

    private static readonly Dictionary<string, List<string>> FlowerColors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Rose"] = ["red", "yellow", "purple"],
        ["Lily"] = ["yellow", "pink", "white"],
        ["Gerbera"] = ["pink", "red", "yellow"],
        ["Freesia"] = ["white", "pink", "red", "yellow"],
        ["Tulip"] = ["red", "yellow", "purple"],
        ["Sunflower"] = ["yellow"]
    };

    public static IEnumerable<string> GetAvailableColorsForFlower(GetAvailableColorsForFlowerRequest request)
    {
        return FlowerColors.TryGetValue(request.FlowerName, out var colors) ? colors : [];
    }

    public static async Task<List<string>> GetBouquetSizes(ApplicationDataContext db)
    {
        return await db.BouquetPrices
            .Select(b => b.Size)
            .ToListAsync();
    }

    public static async Task<BouquetPriceDto?> GetBouquetPrice(ApplicationDataContext db, GetBouquetPriceRequest request)
    {
        var bouquet = await db.BouquetPrices
            .FirstOrDefaultAsync(b => b.Size.ToLower() == request.Size.ToLower());
        if (bouquet is null)
        {
            return null;
        }

        return new BouquetPriceDto(bouquet.Size, bouquet.NumberOfFlowers, bouquet.Description, bouquet.Price);
    }

    public record GetAvailableColorsForFlowerRequest(string FlowerName);
    public record GetBouquetPriceRequest(string Size);
    public record BouquetPriceDto(string Size, int NumberOfFlowers, string Description, decimal Price);
}