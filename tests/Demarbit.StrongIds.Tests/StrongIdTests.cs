using System.Text.Json;

namespace Demarbit.StrongIds.Tests;

[StrongId]
public partial struct TestOrderId { }

[StrongId(BackingType.Int)]
public partial struct TestLineNumber { }

public class StrongIdTests
{
    [Fact]
    public void New_CreatesNonEmptyId()
    {
        var id = TestOrderId.New();
        Assert.NotEqual(TestOrderId.Empty, id);
    }

    [Fact]
    public void Parse_RoundTrips()
    {
        var original = TestOrderId.New();
        var parsed = TestOrderId.Parse(original.ToString());
        Assert.Equal(original, parsed);
    }

    [Fact]
    public void JsonSerialization_RoundTrips()
    {
        var id = TestOrderId.New();
        var json = JsonSerializer.Serialize(id);
        var deserialized = JsonSerializer.Deserialize<TestOrderId>(json);
        Assert.Equal(id, deserialized);
    }
}