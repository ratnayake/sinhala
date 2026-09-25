namespace SinhalaInput.App.Settings;

public interface ISettingsStore
{
    /// <summary>Returns the saved settings, or defaults when none are saved or they are unreadable.</summary>
    UserSettings Load();

    void Save(UserSettings settings);
}
