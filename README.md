# ShortLink

A TinyURL-style URL shortening service proof of concept built with C# .NET 8.

## Features

- Create short URLs with auto-generated or custom short codes
- Resolve short URLs to their original long URLs
- Delete short URLs
- Track click statistics per short URL
- One long URL can map to multiple short codes

## Architecture

Clean layered architecture with four projects:

```
src/
├── ShortLink.Domain            Entities, interfaces, result types (no dependencies)
├── ShortLink.Application       Business logic, validation, DTOs
├── ShortLink.Infrastructure    In-memory repository, Base62 code generator
└── ShortLink.ConsoleApp        Interactive console UI
tests/
└── ShortLink.Tests             67 unit tests (xUnit + Moq)
```

**Key design decisions:**
- `Result<T>` pattern for error handling (no exceptions for business failures)
- `ConcurrentDictionary` for thread-safe in-memory storage
- `Interlocked.Increment` for thread-safe click counting
- Cryptographic `RandomNumberGenerator` for short code generation
- Collision retry logic (up to 10 attempts)

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Run Tests

```bash
dotnet test
```

## Run App

```bash
dotnet run --project src/ShortLink.ConsoleApp
```

## License

MIT
