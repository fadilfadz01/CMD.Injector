using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.Storage;
using System.Text.RegularExpressions;
using System.Threading;
using Windows.Management.Deployment;
using Windows.ApplicationModel;
using Windows.System;
using Windows.ApplicationModel.DataTransfer;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Documents;
using Windows.System.Profile;
using Windows.ApplicationModel.Activation;
using Windows.Storage.AccessCache;
using CMDInjectorHelper;
using WinUniversalTool;
using System.IO.Compression;
using Newtonsoft.Json;
using Windows.Security.Credentials.UI;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CMDInjector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PacMan : Page
    {
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient tClient = new TelnetClient(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        ObservableCollection<PacManHelper.AppsDetails> appsDetails = new ObservableCollection<PacManHelper.AppsDetails>();
        ObservableCollection<PacManHelper.BackupInfo> appsInfo = new ObservableCollection<PacManHelper.BackupInfo>();
        bool buttonOnHold = false;
        int succeeded = 0;
        int failed = 0;
        bool flag = false;
        bool isRunning = false;

        PacManHelper.AppsDetails CurrentApp
        {
            get
            {
                return appsDetails.Where(x => x.Name == AppListCombo.SelectedItem.ToString() || x.DisplayName == AppListCombo.SelectedItem.ToString()).First();
            }
        }

        public PacMan()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            BrowseBtnTip.Visibility = Helper.LocalSettingsHelper.LoadSettings("BrowseBtnTipSettings", true) ? Visibility.Visible : Visibility.Collapsed;
            PacManMangerTip.Visibility = Helper.LocalSettingsHelper.LoadSettings("PacManMangerTipSettings", true) ? Visibility.Visible : Visibility.Collapsed;
            PacmanManager(false);
            Connect();
            Initialize("Loading");
            GetBackupFolder();
        }

        private async void GetBackupFolder()
        {
            if (!await PacManHelper.IsBackupFolderSelected())
            {
                _ = PacManHelper.GetBackupFolder();
            }
        }

        private async void Initialize(string loadingText)
        {
            try
            {
                if (isRunning == true)
                {
                    return;
                }
                isRunning = true;
                AppListCombo.Items.Clear();
                appsDetails.Where(l => true).ToList().All(i => appsDetails.Remove(i));
                IEnumerable<Package> allPackages = new PackageManager().FindPackagesForUser("");
                AppLoadingProg.Maximum = allPackages.Count();
                IEnumerable<PackageTypes> packageTypes = Enum.GetValues(typeof(PackageTypes)).Cast<PackageTypes>();
                foreach (var type in packageTypes)
                {
                    if (type.ToString() == "None" || type.ToString() == "Optional")
                    {
                        continue;
                    }
                    TextBlock textBlock = new TextBlock() { Text = type.ToString(), FontWeight = Windows.UI.Text.FontWeights.Bold, FontSize = 18 };
                    if (Helper.build >= 15063)
                    {
                        textBlock.TextDecorations = Windows.UI.Text.TextDecorations.Underline;
                    }
                    ComboBoxItem cbi = new ComboBoxItem() { Content = textBlock, IsHitTestVisible = false };
                    AppListCombo.Items.Add(cbi);
                    IProgress<double> progress = new Progress<double>(value =>
                    {
                        AppLoadingProg.Value += value;
                        var finalValue = Math.Round(100 * (AppLoadingProg.Value / AppLoadingProg.Maximum));
                        AppLoadingText.Text = $"{loadingText}... {finalValue}%";
                    });
                    var packages = await PacManHelper.GetPackagesByType(type, true, progress);
                    foreach (var package in packages)
                    {
                        AppListCombo.Items.Add(package.DisplayName);
                        appsDetails.Add(package);
                    }
                    AppListCombo.SelectedIndex = 1;
                    if (type.ToString() != "Xap")
                    {
                        TextBlock blankTextBlock = new TextBlock() { Text = string.Empty };
                        ComboBoxItem blankCbi = new ComboBoxItem() { Content = blankTextBlock, IsHitTestVisible = false };
                        AppListCombo.Items.Add(blankCbi);
                    }
                }
                PacmanManager(true);
                AppLoadStack.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
            isRunning = false;
        }

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
                flag = true;
                BrowseBtn.IsEnabled = false;
                AppsPath.IsReadOnly = true;
                EnableLoopbackBtn.IsEnabled = false;
                DisableLoopbackBtn.IsEnabled = false;
                ResultBox.Text = $"Error: {HomeHelper.GetTelnetTroubleshoot()}";
            }
        }

        private async Task SendCommand(string command)
        {
            if (File.Exists($"{PacManHelper.InstallEndFile}")) File.Delete($"{PacManHelper.InstallEndFile}");
            if (File.Exists($"{PacManHelper.MoveEndFile}")) File.Delete($"{PacManHelper.MoveEndFile}");
            await Task.Run(async () =>
            {
                await tClient.Send(command);
            });
        }

        private async void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (buttonOnHold)
                {
                    buttonOnHold = false;
                    return;
                }
                FileOpenPicker picker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.ComputerFolder
                };
                if (RegisterType.IsChecked == true)
                {
                    picker.FileTypeFilter.Add(".xml");
                }
                else
                {
                    picker.FileTypeFilter.Add(".xap");
                    picker.FileTypeFilter.Add(".appx");
                    picker.FileTypeFilter.Add(".appxbundle");
                }
                var files = await picker.PickMultipleFilesAsync();
                if (files.Count > 0)
                {
                    AppsPath.Text = string.Empty;
                    foreach (var file in files)
                    {
                        AppsPath.Text += file.Path + ";";
                    }
                    AppsPath.Text = AppsPath.Text.Remove(AppsPath.Text.Length - 1, 1);
                }
            }
            catch
            {
                //CommonHelper.ThrowException(ex);
            }
        }

        private async void BrowseBtn_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
                buttonOnHold = true;
                if (e.HoldingState != Windows.UI.Input.HoldingState.Started) return;
                FileOpenPicker picker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.ComputerFolder
                };
                if (RegisterType.IsChecked == true)
                {
                    picker.FileTypeFilter.Add(".xml");
                }
                else
                {
                    picker.FileTypeFilter.Add(".xap");
                    picker.FileTypeFilter.Add(".appx");
                    picker.FileTypeFilter.Add(".appxbundle");
                }
                var files = await picker.PickMultipleFilesAsync();
                if (files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (string.IsNullOrWhiteSpace(AppsPath.Text) && string.IsNullOrEmpty(AppsPath.Text))
                        {
                            AppsPath.Text += file.Path + ";";
                        }
                        else
                        {
                            AppsPath.Text += ";" + file.Path + ";";
                        }
                    }
                    AppsPath.Text = AppsPath.Text.Remove(AppsPath.Text.Length - 1, 1);
                }
                BrowseBtnTip.Visibility = Visibility.Collapsed;
                Helper.LocalSettingsHelper.SaveSettings("BrowseBtnTipSettings", false);
            }
            catch
            {
                //CommonHelper.ThrowException(ex);
            }
        }

        private void AppsPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AppsPath.Text == string.Empty)
            {
                InstallBtn.IsEnabled = false;
                AppsPath.TextWrapping = TextWrapping.NoWrap;
            }
            else
            {
                InstallBtn.IsEnabled = true;
                AppsPath.TextWrapping = TextWrapping.Wrap;
            }
        }

        private void InstallBtn_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                await OperationInstall();
            });
        }

        private async Task OperationInstall()
        {
            PacManHelper.installOnProcess = true;
            await Helper.BackgroundTaskHelper.RequestSessionAsync("InstallApp");
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    SeeLogBox.Visibility = Visibility.Collapsed;
                    DeploymentOpt.IsEnabled = false;
                    DeploymentInfoBtn.IsEnabled = false;
                    InstallType.IsEnabled = false;
                    UpdateType.IsEnabled = false;
                    RegisterType.IsEnabled = false;
                    InstallBtn.IsEnabled = false;
                    BrowseBtn.IsEnabled = false;
                    AppsPath.IsEnabled = false;
                    InstallProg.Value = 0;
                    InstallProg.Maximum = AppsPath.Text.Split(';').Length;
                    IndivitualInstProg.IsIndeterminate = true;
                    IndivitualInstProg.Visibility = Visibility.Visible;
                    int appCount = 0;
                    succeeded = 0;
                    failed = 0;
                    PacManHelper.CreateLogFile();
                    foreach (var appPath in AppsPath.Text.Split(';'))
                    {
                        string report = string.Empty;
                        string selectedType = string.Empty;
                        string selectedOption = string.Empty;
                        string selectedStorage = string.Empty;
                        if (InstallType.IsChecked == true)
                        {
                            report = "Installing";
                            selectedType = "/Add";
                        }
                        else if (UpdateType.IsChecked == true)
                        {
                            report = "Updating";
                            selectedType = "/Update";
                        }
                        else if (RegisterType.IsChecked == true)
                        {
                            report = "Registering";
                            selectedType = "/Register";
                        }
                        if (Helper.LocalSettingsHelper.LoadSettings("StorageSet", false))
                        {
                            selectedType = "/AddToVolume";
                            selectedStorage = "/Volume:D:\\";
                        }
                        if (DeploymentOpt.SelectedIndex == 0)
                        {
                            selectedOption = "/DeploymentOption:32768";
                        }
                        else if (DeploymentOpt.SelectedIndex == 1)
                        {
                            selectedOption = "/DeploymentOption:1";
                        }
                        var appName = Path.GetFileName(appPath);
                        ResultBox.Text = $"[{++appCount}/{AppsPath.Text.Split(';').Length}] {report} \"{appName}\".";
                        if ((InstallType.IsChecked == true || UpdateType.IsChecked == true) && Path.GetExtension(appPath).ToLower() != ".xap" && Path.GetExtension(appPath).ToLower() != ".appx" && Path.GetExtension(appPath).ToLower() != ".appxbundle")
                        {
                            _ = SendCommand($"echo CustomErrorNotXml >{PacManHelper.InstallResultFile}&echo. >{PacManHelper.InstallEndFile}");
                        }
                        else if (RegisterType.IsChecked == true && Path.GetExtension(appPath).ToLower() != ".xml")
                        {
                            _ = SendCommand($"echo CustomErrorXml >{PacManHelper.InstallResultFile}&echo. >{PacManHelper.InstallEndFile}");
                        }
                        else
                        {
                            await SendCommand($"MinDeployAppx.exe {selectedType} /PackagePath:\"{appPath}\" {selectedStorage} {selectedOption} >{PacManHelper.InstallResultFile} 2>&1&echo. >{PacManHelper.InstallEndFile}");
                        }
                        while (File.Exists($"{PacManHelper.InstallEndFile}") == false)
                        {
                            await Task.Delay(500);
                        }
                        InstallProg.Value = appCount;
                        string result = await FileIO.ReadTextAsync(await PacManHelper.GetInstallResultFile());
                        if (result.Contains("ReturnCode:[0x0]"))
                        {
                            succeeded++;
                        }
                        /*else if (result.Contains("error 0x8004138A"))
                        {
                            _ = sendCommand();
                        }*/
                        else
                        {
                            failed++;
                            if (result.Contains("ExceptionCode:[0x80004005]"))
                            {
                                await PacManHelper.WriteLogFile($"Package: {appName}\nError: 0x80004005 Please check if the mount point is valid.\n\n");
                            }
                            else if (result.Contains("ExceptionCode:"))
                            {
                                string[] error = File.ReadLines($"{PacManHelper.InstallResultFile}").Take(7).ToArray();
                                string exceptionCode = error[0].Split('[', ']')[1];
                                string errorText = error[1].Substring(10);
                                await PacManHelper.WriteLogFile($"Package: {appName}\nError: " + exceptionCode + " " + errorText + "\n\n");
                            }
                            else if (result.Contains("CustomErrorNotXml"))
                            {
                                await PacManHelper.WriteLogFile($"Package: {appName}\nError: 0x80073cf0 error 0x80070002: Opening the package from location " + appName + " failed.\n\n");
                            }
                            else if (result.Contains("CustomErrorXml"))
                            {
                                await PacManHelper.WriteLogFile($"Package: {appName}\nError: 0x80073cf0 error 0x80070002: Reading the manifest file from location " + appName + " failed.\n\n");
                            }
                            else
                            {
                                string[] error = File.ReadLines($"{PacManHelper.InstallResultFile}").Take(7).ToArray();
                                string returnCode = error[0].Split('[', ']')[1];
                                string returnText = error[1].Split('[', ']')[1];
                                await PacManHelper.WriteLogFile($"Package: {appName}\nError: " + returnCode + " " + returnText + "\n\n");
                            }
                        }
                        await Task.Delay(500);
                    }
                    if (succeeded > 0)
                    {
                        PacmanManager(false);
                        AppLoadingText.Text = "Reloading...";
                        AppLoadingProg.Value = 0;
                        AppLoadStack.Visibility = Visibility.Visible;
                        Initialize("Reloading");
                    }
                    ResultBox.Text = $"Result: {succeeded} Succeeded, {failed} Failed.";
                    if (failed != 0)
                    {
                        SeeLogBox.Visibility = Visibility.Visible;
                        ShowLog();
                    }
                    IndivitualInstProg.Visibility = Visibility.Collapsed;
                    IndivitualInstProg.IsIndeterminate = false;
                    DeploymentOpt.IsEnabled = true;
                    DeploymentInfoBtn.IsEnabled = true;
                    InstallType.IsEnabled = true;
                    UpdateType.IsEnabled = true;
                    RegisterType.IsEnabled = true;
                    InstallBtn.IsEnabled = true;
                    BrowseBtn.IsEnabled = true;
                    AppsPath.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    if (succeeded > 0)
                    {
                        PacmanManager(false);
                        AppLoadingText.Text = "Reloading...";
                        AppLoadingProg.Value = 0;
                        AppLoadStack.Visibility = Visibility.Visible;
                        Initialize("Reloading");
                    }
                    Helper.ThrowException(ex);
                }
            });
            Helper.BackgroundTaskHelper.ClearSession();
            PacManHelper.installOnProcess = false;
        }

        private async void ShowLog()
        {
            string logPath = (await (await PacManHelper.GetLogPath()).GetFileAsync(PacManHelper.LogFileName)).Path;
            string logContent = await PacManHelper.ReadLogFile();
            if (Helper.build < 15063)
            {
                if (failed > 1)
                {
                    await Helper.MessageBox("Log file: " + logPath + "\n\n" + logContent, Helper.SoundHelper.Sound.Alert, $"Failed Packages");
                }
                else
                {
                    await Helper.MessageBox("Log file: " + logPath + "\n\n" + logContent, Helper.SoundHelper.Sound.Alert, $"Failed Package");
                }
            }
            else
            {
                TextBlock resultBlock = new TextBlock()
                {
                    Text = "Log file: " + logPath + "\n\n" + logContent,
                    TextWrapping = TextWrapping.Wrap
                };
                ScrollViewer resultScrollViewer = new ScrollViewer() { Content = resultBlock };
                ContentDialog resultDialog = new ContentDialog()
                {
                    Content = resultScrollViewer,
                    CloseButtonText = "Close",
                };
                if (failed > 1)
                {
                    resultDialog.Title = "Failed Packages";
                }
                else
                {
                    resultDialog.Title = "Failed Package";
                }
                Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                await resultDialog.ShowAsync();
            }
        }

        private void InstallType_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (InstallType.IsChecked == true)
                {
                    InstallBtn.Content = "Install";
                    AppsPath.PlaceholderText = "C:\\Data\\Users\\Public\\Documents\\Test.xap";
                }
                else if (UpdateType.IsChecked == true)
                {
                    InstallBtn.Content = "Update";
                    AppsPath.PlaceholderText = "C:\\Data\\Users\\Public\\Documents\\Test.xap";
                }
                else if (RegisterType.IsChecked == true)
                {
                    InstallBtn.Content = "Register";
                    AppsPath.PlaceholderText = "C:\\Data\\Users\\Public\\Documents\\TestPackage\\AppxManifest.xml";
                }
            }
            catch
            {
                //CommonHelper.ThrowException(ex);
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            while (Helper.LocalSettingsHelper.LoadSettings("LoginTogReg", true) && (await UserConsentVerifier.CheckAvailabilityAsync()) == UserConsentVerifierAvailability.Available)
            {
                await Task.Delay(200);
                if (Helper.userVerified) break;
            }
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
            {
                if (e.Parameter is string)
                {
                    string sharedFile = e.Parameter as string;
                    var strFilePath = await SharedStorageAccessManager.RedeemTokenForFileAsync(sharedFile);
                    if (Path.GetExtension(strFilePath.Path.ToLower()) == ".xap" || Path.GetExtension(strFilePath.Path.ToLower()) == ".appx" || Path.GetExtension(strFilePath.Path.ToLower()) == ".appxbundle" || Path.GetExtension(strFilePath.Path.ToLower()) == ".xml")
                    {
                        ManagerStack.Visibility = Visibility.Collapsed;
                        BrowseBtnTip.Visibility = Visibility.Collapsed;
                        AppsPath.Text = strFilePath.Path;
                        AppsPath.IsReadOnly = true;
                        InstallBtn.IsEnabled = true;
                        BrowseBtn.Visibility = Visibility.Collapsed;
                        if (Path.GetExtension(strFilePath.Path).ToLower() == ".xml")
                        {
                            RegisterType.IsChecked = true;
                            InstallType.Visibility = Visibility.Collapsed;
                            UpdateType.Visibility = Visibility.Collapsed;
                            RegisterType.Margin = new Thickness(40, 0, 0, 0);
                        }
                        else
                        {
                            RegisterType.Visibility = Visibility.Collapsed;
                            InstallType.Margin = new Thickness(15, 0, -30, 0);
                        }
                    }
                }
                else if (e.Parameter is FileActivatedEventArgs)
                {
                    StorageFile strFilePath = (e.Parameter as FileActivatedEventArgs).Files[0] as StorageFile;
                    if (Path.GetExtension(strFilePath.Path.ToLower()) == ".xap" || Path.GetExtension(strFilePath.Path.ToLower()) == ".appx" || Path.GetExtension(strFilePath.Path.ToLower()) == ".appxbundle" || Path.GetExtension(strFilePath.Path.ToLower()) == ".xml" || Path.GetExtension(strFilePath.Path.ToLower()) == ".pmlog" || Path.GetExtension(strFilePath.Path.ToLower()) == ".pmbak")
                    {
                        if (Path.GetExtension(strFilePath.Path.ToLower()) == ".pmlog")
                        {
                            if (Helper.build < 15063)
                            {
                                await Helper.MessageBox(await FileIO.ReadTextAsync(strFilePath), Helper.SoundHelper.Sound.Alert, "PacMan Log");
                            }
                            else
                            {
                                TextBlock resultBlock = new TextBlock()
                                {
                                    Text = await FileIO.ReadTextAsync(strFilePath),
                                    TextWrapping = TextWrapping.Wrap
                                };
                                ScrollViewer resultScrollViewer = new ScrollViewer() { Content = resultBlock };
                                ContentDialog resultDialog = new ContentDialog()
                                {
                                    Title = "PacMan Log",
                                    Content = resultScrollViewer,
                                    CloseButtonText = "Close",
                                };
                                Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                                _ = resultDialog.ShowAsync();
                            }
                        }
                        else if (Path.GetExtension(strFilePath.Path.ToLower()) == ".pmbak")
                        {
                            if (Helper.build < 15063)
                            {
                                _ = Helper.MessageBox("The backup & restore function is unsuppoted on build lower than 15063.");
                                return;
                            }
                            else if (!await Helper.IsCapabilitiesAllowed())
                            {
                                if (!await Helper.AskCapabilitiesPermission()) return;
                            }
                            RestoreBackup(strFilePath);
                        }
                        else
                        {
                            InstallBtn.IsEnabled = true;
                            AppsPath.Text = strFilePath.Path;
                            if (Path.GetExtension(strFilePath.Path.ToLower()) == ".xml")
                            {
                                RegisterType.IsChecked = true;
                            }
                        }
                    }
                }
                else if (e.Parameter is ProtocolActivatedEventArgs)
                {
                    var protocolArgs = e.Parameter as ProtocolActivatedEventArgs;
                    var AbsoluteURI = protocolArgs.Uri.AbsoluteUri;
                    var cleanToken = AbsoluteURI.Replace("cmdinjector::", "").Replace("cmdinjector:", "");
                    var strFilePath = await SharedStorageAccessManager.RedeemTokenForFileAsync(cleanToken);
                    if (Path.GetExtension(strFilePath.Path.ToLower()) == ".xap" || Path.GetExtension(strFilePath.Path.ToLower()) == ".appx" || Path.GetExtension(strFilePath.Path.ToLower()) == ".appxbundle" || Path.GetExtension(strFilePath.Path.ToLower()) == ".xml" || Path.GetExtension(strFilePath.Path).ToLower() == ".pmlog")
                    {
                        InstallBtn.IsEnabled = true;
                        AppsPath.Text = strFilePath.Path;
                        if (Path.GetExtension(strFilePath.Path.ToLower()) == ".xml")
                        {
                            RegisterType.IsChecked = true;
                        }
                    }
                }
                else if (e.Parameter is IReadOnlyList<StorageFile>)
                {
                    AppsPath.Text = (e.Parameter as IReadOnlyList<StorageFile>)[0].Path;
                    DeploymentOpt.SelectedIndex = 1;
                    InstallType.IsChecked = true;
                    InstallBtn.IsEnabled = true;
                }
                else
                {
                    StorageFile strFilePath = e.Parameter as StorageFile;
                    if (Path.GetExtension(strFilePath.Path.ToLower()) == ".xap" || Path.GetExtension(strFilePath.Path.ToLower()) == ".appx" || Path.GetExtension(strFilePath.Path.ToLower()) == ".appxbundle" || Path.GetExtension(strFilePath.Path.ToLower()) == ".xml")
                    {
                        ManagerStack.Visibility = Visibility.Collapsed;
                        BrowseBtnTip.Visibility = Visibility.Collapsed;
                        AppsPath.Text = strFilePath.Path;
                        AppsPath.IsReadOnly = true;
                        InstallBtn.IsEnabled = true;
                        BrowseBtn.Visibility = Visibility.Collapsed;
                        if (Path.GetExtension(strFilePath.Path).ToLower() == ".xml")
                        {
                            RegisterType.IsChecked = true;
                            InstallType.Visibility = Visibility.Collapsed;
                            UpdateType.Visibility = Visibility.Collapsed;
                            RegisterType.Margin = new Thickness(40, 0, 0, 0);
                        }
                        else
                        {
                            RegisterType.Visibility = Visibility.Collapsed;
                            InstallType.Margin = new Thickness(15, 0, -30, 0);
                        }
                    }
                    else if (Path.GetExtension(strFilePath.Path.ToLower()) == ".pmlog")
                    {
                        if (Helper.build < 15063)
                        {
                            await Helper.MessageBox(await FileIO.ReadTextAsync(strFilePath), Helper.SoundHelper.Sound.Alert, "PacMan Log");
                        }
                        else
                        {
                            TextBlock resultBlock = new TextBlock()
                            {
                                Text = await FileIO.ReadTextAsync(strFilePath),
                                TextWrapping = TextWrapping.Wrap
                            };
                            ScrollViewer resultScrollViewer = new ScrollViewer() { Content = resultBlock };
                            ContentDialog resultDialog = new ContentDialog()
                            {
                                Title = "PacMan Log",
                                Content = resultScrollViewer,
                                CloseButtonText = "Close",
                            };
                            Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                            await resultDialog.ShowAsync();
                        }
                    }
                    else if (Path.GetExtension(strFilePath.Path).ToLower() == ".pmbak")
                    {
                        if (Helper.build < 15063)
                        {
                            _ = Helper.MessageBox("The backup & restore function is unsuppoted on build lower than 15063.");
                            return;
                        }
                        else if (!await Helper.IsCapabilitiesAllowed())
                        {
                            if (!await Helper.AskCapabilitiesPermission()) return;
                        }
                        RestoreBackup(strFilePath);
                    }
                }
            }
        }

        private async void RestoreBackup(StorageFile strFilePath)
        {
            try
            {
                string isAppInstalled = string.Empty;
                var appInfoText = await Helper.Archive.ReadTextFromZip(strFilePath.Path, "AppInfo.json");
                PacManHelper.BackupInfo backupFile = JsonConvert.DeserializeObject<PacManHelper.BackupInfo>(appInfoText);
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        if (!isRunning) break;
                        await Task.Delay(200);
                    }
                    foreach (var appDetail in appsDetails)
                    {
                        if (appDetail.FullName == backupFile.FullName)
                        {
                            isAppInstalled = appDetail.Version;
                            break;
                        }
                    }
                });
                var textBlock = new TextBlock();
                textBlock.Inlines.Add(new Run { Text = "Title: ", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = $"{backupFile.Title}\n" });
                textBlock.Inlines.Add(new Run { Text = "Installed version: ", FontWeight = Windows.UI.Text.FontWeights.Bold });
                if (isAppInstalled == string.Empty) textBlock.Inlines.Add(new Run { Text = "Not installed\n" });
                else textBlock.Inlines.Add(new Run { Text = $"{isAppInstalled}\n" });
                textBlock.Inlines.Add(new Run { Text = "Backup version: ", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = $"{backupFile.Version}\n" });
                textBlock.Inlines.Add(new Run { Text = "Created on: ", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = $"{backupFile.CreatedDate}\n" });
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = backupFile.DisplayName,
                    Content = textBlock,
                    PrimaryButtonText = "Restore",
                    SecondaryButtonText = "Cancel"
                };
                Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                var result = await contentDialog.ShowAsync();
                if (result != ContentDialogResult.Primary) return;
                CustomContentDialog dialog;
                if (isAppInstalled == string.Empty)
                {
                    dialog = new CustomContentDialog("Restore", Helper.Archive.CheckFileExist(strFilePath.Path, "Package.zip"), Helper.Archive.CheckFileExist($"{strFilePath.Path}", "Data.zip"), false);
                }
                else
                {
                    dialog = new CustomContentDialog("Restore", Helper.Archive.CheckFileExist(strFilePath.Path, "Package.zip"), Helper.Archive.CheckFileExist($"{strFilePath.Path}", "Data.zip"));
                }
                await dialog.ShowAsync();
                if (dialog.Result == CustomContentDialogResult.Cancel || dialog.Result == CustomContentDialogResult.Nothing) return;
                await OperationRestore(strFilePath.Path, backupFile, dialog);
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
                EndProgression();
                PacmanManager(true);
            }
        }

        private async void AppInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var installedLocation = string.Empty;
                if (CurrentApp.InstalledLocation != null) installedLocation = CurrentApp.InstalledLocation.Path;
                if (Helper.build < 15063)
                {
                    await Helper.MessageBox($"Name: {CurrentApp.Name}\n\nDisplay Name: {CurrentApp.DisplayName}\n\nDescription: {CurrentApp.Description}\n\nFamily Name: {CurrentApp.FamilyName}\n\n" +
                        $"Full Name: {CurrentApp.FullName}\n\nPublisher: {CurrentApp.Publisher}\n\nArchitecture: {CurrentApp.Architecture}\n\nVersion: {CurrentApp.Version}\n\nPublisher ID: {CurrentApp.PublisherID}\n\n" +
                        $"Installed Date: {CurrentApp.InstalledDate}\n\nInstalled Location: {installedLocation}\n\nBundle: {CurrentApp.IsBundle}\n\nDevelopmentMode: {CurrentApp.IsDevelopmentMode}\n\n" +
                        $"Framework: {CurrentApp.IsFramework}\n\nResource Package: {CurrentApp.IsResourcePackage}\n\nXap: {CurrentApp.IsXap}", Helper.SoundHelper.Sound.Alert, "App Info");
                }
                else
                {
                    TextBlock textBlock = new TextBlock()
                    {
                        TextWrapping = TextWrapping.Wrap
                    };
                    textBlock.Inlines.Add(new Run { Text = "Name: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.Name}\n\n" });
                    textBlock.Inlines.Add(new Run { Text = "Display Name: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.DisplayName}\n\n" });
                    textBlock.Inlines.Add(new Run { Text = "Description: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.Description}\n\n" });
                    textBlock.Inlines.Add(new Run { Text = "Family Name: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.FamilyName}\n\n" });
                    textBlock.Inlines.Add(new Run { Text = "Full Name: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.FullName}\n\n" });
                    textBlock.Inlines.Add(new Run { Text = "Publisher: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.Publisher}\n\n" });
                    textBlock.Inlines.Add(new Run { Text = "Architecture: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.Architecture}\n\n" });
                    textBlock.Inlines.Add(new Run { Text = "Version: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.Version}\n\n" });
                    textBlock.Inlines.Add(new Run { Text = "Publisher ID: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.PublisherID}\n\n" });
                    textBlock.Inlines.Add(new Run { Text = "Installed Date: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.InstalledDate}\n\n" });
                    textBlock.Inlines.Add(new Run { Text = "Installed Location: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{installedLocation}\n\n" });
                    textBlock.Inlines.Add(new Run { Text = "Bundle: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.IsBundle}\n\n" });
                    textBlock.Inlines.Add(new Run { Text = "Development Mode: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.IsDevelopmentMode}\n\n" });
                    textBlock.Inlines.Add(new Run { Text = "Framework: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.IsFramework}\n\n" });
                    textBlock.Inlines.Add(new Run { Text = "Resource Package: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.IsResourcePackage}\n\n" });
                    textBlock.Inlines.Add(new Run { Text = "Xap: ", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.IsXap}" });
                    ScrollViewer scrollViewer = new ScrollViewer() { Content = textBlock };
                    ContentDialog contentDialog = new ContentDialog()
                    {
                        Title = "App Info",
                        Content = scrollViewer,
                        CloseButtonText = "OK"
                    };
                    Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                    await contentDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }

        private async void AppDataBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (buttonOnHold)
                {
                    buttonOnHold = false;
                    return;
                }
                if (!await Helper.IsCapabilitiesAllowed())
                {
                    var isAllowed = await Helper.AskCapabilitiesPermission();
                    if (!isAllowed) return;
                }
                if (CurrentApp.InstalledLocation == null)
                {
                    _ = Helper.MessageBox("Failed to open the application data folder, due to the application data location could not obtain.", Helper.SoundHelper.Sound.Error, "Error");
                    return;
                }
                StorageFolder dataFolder = null;
                if (CurrentApp.IsXap) dataFolder = await StorageFolder.GetFolderFromPathAsync($"{PacManHelper.XapAppDataInternal}\\{{{CurrentApp.Name.ToUpper()}}}");
                else dataFolder = await StorageFolder.GetFolderFromPathAsync($"{PacManHelper.AppDataInternal}\\{CurrentApp.FamilyName}");
                var res = await Launcher.LaunchFolderAsync(dataFolder);
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }

        private async void AppDataBtn_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
                buttonOnHold = true;
                if (e.HoldingState != Windows.UI.Input.HoldingState.Started) return;
                if (CurrentApp.InstalledLocation == null)
                {
                    _ = Helper.MessageBox("Failed to copy the application data path to the clipboard.", Helper.SoundHelper.Sound.Error, "Error");
                    return;
                }
                DataPackage dataPath = new DataPackage();
                if (CurrentApp.IsXap) dataPath.SetText($"{PacManHelper.XapAppDataInternal}\\{{{CurrentApp.Name.ToUpper()}}}");
                else dataPath.SetText($"{PacManHelper.AppDataInternal}\\{CurrentApp.FamilyName}");
                Clipboard.SetContent(dataPath);
                PacManMangerTip.Visibility = Visibility.Collapsed;
                Helper.LocalSettingsHelper.SaveSettings("PacManMangerTipSettings", false);
                await Helper.MessageBox("Application Data path copied to clipboard.", Helper.SoundHelper.Sound.Alert);
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }

        private async void AppFoldrBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (buttonOnHold)
                {
                    buttonOnHold = false;
                    return;
                }
                if (CurrentApp.InstalledLocation == null)
                {
                    _ = Helper.MessageBox("Failed to open the application folder, due to the application location could not obtain.", Helper.SoundHelper.Sound.Error, "Error");
                    return;
                }
                await Launcher.LaunchFolderAsync(CurrentApp.InstalledLocation);
            }
            catch (Exception ex) { }
        }

        private async void AppFoldrBtn_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
                buttonOnHold = true;
                if (e.HoldingState != Windows.UI.Input.HoldingState.Started) return;
                if (CurrentApp.InstalledLocation == null)
                {
                    _ = Helper.MessageBox("Failed to copy the application path to the clipboard.", Helper.SoundHelper.Sound.Error, "Error");
                    return;
                }
                DataPackage appPath = new DataPackage();
                if (CurrentApp.InstalledLocation == null) appPath.SetText($"C:\\Data\\Programs\\{{{CurrentApp.Name.ToUpper()}}}");
                else appPath.SetText(CurrentApp.InstalledLocation.Path);
                Clipboard.SetContent(appPath);
                PacManMangerTip.Visibility = Visibility.Collapsed;
                Helper.LocalSettingsHelper.SaveSettings("PacManMangerTipSettings", false);
                await Helper.MessageBox("Application installed path copied to clipboard.", Helper.SoundHelper.Sound.Alert);
            }
            catch (Exception ex)
            {
                //Helper.ThrowException(ex);
            }
        }

        private async void UninstallBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Confirmation",
                    Content = "Are you sure you want to uninstall the app?",
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "No"
                };
                Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                var result = await contentDialog.ShowAsync();
                if (result != ContentDialogResult.Primary)
                {
                    return;
                }
                PacmanManager(false);
                StartProgression("Uninstalling...");
                await new PackageManager().RemovePackageAsync(CurrentApp.FullName);
                AppLoadingText.Text = "Reloading...";
                AppLoadingProg.IsIndeterminate = false;
                AppLoadingProg.Value = 0;
                Initialize("Reloading");
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
                PacmanManager(true);
                EndProgression();
            }
        }

        private void LoopbackBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    if (button.Content.ToString() == "Enable Loopback")
                    {
                        _ = SendCommand("CheckNetIsolation.exe loopbackexempt -a -n=" + CurrentApp.FamilyName);
                        _ = Helper.MessageBox("Loopback exemption is successfully enabled.", Helper.SoundHelper.Sound.Alert);
                    }
                    else
                    {
                        _ = SendCommand("CheckNetIsolation.exe loopbackexempt -d -n=" + CurrentApp.FamilyName);
                        _ = Helper.MessageBox("Loopback exemption is successfully disabled.", Helper.SoundHelper.Sound.Alert);
                    }
                }
                else
                {
                    _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error);
                }
            }
            catch (Exception ex) { }
        }

        private void SeeLog_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            ShowLog();
        }

        private async void LaunchBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (buttonOnHold)
                {
                    buttonOnHold = false;
                    return;
                }
                var entries = await CurrentApp.Package.GetAppListEntriesAsync();
                if (CurrentApp.Package.Id.Name == AppListCombo.SelectedItem.ToString() || (entries.FirstOrDefault() != null && entries.FirstOrDefault().DisplayInfo.DisplayName == AppListCombo.SelectionBoxItem.ToString()))
                {
                    var state = await entries.First().LaunchAsync();
                    if (!state)
                    {
                        _ = Helper.MessageBox($"Failed to launch the app {CurrentApp.DisplayName}.", Helper.SoundHelper.Sound.Error, "Error");
                    }
                }
            }
            catch (Exception ex) { }
        }

        private void LaunchBtn_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
                buttonOnHold = true;
                if (e.HoldingState != Windows.UI.Input.HoldingState.Started) return;
                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    if (CurrentApp.IsXap)
                    {
                        _ = tClient.Send($"{Helper.installedLocation.Path}\\Contents\\BatchScripts\\LaunchApplication.bat True \"{CurrentApp.Name}\"");
                    }
                    else
                    {
                        _ = tClient.Send($"{Helper.installedLocation.Path}\\Contents\\BatchScripts\\LaunchApplication.bat False \"{CurrentApp.FamilyName}\"");
                    }
                }
                else
                {
                    _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                }
            }
            catch (Exception ex) { }
        }

        private async void MoveBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!await Helper.IsCapabilitiesAllowed())
                {
                    var isAllowed = await Helper.AskCapabilitiesPermission();
                    if (!isAllowed) return;
                }
                if (!CurrentApp.IsXap)
                {
                    ContentDialog dialog = new ContentDialog
                    {
                        Content = "The UWP apps can be moved from system Apps & features settings. Do you want to open these settings?",
                        PrimaryButtonText = "Yes",
                        SecondaryButtonText = "No"
                    };
                    Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                    var res = await dialog.ShowAsync();
                    if (res == ContentDialogResult.Primary) await Launcher.LaunchUriAsync(new Uri("ms-settings:appsfeatures"));
                    return;
                }
                if ((await Helper.externalFolder.GetFoldersAsync()).FirstOrDefault() == null)
                {
                    _ = Helper.MessageBox("No sdcard found, please insert one and try again.", Helper.SoundHelper.Sound.Error);
                    return;
                }
                if (CurrentApp.InstalledLocation == null)
                {
                    _ = Helper.MessageBox("Unable to move this Package.", Helper.SoundHelper.Sound.Error);
                    return;
                }
                var isDataFolderExist = false;
                var dataFolder = string.Empty;
                isDataFolderExist = Directory.Exists($"{PacManHelper.XapAppDataInternal}\\{{{CurrentApp.Name.ToUpper()}}}");
                if (isDataFolderExist) dataFolder = $"{PacManHelper.XapAppDataExternal}\\{{{CurrentApp.Name.ToUpper()}}}";
                else dataFolder = $"{PacManHelper.XapAppDataInternal}\\{{{CurrentApp.Name.ToUpper()}}}";
                ContentDialog contentDialog = new ContentDialog()
                {
                    PrimaryButtonText = "Yes",
                    SecondaryButtonText = "No",
                };
                var selectedStorage = string.Empty;
                if (isDataFolderExist)
                {
                    contentDialog.Content = "Are you sure you want to move the app to SD Card?";
                    selectedStorage = "/Volume:D:\\";
                }
                else
                {
                    contentDialog.Content = "Are you sure you want to move the app to Phone storage?";
                    selectedStorage = "/Volume:C:\\";
                }
                Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                var result = await contentDialog.ShowAsync();
                if (result != ContentDialogResult.Primary)
                {
                    return;
                }
                await Helper.BackgroundTaskHelper.RequestSessionAsync("BackgorundNotify");
                PacmanManager(false);
                StartProgression("Moving...");
                await BackupApp(CurrentApp, CustomContentDialogResult.AppData, $"{Helper.cacheFolder.Path}\\MovePackage.pmbak");
                await new PackageManager().RemovePackageAsync(CurrentApp.FullName);
                //await ApplicationData.Current.ClearAsync(ApplicationDataLocality.LocalCache);
                //Helper.Archive.ExtractZip($"{Helper.localFolder.Path}\\{CurrentApp.Name}_{CurrentApp.Version}_{DateTime.Now:yyyyMMdd_HHmmss}.pmbak", Helper.cacheFolder.Path);
                File.Delete(PacManHelper.MoveEndFile);
                File.Delete(PacManHelper.MoveResultFile);
                await SendCommand($"MinDeployAppx.exe /AddToVolume /PackagePath:\"{PacManHelper.GetBackupFile[0]}\" {selectedStorage} /DeploymentOption:32768 >{PacManHelper.MoveResultFile} 2>&1&echo. >{PacManHelper.MoveEndFile}");
                while (File.Exists($"{PacManHelper.MoveEndFile}") == false)
                {
                    await Task.Delay(500);
                }
                string moveResult = await FileIO.ReadTextAsync(await PacManHelper.GetMoveResultFile());
                if (moveResult.Contains("ReturnCode:[0x0]"))
                {
                    Helper.Archive.ExtractZip(PacManHelper.GetBackupFile[1], dataFolder);
                }
                else
                {
                    try
                    {
                        File.Delete(PacManHelper.MoveEndFile);
                        File.Delete(PacManHelper.MoveResultFile);
                        await SendCommand($"MinDeployAppx.exe /Add /PackagePath:\"{PacManHelper.GetBackupFile[0]}\" /DeploymentOption:32768 >{PacManHelper.MoveResultFile} 2>&1&echo. >{PacManHelper.MoveEndFile}");
                        while (File.Exists($"{PacManHelper.MoveEndFile}") == false)
                        {
                            await Task.Delay(500);
                        }
                        var name = $"{{{CurrentApp.Name.ToUpper()}}}";
                        await Task.Run(() =>
                        {
                            Helper.Archive.ExtractZip(PacManHelper.GetBackupFile[1], $"{PacManHelper.XapAppDataInternal}\\{name}");
                        });
                        _ = Helper.MessageBox("Failed to move the package.", Helper.SoundHelper.Sound.Error);
                    }
                    catch (Exception ex)
                    {
                        Helper.ThrowException(ex);
                    }
                }
                await Task.Run(() =>
                {
                    File.Delete($"{Helper.cacheFolder.Path}\\MovePackage.pmbak");
                });
                Helper.BackgroundTaskHelper.ClearSession();
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
            await ApplicationData.Current.ClearAsync(ApplicationDataLocality.LocalCache);
            EndProgression();
            PacmanManager(true);
        }

        private async void BackupBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!await Helper.IsCapabilitiesAllowed())
                {
                    var isAllowed = await Helper.AskCapabilitiesPermission();
                    if (!isAllowed) return;
                }
                var title = $"{CurrentApp.DisplayName}_{CurrentApp.Version}_{DateTime.Now:MMddyyyy_HHmmss}";
                CustomContentDialog dialog = new CustomContentDialog("Backup", true, true, true);
                await dialog.ShowAsync();
                if (dialog.Result == CustomContentDialogResult.Cancel || dialog.Result == CustomContentDialogResult.Nothing) return;
                TextBox titleName = new TextBox
                {
                    AcceptsReturn = false,
                    IsSpellCheckEnabled = false,
                    IsTextPredictionEnabled = false,
                    PlaceholderText = title,
                    Name = "TitleNameBox"
                };
                titleName.TextChanged += (newSender, f) =>
                {
                    foreach (var letter in "\\/:*?\"<>|")
                    {
                        if (titleName.Text.Contains(letter))
                        {
                            var position = titleName.Text.IndexOf(letter);
                            titleName.Text = titleName.Text.Remove(position, 1);
                            titleName.Select(position, 0);
                        }
                    }
                };
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Backup Title",
                    IsSecondaryButtonEnabled = false,
                    PrimaryButtonText = "OK",
                    Content = titleName
                };
                Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                var result = await contentDialog.ShowAsync();
                if (result == ContentDialogResult.None) return;
                if (titleName.Text != string.Empty) title = titleName.Text;
                var backupFolder = await PacManHelper.GetBackupFolder();
                if (backupFolder != null)
                {
                    PacmanManager(false);
                    StartProgression("Backuping...");
                    var package = CurrentApp;
                    await backupFolder.CreateFolderAsync(package.FullName, CreationCollisionOption.OpenIfExists);
                    await BackupApp(package, dialog.Result, $"{backupFolder.Path}\\{package.FullName}\\{title}.pmbak");
                    appsInfo = await PacManHelper.GetBackups(CurrentApp.FullName);
                    RestoreBtn.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
            EndProgression();
            PacmanManager(true);
        }

        private async Task<bool> BackupApp(PacManHelper.AppsDetails package, CustomContentDialogResult result, string destinationFile)
        {
            try
            {
                var dataFolder = string.Empty;
                if (package.IsXap)
                {
                    if (Directory.Exists($"{PacManHelper.XapAppDataInternal}\\{{{CurrentApp.Name.ToUpper()}}}")) dataFolder = $"{PacManHelper.XapAppDataInternal}\\{{{package.Name.ToUpper()}}}";
                    else dataFolder = $"{PacManHelper.XapAppDataExternal}\\{{{package.Name.ToUpper()}}}"; ;
                }
                else
                {
                    if (Directory.Exists($"{PacManHelper.AppDataInternal}\\{CurrentApp.FamilyName}")) dataFolder = $"{PacManHelper.AppDataInternal}\\{CurrentApp.FamilyName}";
                    else dataFolder = $"{PacManHelper.AppDataExternal}\\{CurrentApp.FamilyName}"; ;
                }
                await ApplicationData.Current.ClearAsync(ApplicationDataLocality.LocalCache);
                await Helper.cacheFolder.CreateFolderAsync("Backup", CreationCollisionOption.ReplaceExisting);
                await Task.Run(() =>
                {
                    if (result == CustomContentDialogResult.AppData)
                    {
                        if (package.IsXap) Helper.Archive.CreateZip($"{package.InstalledLocation.Path}\\Install", PacManHelper.GetBackupFile[0], CompressionLevel.Optimal, false, true);
                        else Helper.Archive.CreateZip($"{package.InstalledLocation.Path}", PacManHelper.GetBackupFile[0], CompressionLevel.Optimal, false, true);
                        Helper.Archive.CreateZip(dataFolder, PacManHelper.GetBackupFile[1], CompressionLevel.Optimal, false, true);
                    }
                    else if (result == CustomContentDialogResult.App)
                    {
                        if (package.IsXap) Helper.Archive.CreateZip($"{package.InstalledLocation.Path}\\Install", PacManHelper.GetBackupFile[0], CompressionLevel.Optimal, false, true);
                        else Helper.Archive.CreateZip($"{package.InstalledLocation.Path}", PacManHelper.GetBackupFile[0], CompressionLevel.Optimal, false, true);
                    }
                    else if (result == CustomContentDialogResult.Data)
                    {
                        Helper.Archive.CreateZip(dataFolder, PacManHelper.GetBackupFile[1], CompressionLevel.Optimal, false, true);
                    }
                    Helper.Json.ObjectToJsonFile(PacManHelper.CreateJsonObject(package, Path.GetFileNameWithoutExtension(destinationFile)), PacManHelper.GetBackupFile[2]);
                    Helper.Archive.CreateZip($"{Helper.cacheFolder.Path}\\Backup", destinationFile, CompressionLevel.Fastest, false, true);
                });
                return true;
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
                return false;
            }
        }

        private async void RestoreBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!await Helper.IsCapabilitiesAllowed())
                {
                    var isAllowed = await Helper.AskCapabilitiesPermission();
                    if (!isAllowed) return;
                }
                var backups = await PacManHelper.GetBackups(CurrentApp.FullName);
                ListBox listBox = new ListBox();
                foreach (var backup in backups)
                {
                    var textBlock = new TextBlock { Height = 80 };
                    textBlock.Inlines.Add(new Run { Text = "Title: ", FontWeight = Windows.UI.Text.FontWeights.Bold });
                    textBlock.Inlines.Add(new Run { Text = $"{backup.Title}\n" });
                    textBlock.Inlines.Add(new Run { Text = "Installed version: ", FontWeight = Windows.UI.Text.FontWeights.Bold });
                    textBlock.Inlines.Add(new Run { Text = $"{CurrentApp.Version}\n" });
                    textBlock.Inlines.Add(new Run { Text = "Backup version: ", FontWeight = Windows.UI.Text.FontWeights.Bold });
                    textBlock.Inlines.Add(new Run { Text = $"{backup.Version}\n" });
                    textBlock.Inlines.Add(new Run { Text = "Created on: ", FontWeight = Windows.UI.Text.FontWeights.Bold });
                    textBlock.Inlines.Add(new Run { Text = $"{backup.CreatedDate}\n" });
                    listBox.Items.Add(textBlock);
                }
                listBox.SelectedIndex = 0;
                ContentDialog backupsContentDialog = new ContentDialog()
                {
                    Title = backups[0].DisplayName,
                    Content = listBox,
                    PrimaryButtonText = "Restore",
                    SecondaryButtonText = "Cancel"
                };
                Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                var result = await backupsContentDialog.ShowAsync();
                if (result != ContentDialogResult.Primary) return;
                var backupFolder = await PacManHelper.GetBackupFolder();
                string backupFile = string.Empty;
                if (Path.GetExtension(backups[listBox.SelectedIndex].Title) == ".pmbak") backupFile = $"{backupFolder.Path}\\{backups[listBox.SelectedIndex].FullName}\\{backups[listBox.SelectedIndex].Title}";
                else backupFile = $"{backupFolder.Path}\\{backups[listBox.SelectedIndex].FullName}\\{backups[listBox.SelectedIndex].Title}.pmbak";
                CustomContentDialog dialog = new CustomContentDialog("Restore", Helper.Archive.CheckFileExist(backupFile, "Package.zip"), Helper.Archive.CheckFileExist(backupFile, "Data.zip"));
                await dialog.ShowAsync();
                if (dialog.Result == CustomContentDialogResult.Cancel || dialog.Result == CustomContentDialogResult.Nothing) return;
                await OperationRestore(backupFile, backups[listBox.SelectedIndex], dialog);
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
                EndProgression();
                PacmanManager(true);
            }
            await ApplicationData.Current.ClearAsync(ApplicationDataLocality.LocalCache);
        }

        private async Task OperationRestore(string backupFile, PacManHelper.BackupInfo backupDetails, CustomContentDialog dialog)
        {
            await Helper.BackgroundTaskHelper.RequestSessionAsync("BackgorundNotify");
            PacmanManager(false);
            StartProgression("Restoring...");
            await Helper.cacheFolder.CreateFolderAsync("Backup", CreationCollisionOption.ReplaceExisting);
            Helper.Archive.ExtractZip(backupFile, $"{Helper.cacheFolder.Path}\\Backup");
            if (backupDetails.Type == "Xap")
            {
                var name = $"{{{backupDetails.Name}}}";
                switch (dialog.Result)
                {
                    case CustomContentDialogResult.App:
                        try
                        {
                            if (new PackageManager().FindPackageForUser("", backupDetails.FullName) != null) await new PackageManager().RemovePackageAsync(backupDetails.FullName);
                            File.Delete(PacManHelper.MoveEndFile);
                            File.Delete(PacManHelper.MoveResultFile);
                            await SendCommand($"MinDeployAppx.exe /Add /PackagePath:\"{PacManHelper.GetBackupFile[0]}\" /DeploymentOption:32768 >{PacManHelper.MoveResultFile} 2>&1&echo. >{PacManHelper.MoveEndFile}");
                            while (File.Exists($"{PacManHelper.MoveEndFile}") == false)
                            {
                                await Task.Delay(500);
                            }
                        }
                        catch (Exception ex)
                        {
                            Helper.ThrowException(ex);
                        }
                        break;
                    case CustomContentDialogResult.Data:
                        await Task.Run(() =>
                        {
                            Directory.Delete($"{PacManHelper.XapAppDataInternal}\\{name}", true);
                            Helper.Archive.ExtractZip(PacManHelper.GetBackupFile[1], $"{PacManHelper.XapAppDataInternal}\\{name}");
                        });
                        break;
                    case CustomContentDialogResult.AppData:
                        try
                        {
                            if (new PackageManager().FindPackageForUser("", backupDetails.FullName) != null) await new PackageManager().RemovePackageAsync(backupDetails.FullName);
                            File.Delete(PacManHelper.MoveEndFile);
                            File.Delete(PacManHelper.MoveResultFile);
                            await SendCommand($"MinDeployAppx.exe /Add /PackagePath:\"{PacManHelper.GetBackupFile[0]}\" /DeploymentOption:32768 >{PacManHelper.MoveResultFile} 2>&1&echo. >{PacManHelper.MoveEndFile}");
                            while (File.Exists($"{PacManHelper.MoveEndFile}") == false)
                            {
                                await Task.Delay(500);
                            }
                            await Task.Run(() =>
                            {
                                Helper.Archive.ExtractZip(PacManHelper.GetBackupFile[1], $"{PacManHelper.XapAppDataInternal}\\{name}");
                            });
                        }
                        catch (Exception ex)
                        {
                            Helper.ThrowException(ex);
                        }
                        break;
                }
            }
            else
            {
                var name = backupDetails.FamilyName;
                var fullName = backupDetails.FullName;
                DeploymentResult deploymentResult;
                switch (dialog.Result)
                {
                    case CustomContentDialogResult.App:
                        if (new PackageManager().FindPackageForUser("", backupDetails.FullName) != null) await new PackageManager().RemovePackageAsync(backupDetails.FullName);
                        try
                        {
                            Directory.Delete($"C:\\Data\\PROGRAMS\\{fullName}", true);
                        }
                        catch (Exception ex)
                        {
                            //Helper.ThrowException(ex);
                        }
                        await Task.Run(() =>
                        {
                            Helper.Archive.ExtractZip(PacManHelper.GetBackupFile[0], $"C:\\Data\\PROGRAMS\\{fullName}");
                            File.Delete($"C:\\Data\\PROGRAMS\\{fullName}\\AppxSignature.p7x");
                        });
                        deploymentResult = await new PackageManager().RegisterPackageAsync(new Uri($"C:\\Data\\PROGRAMS\\{fullName}\\AppxMAnifest.xml"), null, DeploymentOptions.DevelopmentMode);
                        if (!deploymentResult.IsRegistered) _ = Helper.MessageBox("Failed to restore the backup package.", Helper.SoundHelper.Sound.Error, "Error");
                        break;
                    case CustomContentDialogResult.Data:
                        await Task.Run(() =>
                        {
                            try
                            {
                                Directory.Delete($"{PacManHelper.AppDataInternal}\\{name}", true);
                            }
                            catch (Exception ex)
                            {
                                //Helper.ThrowException(ex);
                            }
                            try
                            {
                                Helper.Archive.ExtractZip(PacManHelper.GetBackupFile[1], $"{PacManHelper.AppDataInternal}\\{name}");
                            }
                            catch (Exception ex)
                            {
                                //Helper.ThrowException(ex);
                            }
                        });
                        break;
                    case CustomContentDialogResult.AppData:
                        if (new PackageManager().FindPackageForUser("", backupDetails.FullName) != null) await new PackageManager().RemovePackageAsync(backupDetails.FullName);
                        try
                        {
                            Directory.Delete($"C:\\Data\\PROGRAMS\\{fullName}", true);
                        }
                        catch (Exception ex)
                        {
                            //Helper.ThrowException(ex);
                        }
                        await Task.Run(() =>
                        {
                            Helper.Archive.ExtractZip(PacManHelper.GetBackupFile[0], $"C:\\Data\\PROGRAMS\\{fullName}");
                            File.Delete($"C:\\Data\\PROGRAMS\\{fullName}\\AppxSignature.p7x");
                        });
                        deploymentResult = await new PackageManager().RegisterPackageAsync(new Uri($"C:\\Data\\PROGRAMS\\{fullName}\\AppxMAnifest.xml"), null, DeploymentOptions.DevelopmentMode);
                        if (!deploymentResult.IsRegistered)
                        {
                            _ = Helper.MessageBox("Failed to restore the backup package.", Helper.SoundHelper.Sound.Error, "Error");
                        }
                        else
                        {
                            await Task.Run(() =>
                            {
                                try
                                {
                                    Helper.Archive.ExtractZip(PacManHelper.GetBackupFile[1], $"{PacManHelper.AppDataInternal}\\{name}");
                                }
                                catch (Exception ex)
                                {
                                    //Helper.ThrowException(ex);
                                }
                            });
                        }
                        break;
                }
            }
            Helper.BackgroundTaskHelper.ClearSession();
            if (dialog.Result == CustomContentDialogResult.App || dialog.Result == CustomContentDialogResult.AppData)
            {
                AppLoadingText.Text = "Reloading...";
                AppLoadingProg.IsIndeterminate = false;
                AppLoadingProg.Value = 0;
                Initialize("Reloading");
            }
            else
            {
                EndProgression();
                PacmanManager(true);
            }
        }

        private async void AppListCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (!isRunning)
                {
                    AppListCombo.IsEnabled = false;
                    RestoreBtn.IsEnabled = false;
                    if (CurrentApp.DisplayName == "CMD Injector") UninstallBtn.IsEnabled = false;
                    else UninstallBtn.IsEnabled = true;
                    if (await IsBackupExist()) RestoreBtn.IsEnabled = true;
                    else RestoreBtn.IsEnabled = false;
                    if (!isRunning) AppListCombo.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                AppListCombo.IsEnabled = true;
            }
        }

        private async Task<bool> IsBackupExist()
        {
            try
            {
                if (await PacManHelper.IsBackupFolderSelected())
                {
                    var appInfo = await PacManHelper.GetBackups(CurrentApp.FullName);
                    if (appInfo != null && appInfo.Count != 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex) { }
            return false;
        }

        private void StartProgression(string loadingText)
        {
            AppLoadingText.Text = loadingText;
            AppLoadingProg.IsIndeterminate = true;
            AppLoadStack.Visibility = Visibility.Visible;
        }

        private void EndProgression()
        {
            AppLoadingProg.IsIndeterminate = false;
            AppLoadStack.Visibility = Visibility.Collapsed;
        }

        private async void PacmanManager(bool value)
        {
            AppListCombo.IsEnabled = value;
            AppInfoBtn.IsEnabled = value;
            UninstallBtn.IsEnabled = value;
            AppFoldrBtn.IsEnabled = value;
            AppDataBtn.IsEnabled = value;
            LaunchBtn.IsEnabled = value;
            if (Helper.build > 14393) BackupBtn.IsEnabled = value;
            if (!flag)
            {
                if (Helper.build > 14393) MoveBtn.IsEnabled = value;
                EnableLoopbackBtn.IsEnabled = value;
                DisableLoopbackBtn.IsEnabled = value;
                if (await IsBackupExist() && Helper.build > 14393) RestoreBtn.IsEnabled = value;
                else RestoreBtn.IsEnabled = false;
            }
        }

        private async void DeploymentInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Helper.build < 15063)
            {
                _ = Helper.MessageBox(" • Deployment Mode\nUse this option to install unsigned app packages.\n\n" +
                    " • Force Application Shutdown\nThis option will force shutdown the processes that associated with the package to continue the registration.\n\n" +
                    " • None\nThe default behavior is used.\n\n", Helper.SoundHelper.Sound.Alert, "Info");
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Info",
                    CloseButtonText = "Close"
                };
                TextBlock textBlock = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap
                };
                textBlock.Inlines.Add(new Run { Text = $" • Deployment Mode\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "Use this option to install unsigned app packages.\n\n" });
                textBlock.Inlines.Add(new Run { Text = $" • Force Application Shutdown\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "This option will force shutdown the processes that associated with the package to continue the registration.\n\n" });
                textBlock.Inlines.Add(new Run { Text = $" • None\n", FontWeight = Windows.UI.Text.FontWeights.SemiBold });
                textBlock.Inlines.Add(new Run { Text = "The default behavior is used.\n\n" });
                contentDialog.Content = textBlock;
                Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                await contentDialog.ShowAsync();
            }
        }
    }
}
