# neisep.NerdFonts

> **Unofficial .NET integration for [Nerd Fonts](https://www.nerdfonts.com).**
> Not affiliated with or endorsed by the Nerd Fonts project or any of the icon authors below.

## Credits: the people who made these icons

The font and every icon in this package are the work of others:

- **[Nerd Fonts](https://github.com/ryanoasis/nerd-fonts)**, by Ryan L McIntyre and the Nerd Fonts contributors, builds and maintains the *Symbols Nerd Font* bundled here (v3.5.1, unmodified).
- The icons in that font come from:
  [Codicons](https://github.com/microsoft/vscode-codicons) (Microsoft) ·
  [Devicons](https://github.com/devicons/devicon) (devicon contributors, Theodore Vorillas) ·
  [Font Awesome Free](https://fontawesome.com) (Fonticons, Inc.) ·
  [Font Awesome Extension](https://github.com/AndreLZGava/font-awesome-extension) (André Luiz Gava) ·
  [Font Logos](https://github.com/lukas-w/font-logos) (Lukas W) ·
  [Material Design Icons](https://pictogrammers.com) (Pictogrammers) ·
  [Octicons](https://github.com/primer/octicons) (GitHub) ·
  [Seti UI](https://github.com/jesseweed/seti-ui) (Jesse Weed) ·
  [Pomicons](https://github.com/gabrielelana/pomicons) (Davide Bignotti, Gabriele Lana) ·
  [Powerline Symbols](https://github.com/powerline/powerline) (Kim Silkebækken and contributors) ·
  [Powerline Extra Symbols](https://github.com/ryanoasis/powerline-extra-symbols) (Ryan L McIntyre) ·
  [IEC Power Symbols](https://github.com/jloughry/Unicode) (Joe Loughry) ·
  [Weather Icons](https://github.com/erikflowers/weather-icons) (Erik Flowers, Lukas Bischoff) ·
  [Hack](https://github.com/source-foundry/Hack) (Source Foundry Authors).

I (neisep) did **not** create any of these fonts, glyphs, icons or artwork, and I claim no ownership of them. My contribution is the .NET integration: the C# API, the code generator, the packaging and the documentation.

All upstream licenses remain in effect: MIT, CC BY 4.0, Apache-2.0, SIL OFL 1.1 and the Unlicense, depending on the icon set. Only my integration code is MIT-licensed. Brand and logo icons are trademarks of their respective owners.

**→ [Full attribution and licenses (THIRD-PARTY-NOTICES.md)](https://github.com/neisep/neisep.NerdFonts/blob/v1.0.1/THIRD-PARTY-NOTICES.md)** ·
[License texts](https://github.com/neisep/neisep.NerdFonts/tree/v1.0.1/licenses) ·
[Nerd Fonts license](https://github.com/ryanoasis/nerd-fonts/blob/v3.5.1/LICENSE) ·
[Nerd Fonts license audit](https://github.com/ryanoasis/nerd-fonts/blob/v3.5.1/license-audit.md)

The same notice and license files are included inside the NuGet package.

**Shipping an app?** If your app distributes the font (for example via `NerdFont.ExtractTo`), include `THIRD-PARTY-NOTICES.md` and the `licenses/` folder from this package with it.

## What it does

Use the 10,995 Nerd Fonts icons in any .NET app. The font is included, nothing else to install.

```
dotnet add package neisep.NerdFonts
```

## How it works

Every icon is a string constant, like `Oct.GitBranch`. Show that string in the Nerd Font and you get the icon.
So: **register the font once, then use icons anywhere.**

## Avalonia

**1. Register the font** — add this class, and one line in `Program.cs`:

```csharp
using Avalonia.Media.Fonts;
using neisep.NerdFonts;

public sealed class NerdFontCollection : FontCollectionBase
{
    public NerdFontCollection()
    {
        if (!TryAddGlyphTypeface(NerdFont.OpenStream(), out _))
            throw new InvalidOperationException($"Could not load {NerdFont.FamilyName}.");
    }

    public override Uri Key { get; } = new("fonts:NerdFont");
}
```

```csharp
AppBuilder.Configure<App>()
    .UsePlatformDetect()
    .ConfigureFonts(fonts => fonts.AddFontCollection(new NerdFontCollection()));
```

**2. Show an icon:**

```xml
<Window xmlns:nf="using:neisep.NerdFonts" ...>
    <TextBlock FontFamily="fonts:NerdFont#Symbols Nerd Font" Text="{x:Static nf:Oct.GitBranch}" FontSize="24" />
```

## WPF

**1. Register the font** in `App.xaml.cs`:

```csharp
protected override void OnStartup(StartupEventArgs e)
{
    var path = NerdFont.ExtractTo(Path.Combine(AppContext.BaseDirectory, "fonts"));
    Resources["NerdFont"] = new FontFamily(new Uri(path), "./#" + NerdFont.FamilyName);
    base.OnStartup(e);
}
```

**2. Show an icon:**

```xml
<Window xmlns:nf="clr-namespace:neisep.NerdFonts;assembly=neisep.NerdFonts" ...>
    <TextBlock FontFamily="{StaticResource NerdFont}" Text="{x:Static nf:Oct.GitBranch}" FontSize="24" />
```

## WinForms

**1. Register the font** once, for example in `Program.cs`. Keep `fonts` alive while the app runs:

```csharp
using System.Drawing.Text;
using neisep.NerdFonts;

var fonts = new PrivateFontCollection();
fonts.AddFontFile(NerdFont.ExtractTo(Path.Combine(AppContext.BaseDirectory, "fonts")));
var nerdFont = new Font(fonts.Families[0], 24);
```

**2. Show an icon:**

```csharp
var label = new Label { Font = nerdFont, Text = Oct.GitBranch, AutoSize = true };
```

## Finding an icon

Search the [Nerd Fonts cheat sheet](https://www.nerdfonts.com/cheat-sheet) and convert the name:

| Cheat sheet           | C#                     |
|-----------------------|------------------------|
| `nf-oct-git_branch`   | `Oct.GitBranch`        |
| `nf-cod-git_stash`    | `Cod.GitStash`         |
| `nf-md-cloud_off_outline` | `Md.CloudOffOutline` |

The prefix becomes the class, the rest becomes PascalCase. Hover a constant in your IDE to see its original name.

## Updating Nerd Fonts

```
dotnet run tools/Update.cs -- 3.5.1
```

This downloads the font, the glyph names and the upstream license texts (into `licenses/`). Then review `THIRD-PARTY-NOTICES.md` against `licenses/NerdFonts-SymbolsOnly-README.md`. The tests fail until the notices cover every icon set with its upstream license, record the new font's SHA-256, and every changed license file has been reviewed and its hash updated in `tests/neisep.NerdFonts.Tests/NoticeTests.cs`.

## License

- **Integration code** (C# API, code generator, build files): MIT, see [LICENSE](https://github.com/neisep/neisep.NerdFonts/blob/v1.0.1/LICENSE).
- **Symbols Nerd Font and all icons**: the licenses of their creators (MIT, CC BY 4.0, Apache-2.0, SIL OFL 1.1, Unlicense), see [THIRD-PARTY-NOTICES.md](https://github.com/neisep/neisep.NerdFonts/blob/v1.0.1/THIRD-PARTY-NOTICES.md) and the [license texts](https://github.com/neisep/neisep.NerdFonts/tree/v1.0.1/licenses).

These files are also included inside the NuGet package.

**Package icon:** composed of Octicons `git-branch` (GitHub, MIT), Codicons `terminal` (Microsoft, CC BY 4.0), Font Logos `tux` (Lukas W, Unlicense; Tux originally by Larry Ewing, drawn with The GIMP) and Font Awesome `heart` (Fonticons, Inc., CC BY 4.0). Recoloured and composited. Details are in [THIRD-PARTY-NOTICES.md](https://github.com/neisep/neisep.NerdFonts/blob/v1.0.1/THIRD-PARTY-NOTICES.md#5-package-icon).
