using ProductInventory.Domain.Entities;

namespace ProductInventory.Infrastructure.Persistence.Seed;

/// <summary>Fixed sample data applied via EF migration HasData; values must stay deterministic across migrations.</summary>
public static class ProductSeedData
{
    private static readonly DateTime SeedCreatedAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static Product[] Products { get; } =
    [
        new() { Id = 100001, Name = "Wireless Mouse", Description = "2.4GHz wireless mouse with USB receiver", Price = 19.99m, Stock = 150, CreatedAtUtc = SeedCreatedAtUtc },
        new() { Id = 100002, Name = "Mechanical Keyboard", Description = "Tenkeyless mechanical keyboard, blue switches", Price = 79.99m, Stock = 75, CreatedAtUtc = SeedCreatedAtUtc },
        new() { Id = 100003, Name = "27-inch Monitor", Description = "27-inch QHD IPS monitor, 144Hz", Price = 299.00m, Stock = 30, CreatedAtUtc = SeedCreatedAtUtc },
        new() { Id = 100004, Name = "USB-C Hub", Description = "7-in-1 USB-C hub with HDMI and card reader", Price = 34.50m, Stock = 200, CreatedAtUtc = SeedCreatedAtUtc },
        new() { Id = 100005, Name = "Webcam 1080p", Description = "Full HD webcam with built-in microphone", Price = 45.00m, Stock = 0, CreatedAtUtc = SeedCreatedAtUtc },
        new() { Id = 100006, Name = "Laptop Stand", Description = "Aluminum adjustable laptop stand", Price = 29.99m, Stock = 120, CreatedAtUtc = SeedCreatedAtUtc },
        new() { Id = 100007, Name = "Noise Cancelling Headphones", Description = "Over-ear ANC headphones, 30h battery", Price = 149.99m, Stock = 60, CreatedAtUtc = SeedCreatedAtUtc },
        new() { Id = 100008, Name = "External SSD 1TB", Description = "Portable USB 3.2 SSD, 1TB", Price = 89.99m, Stock = 90, CreatedAtUtc = SeedCreatedAtUtc },
        new() { Id = 100009, Name = "Desk Mat", Description = "Extended desk mat, 900x400mm", Price = 15.00m, Stock = 5, CreatedAtUtc = SeedCreatedAtUtc },
        new() { Id = 100010, Name = "Ergonomic Chair", Description = "Mesh-back ergonomic office chair", Price = 219.00m, Stock = 12, CreatedAtUtc = SeedCreatedAtUtc },
    ];
}
