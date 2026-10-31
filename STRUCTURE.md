# Code Structure Index

## src/neisep.NerdFonts/NerdFont.cs

### class NerdFont
- Location: src/neisep.NerdFonts/NerdFont.cs:13
- Namespace: neisep.NerdFonts
- Anchor: public static partial class NerdFont
- Summary: Opens the embedded Symbols Nerd Font and extracts it, reusing existing files only when SHA-256 contents match.
- Tags: font, extraction, resource, corruption, netstandard2.0
- Calls: NerdFont.OpenStream

#### method NerdFont.OpenStream
- Location: src/neisep.NerdFonts/NerdFont.cs:24
- Namespace: neisep.NerdFonts
- Anchor: public static Stream OpenStream() =>

#### method NerdFont.ExtractTo
- Location: src/neisep.NerdFonts/NerdFont.cs:32
- Namespace: neisep.NerdFonts
- Anchor: public static string ExtractTo(string directory)

## tests/neisep.NerdFonts.Tests/FontTests.cs

### class FontTests
- Location: tests/neisep.NerdFonts.Tests/FontTests.cs:6
- Namespace: neisep.NerdFonts.Tests
- Anchor: public class FontTests
- Summary: Checks font headers, family metadata, argument validation, exact extracted bytes, unchanged-file reuse and corrupt-file replacement.
- Tags: font, extraction, regression, corruption

#### method FontTests.OpenStream_returns_a_truetype_font
- Location: tests/neisep.NerdFonts.Tests/FontTests.cs:9
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void OpenStream_returns_a_truetype_font()

#### method FontTests.Embedded_font_family_name_matches_FamilyName
- Location: tests/neisep.NerdFonts.Tests/FontTests.cs:19
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void Embedded_font_family_name_matches_FamilyName()

#### method FontTests.ExtractTo_writes_the_font_and_reuses_an_identical_file
- Location: tests/neisep.NerdFonts.Tests/FontTests.cs:29
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void ExtractTo_writes_the_font_and_reuses_an_identical_file()

#### method FontTests.ExtractTo_replaces_a_different_file
- Location: tests/neisep.NerdFonts.Tests/FontTests.cs:54
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void ExtractTo_replaces_a_different_file(int lengthDifference)

#### method FontTests.ExtractTo_rejects_a_missing_directory
- Location: tests/neisep.NerdFonts.Tests/FontTests.cs:81
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void ExtractTo_rejects_a_missing_directory(string? directory) =>

#### method FontTests.ReadFontBytes
- Location: tests/neisep.NerdFonts.Tests/FontTests.cs:84
- Namespace: neisep.NerdFonts.Tests
- Anchor: private static byte[] ReadFontBytes()

#### method FontTests.ReadFamilyNames
- Location: tests/neisep.NerdFonts.Tests/FontTests.cs:93
- Namespace: neisep.NerdFonts.Tests
- Anchor: private static List<string> ReadFamilyNames(byte[] font)

## tests/neisep.NerdFonts.Tests/IconTests.cs

### class IconTests
- Location: tests/neisep.NerdFonts.Tests/IconTests.cs:8
- Namespace: neisep.NerdFonts.Tests
- Anchor: public class IconTests
- Summary: Verifies each named icon constant against the pinned upstream glyph code point, preserving aliases and C# naming exceptions.
- Tags: glyphs, mappings, aliases, generated, regression

#### method IconTests.IconFields
- Location: tests/neisep.NerdFonts.Tests/IconTests.cs:13
- Namespace: neisep.NerdFonts.Tests
- Anchor: private static IEnumerable<FieldInfo> IconFields() =>

#### method IconTests.Every_glyph_has_a_constant_with_its_code_point
- Location: tests/neisep.NerdFonts.Tests/IconTests.cs:20
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void Every_glyph_has_a_constant_with_its_code_point()

#### method IconTests.Generated_metadata_matches_glyph_names
- Location: tests/neisep.NerdFonts.Tests/IconTests.cs:40
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void Generated_metadata_matches_glyph_names()

#### method IconTests.Known_icons_have_expected_code_points
- Location: tests/neisep.NerdFonts.Tests/IconTests.cs:52
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void Known_icons_have_expected_code_points(string icon, int codePoint) =>

#### method IconTests.MemberName
- Location: tests/neisep.NerdFonts.Tests/IconTests.cs:57
- Namespace: neisep.NerdFonts.Tests
- Anchor: private static string MemberName(string glyphName)

#### method IconTests.Pascal
- Location: tests/neisep.NerdFonts.Tests/IconTests.cs:69
- Namespace: neisep.NerdFonts.Tests
- Anchor: private static string Pascal(string name) =>

## tests/neisep.NerdFonts.Tests/NoticeTests.cs

### class NoticeTests
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:12
- Namespace: neisep.NerdFonts.Tests
- Anchor: public class NoticeTests
- Summary: Guards creator credits, copyright notices, pinned license files, set coverage, font hash and release README links.
- Tags: licenses, attribution, compliance, creators, release

#### method NoticeTests.readonly
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:61
- Namespace: neisep.NerdFonts.Tests
- Anchor: private static readonly (string Name, Regex InCell, Regex InFile)[] Licenses =

#### method NoticeTests.Summary_preserves_original_creator_credits
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:72
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void Summary_preserves_original_creator_credits()

#### method NoticeTests.Notices_preserve_reviewed_copyright_and_attribution
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:99
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void Notices_preserve_reviewed_copyright_and_attribution()

#### method NoticeTests.Readme_preserves_creator_credits
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:126
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void Readme_preserves_creator_credits()

#### method NoticeTests.Summary_covers_every_icon_set_in_the_upstream_table
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:144
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void Summary_covers_every_icon_set_in_the_upstream_table()

#### method NoticeTests.Summary_licenses_match_the_upstream_table
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:154
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void Summary_licenses_match_the_upstream_table()

#### method NoticeTests.Every_license_in_the_summary_is_backed_by_its_cited_license_text
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:170
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void Every_license_in_the_summary_is_backed_by_its_cited_license_text()

#### method NoticeTests.Notices_cover_every_glyph_prefix
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:187
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void Notices_cover_every_glyph_prefix()

#### method NoticeTests.Every_license_file_is_referenced_and_every_reference_exists
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:199
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void Every_license_file_is_referenced_and_every_reference_exists()

#### method NoticeTests.License_files_are_the_reviewed_upstream_files
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:212
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void License_files_are_the_reviewed_upstream_files()

#### method NoticeTests.Notices_record_the_hash_of_the_embedded_font
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:223
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void Notices_record_the_hash_of_the_embedded_font()

#### method NoticeTests.Readme_links_are_absolute_and_repository_links_point_at_this_release
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:233
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void Readme_links_are_absolute_and_repository_links_point_at_this_release()

#### method NoticeTests.Readme_credits_the_creators_before_the_usage_docs
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:252
- Namespace: neisep.NerdFonts.Tests
- Anchor: public void Readme_credits_the_creators_before_the_usage_docs()

#### method NoticeTests.UpstreamRows
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:282
- Namespace: neisep.NerdFonts.Tests
- Anchor: private static List<(string Name, string License)> UpstreamRows() =>

#### method NoticeTests.Cells
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:291
- Namespace: neisep.NerdFonts.Tests
- Anchor: private static string[] Cells(string tableLine) =>

#### method NoticeTests.LicenseReferences
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:295
- Namespace: neisep.NerdFonts.Tests
- Anchor: private static IEnumerable<string> LicenseReferences(string text) =>

#### method NoticeTests.Words
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:299
- Namespace: neisep.NerdFonts.Tests
- Anchor: private static string Words(string phrase) => string.Join(@"\s+", phrase.Split(' ').Select(Regex.Escape));

#### method NoticeTests.PackageVersion
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:301
- Namespace: neisep.NerdFonts.Tests
- Anchor: private static string PackageVersion()

#### method NoticeTests.FindRepoRoot
- Location: tests/neisep.NerdFonts.Tests/NoticeTests.cs:309
- Namespace: neisep.NerdFonts.Tests
- Anchor: private static string FindRepoRoot()

## tools/Update.cs
