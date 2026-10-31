using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace neisep.NerdFonts;

/// <summary>
/// The embedded Symbols Nerd Font that renders the icon constants. The font is the work of
/// Nerd Fonts (https://www.nerdfonts.com) and the upstream icon projects, embedded unmodified under their
/// own licenses, not this package's MIT license. See THIRD-PARTY-NOTICES.md.
/// </summary>
public static partial class NerdFont
{
    /// <summary>Font family name to use once the font is registered with a UI framework.</summary>
    public const string FamilyName = "Symbols Nerd Font";

    /// <summary>File name of the embedded TrueType font.</summary>
    public const string FileName = "SymbolsNerdFont-Regular.ttf";

    private const string ResourceName = "neisep.NerdFonts." + FileName;

    /// <summary>Opens a read-only stream over the embedded font. The caller disposes it.</summary>
    public static Stream OpenStream() =>
        typeof(NerdFont).Assembly.GetManifestResourceStream(ResourceName)
        ?? throw new InvalidOperationException($"Embedded font resource '{ResourceName}' not found.");

    /// <summary>
    /// Writes the font to <paramref name="directory"/> for frameworks that load fonts from disk, such as WPF,
    /// and returns the file path. An existing identical file is reused.
    /// </summary>
    public static string ExtractTo(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("A directory is required.", nameof(directory));

        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, FileName);

        using var source = OpenStream();
        if (File.Exists(path))
        {
            using var existing = File.OpenRead(path);
            if (existing.Length == source.Length)
            {
                using var hash = SHA256.Create();
                if (hash.ComputeHash(existing).SequenceEqual(hash.ComputeHash(source)))
                    return path;

                source.Position = 0;
            }
        }

        using var target = File.Create(path);
        source.CopyTo(target);
        return path;
    }
}
