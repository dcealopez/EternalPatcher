using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EternalPatcher
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Check for updates on launch and load the patch definitions file
            PatcherStatus.Text = "Initializing...";

            Task.Factory.StartNew(() => CheckForUpdates(true))
                .ContinueWith(t => MessageBox.Show(t.Exception.InnerException.Message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error),
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Patch button click event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void PatchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Patcher.AnyPatchesLoaded())
            {
                MessageBox.Show("No patch definitions are loaded. Try checking for updates.",
                    "No patch definitions loaded",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var openFileDialog = new OpenFileDialog()
            {
                Filter = "Exe Files (.exe)|*.exe",
                FilterIndex = 1,
                Multiselect = false,
                FileName = "DOOMEternalx64vk.exe",
                Title = "Open DOOM Eternal game executable file (.exe)"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                CheckUpdatesButton.IsEnabled = false;
                PatchButton.IsEnabled = false;

                Task.Factory.StartNew(() => Patch(openFileDialog.FileName))
                    .ContinueWith(t => MessageBox.Show(t.Exception.InnerException.Message),
                        CancellationToken.None,
                        TaskContinuationOptions.OnlyOnFaulted,
                        TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        /// <summary>
        /// Check updates button click event
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void CheckUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() => CheckForUpdates(false))
                .ContinueWith(t => MessageBox.Show(t.Exception.InnerException.Message, "Unexpected error", MessageBoxButton.OK, MessageBoxImage.Error),
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnFaulted,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// UI method for patching the specified file at the given file path
        /// </summary>
        /// <param name="filePath">file path</param>
        private void Patch(string filePath)
        {
            // First check if the selected executable is valid
            // and if the game build is supported
            Dispatcher.Invoke(() => PatcherStatus.Text = "Checking game version...");

            GameBuild gameBuild = null;

            try
            {
                gameBuild = Patcher.GetGameBuild(filePath);
            }
            catch (Exception)
            {
                MessageBox.Show("An error occured while checking the game version.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Dispatcher.Invoke(() => PatchButton.IsEnabled = true);
                Dispatcher.Invoke(() => CheckUpdatesButton.IsEnabled = true);
                Dispatcher.Invoke(() => PatcherStatus.Text = "Ready.");
                return;
            }

            // Unknown game build
            if (gameBuild == null)
            {
                MessageBox.Show("Make sure the selected file is correct and that " +
                    "you are not selecting an already patched game executable, or check for updates.",
                    "Unsupported game version detected",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Dispatcher.Invoke(() => PatchButton.IsEnabled = true);
                Dispatcher.Invoke(() => CheckUpdatesButton.IsEnabled = true);
                Dispatcher.Invoke(() => PatcherStatus.Text = "Ready.");
                return;
            }

            // Back up the executable?
            var makeBackup = MessageBox.Show("Do you want to make a backup of the current game executable?",
                "Backup",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (makeBackup == MessageBoxResult.Yes)
            {
                var saveFileDialog = new SaveFileDialog()
                {
                    Filter = "All Files (.*)|*.*",
                    FilterIndex = 1,
                    FileName = "DOOMEternalx64vk.exe.bak",
                    Title = "Save DOOM Eternal game executable file backup (.bak)"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        if (File.Exists(saveFileDialog.FileName))
                        {
                            File.Delete(saveFileDialog.FileName);
                        }

                        File.Copy(filePath, saveFileDialog.FileName);
                    }
                    catch (Exception)
                    {
                        var continueOrCancel = MessageBox.Show("An error occured while backing up the game executable. " +
                            "Do you want to continue patching anyways?",
                            "Error",
                            MessageBoxButton.OKCancel,
                            MessageBoxImage.Error);

                        if (continueOrCancel == MessageBoxResult.Cancel)
                        {
                            Dispatcher.Invoke(() => PatchButton.IsEnabled = true);
                            Dispatcher.Invoke(() => CheckUpdatesButton.IsEnabled = true);
                            Dispatcher.Invoke(() => PatcherStatus.Text = "Ready.");
                            return;
                        }
                    }
                }
            }

            // Patch the executable
            Dispatcher.Invoke(() => PatcherStatus.Text = "Patching...");

            try
            {
                var patchingResult = Patcher.ApplyPatches(filePath, gameBuild.Patches);

                // Display patching results
                var succesfulPatches = new StringBuilder();
                var failedPatches = new StringBuilder();
                var resultsString = new StringBuilder();
                int successes = 0;
                int fails = 0;

                foreach (var patchResult in patchingResult)
                {
                    if (patchResult.Success)
                    {
                        succesfulPatches.Append(patchResult.Patch.Description).Append("\n");
                        successes++;
                        continue;
                    }

                    failedPatches.Append(patchResult.Patch.Description).Append("\n");
                    fails++;
                }

                if (successes > 0)
                {
                    resultsString.Append("The following patches were successfuly applied:\n\n");
                    resultsString.Append(succesfulPatches);
                }

                if (fails > 0)
                {
                    if (successes > 0)
                    {
                        resultsString.Append("\n");
                    }

                    resultsString.Append("The following patches could not be applied:\n\n");
                    resultsString.Append(failedPatches);
                }

                resultsString.Append($"\n{successes} patches out of {gameBuild.Patches.Count} applied.");

                MessageBox.Show(resultsString.ToString(),
                    "Patching results",
                    MessageBoxButton.OK,
                    fails > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
            }
            catch (Exception)
            {
                MessageBox.Show($"An error occured while patching the game executable.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                Dispatcher.Invoke(() => PatchButton.IsEnabled = true);
                Dispatcher.Invoke(() => CheckUpdatesButton.IsEnabled = true);
                Dispatcher.Invoke(() => PatcherStatus.Text = "Ready.");
            }
        }

        /// <summary>
        /// UI method to check for patch definition updates
        /// </summary>
        /// <param name="silent">don't show alerts when no updates availabe</param>
        private void CheckForUpdates(bool silent)
        {
            Dispatcher.Invoke(() => PatchButton.IsEnabled = false);
            Dispatcher.Invoke(() => CheckUpdatesButton.IsEnabled = false);
            Dispatcher.Invoke(() => PatcherStatus.Text = "Checking for updates...");

            // Check if there are available updates
            var updatesNeeded = false;

            try
            {
                updatesNeeded = Patcher.AnyUpdateAvailable();

                if (!updatesNeeded)
                {
                    if (!silent)
                    {
                        MessageBox.Show("You are already have the latest patch definitions.",
                            "Check for updates",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }

                    // Load current patch definitions if needed
                    if (!Patcher.AnyPatchesLoaded())
                    {
                        Dispatcher.Invoke(() => PatcherStatus.Text = "Loading patch definitions...");
                        Patcher.LoadPatchDefinitions();
                    }

                    Dispatcher.Invoke(() => PatchButton.IsEnabled = true);
                    Dispatcher.Invoke(() => CheckUpdatesButton.IsEnabled = true);
                    Dispatcher.Invoke(() => PatcherStatus.Text = "Ready.");
                    return;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("An error occured while checking for updates.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Dispatcher.Invoke(() => PatchButton.IsEnabled = true);
                Dispatcher.Invoke(() => CheckUpdatesButton.IsEnabled = true);
                Dispatcher.Invoke(() => PatcherStatus.Text = "Ready.");
                return;
            }

            // Download updates if needed
            if (updatesNeeded)
            {
                try
                {
                    Dispatcher.Invoke(() => PatcherStatus.Text = "Downloading updates...");
                    Patcher.DownloadLatestPatchDefinitions();
                    Dispatcher.Invoke(() => PatcherStatus.Text = "Updates downloaded");
                }
                catch (Exception e)
                {
                    MessageBox.Show("An error occured while downloading the updates.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    Dispatcher.Invoke(() => PatchButton.IsEnabled = true);
                    Dispatcher.Invoke(() => CheckUpdatesButton.IsEnabled = true);
                    Dispatcher.Invoke(() => PatcherStatus.Text = "Ready.");
                    return;
                }
            }

            // Load the updated patch definitions
            try
            {
                Dispatcher.Invoke(() => PatcherStatus.Text = "Loading patch definitions...");
                Patcher.LoadPatchDefinitions();
                MessageBox.Show("Patch definitions have been successfully updated.",
                    "Check for updates",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("An error occured while loading the updated patch definitions.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                Dispatcher.Invoke(() => PatchButton.IsEnabled = true);
                Dispatcher.Invoke(() => CheckUpdatesButton.IsEnabled = true);
                Dispatcher.Invoke(() => PatcherStatus.Text = "Ready.");
            }
        }
    }
}
