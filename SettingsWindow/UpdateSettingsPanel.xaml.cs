using System.Windows;
using WorldClockStatusWindow.Properties;

namespace WorldClockStatusWindow.SettingsWindow;

public partial class UpdateSettingsPanel
{
    public UpdateSettingsPanel()
    {
        InitializeComponent();
    }

    public override string CategoryName => Properties.Resources.optionCategoryUpdate;

    private void HandleCheckVersionNowButtonClick(object sender, RoutedEventArgs e)
    {
        UpdateCheck.DisplayUpdateInformation(true);
    }

    private void OnSaveSettings(object sender, RoutedEventArgs e)
    {
        SaveSettings();
    }

    private void SaveSettings()
    {
        if (!HasLoaded) return;

        Settings.Default.Save();
    }
}