using ShortLink.Application.Services;
using ShortLink.Infrastructure.Generators;
using ShortLink.Infrastructure.Repositories;

var repository = new InMemoryUrlRepository();
var generator = new Base62ShortCodeGenerator();
var service = new UrlService(repository, generator);

Console.WriteLine("========================================");
Console.WriteLine("  Oleg-Neskoromnyi-Adroit URL Shortener");
Console.WriteLine("========================================");

bool running = true;
while (running)
{
    Console.WriteLine();
    Console.WriteLine("1. Create Short URL");
    Console.WriteLine("2. Resolve Short URL (counts as click)");
    Console.WriteLine("3. Delete Short URL");
    Console.WriteLine("4. View URL Statistics");
    Console.WriteLine("5. List All URLs (convenience helper for POC demo)");
    Console.WriteLine("6. Exit");
    Console.Write("\nChoose an option: ");

    var choice = Console.ReadLine()?.Trim();
    Console.WriteLine();

    switch (choice)
    {
        case "1":
            HandleCreate(service);
            break;
        case "2":
            HandleResolve(service);
            break;
        case "3":
            HandleDelete(service);
            break;
        case "4":
            HandleStatistics(service);
            break;
        case "5":
            HandleListAll(service);
            break;
        case "6":
            running = false;
            Console.WriteLine("Goodbye!");
            break;
        default:
            Console.WriteLine("Invalid option. Please enter 1-6.");
            break;
    }
}

static void HandleCreate(UrlService service)
{
    Console.Write("Enter long URL: ");
    var longUrl = Console.ReadLine()?.Trim() ?? string.Empty;

    Console.Write("Enter custom short code (or press Enter to auto-generate): ");
    var customCode = Console.ReadLine()?.Trim();

    var result = service.CreateShortUrl(longUrl,
        string.IsNullOrWhiteSpace(customCode) ? null : customCode);

    if (result.IsSuccess)
    {
        Console.WriteLine($"  Created: {result.Value!.ShortCode} -> {result.Value.LongUrl}");
    }
    else
    {
        Console.WriteLine($"  Error: {result.ErrorMessage}");
    }
}

static void HandleResolve(UrlService service)
{
    Console.Write("Enter short code to resolve: ");
    var shortCode = Console.ReadLine()?.Trim() ?? string.Empty;

    var result = service.ResolveShortUrl(shortCode);

    if (result.IsSuccess)
    {
        Console.WriteLine($"  Redirects to: {result.Value}");
    }
    else
    {
        Console.WriteLine($"  Error: {result.ErrorMessage}");
    }
}

static void HandleDelete(UrlService service)
{
    Console.Write("Enter short code to delete: ");
    var shortCode = Console.ReadLine()?.Trim() ?? string.Empty;

    var result = service.DeleteShortUrl(shortCode);

    if (result.IsSuccess)
    {
        Console.WriteLine($"  Deleted: {shortCode}");
    }
    else
    {
        Console.WriteLine($"  Error: {result.ErrorMessage}");
    }
}

static void HandleStatistics(UrlService service)
{
    Console.Write("Enter short code to view statistics: ");
    var shortCode = Console.ReadLine()?.Trim() ?? string.Empty;

    var result = service.GetStatistics(shortCode);

    if (result.IsSuccess)
    {
        var stats = result.Value!;
        Console.WriteLine($"  Short Code:  {stats.ShortCode}");
        Console.WriteLine($"  Long URL:    {stats.LongUrl}");
        Console.WriteLine($"  Click Count: {stats.ClickCount}");
        Console.WriteLine($"  Created:     {stats.CreatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC");
    }
    else
    {
        Console.WriteLine($"  Error: {result.ErrorMessage}");
    }
}

static void HandleListAll(UrlService service)
{
    var mappings = service.ListAll().ToList();

    if (mappings.Count == 0)
    {
        Console.WriteLine("  No URLs found.");
        return;
    }

    Console.WriteLine($"  {"Short Code",-15} {"Clicks",-8} Long URL");
    Console.WriteLine($"  {new string('-', 15)} {new string('-', 8)} {new string('-', 40)}");

    foreach (var m in mappings)
    {
        Console.WriteLine($"  {m.ShortCode,-15} {m.ClickCount,-8} {m.LongUrl}");
    }

    Console.WriteLine($"\n  Total: {mappings.Count} URL(s)");
}
