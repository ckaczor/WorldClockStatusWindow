using NuGet.Versioning;
using Serilog;
using System.Windows;
using Velopack;
using Velopack.Sources;

namespace WorldClockStatusWindow
{
    internal static class UpdateCheck
    {
        private static UpdateManager _updateManager;

        public static UpdateManager UpdateManager => _updateManager ??= new UpdateManager(new GithubSource("https://github.com/ckaczor/WorldClockStatusWindow", null, false));

        public static string LocalVersion => (UpdateManager.CurrentVersion ?? new SemanticVersion(0, 0, 0)).ToString();

        public static bool IsInstalled => UpdateManager.IsInstalled;

        public static async void DisplayUpdateInformation(bool showIfCurrent)
        {
            UpdateInfo newVersion = null;

            if (IsInstalled)
            {
                newVersion = await UpdateManager.CheckForUpdatesAsync();
            }

            if (newVersion != null)
            {
                // Format the check title
                var updateCheckTitle = string.Format(Properties.Resources.UpdateCheckTitle, Properties.Resources.ApplicationName);

                // Format the message
                var updateCheckMessage = string.Format(Properties.Resources.UpdateCheckNewVersion, Properties.Resources.ApplicationName, newVersion.TargetFullRelease.Version);

                // Ask the user to update
                if (MessageBox.Show(updateCheckMessage, updateCheckTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                Log.Logger.Information("Downloading update");

                await UpdateManager.DownloadUpdatesAsync(newVersion);

                Log.Logger.Information("Installing update");

                UpdateManager.ApplyUpdatesAndRestart(newVersion);
            }
            else if (showIfCurrent)
            {
                // Format the check title
                var updateCheckTitle = string.Format(Properties.Resources.UpdateCheckTitle, Properties.Resources.ApplicationName);

                // Format the message
                var updateCheckMessage = string.Format(Properties.Resources.UpdateCheckCurrent, Properties.Resources.ApplicationName);

                MessageBox.Show(updateCheckMessage, updateCheckTitle, MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
