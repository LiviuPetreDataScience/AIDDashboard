using PdfSharp.Fonts;

namespace AidDashboard.Infrastructure.ImportExport;

/// <summary>
/// Supplies a single TrueType font to PDFsharp by locating a common system font.
/// PDFsharp (cross-platform) requires an explicit font resolver; this searches the usual
/// font locations on Windows, Linux and macOS so PDF export works without bundling a font.
/// </summary>
public class DefaultFontResolver : IFontResolver
{
    private const string FaceName = "AidDefault";

    private static readonly string[] CandidateFontPaths =
    {
        @"C:\Windows\Fonts\arial.ttf",
        @"C:\Windows\Fonts\segoeui.ttf",
        "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
        "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf",
        "/Library/Fonts/Arial.ttf",
        "/System/Library/Fonts/Supplemental/Arial.ttf",
    };

    private static readonly Lazy<byte[]?> FontBytes = new(LoadFirstAvailableFont);

    public byte[]? GetFont(string faceName) => FontBytes.Value;

    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic) =>
        FontBytes.Value is null ? null : new FontResolverInfo(FaceName);

    /// <summary>True when a usable font was found, i.e. PDF export is available.</summary>
    public static bool IsFontAvailable => FontBytes.Value is not null;

    private static byte[]? LoadFirstAvailableFont()
    {
        foreach (var path in CandidateFontPaths)
        {
            if (File.Exists(path))
            {
                return File.ReadAllBytes(path);
            }
        }
        return null;
    }
}
