namespace SinhalaInput.Platform.Windows.Packaging;

/// <summary>
/// Tells whether the current process runs with MSIX package identity, which changes how
/// OS integrations such as start-at-sign-in must be done (manifest extensions rather than
/// per-user registry values).
/// </summary>
public static class PackageIdentity
{
    private static readonly Lazy<bool> s_isPackaged = new(DetectIsPackaged);

    public static bool IsPackaged => s_isPackaged.Value;

    private static bool DetectIsPackaged()
    {
        // A zero-length query only asks whether an identity exists: a packaged process gets
        // ERROR_INSUFFICIENT_BUFFER back, an unpackaged one APPMODEL_ERROR_NO_PACKAGE.
        uint length = 0;
        return NativeMethods.GetCurrentPackageFullName(ref length, 0) != NativeMethods.APPMODEL_ERROR_NO_PACKAGE;
    }
}
