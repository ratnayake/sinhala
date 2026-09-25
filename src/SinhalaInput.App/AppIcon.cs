using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SinhalaInput.App;

/// <summary>Loads the embedded application icon for the tray and WPF windows.</summary>
public static class AppIcon
{
    public const string ResourceName = "SinhalaInput.App.Assets.SinhalaInput.ico";

    public static Stream? OpenStream() =>
        typeof(AppIcon).Assembly.GetManifestResourceStream(ResourceName);

    /// <summary>
    /// Returns the icon frame best matching the tray's small-icon size, or the generic system icon
    /// if the resource cannot be read, so a missing asset never prevents the app from starting.
    /// </summary>
    public static System.Drawing.Icon CreateTrayIcon()
    {
        try
        {
            using Stream? stream = OpenStream();
            if (stream is not null)
            {
                return new System.Drawing.Icon(stream, System.Windows.Forms.SystemInformation.SmallIconSize);
            }
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or System.ComponentModel.Win32Exception)
        {
        }

        // Cloned because SystemIcons hands out a shared cached instance and callers own (dispose) the result.
        return (System.Drawing.Icon)System.Drawing.SystemIcons.Application.Clone();
    }

    /// <summary>
    /// Returns the largest frame of the icon. WPF derives both title-bar and taskbar sizes from the
    /// frame's decoder when it is used as <c>Window.Icon</c>, and the large frame scales down cleanly
    /// for in-window images.
    /// </summary>
    public static ImageSource? CreateWindowIcon()
    {
        try
        {
            using Stream? stream = OpenStream();
            if (stream is null)
            {
                return null;
            }

            BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            BitmapFrame frame = decoder.Frames.MaxBy(f => f.PixelWidth)!;
            frame.Freeze();
            return frame;
        }
        catch (Exception ex) when (ex is NotSupportedException or IOException or FileFormatException)
        {
            return null;
        }
    }
}
