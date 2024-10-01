﻿using ChrisKaczor.Wpf.Windows;
using ChrisKaczor.Wpf.Windows.FloatingStatusWindow;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using WorldClockStatusWindow.Properties;
using WorldClockStatusWindow.SettingsWindow;

namespace WorldClockStatusWindow;

internal class WindowSource : IWindowSource, IDisposable
{
    private readonly FloatingStatusWindow _floatingStatusWindow;
    private readonly Timer _timer;
    private readonly Dispatcher _dispatcher;

    private List<TimeZoneEntry> _timeZoneEntries;

    internal WindowSource()
    {
        _floatingStatusWindow = new FloatingStatusWindow(this);
        _floatingStatusWindow.SetText(Resources.Loading);

        _dispatcher = Dispatcher.CurrentDispatcher;

        _timer = new Timer(1000);

        Task.Factory.StartNew(UpdateApp).ContinueWith(task => Start(task.Result.Result));
    }

    private async Task<bool> UpdateApp()
    {
        try
        {
            if (!UpdateCheck.IsInstalled)
                return false;

            if (!Settings.Default.CheckVersionAtStartup)
                return false;

            Log.Logger.Information("Checking for update");

            await _dispatcher.InvokeAsync(() => _floatingStatusWindow.SetText(Resources.CheckingForUpdate));

            var newVersion = await UpdateCheck.UpdateManager.CheckForUpdatesAsync();

            if (newVersion == null)
                return false;

            Log.Logger.Information("Downloading update");

            await _dispatcher.InvokeAsync(() => _floatingStatusWindow.SetText(Resources.DownloadingUpdate));

            await UpdateCheck.UpdateManager.DownloadUpdatesAsync(newVersion);

            Log.Logger.Information("Installing update");

            await _dispatcher.InvokeAsync(() => _floatingStatusWindow.SetText(Resources.InstallingUpdate));

            UpdateCheck.UpdateManager.ApplyUpdatesAndRestart(newVersion);
        }
        catch (Exception e)
        {
            Log.Logger.Error(e, nameof(UpdateApp));
        }

        return true;
    }

    private void Start(bool hasUpdate)
    {
        Log.Logger.Information($"Start: hasUpdate={hasUpdate}");

        if (hasUpdate)
            return;

        Log.Logger.Information("Load");

        Load();

        Log.Logger.Information("Starting timer");

        _timer.Elapsed += HandleTimerElapsed;
        _timer.Enabled = true;
    }

    private void Load()
    {
        _timeZoneEntries = JsonSerializer.Deserialize<List<TimeZoneEntry>>(Settings.Default.TimeZones);

        if (_timeZoneEntries.Any())
            return;

        _timeZoneEntries.Add(new TimeZoneEntry { Label = "UTC", TimeZoneId = "UTC" });
        _timeZoneEntries.Add(new TimeZoneEntry { Label = "IST", TimeZoneId = "India Standard Time" });
        _timeZoneEntries.Add(new TimeZoneEntry { Label = "CET", TimeZoneId = "Central Europe Standard Time" });
        _timeZoneEntries.Add(new TimeZoneEntry { Label = "Local", TimeZoneId = string.Empty });

        Save();
    }

    private void Save()
    {
        Settings.Default.TimeZones = JsonSerializer.Serialize(_timeZoneEntries);
        Settings.Default.Save();
    }

    private void HandleTimerElapsed(object sender, ElapsedEventArgs e)
    {
        var text = new StringBuilder();

        var now = DateTimeOffset.Now;

        var labelLength = _timeZoneEntries.Max(x => x.Label.Length);

        foreach (var timeZoneEntry in _timeZoneEntries)
        {
            var timeZone = timeZoneEntry.TimeZoneId == string.Empty ? TimeZoneInfo.Local : TimeZoneInfo.FindSystemTimeZoneById(timeZoneEntry.TimeZoneId);

            if (text.Length > 0)
                text.AppendLine();

            text.Append($"{timeZoneEntry.Label.PadLeft(labelLength)}: {TimeZoneInfo.ConvertTime(now, timeZone).ToString(Settings.Default.TimeFormat)}");
        }

        _dispatcher.Invoke(() => _floatingStatusWindow.SetText(text.ToString()));
    }

    public void Dispose()
    {
        _timer.Enabled = false;
        _timer.Dispose();

        _floatingStatusWindow.Save();
        _floatingStatusWindow.Dispose();
    }

    public Guid Id => Guid.Parse("29DF6CFD-6783-406F-AE12-4723EB7741EA");

    public string Name => Resources.ApplicationName;

    public System.Drawing.Icon Icon => Resources.ApplicationIcon;

    public bool HasSettingsMenu => true;

    public bool HasAboutMenu => false;

    public void ShowAbout()
    {
    }

    public void ShowSettings()
    {
        var categoryPanels = new List<CategoryPanelBase>
        {
            new GeneralSettingsPanel(),
            new UpdateSettingsPanel(),
            new AboutSettingsPanel()
        };

        var settingsWindow = new CategoryWindow(categoryPanels, Resources.SettingsTitle, Resources.CloseButtonText);

        var dialogResult = settingsWindow.ShowDialog();

        if (!dialogResult.GetValueOrDefault(false))
            return;

        Save();
    }

    public bool HasRefreshMenu => false;

    public void Refresh()
    {
    }

    public string WindowSettings
    {
        get => Settings.Default.WindowSettings;
        set
        {
            Settings.Default.WindowSettings = value;
            Settings.Default.Save();
        }
    }
}