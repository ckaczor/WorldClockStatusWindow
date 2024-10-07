using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace WorldClockStatusWindow.SettingsWindow;

public partial class TimeZonesSettingsPanel
{
    private CollectionViewSource _collectionViewSource;

    public TimeZonesSettingsPanel()
    {
        InitializeComponent();
    }

    public override string CategoryName => Properties.Resources.optionCategoryTimeZones;

    public override void LoadPanel(Window parentWindow)
    {
        base.LoadPanel(parentWindow);

        if (_collectionViewSource == null)
        {
            _collectionViewSource = new CollectionViewSource { Source = Data.TimeZoneEntries };

            TimeZoneDataGrid.ItemsSource = _collectionViewSource.View;
        }

        _collectionViewSource.View.Refresh();

        if (TimeZoneDataGrid.Items.Count > 0)
            TimeZoneDataGrid.SelectedIndex = 0;

        SetTimeZoneButtonStates();
    }

    private void HandleTimeZoneDataGridSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SetTimeZoneButtonStates();
    }

    private void SetTimeZoneButtonStates()
    {
        AddTimeZoneButton.IsEnabled = true;
        EditTimeZoneButton.IsEnabled = TimeZoneDataGrid.SelectedItems.Count == 1;
        DeleteTimeZoneButton.IsEnabled = TimeZoneDataGrid.SelectedItems.Count > 0;
    }

    private void HandleAddTimeZoneButtonClick(object sender, RoutedEventArgs e)
    {
        AddTimeZone();
    }

    private void HandleEditTimeZoneButtonClick(object sender, RoutedEventArgs e)
    {
        EditSelectedTimeZone();
    }

    private void HandleDeleteTimeZoneButtonClick(object sender, RoutedEventArgs e)
    {
        DeleteSelectedTimeZones();
    }

    private void HandleTimeZoneDataGridRowMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        EditSelectedTimeZone();
    }

    private void AddTimeZone()
    {
        var timeZoneEntry = new TimeZoneEntry { TimeZoneId = TimeZoneInfo.Local.Id };

        var timeZoneWindow = new TimeZoneWindow();

        var result = timeZoneWindow.Display(timeZoneEntry, Window.GetWindow(this));

        if (!result.HasValue || !result.Value)
            return;

        TimeZoneDataGrid.SelectedItem = timeZoneEntry;

        SetTimeZoneButtonStates();
    }

    private void EditSelectedTimeZone()
    {
        if (TimeZoneDataGrid.SelectedItem == null)
            return;

        var timeZoneEntry = (TimeZoneEntry) TimeZoneDataGrid.SelectedItem;

        var timeZoneWindow = new TimeZoneWindow();

        timeZoneWindow.Display(timeZoneEntry, Window.GetWindow(this));
    }

    private void DeleteSelectedTimeZones()
    {
        if (MessageBox.Show(ParentWindow!, Properties.Resources.ConfirmDeleteTimeZones, Properties.Resources.ConfirmDeleteTitle, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.No)
            return;

        var selectedItems = new TimeZoneEntry[TimeZoneDataGrid.SelectedItems.Count];

        TimeZoneDataGrid.SelectedItems.CopyTo(selectedItems, 0);

        foreach (var timeZoneEntry in selectedItems)
            Data.TimeZoneEntries.Remove(timeZoneEntry);

        SetTimeZoneButtonStates();
    }
}