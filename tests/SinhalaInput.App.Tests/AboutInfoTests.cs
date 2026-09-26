using SinhalaInput.App;

namespace SinhalaInput.App.Tests;

public class AboutInfoTests
{
    [Fact]
    public void Current_ShowsRequestedAuthorName()
    {
        Assert.Equal("Isuru Ratnayake", AboutInfo.Current.Author);
        Assert.Equal("Author: Isuru Ratnayake", AboutInfo.Current.AuthorText);
    }

    [Fact]
    public void Current_ShowsPublisher()
    {
        Assert.Equal("Ratcon", AboutInfo.Current.Publisher);
        Assert.Equal("Publisher: Ratcon", AboutInfo.Current.PublisherText);
    }

    [Fact]
    public void Current_ShowsProjectVersionWithoutBuildMetadata()
    {
        Assert.Equal("1.0.0", AboutInfo.Current.Version);
        Assert.Equal("Version 1.0.0", AboutInfo.Current.VersionText);
    }

    [Fact]
    public void Current_HasProductNameAndDescription()
    {
        Assert.Equal("EasyAkuru", AboutInfo.Current.ProductName);
        Assert.False(string.IsNullOrWhiteSpace(AboutInfo.Current.Description));
    }

    [Fact]
    public void FromAssembly_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => AboutInfo.FromAssembly(null!));
    }

    [Fact]
    public void AppIcon_EmbeddedResourceIsMultiResolutionIco()
    {
        using Stream? stream = AppIcon.OpenStream();
        Assert.NotNull(stream);

        using var reader = new BinaryReader(stream);
        Assert.Equal(0, reader.ReadUInt16());
        Assert.Equal(1, reader.ReadUInt16());
        Assert.True(reader.ReadUInt16() >= 5);
    }

    [Fact]
    public void AppIcon_CreateTrayIcon_LoadsEmbeddedIcon()
    {
        using System.Drawing.Icon icon = AppIcon.CreateTrayIcon();
        using System.Drawing.Bitmap bitmap = icon.ToBitmap();

        // Top-centre pixel is inside the maroon background (clear of the rounded corners and the glyph),
        // which the generic SystemIcons.Application fallback would not match.
        System.Drawing.Color pixel = bitmap.GetPixel(bitmap.Width / 2, 1);
        Assert.Equal((141, 21, 58), (pixel.R, pixel.G, pixel.B));
    }
}
