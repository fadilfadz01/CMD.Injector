using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ndtklib;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Profile;
using Windows.UI.StartScreen;
using Windows.Networking.BackgroundTransfer;
using Windows.ApplicationModel.Core;
using System.Threading;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
using System.Reflection;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using CMDInjectorHelper;
using WinUniversalTool;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Registry;
using Windows.Management.Deployment;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CMDInjector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        NativeRegistry reg = new NativeRegistry();
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient tClient = new TelnetClient(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        bool flag = false;

        private void Connect()
        {
            _ = tClient.Connect();
            long i = 0;
            while (tClient.IsConnected == false && i < 1000000)
            {
                i++;
            }
        }

        private async void GetExternalAsync()
        {
            StorageFolder sdCard = (await Helper.externalFolder.GetFoldersAsync()).FirstOrDefault();
            if (sdCard == null)
            {
                StorageTog.IsEnabled = false;
                StorageTog.IsOn = false;
                Helper.LocalSettingsHelper.SaveSettings("StorageSet", false);
            }
            else
            {
                StorageTog.IsOn = Helper.LocalSettingsHelper.LoadSettings("StorageSet", false);
            }
        }

        public Settings()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            Connect();
            Initialize();
        }

        private async void Initialize()
        {
            if (Helper.build < 10572)
            {
                MenuTransitionTog.IsEnabled = false;
            }
            else
            {
                MenuTransitionTog.IsOn = Helper.LocalSettingsHelper.LoadSettings("MenuTransition", true);
            }
            if (Helper.build < 10586)
            {
                LoginReqTog.IsEnabled = false;
            }
            if (Helper.build <= 14393)
            {
                if (Helper.build < 14393)
                {
                    ConKeyBtnTog.IsEnabled = false;
                    Helper.LocalSettingsHelper.SaveSettings("ConKeyBtnSet", false);
                }
                BackupFoldBtn.IsEnabled = false;
            }

            if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\DefaultApplications", ".xap", Helper.RegistryHelper.RegistryType.REG_SZ) == "CMDInjector_kqyng60eng17c")
            {
                DefaultTog.IsOn = true;
            }
            else
            {
                DefaultTog.IsOn = false;
            }

            GetExternalAsync();
            SplashScrTog.IsOn = Helper.LocalSettingsHelper.LoadSettings("SplashScreen", true);
            SplashScrCombo.SelectedIndex = Helper.LocalSettingsHelper.LoadSettings("SplashAnimIndex", 0);
            if (Helper.build >= 10586) LoginReqTog.IsOn = Helper.LocalSettingsHelper.LoadSettings("LoginTogReg", true);
            CommandsWrapToggle.IsOn = Helper.LocalSettingsHelper.LoadSettings("CommandsTextWrap", false);
            ConKeyBtnTog.IsOn = Helper.LocalSettingsHelper.LoadSettings("ConKeyBtnSet", false);
            ConsoleFontSizeBox.SelectedIndex = Helper.LocalSettingsHelper.LoadSettings("ConFontSizeSet", 3);
            ThemeToggle.IsOn = Helper.LocalSettingsHelper.LoadSettings("ThemeSettings", false);
            AccentToggle.IsOn = Helper.LocalSettingsHelper.LoadSettings("AccentSettings", false);
            HamBurMenu.IsOn = Helper.LocalSettingsHelper.LoadSettings("SplitView", false);
            SnapNotifTog.IsOn = Helper.LocalSettingsHelper.LoadSettings("SnapperNotify", true);
            SnapSoundTog.IsOn = Helper.LocalSettingsHelper.LoadSettings("SnapSoundTog", true);
            ArgConfirmTog.IsOn = Helper.LocalSettingsHelper.LoadSettings("TerminalRunArg", true);

            if (await Helper.IsCapabilitiesAllowed())
            {
                ResCapTog.IsOn = true;
                ResCapTog.IsEnabled = false;
            }

            if (!Helper.LocalSettingsHelper.LoadSettings("PMLogPath", false))
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PMLogPath", KnownFolders.DocumentsLibrary);
                Helper.LocalSettingsHelper.SaveSettings("PMLogPath", true);
            }
            if ((await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("PMLogPath")).Path.ToUpper() == @"C:\Data\USERS\DefApps\APPDATA\ROAMING\MICROSOFT\WINDOWS\Libraries\Documents.library-ms".ToUpper())
            {
                LogPathBox.Text = "C:\\Data\\Users\\Public\\Documents\\PacMan_Installer.pmlog";
            }
            else
            {
                LogPathBox.Text = $"{(await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("PMLogPath")).Path}\\PacMan_Installer.pmlog";
            }
            if (ThemeToggle.IsOn)
            {
                CustomTheme.SelectedIndex = Helper.LocalSettingsHelper.LoadSettings("Theme", 0);
            }

            if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\CI", "UMCIAuditMode", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001")
            {
                UMCIToggle.IsOn = true;
            }
            else
            {
                UMCIToggle.IsOn = false;
            }

            if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\KeepWiFiOnSvc", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000004")
            {
                WifiServiceToggle.IsOn = false;
            }
            else
            {
                WifiServiceToggle.IsOn = true;
            }

            if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Services\\Bootsh", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000004")
            {
                BootshToggle.IsOn = false;
            }
            else
            {
                BootshToggle.IsOn = true;
            }

            foreach (var color in typeof(Colors).GetRuntimeProperties())
            {
                if (color.Name != "AliceBlue" && color.Name != "AntiqueWhite" && color.Name != "Azure" && color.Name != "Beige" && color.Name != "Bisque" && color.Name != "Black" && color.Name != "BlanchedAlmond" && color.Name != "Cornsilk" && color.Name != "FloralWhite" && color.Name != "Gainsboro" && color.Name != "GhostWhite" && color.Name != "Honeydew" && color.Name != "Ivory" && color.Name != "Lavender" && color.Name != "LavenderBlush" && color.Name != "LemonChiffon"
                && color.Name != "LightCyan" && color.Name != "LightGoldenrodYellow" && color.Name != "LightGray" && color.Name != "LightYellow" && color.Name != "Linen" && color.Name != "MintCream" && color.Name != "MistyRose" && color.Name != "Moccasin" && color.Name != "OldLace" && color.Name != "PapayaWhip" && color.Name != "SeaShell" && color.Name != "Snow" && color.Name != "Transparent" && color.Name != "White" && color.Name != "WhiteSmoke")
                {
                    var cbi = new ComboBoxItem();
                    var colorStack = new StackPanel();
                    var selectColor = new Rectangle();
                    var colorText = new TextBlock();
                    colorStack.Orientation = Orientation.Horizontal;
                    selectColor.Width = 20;
                    selectColor.Height = 20;
                    selectColor.Margin = new Thickness(0, 0, 10, 0);
                    selectColor.Fill = new SolidColorBrush((Color)color.GetValue(null));
                    colorText.Text = color.Name;
                    colorText.VerticalAlignment = VerticalAlignment.Center;
                    colorStack.Children.Add(selectColor);
                    colorStack.Children.Add(colorText);
                    cbi.Content = colorStack;
                    CustomAccent.Items.Add(cbi);
                    SolidColorBrush solidColor = (SolidColorBrush)selectColor.Fill;
                }
            }

            CustomAccent.SelectedIndex = Helper.LocalSettingsHelper.LoadSettings("Accent", 11);

            flag = true;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (await PacManHelper.IsBackupFolderSelected()) BackupFoldBox.Text = (await PacManHelper.GetBackupFolder()).Path;
        }

        private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            Helper.LocalSettingsHelper.SaveSettings("ThemeSettings", ThemeToggle.IsOn);
            if (ThemeToggle.IsOn)
            {
                CustomTheme.Visibility = Visibility.Visible;
                CustomTheme.SelectedIndex = Helper.LocalSettingsHelper.LoadSettings("Theme", 0);
                if (CustomTheme.SelectedIndex == 0)
                {
                    Helper.LocalSettingsHelper.SaveSettings("Theme", CustomTheme.SelectedIndex);
                    ((Frame)Window.Current.Content).RequestedTheme = ElementTheme.Dark;
                    Helper.color = Colors.Black;
                }
                else
                {
                    Helper.LocalSettingsHelper.SaveSettings("Theme", CustomTheme.SelectedIndex);
                    ((Frame)Window.Current.Content).RequestedTheme = ElementTheme.Light;
                    Helper.color = Colors.White;
                }
            }
            else
            {
                CustomTheme.Visibility = Visibility.Collapsed;
                ((Frame)Window.Current.Content).RequestedTheme = ElementTheme.Default;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Control Panel\\Theme", "CurrentTheme", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000000")
                {
                    Helper.color = Colors.White;
                }
                else
                {
                    Helper.color = Colors.Black;
                }
            }
        }

        private void AccentToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                Helper.LocalSettingsHelper.SaveSettings("AccentSettings", AccentToggle.IsOn);
                if (AccentToggle.IsOn)
                {
                    CustomAccent.Visibility = Visibility.Visible;
                    AccentResources();
                }
                else
                {
                    CustomAccent.Visibility = Visibility.Collapsed;
                    Color accentColor = new UISettings().GetColorValue(UIColorType.Accent);
                    (App.Current.Resources["AppAccentColor"] as SolidColorBrush).Color = accentColor;
                    (App.Current.Resources["ToggleSwitchFillOn"] as SolidColorBrush).Color = accentColor;
                    (App.Current.Resources["TextControlBorderBrushFocused"] as SolidColorBrush).Color = accentColor;
                    (App.Current.Resources["RadioButtonOuterEllipseCheckedStroke"] as SolidColorBrush).Color = accentColor;
                    (App.Current.Resources["SliderTrackValueFill"] as SolidColorBrush).Color = accentColor;
                    (App.Current.Resources["SliderThumbBackground"] as SolidColorBrush).Color = accentColor;
                    (App.Current.Resources["SystemControlHighlightAccentBrush"] as SolidColorBrush).Color = accentColor;
                    (App.Current.Resources["SystemControlHighlightListAccentLowBrush"] as SolidColorBrush).Color = Color.FromArgb(204, accentColor.R, accentColor.G, accentColor.B);
                    (App.Current.Resources["SystemControlHighlightListAccentHighBrush"] as SolidColorBrush).Color = Color.FromArgb(242, accentColor.R, accentColor.G, accentColor.B);
                    if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                    {
                        var statusBar = StatusBar.GetForCurrentView();
                        if (statusBar != null)
                        {
                            statusBar.ForegroundColor = accentColor;
                        }
                    }
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private void CustomTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CustomTheme.SelectedIndex == 0)
            {
                Helper.LocalSettingsHelper.SaveSettings("Theme", CustomTheme.SelectedIndex);
                ((Frame)Window.Current.Content).RequestedTheme = ElementTheme.Dark;
                Helper.color = Colors.Black;
            }
            else
            {
                Helper.LocalSettingsHelper.SaveSettings("Theme", CustomTheme.SelectedIndex);
                ((Frame)Window.Current.Content).RequestedTheme = ElementTheme.Light;
                Helper.color = Colors.White;
            }
        }

        private void CustomAccent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AccentResources();
        }

        private void AccentResources()
        {
            try
            {
                if (flag == true)
                {
                    int count = 0;
                    foreach (var color in typeof(Colors).GetRuntimeProperties())
                    {
                        if (color.Name != "AliceBlue" && color.Name != "AntiqueWhite" && color.Name != "Azure" && color.Name != "Beige" && color.Name != "Bisque" && color.Name != "Black" && color.Name != "BlanchedAlmond" && color.Name != "Cornsilk" && color.Name != "FloralWhite" && color.Name != "Gainsboro" && color.Name != "GhostWhite" && color.Name != "Honeydew" && color.Name != "Ivory" && color.Name != "Lavender" && color.Name != "LavenderBlush" && color.Name != "LemonChiffon"
                        && color.Name != "LightCyan" && color.Name != "LightGoldenrodYellow" && color.Name != "LightGray" && color.Name != "LightYellow" && color.Name != "Linen" && color.Name != "MintCream" && color.Name != "MistyRose" && color.Name != "Moccasin" && color.Name != "OldLace" && color.Name != "PapayaWhip" && color.Name != "SeaShell" && color.Name != "Snow" && color.Name != "Transparent" && color.Name != "White" && color.Name != "WhiteSmoke")
                        {
                            if (CustomAccent.SelectedIndex == count)
                            {
                                var accentColor = (Color)color.GetValue(null);
                                (App.Current.Resources["AppAccentColor"] as SolidColorBrush).Color = accentColor;
                                (App.Current.Resources["ToggleSwitchFillOn"] as SolidColorBrush).Color = accentColor;
                                (App.Current.Resources["TextControlBorderBrushFocused"] as SolidColorBrush).Color = accentColor;
                                (App.Current.Resources["RadioButtonOuterEllipseCheckedStroke"] as SolidColorBrush).Color = accentColor;
                                (App.Current.Resources["SliderTrackValueFill"] as SolidColorBrush).Color = accentColor;
                                (App.Current.Resources["SliderThumbBackground"] as SolidColorBrush).Color = accentColor;
                                (App.Current.Resources["SystemControlHighlightAccentBrush"] as SolidColorBrush).Color = accentColor;
                                (App.Current.Resources["SystemControlHighlightListAccentLowBrush"] as SolidColorBrush).Color = Color.FromArgb(204, accentColor.R, accentColor.G, accentColor.B);
                                (App.Current.Resources["SystemControlHighlightListAccentHighBrush"] as SolidColorBrush).Color = Color.FromArgb(242, accentColor.R, accentColor.G, accentColor.B);
                                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                                {
                                    var statusBar = StatusBar.GetForCurrentView();
                                    if (statusBar != null)
                                    {
                                        statusBar.ForegroundColor = accentColor;
                                    }
                                }
                                Helper.LocalSettingsHelper.SaveSettings("Accent", CustomAccent.SelectedIndex);
                                break;
                            }
                            count++;
                        }
                    }
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private void HamBurMenu_Toggled(object sender, RoutedEventArgs e)
        {
            if (HamBurMenu.IsOn)
            {
                Helper.LocalSettingsHelper.SaveSettings("SplitView", true);
                Helper.pageNavigation.Invoke(45, null);
            }
            else
            {
                Helper.LocalSettingsHelper.SaveSettings("SplitView", false);
                Helper.pageNavigation.Invoke(0, null);
            }
        }

        private void CommandsWrapToggle_Toggled(object sender, RoutedEventArgs e)
        {
            Helper.LocalSettingsHelper.SaveSettings("CommandsTextWrap", CommandsWrapToggle.IsOn);
        }

        private void ConsoleFontSizeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Helper.LocalSettingsHelper.SaveSettings("ConFontSizeSet", ConsoleFontSizeBox.SelectedIndex);
        }

        private void ConKeyBtnTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (ConKeyBtnTog.IsOn)
            {
                Helper.LocalSettingsHelper.SaveSettings("ConKeyBtnSet", true);
            }
            else
            {
                Helper.LocalSettingsHelper.SaveSettings("ConKeyBtnSet", false);
            }
        }

        private void SnapNotifTog_Toggled(object sender, RoutedEventArgs e)
        {
            Helper.LocalSettingsHelper.SaveSettings("SnapperNotify", SnapNotifTog.IsOn);
        }

        private void BootshToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (BootshToggle.IsOn)
            {
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Services\\Bootsh", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000002");
            }
            else
            {
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Services\\Bootsh", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000004");
            }
            if (flag == true)
            {
                BootshIndicator.Visibility = Visibility.Visible;
            }
        }

        private void UMCIToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UMCIToggle.IsOn)
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\CI", "UMCIAuditMode", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                }
                else
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\CI", "UMCIAuditMode", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000000");
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
            if (flag == true)
            {
                UMCIModeIndicator.Visibility = Visibility.Visible;
            }
        }

        private void WifiServiceToggle_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (WifiServiceToggle.IsOn)
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\KeepWiFiOnSvc", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000002");
                }
                else
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\KeepWiFiOnSvc", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000004");
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
            if (flag == true)
            {
                WifiServiceIndicator.Visibility = Visibility.Visible;
            }
        }

        private void StorageTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (StorageTog.IsOn)
            {
                Helper.LocalSettingsHelper.SaveSettings("StorageSet", true);
            }
            else
            {
                Helper.LocalSettingsHelper.SaveSettings("StorageSet", false);
            }
        }

        private void DefaultTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (DefaultTog.IsOn)
            {
                try
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\DefaultApplications", ".xap", Helper.RegistryHelper.RegistryType.REG_SZ, "CMDInjector_kqyng60eng17c");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\DefaultApplications", ".appx", Helper.RegistryHelper.RegistryType.REG_SZ, "CMDInjector_kqyng60eng17c");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\DefaultApplications", ".appxbundle", Helper.RegistryHelper.RegistryType.REG_SZ, "CMDInjector_kqyng60eng17c");
                }
                catch (Exception ex) { Helper.ThrowException(ex); }
            }
            else
            {
                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    _ = tClient.Send("reg delete HKLM\\Software\\Microsoft\\DefaultApplications /v .xap /f" +
                        "&reg delete HKLM\\Software\\Microsoft\\DefaultApplications /v .appx /f" +
                        "&reg delete HKLM\\Software\\Microsoft\\DefaultApplications /v .appxbundle /f");
                }
                else
                {
                    _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                }
            }
            if (flag == true)
            {
                DefInstIndicator.Visibility = Visibility.Visible;
            }
        }

        private async void LogPathBtn_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder LogPath = await folderPicker.PickSingleFolderAsync();
            if (LogPath == null)
            {
                return;
            }
            StorageApplicationPermissions.FutureAccessList.AddOrReplace("PMLogPath", LogPath);
            LogPathBox.Text = $"{(await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("PMLogPath")).Path}\\PacMan_Installer.pmlog";
        }

        private void ArgConfirmTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (ArgConfirmTog.IsOn)
            {
                Helper.LocalSettingsHelper.SaveSettings("TerminalRunArg", true);
            }
            else
            {
                Helper.LocalSettingsHelper.SaveSettings("TerminalRunArg", false);
            }
        }

        private void SplashScrTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (SplashScrTog.IsOn)
            {
                Helper.LocalSettingsHelper.SaveSettings("SplashScreen", true);
                SplashScrCombo.Visibility = Visibility.Visible;
            }
            else
            {
                Helper.LocalSettingsHelper.SaveSettings("SplashScreen", false);
                SplashScrCombo.Visibility = Visibility.Collapsed;
            }
        }

        private void SplashScrCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var obj = sender as ComboBox;
            Helper.LocalSettingsHelper.SaveSettings("SplashAnim", (obj.SelectedItem as ComboBoxItem).Content.ToString());
            Helper.LocalSettingsHelper.SaveSettings("SplashAnimIndex", obj.SelectedIndex);
        }

        private void LoginReqTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (LoginReqTog.IsOn)
            {
                Helper.LocalSettingsHelper.SaveSettings("LoginTogReg", true);
            }
            else
            {
                Helper.LocalSettingsHelper.SaveSettings("LoginTogReg", false);
            }
        }

        private async void BackupFoldBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = await PacManHelper.SetBackupFolder();
            if (result)
            {
                BackupFoldBox.Text = (await PacManHelper.GetBackupFolder()).Path;
            }
        }

        private async void StartupRstBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = await Helper.MessageBox("Are you sure you want to reset the Startup commands?", Helper.SoundHelper.Sound.Alert, "", "No", true, "Yes");
            if (result == 0)
            {
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\Startup.bat", @"C:\Windows\System32\Startup.bat");
            }
        }

        private async void ResCapTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (ResCapTog.IsOn && flag)
            {
                ResCapTog.IsEnabled = false;
                var result = await Helper.AskCapabilitiesPermission();
                if (!result)
                {
                    ResCapTog.IsEnabled = true;
                    ResCapTog.IsOn = false;
                }
            }
        }

        private void SnapSoundTog_Toggled(object sender, RoutedEventArgs e)
        {
            Helper.LocalSettingsHelper.SaveSettings("SnapSoundTog", SnapSoundTog.IsOn);
        }

        private void MenuTransitionTog_Toggled(object sender, RoutedEventArgs e)
        {
            Helper.LocalSettingsHelper.SaveSettings("MenuTransition", MenuTransitionTog.IsOn);
        }
    }
}
