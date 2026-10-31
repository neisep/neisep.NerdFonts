// Downloads a Nerd Fonts release and regenerates the icon constants.
// Usage (from the repository root): dotnet run tools/Update.cs -- 3.5.1
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

const string FontFile = "SymbolsNerdFont-Regular.ttf";
// Members a static class inherits from object; an icon with one of these names would hide it.
string[] ObjectMembers = ["Equals", "GetHashCode", "GetType", "MemberwiseClone", "ReferenceEquals", "ToString", "Finalize"];

if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: dotnet run tools/Update.cs -- <nerd-fonts-version>");
    return 1;
}

var version = args[0].TrimStart('v');
// `dotnet run tools/Update.cs` may start in tools/, so look upwards for the repository root.
var root = Directory.GetCurrentDirectory();
while (!File.Exists(Path.Combine(root, "neisep.NerdFonts.slnx")))
{
    root = Path.GetDirectoryName(root);
    if (root is null)
    {
        Console.Error.WriteLine("Run this from inside the repository.");
        return 1;
    }
}

var libDir = Path.Combine(root, "src", "neisep.NerdFonts");
var dataDir = Path.Combine(root, "data");
var licensesDir = Path.Combine(root, "licenses");

// Upstream license texts shipped in the package, copied verbatim. Files hosted by Nerd Fonts follow the
// release tag; the others are pinned to the commit of the version Nerd Fonts bundles. When Nerd Fonts
// updates an icon set, update its pin here and its entry in THIRD-PARTY-NOTICES.md.
var nerdFontsRaw = $"https://raw.githubusercontent.com/ryanoasis/nerd-fonts/v{version}";
(string File, string Url)[] licenseFiles =
[
    ("Codicons-LICENSE.txt", $"{nerdFontsRaw}/src/glyphs/codicons/LICENSE.txt"),
    ("FontAwesome-LICENSE.txt", $"{nerdFontsRaw}/src/glyphs/font-awesome/LICENSE.txt"),
    ("MaterialDesignIcons-LICENSE.txt", $"{nerdFontsRaw}/src/glyphs/materialdesign/LICENSE"),
    ("Octicons-LICENSE.txt", $"{nerdFontsRaw}/src/glyphs/octicons/LICENSE"),
    ("Pomicons-LICENSE.txt", $"{nerdFontsRaw}/src/glyphs/pomicons/LICENSE"),
    ("PowerlineExtra-LICENSE.txt", $"{nerdFontsRaw}/src/glyphs/powerline-extra/LICENSE"),
    ("PowerlineSymbols-LICENSE.txt", $"{nerdFontsRaw}/src/glyphs/powerline-symbols/LICENSE.txt"),
    ("WeatherIcons-OFL.txt", $"{nerdFontsRaw}/src/glyphs/weather-icons/OFL.txt"),
    // devicons/devicon v2.17.0
    ("Devicon-LICENSE.txt", "https://raw.githubusercontent.com/devicons/devicon/54cfe13ac10eaa1ef817a343ab0a9437eb3c2e08/LICENSE"),
    ("Devicons-vorillaz-LICENSE.txt", "https://raw.githubusercontent.com/vorillaz/devicons/8947499eeac633315de2a76fde08fc71619dd4ba/LICENSE"),
    ("FontAwesomeExtension-LICENSE.txt", "https://raw.githubusercontent.com/AndreLZGava/font-awesome-extension/09d80249058ee8018a45da30add4339289dbb466/LICENCE"),
    // lukas-w/font-logos v1.4.0
    ("FontLogos-LICENSE.txt", "https://raw.githubusercontent.com/lukas-w/font-logos/2db3df547108b28bd4972e685e2b682092d78498/LICENSE"),
    ("SetiUI-LICENSE.md", "https://raw.githubusercontent.com/jesseweed/seti-ui/2d6c5e68b4ded73c92dac291845ee44e1182d511/LICENSE.md"),
    ("IEC-PowerSymbols-LICENSE.txt", "https://raw.githubusercontent.com/jloughry/Unicode/805536fb6f93604da985d80e0c6b099f98f8b4d9/LICENSE.txt"),
    // source-foundry/Hack v3.003
    ("Hack-LICENSE.md", "https://raw.githubusercontent.com/source-foundry/Hack/b47d21f808fbf4784565feccde2213ef2af924b3/LICENSE.md"),
    // Required by Apache-2.0 section 4(a) for Material Design Icons.
    ("Apache-2.0.txt", "https://www.apache.org/licenses/LICENSE-2.0.txt"),
];

using var http = new HttpClient();
Console.WriteLine($"Downloading Nerd Fonts v{version}…");
var json = await http.GetStringAsync($"https://raw.githubusercontent.com/ryanoasis/nerd-fonts/v{version}/glyphnames.json");
var zip = await http.GetByteArrayAsync($"https://github.com/ryanoasis/nerd-fonts/releases/download/v{version}/NerdFontsSymbolsOnly.zip");

using (var archive = new ZipArchive(new MemoryStream(zip)))
{
    Extract(archive, FontFile, Path.Combine(libDir, "Resources", FontFile));
    Extract(archive, "LICENSE", Path.Combine(licensesDir, "NerdFonts-LICENSE.txt"));
    Extract(archive, "README.md", Path.Combine(licensesDir, "NerdFonts-SymbolsOnly-README.md"));
}
await File.WriteAllTextAsync(Path.Combine(dataDir, "glyphnames.json"), json);

foreach (var (file, url) in licenseFiles)
    await File.WriteAllBytesAsync(Path.Combine(licensesDir, file), await http.GetByteArrayAsync(url));

using var document = JsonDocument.Parse(json);
var metadataVersion = document.RootElement.GetProperty("METADATA").GetProperty("version").GetString();
if (metadataVersion != version)
    throw new InvalidDataException($"glyphnames.json reports version {metadataVersion}, expected {version}.");

var glyphs = document.RootElement.EnumerateObject()
    .Where(p => p.Name != "METADATA")
    .Select(p => (Name: p.Name, Code: int.Parse(p.Value.GetProperty("code").GetString()!, NumberStyles.HexNumber)))
    .ToList();

// Fail before touching the generated sources if any icon set has no attribution.
foreach (var prefix in glyphs.Select(g => SetPrefix(g.Name)).Distinct()) IconSet(prefix);

var iconsDir = Path.Combine(libDir, "Icons");
Directory.CreateDirectory(iconsDir);
foreach (var old in Directory.GetFiles(iconsDir, "*.g.cs")) File.Delete(old);

foreach (var set in glyphs.GroupBy(g => SetPrefix(g.Name)).OrderBy(s => s.Key, StringComparer.Ordinal))
{
    var className = Pascal(set.Key);
    var (title, credit) = IconSet(set.Key);
    var used = new HashSet<string>(StringComparer.Ordinal);
    var code = new StringBuilder()
        .AppendLine($"// <auto-generated>Generated by tools/Update.cs from Nerd Fonts v{version}. Do not edit.</auto-generated>")
        .AppendLine("namespace neisep.NerdFonts;")
        .AppendLine()
        .AppendLine($"/// <summary>{title} icons (<c>nf-{set.Key}-*</c>), bundled via Nerd Fonts. {credit}. See THIRD-PARTY-NOTICES.md.</summary>")
        .AppendLine($"public static class {className}")
        .AppendLine("{");

    foreach (var glyph in set.OrderBy(g => g.Name, StringComparer.Ordinal))
    {
        var member = Pascal(glyph.Name[(set.Key.Length + 1)..]);
        if (char.IsDigit(member[0])) member = "_" + member;
        if (member == className || ObjectMembers.Contains(member)) member += "Icon";
        if (used.Contains(member)) member += $"_{glyph.Code:X}";
        if (!used.Add(member)) throw new InvalidDataException($"Duplicate member {className}.{member}");

        var literal = glyph.Code <= 0xFFFF ? $"\\u{glyph.Code:X4}" : $"\\U{glyph.Code:X8}";
        code.AppendLine($"    /// <summary><c>nf-{glyph.Name}</c> (U+{glyph.Code:X4})</summary>")
            .AppendLine($"    public const string {member} = \"{literal}\";");
    }

    code.AppendLine("}");
    await File.WriteAllTextAsync(Path.Combine(iconsDir, $"{className}.g.cs"), code.ToString());
    Console.WriteLine($"{className,-12} {set.Count(),5} icons");
}

await File.WriteAllTextAsync(Path.Combine(libDir, "NerdFont.g.cs"),
    $$"""
    // <auto-generated>Generated by tools/Update.cs. Do not edit.</auto-generated>
    namespace neisep.NerdFonts;

    public static partial class NerdFont
    {
        /// <summary>Nerd Fonts release the icons and font come from.</summary>
        public const string Version = "{{version}}";

        /// <summary>Number of icon constants.</summary>
        public const int IconCount = {{glyphs.Count}};
    }

    """);

Console.WriteLine($"Generated {glyphs.Count} icons from Nerd Fonts v{version}.");
Console.WriteLine("Now review THIRD-PARTY-NOTICES.md against licenses/NerdFonts-SymbolsOnly-README.md, and every changed file in licenses/");
Console.WriteLine("(then update its hash in tests/neisep.NerdFonts.Tests/NoticeTests.cs), and data/glyphnames.json (then update its hash");
Console.WriteLine("in tests/neisep.NerdFonts.Tests/IconTests.cs); the tests fail until all are done.");
return 0;

static void Extract(ZipArchive archive, string entryName, string destination)
{
    var entry = archive.GetEntry(entryName) ?? throw new InvalidDataException($"{entryName} not found in release archive.");
    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
    entry.ExtractToFile(destination, overwrite: true);
}

static string SetPrefix(string glyphName) => glyphName[..glyphName.IndexOf('-')];

static string Pascal(string name) =>
    string.Concat(name.Split('_', '-').Where(p => p.Length > 0).Select(p => char.ToUpperInvariant(p[0]) + p[1..]));

// Title and credit for each icon set. An unknown prefix means Nerd Fonts added a set: attribute it here and in
// THIRD-PARTY-NOTICES.md before generating, rather than shipping it unattributed.
static (string Title, string Credit) IconSet(string prefix) => prefix switch
{
    "cod" => ("Codicons", "By Microsoft Corporation, CC BY 4.0"),
    "custom" => ("Custom", "By Nerd Fonts contributors, MIT; logos are trademarks of their owners"),
    "dev" => ("Devicons", "By the devicon contributors and Theodore Vorillas, MIT; logos are trademarks of their owners"),
    "extra" => ("Extra", "By Nerd Fonts contributors (extraglyphs, based on Hack by Source Foundry Authors), MIT"),
    "fa" => ("Font Awesome", "Font Awesome Free by Fonticons, Inc., CC BY 4.0; brand icons are trademarks of their owners"),
    "fae" => ("Font Awesome Extension", "By André Luiz Gava, MIT"),
    "iec" => ("IEC Power Symbols", "By Joe Loughry, MIT"),
    "indent" or "indentation" => ("Indentation", "By Nerd Fonts contributors, MIT"),
    "linux" => ("Font Logos", "By Lukas W, Unlicense; logos are trademarks of their owners"),
    "md" => ("Material Design", "Material Design Icons by Pictogrammers, Apache-2.0"),
    "oct" => ("Octicons", "By GitHub, Inc., MIT"),
    "pl" => ("Powerline", "Powerline Symbols by Kim Silkebækken and contributors, MIT"),
    "ple" => ("Powerline Extra", "Powerline Extra Symbols by Ryan L McIntyre, MIT"),
    "pom" => ("Pomicons", "By Davide Bignotti and Gabriele Lana, SIL OFL 1.1"),
    "seti" => ("Seti-UI", "Seti UI by Jesse Weed and Nerd Fonts contributors, MIT"),
    "weather" => ("Weather Icons", "By Erik Flowers and Lukas Bischoff, SIL OFL 1.1"),
    _ => throw new InvalidDataException(
        $"Unknown icon set prefix '{prefix}'. Add its credit to IconSet in tools/Update.cs and to THIRD-PARTY-NOTICES.md.")
};
