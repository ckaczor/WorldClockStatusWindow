using System.Collections.ObjectModel;
using System.Text.Json;
using WorldClockStatusWindow.Properties;

namespace WorldClockStatusWindow;

internal static class Data
{
    internal static ObservableCollection<TimeZoneEntry> TimeZoneEntries { get; set; }

    internal static void Load()
    {
        TimeZoneEntries = JsonSerializer.Deserialize<ObservableCollection<TimeZoneEntry>>(Settings.Default.TimeZones);
    }

    internal static void Save()
    {
        Settings.Default.TimeZones = JsonSerializer.Serialize(TimeZoneEntries);
        Settings.Default.Save();
    }
}