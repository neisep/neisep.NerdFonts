# neisep.NerdFonts

Use the 10,995 [Nerd Fonts](https://www.nerdfonts.com) icons in any .NET app. The font is included, nothing else to install.

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

## License

MIT. The font bundles icon sets under their own licenses, see [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).
