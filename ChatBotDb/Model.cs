namespace ChatBotDb;

public class Conversation
{
    public int Id { get; set; }

    /// <summary>Serialized session JSON for both Traditional and Agent Framework implementations.</summary>
    public string? SessionData { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string Flower { get; set; } = "";
    public string Color { get; set; } = "";
    public string Size { get; set; } = "";
    public decimal Price { get; set; }
}

public class BouquetPrice
{
    public int Id { get; set; }
    public string Size { get; set; } = "";
    public int NumberOfFlowers { get; set; }
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
}
