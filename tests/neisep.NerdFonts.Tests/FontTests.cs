using System.Buffers.Binary;
using System.Text;

namespace neisep.NerdFonts.Tests;

public class FontTests
{
    [Fact]
    public void OpenStream_returns_a_truetype_font()
    {
        using var stream = NerdFont.OpenStream();
        var header = new byte[4];
        stream.ReadExactly(header);

        Assert.Equal(0x00010000u, BinaryPrimitives.ReadUInt32BigEndian(header));
    }

    [Fact]
    public void Embedded_font_family_name_matches_FamilyName()
    {
        using var stream = NerdFont.OpenStream();
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);

        Assert.Contains(NerdFont.FamilyName, ReadFamilyNames(buffer.ToArray()));
    }

    [Fact]
    public void ExtractTo_writes_the_font_and_reuses_an_identical_file()
    {
        var directory = Path.Combine(Path.GetTempPath(), "neisep.NerdFonts.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            var path = NerdFont.ExtractTo(directory);
            using var stream = NerdFont.OpenStream();

            Assert.Equal(Path.Combine(directory, NerdFont.FileName), path);
            Assert.Equal(stream.Length, new FileInfo(path).Length);
            Assert.Equal(path, NerdFont.ExtractTo(directory));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ExtractTo_rejects_a_missing_directory(string? directory) =>
        Assert.Throws<ArgumentException>(() => NerdFont.ExtractTo(directory!));

    /// <summary>Reads the font family names (name ID 1) from the TrueType 'name' table.</summary>
    private static List<string> ReadFamilyNames(byte[] font)
    {
        var span = font.AsSpan();
        int tableCount = BinaryPrimitives.ReadUInt16BigEndian(span[4..]);
        for (var i = 0; i < tableCount; i++)
        {
            var record = span[(12 + i * 16)..];
            if (Encoding.ASCII.GetString(record[..4]) != "name") continue;

            var table = span[(int)BinaryPrimitives.ReadUInt32BigEndian(record[8..])..];
            int count = BinaryPrimitives.ReadUInt16BigEndian(table[2..]);
            int strings = BinaryPrimitives.ReadUInt16BigEndian(table[4..]);
            var names = new List<string>();
            for (var n = 0; n < count; n++)
            {
                var entry = table[(6 + n * 12)..];
                int platform = BinaryPrimitives.ReadUInt16BigEndian(entry);
                int nameId = BinaryPrimitives.ReadUInt16BigEndian(entry[6..]);
                if (nameId != 1 || platform != 3) continue;

                int length = BinaryPrimitives.ReadUInt16BigEndian(entry[8..]);
                int offset = BinaryPrimitives.ReadUInt16BigEndian(entry[10..]);
                names.Add(Encoding.BigEndianUnicode.GetString(table.Slice(strings + offset, length)));
            }
            return names;
        }
        throw new InvalidDataException("Font has no 'name' table.");
    }
}
