# Demarbit.StrongIds

Strongly-typed IDs for .NET — eliminate primitive obsession with source-generated value types that include equality, comparison, parsing, JSON serialization, and type conversion out of the box.

[![CI/CD](https://github.com/DemarbitBV/strongids/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/DemarbitBV/strongids/actions/workflows/ci-cd.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=DemarbitBV_strongids&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=DemarbitBV_strongids)
[![NuGet](https://img.shields.io/nuget/v/Demarbit.StrongIds)](https://www.nuget.org/packages/Demarbit.StrongIds)

## Installation

```bash
dotnet add package Demarbit.StrongIds
```

The source generator is bundled — no additional packages or configuration needed.

## What's Included

**`[StrongId]` attribute** — decorate a `partial struct` and the source generator produces a complete value type with equality, comparison, parsing, `ToString()`, and explicit/implicit conversions.

**Backing types** — `Guid` (default), `Int`, `Long`, and `String`. Choose the one that matches your persistence layer.

**JSON serialization** — a `JsonConverter<T>` is generated for each ID, so `System.Text.Json` serialization works without any configuration.

**Type conversion** — a `TypeConverter` is generated for ASP.NET Core model binding, route parameters, and query strings.

## Quick Start

### Define a Strongly-Typed ID

```csharp
using Demarbit.StrongIds;

[StrongId]
public partial struct OrderId { }
```

That's it. The generator produces a `readonly partial struct` with a `Guid` backing type. You get:

- `OrderId.New()` — creates a new ID with a random `Guid`
- `OrderId.From(guid)` — wraps an existing `Guid`
- `OrderId.Empty` — the default/empty value
- `OrderId.Parse(string)` and `OrderId.TryParse(string, out OrderId)` — `IParsable<T>` support
- Full `IEquatable<OrderId>` and `IComparable<OrderId>` implementations
- `==`, `!=` operators
- Implicit conversion to `Guid`, explicit conversion from `Guid`
- Built-in `JsonConverter` and `TypeConverter`

### Choosing a Backing Type

```csharp
[StrongId(BackingType.Guid)]    // default
public partial struct OrderId { }

[StrongId(BackingType.Int)]
public partial struct LineNumber { }

[StrongId(BackingType.Long)]
public partial struct EventSequence { }

[StrongId(BackingType.String)]
public partial struct Slug { }
```

### Using with Entities

Strong IDs pair naturally with [Demarbit.Shared.Domain](https://www.nuget.org/packages/Demarbit.Shared.Domain) generic entities:

```csharp
[StrongId]
public partial struct ProductId { }

public class Product : AggregateRoot<ProductId>
{
    public string Name { get; private set; }

    public Product(ProductId id, string name) : base(id)
    {
        Name = Guard.Against.NullOrWhiteSpace(name, nameof(name));
    }
}

// Usage
var id = ProductId.New();
var product = new Product(id, "Widget");
```

### API Controllers

The generated `TypeConverter` means ASP.NET Core binds route parameters automatically:

```csharp
[HttpGet("{id}")]
public async Task<IActionResult> Get(OrderId id)
{
    // id is parsed from the route string — no manual conversion needed
}
```

### JSON Serialization

Works out of the box with `System.Text.Json`:

```csharp
var order = new { Id = OrderId.New(), Name = "Test" };
var json = JsonSerializer.Serialize(order);
// {"Id":"d4f5a1b2-...","Name":"Test"}
```

## Design Principles

This package follows a few deliberate constraints:

- **Zero runtime dependencies** — the attribute library has no dependencies. The source generator runs at compile time only and does not ship with your application.
- **Compile-time generation** — no reflection, no runtime code generation. The generated code is plain C# that you can inspect in your IDE.
- **Small surface area** — one attribute, one enum, four backing types. Everything else is generated.
- **Framework integration** — `IEquatable<T>`, `IComparable<T>`, `IParsable<T>`, `JsonConverter<T>`, and `TypeConverter` are implemented so strong IDs work everywhere primitive types do.

## Architecture Fit

This package complements the other Demarbit shared libraries:

```
Demarbit.StrongIds                  ← this package (zero runtime deps)
Demarbit.Shared.Domain              ← EntityBase<TId> accepts any IEquatable<TId>
Demarbit.Shared.Application         ← CQRS commands/queries use strong IDs as parameters
Demarbit.Shared.Infrastructure      ← EF Core value converters for strong IDs
```

Each package can be used independently. A project using strong IDs without the shared domain library works fine — the generated structs implement `IEquatable<T>` which satisfies any generic constraint.

## License

[MIT](LICENSE)