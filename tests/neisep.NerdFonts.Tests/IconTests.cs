using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace neisep.NerdFonts.Tests;

public class IconTests
{
    private static readonly string GlyphNamesPath = Path.Combine(AppContext.BaseDirectory, "glyphnames.json");

    private static readonly JsonElement GlyphNames = JsonDocument.Parse(File.ReadAllText(GlyphNamesPath)).RootElement;

    /// <summary>
    /// SHA-256 of data/glyphnames.json as downloaded by tools/Update.cs from Nerd Fonts v3.5.1. The other tests derive the
    /// expected constants from this file, so an edit to it alone would go unnoticed. After an update, review the change, then
    /// update this hash.
    /// </summary>
    private const string ReviewedGlyphNamesSha256 = "d2fa6615a38eb527462cb71ff17aa44b1d6453d437ed263ab8d5b458393669e8";

    [Fact]
    public void Glyph_names_are_the_reviewed_upstream_file() =>
        Assert.True(
            ReviewedGlyphNamesSha256 == Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(GlyphNamesPath))).ToLowerInvariant(),
            $"data/glyphnames.json changed. Review it against the upstream release, then update {nameof(ReviewedGlyphNamesSha256)}.");

    private static IEnumerable<FieldInfo> IconFields() =>
        typeof(NerdFont).Assembly.GetExportedTypes()
            .Where(t => t != typeof(NerdFont))
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(f => f.IsLiteral && f.FieldType == typeof(string));

    [Fact]
    public void Every_glyph_has_a_constant_with_its_code_point()
    {
        var glyphs = GlyphNames.EnumerateObject()
            .Where(p => p.Name != "METADATA")
            .ToList();
        var actual = IconFields().ToDictionary(
            field => $"{field.DeclaringType!.Name}.{field.Name}",
            field => (string)field.GetRawConstantValue()!, StringComparer.Ordinal);

        Assert.Equal(glyphs.Count, actual.Count);
        Assert.All(glyphs, glyph =>
        {
            var member = MemberName(glyph.Name);
            Assert.True(actual.TryGetValue(member, out var value), $"nf-{glyph.Name} has no constant {member}.");
            var expected = char.ConvertFromUtf32(int.Parse(glyph.Value.GetProperty("code").GetString()!, NumberStyles.HexNumber));
            Assert.True(expected == value, $"{member} does not have the code point of nf-{glyph.Name}.");
        });
    }

    [Fact]
    public void Generated_metadata_matches_glyph_names()
    {
        Assert.Equal(NerdFont.Version, GlyphNames.GetProperty("METADATA").GetProperty("version").GetString());
        Assert.Equal(NerdFont.IconCount, IconFields().Count());
    }

    [Theory]
    [InlineData(Oct.GitBranch, 0xF418)]
    [InlineData(Cod.GitStash, 0xEC26)]
    [InlineData(Dev.Git, 0xE702)]
    [InlineData(Fa._500px, 0xF26E)]
    [InlineData(Md.CloudOffOutline, 0xF0164)]
    public void Known_icons_have_expected_code_points(string icon, int codePoint) =>
        Assert.Equal(codePoint, char.ConvertToUtf32(icon, 0));

    // The pinned data has two names that require a suffix to avoid C# member-name conflicts.
    // Keep these expectations explicit rather than reproducing the generator's collision handling.
    private static string MemberName(string glyphName)
    {
        if (glyphName == "fa-equals") return "Fa.EqualsIcon";
        if (glyphName == "fa-fa") return "Fa.FaIcon";

        var separator = glyphName.IndexOf('-');
        var className = Pascal(glyphName[..separator]);
        var member = Pascal(glyphName[(separator + 1)..]);
        if (char.IsDigit(member[0])) member = "_" + member;
        return $"{className}.{member}";
    }

    private static string Pascal(string name) =>
        Regex.Replace(name, @"(?:^|[_-])([^_-])", match => match.Groups[1].Value.ToUpperInvariant());
}
