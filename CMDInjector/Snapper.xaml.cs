using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.System;
using Windows.Media.Editing;
using Windows.UI.ViewManagement;
using Windows.Foundation.Metadata;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.ApplicationModel;
using System.Threading;
using Windows.System.Display;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.UI.Core;
using CMDInjectorHelper;
using WinUniversalTool;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CMDInjector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Snapper : Page
    {
        bool isConvertInProgress = false;
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient tClient = new TelnetClient(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        StorageFolder SnapPicturesFolder;
        StorageFolder snapVideosFolder;
        StorageFolder ShotsFolder;
        StorageFolder ClipsFolder;
        DisplayRequest displayRequest;
        ExtendedExecutionSession session = null;
        string videoFilename;
        bool isCapturing = false;
        bool isRecording = false;
        bool IsSessionStarted = false;
        bool SupportBackground = false;

        private void Connect()
        {
            _ = tClient.Connect();
            long i = 0;
            while (tClient.IsConnected == false && i < 1000000)
            {
                i++;
            }
            if (!tClient.IsConnected || !HomeHelper.IsConnected())
            {
                AmountBox.IsReadOnly = true;
                DelayBox.IsReadOnly = true;
                FrameRateBox.IsReadOnly = true;
                BitRateBox.IsReadOnly = true;
                RecordBtn.IsEnabled = false;
                _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
            }
        }

        private async Task SendCommand(string command)
        {
            await Task.Run(() =>
            {
                _ = tClient.Send(command);
            });
        }

        private async void FolderGen()
        {
            StorageFolder picturesFolder = KnownFolders.PicturesLibrary;
            StorageFolder videosFolder = KnownFolders.VideosLibrary;
            SnapPicturesFolder = await picturesFolder.CreateFolderAsync("Snapper", CreationCollisionOption.OpenIfExists);
            snapVideosFolder = await videosFolder.CreateFolderAsync("Snapper", CreationCollisionOption.OpenIfExists);
            ClipsFolder = await Helper.localFolder.CreateFolderAsync("SnapperRecords", CreationCollisionOption.ReplaceExisting);
        }

        public Snapper()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            FolderGen();
            if (HomeHelper.IsCMDInjected())
            {
                Connect();
            }
            else
            {
                _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                AmountBox.IsReadOnly = true;
                DelayBox.IsReadOnly = true;
                FrameRateBox.IsReadOnly = true;
                BitRateBox.IsReadOnly = true;
                RecordBtn.IsEnabled = false;
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
            {
                if (e.Parameter.ToString() == "StopSnapper=StopCapturing;OpenSnapper")
                {
                    if (CaptureBtn.Content as string == "Stop Capturing")
                    {
                        OperationCapture();
                    }
                }
                else if (e.Parameter.ToString() == "StopSnapper=StopRecording;OpenSnapper")
                {
                    if (RecordBtn.Content as string == "Stop Recording")
                    {
                        OperationRecord();
                    }
                }
                else if (e.Parameter.ToString() == "OpenImage")
                {
                    try
                    {
                        var OutputFile = await Launcher.LaunchFolderAsync(ShotsFolder);
                        if (OutputFile == false)
                        {
                            _ = Helper.MessageBox("Failed to open the file.", Helper.SoundHelper.Sound.Error, "Error Opening");
                        }
                    }
                    catch (Exception ex) { Helper.ThrowException(ex); }
                }
                else if (e.Parameter.ToString() == "OpenVideo")
                {
                    if (!string.IsNullOrEmpty(videoFilename))
                    {
                        var OutputFile = await Launcher.LaunchFileAsync(await snapVideosFolder.GetFileAsync(videoFilename));
                        if (OutputFile == false)
                        {
                            _ = Helper.MessageBox("Failed to open the file.", Helper.SoundHelper.Sound.Error, "Error Opening");
                        }
                    }
                }
            }
        }

        private void CaptureBtn_Click(object sender, RoutedEventArgs e)
        {
            OperationCapture();
        }

        private void RecordBtn_Click(object sender, RoutedEventArgs e)
        {
            OperationRecord();
        }

        private void AmountBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AmountBox.Text != string.Empty && DelayBox.Text != string.Empty && !AmountBox.Text.Contains(".") && !DelayBox.Text.Contains(".") && Convert.ToInt32(AmountBox.Text) > 0 && Convert.ToInt32(AmountBox.Text) <= 999 && Convert.ToInt32(DelayBox.Text) > 0 && Convert.ToInt32(DelayBox.Text) <= 60)
            {
                CaptureBtn.IsEnabled = true;
            }
            else
            {
                CaptureBtn.IsEnabled = false;
            }
        }

        private async void OpenShortsBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Launcher.LaunchFolderAsync(SnapPicturesFolder);
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private async void OpenVideosBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await Launcher.LaunchFolderAsync(snapVideosFolder);
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private void FrameRateBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FrameRateBox.Text != string.Empty && BitRateBox.Text != string.Empty && !FrameRateBox.Text.Contains(".") && !BitRateBox.Text.Contains(".") && Convert.ToInt32(FrameRateBox.Text) > 0 && Convert.ToInt32(FrameRateBox.Text) <= 30 && Convert.ToInt32(BitRateBox.Text) > 0 && Convert.ToInt32(BitRateBox.Text) <= 9999)
            {
                RecordBtn.IsEnabled = true;
            }
            else
            {
                RecordBtn.IsEnabled = false;
            }
        }

        private void OperationCapture()
        {
            Task.Run(async () =>
            {
                await Helper.BackgroundTaskHelper.RequestSessionAsync("BackgorundNotify");
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                {
                    if (displayRequest == null)
                    {
                        displayRequest = new DisplayRequest();
                        displayRequest.RequestActive();
                    }
                    if ((CaptureBtn.Content as string) == "Start Capture")
                    {
                        PlaySnapperSound(Helper.SoundHelper.Sound.Capture);
                        CaptureBtn.Content = "Stop Capturing";
                        RecordBtn.IsEnabled = false;
                        AmountBox.IsEnabled = false;
                        DelayBox.IsEnabled = false;
                        isCapturing = true;
                        if (Helper.LocalSettingsHelper.LoadSettings("SnapperNotify", true)) Helper.NotificationHelper.PushNotification("Capturing screenshots...", "Snapper", "OpenSnapper", null, int.Parse(AmountBox.Text) * int.Parse(DelayBox.Text) + 5, "Stop Capturing", "StopSnapper", "StopCapturing");
                        ShotsFolder = await SnapPicturesFolder.CreateFolderAsync($"{DateTime.Now:yyyyMMdd_HHmmss}", CreationCollisionOption.OpenIfExists);
                        for (int i = 0; i < Convert.ToInt32(DelayBox.Text); i++)
                        {
                            if (!isCapturing) break;
                            PlaySnapperSound(Helper.SoundHelper.Sound.Tick);
                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }
                        for (int i = 0; i < Convert.ToInt32(AmountBox.Text); i++)
                        {
                            if (!isCapturing) break;
                            PlaySnapperSound(Helper.SoundHelper.Sound.Capture);
                            await SendCommand($"ScreenCapture.exe \"{ShotsFolder.Path}\\SnapperShot_{i}.jpg\"");
                            await Task.Delay(500);
                            if (i + 1 < Convert.ToInt32(AmountBox.Text))
                            {
                                for (int j = 0; j < Convert.ToInt32(DelayBox.Text); j++)
                                {
                                    if (!isCapturing) break;
                                    PlaySnapperSound(Helper.SoundHelper.Sound.Tick);
                                    await Task.Delay(TimeSpan.FromSeconds(1));
                                }
                            }
                        }
                        CaptureBtn.Content = "Start Capture";
                        CaptureBtn.IsEnabled = IsEnabled;
                        if (BitRateBox.Text != string.Empty && FrameRateBox.Text != string.Empty) RecordBtn.IsEnabled = true;
                        AmountBox.IsEnabled = true;
                        DelayBox.IsEnabled = true;
                        if (Helper.LocalSettingsHelper.LoadSettings("SnapperNotify", true)) Helper.NotificationHelper.PushNotification("The captured shots has been saved to " + ShotsFolder.Path + ".", "Snapper", "OpenImage", null, 120);
                        Helper.BackgroundTaskHelper.ClearSession();
                    }
                    else
                    {
                        isCapturing = false;
                        CaptureBtn.Content = "Start Capture";
                        CaptureBtn.IsEnabled = false;
                    }
                    if (displayRequest != null)
                    {
                        displayRequest.RequestRelease();
                        displayRequest = null;
                    }
                });
            });
        }

        private void OperationRecord()
        {
            try
            {
                Task.Run(async () =>
                {
                    await Helper.BackgroundTaskHelper.RequestSessionAsync("BackgorundNotify");
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        if (displayRequest == null)
                        {
                            displayRequest = new DisplayRequest();
                            displayRequest.RequestActive();
                        }
                        if ((RecordBtn.Content as string) == "Start Record")
                        {
                            Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.StartRecord);
                            RecordBtn.Content = "Stop Recording";
                            CaptureBtn.IsEnabled = false;
                            FrameRateBox.IsEnabled = false;
                            BitRateBox.IsEnabled = false;
                            if (Helper.LocalSettingsHelper.LoadSettings("SnapperNotify", true)) Helper.NotificationHelper.PushNotification("Recording screen...", "Snapper", "OpenSnapper", "RecordTag", 1200, "Stop Recording", "StopSnapper", "StopRecording");
                            File.Delete($"{Helper.localFolder.Path}\\RecordStop.txt");
                            File.Delete($"{Helper.localFolder.Path}\\RecordEnd.txt");
                            _ = SendCommand(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\SnapperRecorder.bat 5000 0 " + Helper.localFolder.Path + " " + ClipsFolder.Path);
                            while (File.Exists($"{Helper.localFolder.Path}\\RecordEnd.txt") == false)
                            {
                                await Task.Delay(500);
                            }
                            await Helper.localFolder.CreateFileAsync("RecordStop.txt", CreationCollisionOption.OpenIfExists);
                            File.Delete($"{Helper.localFolder.Path}\\RecordEnd.txt");
                            RecordBtn.Content = "Processing...";
                            RecordBtn.IsEnabled = false;
                            if (AmountBox.Text != string.Empty && DelayBox.Text != string.Empty) CaptureBtn.IsEnabled = true;
                            ToastNotificationManager.History.Remove("RecordTag");
                            if (isConvertInProgress == false)
                            {
                                isConvertInProgress = true;
                                IProgress<double> progress = new Progress<double>(value =>
                                {
                                    var finalValue = Math.Round(value);
                                    RecordBtn.Content = $"Processing... {finalValue}%";
                                });
                                videoFilename = await Images2Video.Convert(ClipsFolder, Convert.ToInt32(FrameRateBox.Text), Convert.ToInt32(BitRateBox.Text), snapVideosFolder, progress);
                                isConvertInProgress = false;
                            }
                            while (isConvertInProgress)
                            {
                                await Task.Delay(100);
                            }
                            await Helper.localFolder.CreateFolderAsync("SnapperRecords", CreationCollisionOption.ReplaceExisting);
                            RecordBtn.Content = "Start Record";
                            RecordBtn.IsEnabled = true;
                            FrameRateBox.IsEnabled = true;
                            BitRateBox.IsEnabled = true;
                            if (Helper.LocalSettingsHelper.LoadSettings("SnapperNotify", true)) Helper.NotificationHelper.PushNotification("The recorded clip has been saved to " + snapVideosFolder.Path + "\\" + videoFilename + ".", "Snapper", "OpenVideo", null, 120);
                            Helper.BackgroundTaskHelper.ClearSession();
                        }
                        else
                        {
                            Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.StopRecord);
                            await Helper.localFolder.CreateFileAsync("RecordEnd.txt", CreationCollisionOption.OpenIfExists);
                        }
                        if (displayRequest != null)
                        {
                            displayRequest.RequestRelease();
                            displayRequest = null;
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }

        private void PlaySnapperSound(Helper.SoundHelper.Sound sound)
        {
            if (Helper.LocalSettingsHelper.LoadSettings("SnapSoundTog", true))
            {
                Helper.SoundHelper.PlaySound(sound);
            }
        }
    }
}
