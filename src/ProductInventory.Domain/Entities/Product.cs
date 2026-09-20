namespace ProductInventory.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    // optimistic concurrency token, auto-maintained by SQL Server
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
