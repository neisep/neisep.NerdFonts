using System.Globalization;
using System.Reflection;
using System.Text.Json;

namespace neisep.NerdFonts.Tests;

public class IconTests
{
    private static readonly JsonElement GlyphNames =
        JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "glyphnames.json"))).RootElement;

    private static IEnumerable<string> IconValues() =>
        typeof(NerdFont).Assembly.GetExportedTypes()
            .Where(t => t != typeof(NerdFont))
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(f => f.IsLiteral && f.FieldType == typeof(string))
            .Select(f => (string)f.GetRawConstantValue()!);

    [Fact]
    public void Every_glyph_has_a_constant_with_its_code_point()
    {
        var expected = GlyphNames.EnumerateObject()
            .Where(p => p.Name != "METADATA")
            .Select(p => char.ConvertFromUtf32(int.Parse(p.Value.GetProperty("code").GetString()!, NumberStyles.HexNumber)))
            .Order(StringComparer.Ordinal);

        Assert.Equal(expected, IconValues().Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Generated_metadata_matches_glyph_names()
    {
        Assert.Equal(NerdFont.Version, GlyphNames.GetProperty("METADATA").GetProperty("version").GetString());
        Assert.Equal(NerdFont.IconCount, IconValues().Count());
    }

    [Theory]
    [InlineData(Oct.GitBranch, 0xF418)]
    [InlineData(Cod.GitStash, 0xEC26)]
    [InlineData(Dev.Git, 0xE702)]
    [InlineData(Fa._500px, 0xF26E)]
    [InlineData(Md.CloudOffOutline, 0xF0164)]
    public void Known_icons_have_expected_code_points(string icon, int codePoint) =>
        Assert.Equal(codePoint, char.ConvertToUtf32(icon, 0));
}
