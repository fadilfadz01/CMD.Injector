using CMDInjectorHelper;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CMDInjector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class About : Page
    {
        public About()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            versionText.Text = $"CMD Injector v{Helper.currentVersion}";
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
            {
                if (e.Parameter.ToString() == "DownloadUpdate")
                {
                    if (UpdateBtn.Content as string == "Check For Update")
                    {
                        await StartUpdate(); ;
                    }
                }
                if (e.Parameter.ToString() == "Updation")
                {
                    try
                    {
                        StorageFolder downloadFolder = await AboutHelper.GetDownloadsFolder();
                        await Launcher.LaunchFolderAsync(downloadFolder);
                    }
                    catch (Exception ex)
                    {
                        Helper.ThrowException(ex);
                    }
                }
            }
        }

        private async void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            await StartUpdate();
        }

        private async Task StartUpdate()
        {
            await Task.Run(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    try
                    {
                        ToastNotificationManager.History.Remove("CheckUpdateTag");
                        UpdateBtn.IsEnabled = false;
                        UpdateBtn.Content = "Checking For Updade";
                        var latestRelease = await AboutHelper.IsNewUpdateAvailable();
                        if (latestRelease == null || latestRelease.Assets == null || latestRelease.Assets.Count < 1)
                        {
                            Helper.NotificationHelper.PushNotification("No new updates available right now. Please check again later.", "No Updates");
                            UpdateBtn.IsEnabled = true;
                            UpdateBtn.Content = "Check For Updade";
                            return;
                        }
                        UpdateBtn.Content = "Check For Update";
                        UpdateBtn.IsEnabled = true;
                        double megaSize = ConvertBytesToMegabytes(latestRelease.Assets[0].Size);
                        var result = await Helper.MessageBox($"{latestRelease.Name}\n\nChanges:\n{await AboutHelper.GetLatestReleaseNote()}\n\nPackage: {latestRelease.Assets[0].Name}\nType: {latestRelease.Assets[0].ContentType}\nSize: {String.Format("{0:0.000}", megaSize)}MB", Helper.SoundHelper.Sound.Alert, "Update Available", "Cancel", true, "Download");
                        if (result != 0)
                        {
                            return;
                        }
                        StorageFolder downloadFolder = await AboutHelper.GetDownloadsFolder();
                        if (downloadFolder == null)
                        {
                            _ = Helper.MessageBox("No download location is selected. The download is canceled.", Helper.SoundHelper.Sound.Error, "Missing Location", "Ok");
                            return;
                        }
                        StorageFile updatePackage = await downloadFolder.CreateFileAsync(latestRelease.Assets[0].Name, CreationCollisionOption.ReplaceExisting);
                        Uri sourceFile = new Uri(latestRelease.Assets[0].BrowserDownloadUrl);
                        UpdateBtn.Content = "Preparing To Download";
                        UpdateBtn.IsEnabled = false;
                        IProgress<int> progress = new Progress<int>(async value =>
                        {
                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                if (value == 100)
                                {
                                    Helper.NotificationHelper.PushNotification("The update has been downloaded to " + updatePackage.Path + ".", "Update Downloaded", "Updation", null, 30);

                                }
                                else if (value == -100)
                                {
                                    Helper.NotificationHelper.PushNotification("Download Failed", "Downloading the update is failed. Please try downloading again.", null, null, 120);
                                }
                                else if (value == -50)
                                {
                                    UpdateBtn.Content = "Waiting for network";
                                    Helper.NotificationHelper.PushNotification("The update download has paused due to the internet connection lost. It will continue once the connection is back.", "Connection Lost", "", "ConnectionLost");
                                }
                                else
                                {
                                    double doubleValue = Convert.ToDouble("0." + value);
                                    UpdateBtn.Content = $"Downloading Update... {value}%";
                                    if (Helper.NotificationHelper.IsToastAlreadyThere("ConnectionLost")) ToastNotificationManager.History.Remove("ConnectionLost");
                                }
                            });
                        });
                        await Helper.DownloadFile(sourceFile, updatePackage, progress);
                        if (Helper.build > 14393 && !PacManHelper.installOnProcess)
                        {
                            IProgress<double> iProgress = new Progress<double>(value =>
                            {
                                UpdateBtn.Content = $"Extracting package... {value}%";
                            });
                            var baseFolder = await Helper.Archive.ExtractZipWithProgress(updatePackage, Helper.cacheFolder, false, true, iProgress);
                            if (baseFolder != null)
                            {
                                var updateFiles = await baseFolder.GetFilesAsync();
                                Helper.pageNavigation.Invoke(4, null);
                                Frame.Navigate(typeof(PacMan), updateFiles);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "Excep_FromHResult 0x80072EE4")
                        {
                            Helper.NotificationHelper.PushNotification("Downloading the update is failed. Please try downloading again.", "Download Failed", null, null, 120);
                        }
                        else
                        {
                            Helper.NotificationHelper.PushNotification("Please turn on the cellular data or connect to a wifi.", "No Network");
                        }
                    }
                    UpdateBtn.Content = "Check For Update";
                    UpdateBtn.IsEnabled = true;
                });
            });
        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        private async void ChangelogBtn_Click(object sender, RoutedEventArgs e)
        {
            await Changelog.DisplayLog();
        }

        private async void PermissionHelp_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            if (Helper.build < 15063)
            {
                await Helper.MessageBox("The capabilities that begin with a * are optional capabilities.\n\n" +
                    " • Access your Internet connection\n" +
                    "Check the App update and download if available.\n\n" +
                    " • Use your documents library\n" +
                    "Store the PacMan installer log to the documents directory by default.\n\n" +
                    " • Use your pictures library\n" +
                    "Save the Snapper shots to the pictures directory.\n\n" +
                    " • Use your video library\n" +
                    "Save the Snapper clips to the videos directory.\n\n" +
                    " • Use data stored on an external storage device\n" +
                    "Install and move apps to the SD Card storage.\n\n" +
                    " • Gather information about other apps\n" +
                    "Used by the PacMan manager to read the installed apps information.\n\n" +
                    " • Manage other apps directly\n" +
                    "Used by the PacMan manager to launch, uninstall apps etc...\n\n" +
                    " • id_cap_runtime_config\n" +
                    "Read and write the registry and CMD components.\n\n" +
                    " • id_cap_oem_custom\n" +
                    "Same as the above capability id_cap_runtime_config.\n\n" +
                    " * extendedBackgroundTaskTime\n" +
                    "Helps Snapper screen capturer, screen recorder, and app update downloads running in the background.\n\n" +
                    " * Begin an unconstrained extended execution session\n" +
                    "Same as the above capability extendedBackgroundTaskTime.\n\n" +
                    " * OEM and MO Partner App\n" +
                    "Copy the glance screen files to the ..\\OEM\\Public directory on glance restoration.\n\n" +
                    " * id_cap_chamber_profile_code_rw\n" +
                    "Used by the PacMan manager to backup and move the apps package, and the TweakBox search options to read the apps AppxManifest.xml file and get the ProductID.\n\n" +
                    " * id_cap_chamber_profile_data_rw\n" +
                    "Used by the PacMan manager to backup the apps data.\n\n" +
                    " * id_cap_public_folder_full\n" +
                    "Used by the PacMan manager to launch the apps data directory in the File Explorer.\n\n", Helper.SoundHelper.Sound.Alert, "Permissions", "OK");
            }
            else
            {
                TextBlock textBlock = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap
                };
                textBlock.Inlines.Add(new Run { Text = "The capabilities that begin with a * are optional capabilities.\n\n", Foreground =  new SolidColorBrush(Colors.Yellow) });
                textBlock.Inlines.Add(new Run { Text = " • Access your Internet connection\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Check the App update and download if available.\n\n" });
                textBlock.Inlines.Add(new Run { Text = " • Use your documents library\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Store the PacMan installer log to the documents directory by default.\n\n" });
                textBlock.Inlines.Add(new Run { Text = " • Use your pictures library\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Save the Snapper shots to the pictures directory.\n\n" });
                textBlock.Inlines.Add(new Run { Text = " • Use your video library\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Save the Snapper clips to the videos directory.\n\n" });
                textBlock.Inlines.Add(new Run { Text = " • Use data stored on an external storage device\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Install and move apps to the SD Card storage.\n\n" });
                textBlock.Inlines.Add(new Run { Text = " • Gather information about other apps\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Used by the PacMan manager to read the installed apps information.\n\n" });
                textBlock.Inlines.Add(new Run { Text = " • Manage other apps directly\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Used by the PacMan manager to launch, uninstall apps etc...\n\n" });
                textBlock.Inlines.Add(new Run { Text = " • id_cap_runtime_config\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Read and write the registry and CMD components.\n\n" });
                textBlock.Inlines.Add(new Run { Text = " • id_cap_oem_custom\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Same as the above capability id_cap_runtime_config.\n\n" });
                textBlock.Inlines.Add(new Run { Text = " * extendedBackgroundTaskTime\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Helps Snapper screen capturer, screen recorder, and app update downloads running in the background.\n\n" });
                textBlock.Inlines.Add(new Run { Text = " * Begin an unconstrained extended execution session\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Same as the above capability extendedBackgroundTaskTime.\n\n" });
                textBlock.Inlines.Add(new Run { Text = " * OEM and MO Partner App\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Copy the glance screen files to the ..\\OEM\\Public directory on glance restoration.\n\n" });
                textBlock.Inlines.Add(new Run { Text = " * id_cap_chamber_profile_code_rw\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Used by the PacMan manager to backup and move the apps package, and the TweakBox search options to read the apps AppxManifest.xml file and get the ProductID.\n\n" });
                textBlock.Inlines.Add(new Run { Text = " * id_cap_chamber_profile_data_rw\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Used by the PacMan manager to backup the apps data.\n\n" });
                textBlock.Inlines.Add(new Run { Text = " * id_cap_public_folder_full\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Used by the PacMan manager to launch the apps data directory in the File Explorer.\n\n" });
                ScrollViewer scrollViewer = new ScrollViewer
                {
                    Content = textBlock
                };
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Permissions",
                    Content = scrollViewer,
                    PrimaryButtonText = "OK"
                };
                Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                await contentDialog.ShowAsync();
            }
        }
    }
}
