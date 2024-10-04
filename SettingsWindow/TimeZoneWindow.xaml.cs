using ChrisKaczor.Wpf.Validation;
using System;
using System.Windows;

namespace WorldClockStatusWindow.SettingsWindow;

public partial class TimeZoneWindow
{
    public TimeZoneWindow()
    {
        InitializeComponent();
    }

    public bool? Display(TimeZoneEntry timeZoneEntry, Window owner)
    {
        DataContext = timeZoneEntry;

        TimeZoneComboBox.ItemsSource = TimeZoneInfo.GetSystemTimeZones();

        Title = string.IsNullOrWhiteSpace(timeZoneEntry.Label) ? Properties.Resources.TimeZoneWindowAdd : Properties.Resources.TimeZoneWindowEdit;

        Owner = owner;

        return ShowDialog();
    }

    private void HandleOkayButtonClick(object sender, RoutedEventArgs e)
    {
        if (!this.IsValid())
            return;

        var timeZoneEntry = (TimeZoneEntry)DataContext;

        if (!Data.TimeZoneEntries.Contains(timeZoneEntry))
            Data.TimeZoneEntries.Add(timeZoneEntry);

        Data.Save();

        DialogResult = true;

        Close();
    }
}