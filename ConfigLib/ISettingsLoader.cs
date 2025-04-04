namespace ConfigLib
{
    public interface ISettingsLoader
    {
        Config LoadSettings(string settingsFile);
    }
}