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
            Assert.Equal(Path.Combine(directory, NerdFont.FileName), path);
            Assert.Equal(ReadFontBytes(), File.ReadAllBytes(path));

            File.SetLastWriteTimeUtc(path, new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var written = File.GetLastWriteTimeUtc(path);
            Assert.Equal(path, NerdFont.ExtractTo(directory));
            Assert.Equal(written, File.GetLastWriteTimeUtc(path));
            Assert.Equal(ReadFontBytes(), File.ReadAllBytes(path));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void ExtractTo_replaces_a_different_file(int lengthDifference)
    {
        var directory = Path.Combine(Path.GetTempPath(), "neisep.NerdFonts.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var expected = ReadFontBytes();
            var corrupt = new byte[expected.Length + lengthDifference];
            Array.Copy(expected, corrupt, Math.Min(expected.Length, corrupt.Length));
            // Corrupt the final byte so a same-length file must be checked all the way through.
            corrupt[^1] ^= 0xFF;
            var path = Path.Combine(directory, NerdFont.FileName);
            File.WriteAllBytes(path, corrupt);

            Assert.Equal(path, NerdFont.ExtractTo(directory));
            Assert.Equal(expected, File.ReadAllBytes(path));
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

    private static byte[] ReadFontBytes()
    {
        using var stream = NerdFont.OpenStream();
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

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
