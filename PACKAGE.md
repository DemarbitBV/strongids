# Demarbit.StrongIds

Compile-time source generator that eliminates primitive obsession by turning `partial struct` declarations into fully-featured strongly-typed IDs with equality, comparison, parsing, JSON serialization, and type conversion.

- **Target framework:** netstandard2.0 (compatible with .NET Standard 2.0+, .NET Core 2.0+, .NET 5+)
- **Runtime dependencies:** None
- **License:** MIT

## Quick Start

There is no DI registration or service setup required. This package is a compile-time source generator.

1. Install the `Demarbit.StrongIds` NuGet package.
2. Declare a `partial struct` with the `[StrongId]` attribute:

```csharp
using Demarbit.StrongIds;

[StrongId] // defaults to BackingType.Guid
public partial struct OrderId { }
```

3. Use the generated type:

```csharp
var id = OrderId.New();          // random Guid-backed ID
var id2 = OrderId.From(someGuid); // wrap an existing Guid
string json = JsonSerializer.Serialize(id); // "\"d4f1a2b3-...\""
```

No additional configuration, startup code, or DI wiring is needed.

## Core Concepts

### Strongly-Typed IDs via Source Generation

This package implements the **Strong ID** pattern (also known as typed identifiers) to prevent primitive obsession in domain models. Instead of passing raw `Guid`, `int`, `long`, or `string` values around, you define a dedicated struct type for each identifier.

At compile time, the Roslyn incremental source generator detects structs decorated with `[StrongId]` and emits a complete implementation including:

- A `Value` property exposing the underlying primitive
- Factory methods (`New()`, `From()`, `Empty`)
- Full `IEquatable<T>`, `IComparable<T>`, and `IParsable<T>` implementations
- Equality operators (`==`, `!=`)
- Explicit conversion from the backing type, implicit conversion to the backing type
- A nested `JsonConverter<T>` for System.Text.Json serialization
- A nested `TypeConverter` for ASP.NET Core model binding and route parameters

### Design Decisions

- **Struct-only:** The attribute can only be applied to `struct` declarations (enforced by `AttributeTargets.Struct`). The generated struct is `readonly partial`.
- **Backing type selection:** Choose the underlying primitive via the `BackingType` enum parameter. Defaults to `Guid`.
- **Zero runtime cost:** The generator runs at compile time. The generated code has no dependency on the `Demarbit.StrongIds` assembly at runtime beyond the attribute itself (which is netstandard2.0 with no transitive dependencies).
- **Accessibility preservation:** If the source struct is `public`, all generated members and nested types are `public`. If `internal`, they are `internal`.

## Public API Reference

### Demarbit.StrongIds.StrongIdAttribute

```csharp
[AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public class StrongIdAttribute : Attribute
{
    public BackingType BackingType { get; }
    public StrongIdAttribute(BackingType backingType = BackingType.Guid);
}
```

The marker attribute that triggers source generation. Apply to a `partial struct`.

**Parameters:**
- `backingType` (`BackingType`, optional, default: `BackingType.Guid`) — the primitive type backing the ID.

### Demarbit.StrongIds.BackingType

```csharp
public enum BackingType
{
    Guid,   // System.Guid — default
    Int,    // System.Int32
    Long,   // System.Int64
    String  // System.String
}
```

Specifies the underlying primitive type for the generated strong ID.

### Generated Members (per backing type)

When you write:

```csharp
[StrongId(BackingType.Guid)]
public partial struct OrderId { }
```

The generator emits the following `readonly partial struct`:

#### Common to All Backing Types

| Member | Signature |
|--------|-----------|
| Constructor | `OrderId(<backing> value)` |
| Value property | `<backing> Value { get; }` |
| Factory: From | `static OrderId From(<backing> value)` |
| Factory: Empty | `static OrderId Empty` |
| Equality | `bool Equals(OrderId other)` |
| Equality | `override bool Equals(object? obj)` |
| Hash code | `override int GetHashCode()` |
| Operators | `operator ==(OrderId, OrderId)`, `operator !=(OrderId, OrderId)` |
| Comparison | `int CompareTo(OrderId other)` |
| Parse | `static OrderId Parse(string s, IFormatProvider? provider = null)` |
| TryParse | `static bool TryParse(string? s, IFormatProvider? provider, out OrderId result)` |
| ToString | `override string ToString()` |
| Explicit cast | `explicit operator OrderId(<backing> value)` |
| Implicit cast | `implicit operator <backing>(OrderId id)` |
| JSON converter | Nested `OrderIdJsonConverter : JsonConverter<OrderId>` (auto-applied via `[JsonConverter]`) |
| Type converter | Nested `OrderIdTypeConverter : TypeConverter` (auto-applied via `[TypeConverter]`) |

#### Guid-Specific

| Member | Signature | Notes |
|--------|-----------|-------|
| Factory: New | `static OrderId New()` | Creates with `Guid.NewGuid()` |
| Empty value | `Guid.Empty` | |
| TypeConverter | Converts from `string` and `Guid` | |

#### Int-Specific

| Member | Signature | Notes |
|--------|-----------|-------|
| Empty value | `0` | |
| No `New()` factory | — | No random generation for integers |
| TypeConverter | Converts from `string` and `int` | |

#### Long-Specific

| Member | Signature | Notes |
|--------|-----------|-------|
| Empty value | `0L` | |
| No `New()` factory | — | No random generation for longs |
| TypeConverter | Converts from `string` and `long` | |

#### String-Specific

| Member | Signature | Notes |
|--------|-----------|-------|
| Value property | `string Value => _value ?? string.Empty` | Null-safe via backing field |
| Factory: From | `static OrderId From(string value)` | Throws `ArgumentException` if null or empty |
| Empty value | `new("")` | |
| TryParse | Returns `false` for null/empty strings | |
| TypeConverter | Converts from `string` only | |

### Implemented Interfaces (Generated)

Every generated struct implements:

- `IEquatable<T>` — value-based equality on the backing field
- `IComparable<T>` — delegates to the backing type's `CompareTo`
- `IParsable<T>` — parses from string representation of the backing type

### Applied Attributes (Generated)

Every generated struct is decorated with:

- `[JsonConverter(typeof(<Name>JsonConverter))]` — System.Text.Json serialization
- `[TypeConverter(typeof(<Name>TypeConverter))]` — ASP.NET Core model binding / route parameters

## Usage Patterns & Examples

### Defining Strong IDs

```csharp
using Demarbit.StrongIds;

// Guid-backed (default) — most common for domain entity IDs
[StrongId]
public partial struct OrderId { }

// Int-backed — for legacy database IDs
[StrongId(BackingType.Int)]
public partial struct LegacyCustomerId { }

// Long-backed — for high-volume sequential IDs
[StrongId(BackingType.Long)]
public partial struct EventSequenceNumber { }

// String-backed — for external system identifiers
[StrongId(BackingType.String)]
public partial struct ExternalReferenceId { }
```

### Creating and Using IDs

```csharp
// Guid-backed
var orderId = OrderId.New();               // random
var orderId2 = OrderId.From(existingGuid); // from known value
var empty = OrderId.Empty;                 // sentinel

// Int/Long-backed
var customerId = LegacyCustomerId.From(42);
var seqNum = EventSequenceNumber.From(100_000L);

// String-backed
var refId = ExternalReferenceId.From("EXT-12345");
// ExternalReferenceId.From("") → throws ArgumentException
// ExternalReferenceId.From(null) → throws ArgumentException
```

### Equality and Comparison

```csharp
var a = OrderId.New();
var b = OrderId.From(a.Value);

bool same = a == b;        // true
bool diff = a != b;        // false
bool eq = a.Equals(b);     // true

// Sorting
var ids = new List<OrderId> { id3, id1, id2 };
ids.Sort(); // uses CompareTo
```

### Accessing the Underlying Value

```csharp
var orderId = OrderId.New();

// Via property
Guid raw = orderId.Value;

// Via implicit conversion
Guid raw2 = orderId; // implicit operator Guid

// Via explicit conversion (the other direction)
OrderId fromGuid = (OrderId)someGuid; // explicit operator OrderId
```

### JSON Serialization (System.Text.Json)

```csharp
var order = new Order { Id = OrderId.New(), Total = 99.99m };

// Serializes the ID as its raw value (not a nested object)
string json = JsonSerializer.Serialize(order);
// {"Id":"d4f1a2b3-...","Total":99.99}

var deserialized = JsonSerializer.Deserialize<Order>(json);
// deserialized.Id is an OrderId, not a raw Guid
```

No `JsonSerializerOptions` configuration is needed. The `[JsonConverter]` attribute on the generated struct handles it automatically.

### ASP.NET Core Model Binding and Route Parameters

```csharp
// The TypeConverter enables direct use in controllers:
[HttpGet("orders/{id}")]
public IActionResult GetOrder(OrderId id)
{
    // id is automatically parsed from the route string
    // Works for query parameters too: /orders?id=d4f1a2b3-...
}
```

No custom model binder registration is required. The `[TypeConverter]` attribute on the generated struct handles it automatically.

### Parsing from Strings

```csharp
// Parse (throws on failure)
var id = OrderId.Parse("d4f1a2b3-c4d5-e6f7-8901-234567890abc");

// TryParse (safe)
if (OrderId.TryParse(input, null, out var parsed))
{
    // use parsed
}
```

### Using in Entity/Domain Models

```csharp
using Demarbit.StrongIds;

[StrongId]
public partial struct CustomerId { }

[StrongId]
public partial struct OrderId { }

public class Order
{
    public OrderId Id { get; init; }
    public CustomerId CustomerId { get; init; }
    public decimal Total { get; init; }
}

// The compiler prevents mixing up IDs:
// order.CustomerId = orderId; // compile error — different types
```

### EF Core Value Conversions

EF Core can work with strong IDs using value conversions. Configure in your `DbContext`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Order>(entity =>
    {
        entity.Property(o => o.Id)
              .HasConversion(
                  id => id.Value,        // OrderId → Guid (implicit also works)
                  value => new OrderId(value));  // Guid → OrderId
    });
}
```

Or define a reusable `ValueConverter<TId, TBacking>` for each strong ID type.

### Internal Visibility

```csharp
// If the struct is internal, all generated members are also internal
[StrongId]
internal partial struct InternalOrderId { }
```

## What NOT to Do

```csharp
// DON'T: forget the 'partial' keyword — the generator cannot extend the struct
[StrongId]
public struct OrderId { } // compile error: generated code cannot be added

// DON'T: apply to a class — the attribute targets structs only
[StrongId]
public partial class OrderId { } // attribute ignored, no code generated

// DON'T: use default(StringId) and expect a non-empty value
var id = default(ExternalReferenceId);
// id.Value is "" (empty string, not null) — but From() rejects empty strings
// Be explicit: use ExternalReferenceId.Empty if you need a sentinel value
```

## Integration Points

This package has **no DI registration, middleware, pipeline behaviors, or configuration**. It is a pure compile-time source generator.

The generated nested types provide automatic integration with:

| Integration | Mechanism | What It Enables |
|-------------|-----------|-----------------|
| System.Text.Json | `[JsonConverter]` attribute + nested `JsonConverter<T>` | Automatic serialization/deserialization as raw value |
| ASP.NET Core | `[TypeConverter]` attribute + nested `TypeConverter` | Route parameter binding, query string binding, model binding |
| LINQ / Collections | `IEquatable<T>`, `GetHashCode()` | Correct behavior in `HashSet<T>`, `Dictionary<TKey,TValue>`, LINQ `Distinct()` |
| Sorting | `IComparable<T>` | `List<T>.Sort()`, `OrderBy()` |
| String parsing | `IParsable<T>` | Generic parsing APIs, minimal API parameter binding |

## Dependencies & Compatibility

### Runtime Dependencies

**None.** The `Demarbit.StrongIds` package ships the attribute types (netstandard2.0) and bundles the source generator as an analyzer DLL. There are no transitive NuGet dependencies for consumers.

### Compile-Time Dependencies (internal to the generator)

- `Microsoft.CodeAnalysis.CSharp` 5.0.0 — Roslyn APIs for incremental source generation

### Compatibility

- Any project targeting .NET Standard 2.0 or higher can reference this package
- The generated code uses `IParsable<T>` which requires .NET 7+. If targeting earlier frameworks, the generated code will not compile for the `IParsable<T>` portion
- System.Text.Json must be available in the consuming project for JSON converter support (included in .NET Core 3.0+ / .NET 5+; available as a NuGet package for .NET Standard 2.0)

### Peer Dependencies

None. This package is self-contained.
