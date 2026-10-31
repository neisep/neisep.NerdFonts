using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace neisep.NerdFonts.Tests;

/// <summary>
/// Guards the attribution that must accompany the bundled font: every icon set is credited with the license the upstream
/// table gives it, every cited license text exists and is the reviewed upstream file, the documented font hash matches
/// the embedded font, and README links point at the release tag on GitHub.
/// </summary>
public class NoticeTests
{
    private const string RepoUrl = "https://github.com/neisep/neisep.NerdFonts";

    private static readonly string Root = FindRepoRoot();
    private static readonly string Notices = File.ReadAllText(Path.Combine(Root, "THIRD-PARTY-NOTICES.md"));
    private static readonly string Readme = File.ReadAllText(Path.Combine(Root, "README.md"));

    /// <summary>
    /// SHA-256 of every file in licenses/, as compared byte for byte with the pinned upstream sources in tools/Update.cs.
    /// When an update changes a file, review the upstream change against THIRD-PARTY-NOTICES.md, then update the hash.
    /// </summary>
    private static readonly Dictionary<string, string> ReviewedLicenseFiles = new(StringComparer.Ordinal)
    {
        ["Apache-2.0.txt"] = "cfc7749b96f63bd31c3c42b5c471bf756814053e847c10f3eb003417bc523d30",
        ["Codicons-LICENSE.txt"] = "d6239afa918961b465b07bf7411cbe34ff6685854f58553db7966f4881a0211f",
        ["Devicon-LICENSE.txt"] = "121194741d4a915b9f5890fdd6dd95121f9b1f816517c792358d72d7c838d664",
        ["Devicons-vorillaz-LICENSE.txt"] = "508a77d2e7b51d98adeed32648ad124b7b30241a8e70b2e72c99f92d8e5874d1",
        ["FontAwesomeExtension-LICENSE.txt"] = "b47b15d771c4507a75ff65beca3a78168d6af4cca9b45177a1aa817139c64ba5",
        ["FontAwesome-LICENSE.txt"] = "de784a808496d49f5f80dd9d1d6e9a51cff712cfbc339e7c25155631ae3cdb5d",
        ["FontLogos-LICENSE.txt"] = "88d9b4eb60579c191ec391ca04c16130572d7eedc4a86daa58bf28c6e14c9bcd",
        ["Hack-LICENSE.md"] = "1f61bb7c790c59b4b0ecdf304628b94e42ae4c8020094a8c3da73381ab212623",
        ["IEC-PowerSymbols-LICENSE.txt"] = "11aef1cdf2d58d822e6857e2574870ccc59d4900052bf5c098e319d750e7ac3d",
        ["MaterialDesignIcons-LICENSE.txt"] = "3bae6305bcaa6d10219b999f65c342034c2d40b1c0e8567fa08605e73ccba829",
        ["NerdFonts-LICENSE.txt"] = "84a7a98c82140fb12c37fe42b93805baa16024cb3e5acc599b7ffe612c55d847",
        ["NerdFonts-SymbolsOnly-README.md"] = "a4463554f951c8153d0bdfcb6e03b382baf99dcdd3f99790014427687abdc55b",
        ["Octicons-LICENSE.txt"] = "4f886642f9e8d99eeadd484acdac011c3fde8a338faf52f5ca1408c13b9a9b63",
        ["Pomicons-LICENSE.txt"] = "cb4797a9745683f287c28833db596e9d68ec43082d59359b3a49c504f0f610ce",
        ["PowerlineExtra-LICENSE.txt"] = "544307a58e6826c24145b2af1c3578b53e2e21ae0e6c67ab4c6abd544304d26a",
        ["PowerlineSymbols-LICENSE.txt"] = "46d5355e4734468135d8e4a30d0916292cc4df2b3ec30aa68c866f82497933be",
        ["SetiUI-LICENSE.md"] = "d4d2d888c6a0a220a7b0e2d6013cf37539b0e05ef937002b3b33bdb364575b88",
        ["WeatherIcons-OFL.txt"] = "69cdedc8a4f57a30d68e4f045af8b9cce0ec3b1a6e7eec1ad41737c894f26790",
    };

    /// <summary>License names used in the upstream Nerd Fonts table, mapped to the name the notices must use.</summary>
    private static readonly Dictionary<string, string> UpstreamLicenseNames = new(StringComparer.Ordinal)
    {
        ["MIT"] = "MIT",
        ["CC BY 4.0"] = "CC BY 4.0",
        ["Apache 2.0"] = "Apache-2.0",
        ["OFL 1.1"] = "SIL OFL 1.1",
        ["OFL 1.1 RFN"] = "SIL OFL 1.1",
        ["unlicensed"] = "Unlicense",
    };

    /// <summary>
    /// For each license name the notices use: how to find it in the summary's License column, and text that a cited
    /// license file must contain for that license (the license itself, not just a mention of its name).
    /// </summary>
    private static readonly (string Name, Regex InCell, Regex InFile)[] Licenses =
    [
        ("MIT", new(@"\bMIT\b"), new(Words("Permission is hereby granted, free of charge") + "(?s).*" + Words("THE SOFTWARE IS PROVIDED \"AS IS\""))),
        ("CC BY 4.0", new("CC BY 4\\.0"), new("Attribution 4\\.0 International|creativecommons\\.org/licenses/by/4\\.0")),
        ("Apache-2.0", new("Apache-2\\.0"), new(Words("TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION"))),
        ("SIL OFL 1.1", new("SIL OFL 1\\.1"), new(Words("SIL OPEN FONT LICENSE Version 1.1") + "(?s).*" + Words("PERMISSION & CONDITIONS"))),
        ("Unlicense", new(@"\bUnlicense\b"), new(Words("This is free and unencumbered software released into the public domain"))),
        ("Bitstream Vera", new("Bitstream Vera"), new(Words("Bitstream Vera is a trademark of Bitstream, Inc."))),
    ];

    [Fact]
    public void Summary_preserves_original_creator_credits()
    {
        var expected = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["Nerd Fonts (font, glyph names, original icons)"] = ["Ryan L McIntyre", "Nerd Fonts contributors"],
            ["Codicons"] = ["Microsoft Corporation"],
            ["Devicons"] = ["konpa", "devicon contributors", "Theodore Vorillas"],
            ["Extra glyphs (progress indicators, heavy angle brackets)"] = ["Nerd Fonts contributors", "Source Foundry Authors"],
            ["Font Awesome Free"] = ["Fonticons, Inc."],
            ["Font Awesome Extension"] = ["André Luiz Gava"],
            ["Font Logos"] = ["Lukas W"],
            ["Material Design Icons"] = ["Pictogrammers"],
            ["Octicons"] = ["GitHub, Inc."],
            ["Seti UI and Nerd Fonts originals"] = ["Jesse Weed", "Nerd Fonts contributors"],
            ["Pomicons"] = ["Davide Bignotti", "Gabriele Lana"],
            ["Powerline Extra Symbols"] = ["Ryan L McIntyre"],
            ["Powerline Symbols"] = ["Kim Silkebækken", "contributors"],
            ["IEC Power Symbols"] = ["Joe Loughry"],
            ["Weather Icons"] = ["Erik Flowers", "Lukas Bischoff"],
        };
        var summary = SummaryRows().ToDictionary(row => row["Icon set"], StringComparer.Ordinal);

        Assert.All(expected, set =>
            Assert.All(set.Value, creator => Assert.Contains(creator, summary[set.Key]["Created by / ©"])));
    }

    [Fact]
    public void Notices_preserve_reviewed_copyright_and_attribution()
    {
        string[] required =
        [
            "Copyright (c) 2014 Ryan L McIntyre",
            "Copyright (c) 2015 konpa",
            "Copyright (c) 2018 Source Foundry Authors",
            "Copyright (c) 2003 by Bitstream, Inc. All Rights Reserved. Bitstream Vera is a trademark of Bitstream, Inc.",
            "Copyright 2023 Fonticons, Inc.",
            "Copyright 2026 Fonticons, Inc.",
            "Copyright (c) 2017 André Luiz Gava",
            "Copyright (c) 2014-2026, Lukas W",
            "Copyright (c) 2023 GitHub Inc.",
            "Copyright (c) 2014 Jesse Weed",
            "Copyright (c) 2021, Gabriele Lana gabriele.lana@gmail.com with Reserved Font Name Pomicons.",
            "Copyright (c) 2014 by Davide Bignotti. All rights reserved.",
            "Pomicons is a trademark of Davide Bignotti.",
            "Copyright (c) 2016 Ryan L McIntyre",
            "Copyright 2013 Kim Silkebækken and other contributors",
            "Copyright (c) 2013 Joe Loughry",
            "Larry Ewing (lewing@isc.tamu.edu) using The GIMP",
        ];

        Assert.All(required, notice => Assert.Contains(notice, Notices));
    }

    [Fact]
    public void Readme_preserves_creator_credits()
    {
        var start = Readme.IndexOf("## Credits", StringComparison.Ordinal);
        var end = Readme.IndexOf("## What it does", StringComparison.Ordinal);
        Assert.InRange(start, 0, end);
        var credits = Readme[start..end];
        string[] creators =
        [
            "Ryan L McIntyre", "Nerd Fonts contributors", "Microsoft", "devicon contributors", "Theodore Vorillas",
            "Fonticons, Inc.", "André Luiz Gava", "Lukas W", "Pictogrammers", "GitHub", "Jesse Weed",
            "Davide Bignotti", "Gabriele Lana", "Kim Silkebækken", "Joe Loughry", "Erik Flowers",
            "Lukas Bischoff", "Source Foundry Authors",
        ];

        Assert.All(creators, creator => Assert.Contains(creator, credits));
    }

    [Fact]
    public void Summary_covers_every_icon_set_in_the_upstream_table()
    {
        var credited = SummaryRows().Select(row => row["Nerd Fonts table name"]).ToHashSet(StringComparer.Ordinal);
        var upstream = UpstreamRows();

        Assert.NotEmpty(upstream);
        Assert.All(upstream, set => Assert.Contains(set.Name, credited));
    }

    [Fact]
    public void Summary_licenses_match_the_upstream_table()
    {
        var summary = SummaryRows().ToDictionary(row => row["Nerd Fonts table name"], StringComparer.Ordinal);

        Assert.All(UpstreamRows(), set =>
        {
            Assert.True(UpstreamLicenseNames.TryGetValue(set.License, out var expected),
                $"Upstream license '{set.License}' of '{set.Name}' is unknown: review it and add it to {nameof(UpstreamLicenseNames)}.");
            var cell = summary[set.Name]["License"];
            Assert.True(cell.Contains(expected, StringComparison.Ordinal), $"'{set.Name}' is {expected} upstream, but the notices say: {cell}");
            if (set.License.Contains("RFN", StringComparison.Ordinal))
                Assert.Contains("Reserved Font Name", cell);
        });
    }

    [Fact]
    public void Every_license_in_the_summary_is_backed_by_its_cited_license_text()
    {
        Assert.All(SummaryRows(), row =>
        {
            var named = Licenses.Where(l => l.InCell.IsMatch(row["License"])).ToList();
            Assert.True(named.Count > 0, $"No known license named for '{row["Icon set"]}': {row["License"]}");

            var cited = LicenseReferences(row["License text"]).ToList();
            Assert.NotEmpty(cited);
            var texts = string.Join("\n", cited.Select(file => File.ReadAllText(Path.Combine(Root, "licenses", file))));

            Assert.All(named, license => Assert.True(license.InFile.IsMatch(texts),
                $"'{row["Icon set"]}' is credited as {license.Name}, but its license files ({string.Join(", ", cited)}) do not contain that license."));
        });
    }

    [Fact]
    public void Notices_cover_every_glyph_prefix()
    {
        using var glyphs = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root, "data", "glyphnames.json")));
        var prefixes = glyphs.RootElement.EnumerateObject()
            .Where(p => p.Name != "METADATA")
            .Select(p => p.Name[..p.Name.IndexOf('-')])
            .Distinct();

        Assert.All(prefixes, prefix => Assert.Contains($"`nf-{prefix}-*`", Notices));
    }

    [Fact]
    public void Every_license_file_is_referenced_and_every_reference_exists()
    {
        var referenced = LicenseReferences(Notices).ToHashSet(StringComparer.Ordinal);
        var present = Directory.GetFiles(Path.Combine(Root, "licenses"))
            .Select(Path.GetFileName)
            .ToHashSet(StringComparer.Ordinal);

        Assert.All(referenced, file => Assert.True(
            new FileInfo(Path.Combine(Root, "licenses", file)).Length > 0, $"licenses/{file} is missing or empty."));
        Assert.Empty(present.Except(referenced));
    }

    [Fact]
    public void License_files_are_the_reviewed_upstream_files()
    {
        var present = Directory.GetFiles(Path.Combine(Root, "licenses")).ToDictionary(path => new FileInfo(path).Name, StringComparer.Ordinal);

        Assert.Equal(ReviewedLicenseFiles.Keys.Order(StringComparer.Ordinal), present.Keys.Order(StringComparer.Ordinal));
        Assert.All(ReviewedLicenseFiles, file => Assert.True(
            file.Value == Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(present[file.Key]))).ToLowerInvariant(),
            $"licenses/{file.Key} changed. Review it against its upstream source and THIRD-PARTY-NOTICES.md, then update {nameof(ReviewedLicenseFiles)}."));
    }

    [Fact]
    public void Notices_record_the_hash_of_the_embedded_font()
    {
        var documented = Regex.Match(Notices, @"SHA-256 \| `([0-9a-f]{64})`");
        Assert.True(documented.Success, "THIRD-PARTY-NOTICES.md must state the font's SHA-256.");

        using var font = NerdFont.OpenStream();
        Assert.Equal(documented.Groups[1].Value, Convert.ToHexString(SHA256.HashData(font)).ToLowerInvariant());
    }

    [Fact]
    public void Readme_links_are_absolute_and_repository_links_point_at_this_release()
    {
        // nuget.org renders the packaged README and cannot resolve relative links. Links into this repository must use
        // the release tag, so that each version's page shows the notices that shipped with it.
        var links = Regex.Matches(Readme, @"\]\(([^)\s]+)\)").Select(m => m.Groups[1].Value).ToList();
        var release = new Regex($@"^{Regex.Escape(RepoUrl)}/(?:blob|tree)/v{Regex.Escape(PackageVersion())}/([^#]+)");

        Assert.NotEmpty(links);
        Assert.All(links, link => Assert.StartsWith("https://", link));
        Assert.All(links.Where(link => link.StartsWith(RepoUrl + "/", StringComparison.Ordinal)), link =>
        {
            var match = release.Match(link);
            Assert.True(match.Success, $"README links to {link}, not to the release tag v{PackageVersion()}.");
            var path = Path.Combine(Root, match.Groups[1].Value);
            Assert.True(File.Exists(path) || Directory.Exists(path), $"README links to a missing repository path: {link}");
        });
    }

    [Fact]
    public void Readme_credits_the_creators_before_the_usage_docs()
    {
        var credits = Readme.IndexOf("## Credits", StringComparison.Ordinal);
        var usage = Readme.IndexOf("## What it does", StringComparison.Ordinal);

        Assert.InRange(credits, 0, usage);
        Assert.Contains("Unofficial", Readme[..credits]);
        Assert.Contains($"{RepoUrl}/blob/v{PackageVersion()}/THIRD-PARTY-NOTICES.md", Readme[credits..usage]);
    }

    /// <summary>Rows of the table in section "2. Summary" of the notices, keyed by column header.</summary>
    private static List<Dictionary<string, string>> SummaryRows()
    {
        var start = Notices.IndexOf("\n## 2. Summary", StringComparison.Ordinal);
        Assert.True(start >= 0, "THIRD-PARTY-NOTICES.md has no '## 2. Summary' section.");
        var end = Notices.IndexOf("\n## ", start + 1, StringComparison.Ordinal);
        var lines = Notices[start..(end < 0 ? Notices.Length : end)].Split('\n')
            .Where(line => line.StartsWith("| ", StringComparison.Ordinal))
            .Select(Cells)
            .ToList();

        var header = lines[0];
        var rows = lines.Skip(1)
            .Select(cells => header.Zip(cells).ToDictionary(c => c.First, c => c.Second, StringComparer.Ordinal))
            .ToList();
        Assert.NotEmpty(rows);
        return rows;
    }

    /// <summary>Icon set name and license from the "Icon sets" table Nerd Fonts ships with the font.</summary>
    private static List<(string Name, string License)> UpstreamRows() =>
        File.ReadAllLines(Path.Combine(Root, "licenses", "NerdFonts-SymbolsOnly-README.md"))
            .SkipWhile(line => line != "## Icon sets")
            .Where(line => line.StartsWith("| ", StringComparison.Ordinal))
            .Skip(1) // header row
            .Select(Cells)
            .Select(cells => (cells[0], cells[3]))
            .ToList();

    private static string[] Cells(string tableLine) =>
        tableLine.Trim().Trim('|').Split('|').Select(cell => cell.Trim()).ToArray();

    /// <summary>Repository-relative license file names; external URLs such as creativecommons.org/licenses/by/ are not files.</summary>
    private static IEnumerable<string> LicenseReferences(string text) =>
        Regex.Matches(text, @"(?<![/\w])licenses/([A-Za-z0-9._-]+)").Select(m => m.Groups[1].Value).Distinct();

    /// <summary>Pattern matching the words of <paramref name="phrase"/> separated by any whitespace, as license texts wrap lines.</summary>
    private static string Words(string phrase) => string.Join(@"\s+", phrase.Split(' ').Select(Regex.Escape));

    private static string PackageVersion()
    {
        var project = File.ReadAllText(Path.Combine(Root, "src", "neisep.NerdFonts", "neisep.NerdFonts.csproj"));
        var version = Regex.Match(project, "<Version>([^<]+)</Version>");
        Assert.True(version.Success, "neisep.NerdFonts.csproj has no <Version>.");
        return version.Groups[1].Value;
    }

    private static string FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "neisep.NerdFonts.slnx")))
                return dir.FullName;
        }
        throw new InvalidOperationException("Repository root (neisep.NerdFonts.slnx) not found.");
    }
}
