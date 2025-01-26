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
using Windows.UI.Popups;
using Windows.System.Profile;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.StartScreen;
using Windows.Storage.AccessCache;
using ndtklib;
using Windows.ApplicationModel;
using System.Text.RegularExpressions;
using Registry;
using Windows.UI;
using System.Reflection;
using Windows.UI.Xaml.Shapes;
using System.Threading;
using SharpDX.DirectWrite;
using System.Collections.ObjectModel;
using System.IO.Compression;
using Windows.UI.ViewManagement;
using Windows.Graphics.Display;
using CMDInjectorHelper;
using WinUniversalTool;
using Windows.System.UserProfile;
using System.Xml.Linq;
using Windows.Management.Deployment;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CMDInjector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TweakBox : Page
    {
        NRPC rpc = new NRPC();
        NativeRegistry reg = new NativeRegistry();
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient tClient = new TelnetClient(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        string[] Paths;
        string TileIcon;
        ObservableCollection<int> glyphUnicodes = new ObservableCollection<int>();
        ObservableCollection<PacManHelper.AppsDetails> Packages = new ObservableCollection<PacManHelper.AppsDetails>();
        bool buttonOnHold = false;
        bool flag = false;
        bool secondFlag = false;

        private void Connect()
        {
            _ = tClient.Connect();
            long i = 0;
            while (tClient.IsConnected == false && i < 1000000)
            {
                i++;
            }
        }

        public TweakBox()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            rpc.Initialize();
            Connect();
            Init();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Initialize();
            if (Helper.LocalSettingsHelper.LoadSettings("UnlockHidden", false))
            {
                if (Helper.build == 14393)
                {
                    UfpModeStack.Visibility = Visibility.Visible;
                }
                SecMgrStack.Visibility = Visibility.Visible;
                NDTKStack.Visibility = Visibility.Visible;
                Helper.LocalSettingsHelper.SaveSettings("UnlockHidden", false);
            }
            else
            {
                UfpModeStack.Visibility = Visibility.Collapsed;
                SecMgrStack.Visibility = Visibility.Collapsed;
                NDTKStack.Visibility = Visibility.Collapsed;
            }
        }

        private async void Init()
        {
            if (tClient.IsConnected && HomeHelper.IsConnected())
            {
                AutoWallTog.IsEnabled = true;
                DisplayOrient.IsEnabled = true;
            }

            if (Helper.build < 10549)
            {
                LiveLockStack.Visibility = Visibility.Collapsed;
            }

            if (Helper.build < 10570)
            {
                SearchOptStack.Visibility = Visibility.Collapsed;
            }

            if (Helper.build < 14393)
            {
                DisplayOrient.IsEnabled = false;
                GlanceTog.IsEnabled = false;
                ZipFileBtn.IsEnabled = false;
            }

            if (Helper.build < 14320)
            {
                VolOptStack.Visibility = Visibility.Collapsed;
            }
            else
            {
                AUBtn.IsEnabled = false;
            }

            if (Helper.build < 15063)
            {
                NavRoots.Visibility = Visibility.Collapsed;
            }
            else
            {
                CUBtn.IsEnabled = false;
                InitializeFolders();
            }

            if (Helper.build >= 15254)
            {
                FCUBtn.IsEnabled = false;
            }

            if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\NokiaSvcHost\\Plugins\\NsgExtA\\NdtkSvc", "Path", Helper.RegistryHelper.RegistryType.REG_SZ).ToLower() == "c:\\windows\\system32\\ndtksvc.dll")
            {
                RestoreNDTKTog.IsOn = true;
            }
            else
            {
                RestoreNDTKTog.IsOn = false;
            }

            TipText.Visibility = Helper.LocalSettingsHelper.LoadSettings("TipSettings", true) ? Visibility.Visible : Visibility.Collapsed;

            if (Helper.LocalSettingsHelper.LoadSettings("StartWallSwitch", false))
            {
                StartWallTog.IsOn = true;
                WallSwitchExtraStack.Visibility = Visibility.Visible;
            }
            else
            {
                StartWallTog.IsOn = false;
                WallSwitchExtraStack.Visibility = Visibility.Collapsed;
            }
            var libraryPath = (await TweakBoxHelper.GetWallpaperLibrary()).Path;
            if (libraryPath.Contains(Helper.installedLocation.Path)) StartWallLibPathBox.Text = "CMDInjector:\\Assets\\Images\\Lockscreens\\Stripes";
            else StartWallLibPathBox.Text = libraryPath;
            StartWallTrigCombo.SelectedIndex = Helper.LocalSettingsHelper.LoadSettings("StartWallTrigger", 0);
            StartWallInterCombo.SelectedIndex = Helper.LocalSettingsHelper.LoadSettings("StartWallInterItem", 0);

            foreach (var color in typeof(Colors).GetRuntimeProperties())
            {
                if (color.Name != "AliceBlue" && color.Name != "AntiqueWhite" && color.Name != "Azure" && color.Name != "Beige" && color.Name != "Bisque" && color.Name != "Black" && color.Name != "BlanchedAlmond" && color.Name != "Cornsilk" && color.Name != "FloralWhite" && color.Name != "Gainsboro" && color.Name != "GhostWhite" && color.Name != "Honeydew" && color.Name != "Ivory" && color.Name != "Lavender" && color.Name != "LavenderBlush" && color.Name != "LemonChiffon"
                && color.Name != "LightCyan" && color.Name != "LightGoldenrodYellow" && color.Name != "LightGray" && color.Name != "LightYellow" && color.Name != "Linen" && color.Name != "MintCream" && color.Name != "MistyRose" && color.Name != "Moccasin" && color.Name != "OldLace" && color.Name != "PapayaWhip" && color.Name != "SeaShell" && color.Name != "Snow" && color.Name != "Transparent" && color.Name != "White" && color.Name != "WhiteSmoke")
                {
                    var selectColor = new Rectangle { Width = 20, Height = 20, Margin = new Thickness(0, 0, 10, 0), Fill = new SolidColorBrush((Color)color.GetValue(null)) };
                    var colorText = new TextBlock { Text = color.Name, VerticalAlignment = VerticalAlignment.Center };
                    var colorStack = new StackPanel { Orientation = Orientation.Horizontal };
                    colorStack.Children.Add(selectColor);
                    colorStack.Children.Add(colorText);
                    AccentColorCombo.Items.Add(new ComboBoxItem { Content = colorStack });
                }
            }
            foreach (var color in typeof(Colors).GetRuntimeProperties())
            {
                if (color.Name != "AliceBlue" && color.Name != "AntiqueWhite" && color.Name != "Azure" && color.Name != "Beige" && color.Name != "Bisque" && color.Name != "Black" && color.Name != "BlanchedAlmond" && color.Name != "Cornsilk" && color.Name != "FloralWhite" && color.Name != "Gainsboro" && color.Name != "GhostWhite" && color.Name != "Honeydew" && color.Name != "Ivory" && color.Name != "Lavender" && color.Name != "LavenderBlush" && color.Name != "LemonChiffon"
                && color.Name != "LightCyan" && color.Name != "LightGoldenrodYellow" && color.Name != "LightGray" && color.Name != "LightYellow" && color.Name != "Linen" && color.Name != "MintCream" && color.Name != "MistyRose" && color.Name != "Moccasin" && color.Name != "OldLace" && color.Name != "PapayaWhip" && color.Name != "SeaShell" && color.Name != "Snow" && color.Name != "Transparent" && color.Name != "White" && color.Name != "WhiteSmoke")
                {
                    var selectColor = new Rectangle { Width = 20, Height = 20, Margin = new Thickness(0, 0, 10, 0), Fill = new SolidColorBrush((Color)color.GetValue(null)) };
                    var colorText = new TextBlock { Text = color.Name, VerticalAlignment = VerticalAlignment.Center };
                    var colorStack = new StackPanel { Orientation = Orientation.Horizontal };
                    colorStack.Children.Add(selectColor);
                    colorStack.Children.Add(colorText);
                    AccentColorTwoCombo.Items.Add(new ComboBoxItem { Content = colorStack });
                }
            }

            reg.ReadDWORD(RegistryHive.HKLM, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionBlackReplacementColor", out uint BurnInProtectionBlackReplacementColor);
            foreach (var color in typeof(Colors).GetRuntimeProperties())
            {
                if (color.Name != "AliceBlue" && color.Name != "AntiqueWhite" && color.Name != "Azure" && color.Name != "Beige" && color.Name != "Bisque" && color.Name != "Black" && color.Name != "BlanchedAlmond" && color.Name != "Cornsilk" && color.Name != "FloralWhite" && color.Name != "Gainsboro" && color.Name != "GhostWhite" && color.Name != "Honeydew" && color.Name != "Ivory" && color.Name != "Lavender" && color.Name != "LavenderBlush" && color.Name != "LemonChiffon"
                && color.Name != "LightCyan" && color.Name != "LightGoldenrodYellow" && color.Name != "LightGray" && color.Name != "LightYellow" && color.Name != "Linen" && color.Name != "MintCream" && color.Name != "MistyRose" && color.Name != "Moccasin" && color.Name != "OldLace" && color.Name != "PapayaWhip" && color.Name != "SeaShell" && color.Name != "Snow" && color.Name != "Transparent" && color.Name != "White" && color.Name != "WhiteSmoke")
                {
                    var selectColor = new Rectangle { Width = 20, Height = 20, Margin = new Thickness(0, 0, 10, 0), Fill = new SolidColorBrush((Color)color.GetValue(null)) };
                    var colorText = new TextBlock { Text = color.Name, VerticalAlignment = VerticalAlignment.Center };
                    var colorStack = new StackPanel { Orientation = Orientation.Horizontal };
                    colorStack.Children.Add(selectColor);
                    colorStack.Children.Add(colorText);
                    var cbi = new ComboBoxItem { Content = colorStack };
                    ColorPickCombo.Items.Add(cbi);
                    SolidColorBrush solidColor = (SolidColorBrush)selectColor.Fill;
                    if (Convert.ToInt32(solidColor.Color.ToString().Remove(0, 3), 16) == BurnInProtectionBlackReplacementColor)
                    {
                        ColorPickCombo.SelectedItem = cbi;
                    }
                }
            }

            IEnumerable<Package> allPackages = null;
            await Task.Run(() => { allPackages = new PackageManager().FindPackagesForUserWithPackageTypes("", PackageTypes.Main); });
            SearchAppLoadingProg.Maximum = allPackages.Count();
            IProgress<double> progress = new Progress<double>(value =>
            {
                SearchAppLoadingProg.Value += value;
                var finalValue = Math.Round(100 * (SearchAppLoadingProg.Value / SearchAppLoadingProg.Maximum));
                SearchAppLoadingText.Text = $"Loading... {finalValue}%";
            });
            var pressFound = false;
            var holdFound = false;
            await Task.Run(async () => { Packages = await PacManHelper.GetPackagesByType(PackageTypes.Main, false, progress); });
            foreach (var Package in Packages)
            {
                SearchPressAppsCombo.Items.Add(Package.DisplayName);
                SearchHoldAppsCombo.Items.Add(Package.DisplayName);
                try
                {
                    var manifest = await Package.InstalledLocation.GetFileAsync("AppxManifest.xml");
                    var tags = XElement.Load(manifest.Path).Elements().Where(i => i.Name.LocalName == "PhoneIdentity");
                    var attributes = tags.Attributes().Where(i => i.Name.LocalName == "PhoneProductId");
                    var press = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Input\\WEH\\Buttons\\WEHButton4\\Press", "AppID", Helper.RegistryHelper.RegistryType.REG_SZ);
                    var hold = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Input\\WEH\\Buttons\\WEHButton4\\PressAndHold", "AppID", Helper.RegistryHelper.RegistryType.REG_SZ);
                    try
                    {
                        if (pressFound == false)
                        {
                            if (press == "")
                            {
                                SearchPressAppsCombo.SelectedIndex = 1;
                                pressFound = true;
                            }
                            else if (press == "{None}")
                            {
                                SearchPressAppsCombo.SelectedIndex = 2;
                                pressFound = true;
                            }
                            else if (press == $"{{{attributes.First().Value}}}")
                            {
                                SearchPressAppsCombo.SelectedIndex = SearchPressAppsCombo.Items.Count - 1;
                                pressFound = true;
                            }
                            else
                            {
                                SearchPressAppsCombo.SelectedIndex = 0;
                            }
                            if (SearchPressAppsCombo.SelectedItem.ToString() == "CMD Injector") SearchPressParaCombo.Visibility = Visibility.Visible;
                        }
                    }
                    catch (Exception ex)
                    {
                        //Helper.ThrowException(ex);
                    }
                    try
                    {
                        if (holdFound == false)
                        {
                            if (hold == "")
                            {
                                SearchHoldAppsCombo.SelectedIndex = 1;
                                holdFound = true;
                            }
                            else if (hold == "{None}")
                            {
                                SearchHoldAppsCombo.SelectedIndex = 2;
                                holdFound = true;
                            }
                            else if (hold == $"{{{attributes.First().Value}}}")
                            {
                                SearchHoldAppsCombo.SelectedIndex = SearchHoldAppsCombo.Items.Count - 1;
                                holdFound = true;
                            }
                            else
                            {
                                SearchHoldAppsCombo.SelectedIndex = 0;
                            }
                            if (SearchHoldAppsCombo.SelectedItem.ToString() == "CMD Injector") SearchHoldParaCombo.Visibility = Visibility.Visible;
                        }
                    }
                    catch (Exception ex)
                    {
                        //Helper.ThrowException(ex);
                    }
                }
                catch (Exception ex)
                {
                    //Helper.ThrowException(ex);
                }
            }
            SearchPressAppsCombo.IsEnabled = true;
            SearchPressParaCombo.IsEnabled = true;
            SearchHoldAppsCombo.IsEnabled = true;
            SearchHoldParaCombo.IsEnabled = true;
            SearchAppLoadStack.Visibility = Visibility.Collapsed;
            var pressParam = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Input\\WEH\\Buttons\\WEHButton4\\Press", "AppParam", Helper.RegistryHelper.RegistryType.REG_SZ);
            var HoldParam = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Input\\WEH\\Buttons\\WEHButton4\\PressAndHold", "AppParam", Helper.RegistryHelper.RegistryType.REG_SZ);
            if (pressParam == "") SearchPressParaCombo.SelectedIndex = 1;
            else if (pressParam == "HomePage") SearchPressParaCombo.SelectedIndex = 4;
            else if (pressParam == "TerminalPage") SearchPressParaCombo.SelectedIndex = 5;
            else if (pressParam == "StartupPage") SearchPressParaCombo.SelectedIndex = 6;
            else if (pressParam == "PacManPage") SearchPressParaCombo.SelectedIndex = 7;
            else if (pressParam == "SnapperPage") SearchPressParaCombo.SelectedIndex = 8;
            else if (pressParam == "BootConfigPage") SearchPressParaCombo.SelectedIndex = 9;
            else if (pressParam == "TweakBoxPage") SearchPressParaCombo.SelectedIndex = 10;
            else if (pressParam == "SettingsPage") SearchPressParaCombo.SelectedIndex = 11;
            else if (pressParam == "HelpPage") SearchPressParaCombo.SelectedIndex = 12;
            else if (pressParam == "AboutPage") SearchPressParaCombo.SelectedIndex = 13;
            else if (pressParam == "Shutdown") SearchPressParaCombo.SelectedIndex = 16;
            else if (pressParam == "Restart") SearchPressParaCombo.SelectedIndex = 17;
            else if (pressParam == "Lockscreen") SearchPressParaCombo.SelectedIndex = 18;
            else if (pressParam == "FFULoader") SearchPressParaCombo.SelectedIndex = 19;
            else if (pressParam == "VolUp") SearchPressParaCombo.SelectedIndex = 22;
            else if (pressParam == "VolDown") SearchPressParaCombo.SelectedIndex = 23;
            else if (pressParam == "VolMute") SearchPressParaCombo.SelectedIndex = 24;
            if (HoldParam == "") SearchHoldParaCombo.SelectedIndex = 1;
            else if (HoldParam == "HomePage") SearchHoldParaCombo.SelectedIndex = 4;
            else if (HoldParam == "TerminalPage") SearchHoldParaCombo.SelectedIndex = 5;
            else if (HoldParam == "StartupPage") SearchHoldParaCombo.SelectedIndex = 6;
            else if (HoldParam == "PacManPage") SearchHoldParaCombo.SelectedIndex = 7;
            else if (HoldParam == "SnapperPage") SearchHoldParaCombo.SelectedIndex = 8;
            else if (HoldParam == "BootConfigPage") SearchHoldParaCombo.SelectedIndex = 9;
            else if (HoldParam == "TweakBoxPage") SearchHoldParaCombo.SelectedIndex = 10;
            else if (HoldParam == "SettingsPage") SearchHoldParaCombo.SelectedIndex = 11;
            else if (HoldParam == "HelpPage") SearchHoldParaCombo.SelectedIndex = 12;
            else if (HoldParam == "AboutPage") SearchHoldParaCombo.SelectedIndex = 13;
            else if (HoldParam == "Shutdown") SearchHoldParaCombo.SelectedIndex = 16;
            else if (HoldParam == "Restart") SearchHoldParaCombo.SelectedIndex = 17;
            else if (HoldParam == "Lockscreen") SearchHoldParaCombo.SelectedIndex = 18;
            else if (HoldParam == "FFULoader") SearchHoldParaCombo.SelectedIndex = 19;
            else if (HoldParam == "VolUp") SearchHoldParaCombo.SelectedIndex = 22;
            else if (HoldParam == "VolDown") SearchHoldParaCombo.SelectedIndex = 23;
            else if (HoldParam == "VolMute") SearchHoldParaCombo.SelectedIndex = 24;

            secondFlag = true;
        }

        private async void Initialize()
        {
            try
            {
                DisplayOrient.SelectedIndex = Helper.LocalSettingsHelper.LoadSettings("OrientSet", 0);
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\NOKIA\\Display\\ColorAndLight", "UserSettingNoBrightnessSettings", Helper.RegistryHelper.RegistryType.REG_DWORD) != string.Empty) BrightTog.IsOn = true;

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Control Panel\\Theme", "CurrentTheme", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000000")
                {
                    BackgModeCombo.SelectedIndex = 1;
                }
                else
                {
                    BackgModeCombo.SelectedIndex = 0;
                }
                AutoBackgCombo.SelectedIndex = Helper.LocalSettingsHelper.LoadSettings("AutoThemeMode", 0);
                BackgStartTime.Time = new TimeSpan(Convert.ToInt32(Helper.LocalSettingsHelper.LoadSettings("AutoThemeLight", "06:00").Split(':')[0]), Convert.ToInt32(Helper.LocalSettingsHelper.LoadSettings("AutoThemeLight", "06:00").Split(':')[1]), 00);
                BackgStopTime.Time = new TimeSpan(Convert.ToInt32(Helper.LocalSettingsHelper.LoadSettings("AutoThemeDark", "18:00").Split(':')[0]), Convert.ToInt32(Helper.LocalSettingsHelper.LoadSettings("AutoThemeDark", "18:00").Split(':')[1]), 00);

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\DeviceUpdate\\Agent\\Settings", "GuidOfCategoryToScan", Helper.RegistryHelper.RegistryType.REG_SZ) == "F1E8E1CD-9819-4AC5-B0A7-2AFF3D29B46E") UptTog.IsOn = false;
                else UptTog.IsOn = true;

                if (await Helper.IsCapabilitiesAllowed()){
                    if (File.Exists("C:\\Data\\SharedData\\OEM\\Public\\NsgGlance_NlpmService_4.1.12.4.dll") && reg.ReadString(RegistryHive.HKLM, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", out string FontFile) && reg.ReadDWORD(RegistryHive.HKLM, "SOFTWARE\\OEM\\Nokia\\lpm", "Enabled", out uint GlanceEnabled)) GlanceTog.IsOn = true;
                    else GlanceTog.IsOn = false;
                }
                else
                {
                    GlanceTog.IsOn = false;
                }
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ).IndexOf("lpmFont_WVGA.bin", StringComparison.OrdinalIgnoreCase) >= 0) FontFileBox.SelectedIndex = 1;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ).IndexOf("lpmFont_720P.bin", StringComparison.OrdinalIgnoreCase) >= 0) FontFileBox.SelectedIndex = 2;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ).IndexOf("lpmFont_720P_hi.bin", StringComparison.OrdinalIgnoreCase) >= 0) FontFileBox.SelectedIndex = 3;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ).IndexOf("lpmFont_WXGA.bin", StringComparison.OrdinalIgnoreCase) >= 0) FontFileBox.SelectedIndex = 4;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ).IndexOf("lpmFont_FHD.bin", StringComparison.OrdinalIgnoreCase) >= 0) FontFileBox.SelectedIndex = 5;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ).IndexOf("lpmFont_FHD_hi.bin", StringComparison.OrdinalIgnoreCase) >= 0) FontFileBox.SelectedIndex = 6;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ).IndexOf("lpmFont_WQHD.bin", StringComparison.OrdinalIgnoreCase) >= 0) FontFileBox.SelectedIndex = 7;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ).IndexOf("lpmFont_WQHD_hi.bin", StringComparison.OrdinalIgnoreCase) >= 0) FontFileBox.SelectedIndex = 8;
                if (reg.ReadDWORD(RegistryHive.HKLM, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", out uint ClockAndIndicatorsCustomColor) && ClockAndIndicatorsCustomColor != 0)
                {
                    FontColorTog.IsOn = true;
                    RedRadio.IsChecked = false;
                    CyanRadio.IsChecked = false;
                    GreenRadio.IsChecked = false;
                    MagentaRadio.IsChecked = false;
                    BlueRadio.IsChecked = false;
                    YellowRadio.IsChecked = false;
                    if (ClockAndIndicatorsCustomColor == 16711680) RedRadio.IsChecked = true;
                    else if (ClockAndIndicatorsCustomColor == 65280) GreenRadio.IsChecked = true;
                    else if (ClockAndIndicatorsCustomColor == 255) BlueRadio.IsChecked = true;
                    else if (ClockAndIndicatorsCustomColor == 65535) CyanRadio.IsChecked = true;
                    else if (ClockAndIndicatorsCustomColor == 16711935) MagentaRadio.IsChecked = true;
                    else if (ClockAndIndicatorsCustomColor == 16776960) YellowRadio.IsChecked = true;
                    GlanceColorStack.Visibility = Visibility.Visible;
                }
                else
                {
                    FontColorTog.IsOn = false;
                    GlanceColorStack.Visibility = Visibility.Collapsed;
                }
                GlanceAutoColor.SelectedIndex = Helper.LocalSettingsHelper.LoadSettings("GlanceAutoColorEnabled", 0);
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD).IndexOf("00ff0000", StringComparison.OrdinalIgnoreCase) >= 0) RedRadio.IsChecked = true;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD).IndexOf("0000ff00", StringComparison.OrdinalIgnoreCase) >= 0) GreenRadio.IsChecked = true;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD).IndexOf("000000ff", StringComparison.OrdinalIgnoreCase) >= 0) BlueRadio.IsChecked = true;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD).IndexOf("0000ffff", StringComparison.OrdinalIgnoreCase) >= 0) CyanRadio.IsChecked = true;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD).IndexOf("00ff00ff", StringComparison.OrdinalIgnoreCase) >= 0) MagentaRadio.IsChecked = true;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD).IndexOf("00ffff00", StringComparison.OrdinalIgnoreCase) >= 0) YellowRadio.IsChecked = true;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "MoveClock", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001") MoveClockTog.IsOn = true;
                else MoveClockTog.IsOn = false;

                if (File.Exists(Helper.localFolder.Path + "\\LiveLockscreen.bat"))
                {
                    AutoWallTog.IsOn = true;
                }
                if (!File.Exists($"{Helper.localFolder.Path}\\Lockscreen.dat"))
                {
                    WallCollectionCombo.SelectedIndex = 0;
                    await FileIO.WriteTextAsync(await Helper.localFolder.CreateFileAsync("Lockscreen.dat"), $"{Helper.installedLocation.Path}\\Assets\\Images\\Lockscreens\\RedMoon\n65\nTrue\n00:00\n00:00\nTrue");
                }
                var LockscreenData = await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"));
                if (File.ReadLines((await Helper.localFolder.GetFileAsync("Lockscreen.dat")).Path).Count() <= 2)
                {
                    await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{LockscreenData.Split('\n')[0]}\n{LockscreenData.Split('\n')[1]}\nTrue\n00:00\n00:00\nTrue");
                }
                else if (File.ReadLines((await Helper.localFolder.GetFileAsync("Lockscreen.dat")).Path).Count() <= 6)
                {
                    await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{LockscreenData.Split('\n')[0]}\n{LockscreenData.Split('\n')[1]}\n{LockscreenData.Split('\n')[2]}\n{LockscreenData.Split('\n')[3]}\n{LockscreenData.Split('\n')[4]}\nTrue");
                }
                LockscreenData = await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"));
                if (System.IO.Path.GetFileName(LockscreenData.Split('\n')[0]) == "RedMoon")
                {
                    WallCollectionCombo.SelectedIndex = 0;
                }
                else if (System.IO.Path.GetFileName(LockscreenData.Split('\n')[0]) == "Flowers")
                {
                    WallCollectionCombo.SelectedIndex = 1;
                }
                else if (System.IO.Path.GetFileName(LockscreenData.Split('\n')[0]) == "Timelapse")
                {
                    WallCollectionCombo.SelectedIndex = 2;
                }
                else
                {
                    WallCollectionStack.Visibility = Visibility.Visible;
                    WallCollectionCombo.SelectedIndex = 3;
                }
                if (LockscreenData.Split('\n')[0].Contains(Helper.localFolder.Path))
                {
                    WallCollectionBox.Text = $"CMDInjector:\\Library\\{System.IO.Path.GetFileName(LockscreenData.Split('\n')[0])}";
                }
                else if (LockscreenData.Split('\n')[0].Contains(Helper.installedLocation.Path))
                {
                    WallCollectionBox.Text = $"CMDInjector:\\Assets\\Images\\Lockscreens\\{System.IO.Path.GetFileName(LockscreenData.Split('\n')[0])}";
                }
                else
                {
                    WallCollectionBox.Text = LockscreenData.Split('\n')[0];
                }
                WallIntervalBox.Text = LockscreenData.Split('\n')[1];
                if (LockscreenData.Split('\n')[2] == "True")
                {
                    WallRevLoopTog.IsOn = true;
                }
                if (Helper.LocalSettingsHelper.LoadSettings("ActiveHours", true) && LockscreenData.Split('\n')[3] != LockscreenData.Split('\n')[4])
                {
                    ActiveHoursTog.IsOn = true;
                    ActiveHoursStack.Visibility = Visibility.Visible;
                }
                else
                {
                    ActiveHoursTog.IsOn = false;
                    ActiveHoursStack.Visibility = Visibility.Collapsed;
                }
                if (flag == false)
                {
                    await Task.Delay(200);
                    StartTimePkr.Time = new TimeSpan(Convert.ToInt32(LockscreenData.Split('\n')[3].Split(':')[0]), Convert.ToInt32(LockscreenData.Split('\n')[3].Split(':')[1]), 00);
                    StopTimePkr.Time = new TimeSpan(Convert.ToInt32(LockscreenData.Split('\n')[4].Split(':')[0]), Convert.ToInt32(LockscreenData.Split('\n')[4].Split(':')[1]), 00);
                }
                if (LockscreenData.Split('\n')[5] == "True")
                {
                    WallDisBatSavTog.IsOn = true;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7ea2e1ac-2e61-4728-aaa3-896d9d0a9f0e}\\Elements\\16000069", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01") && Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7ea2e1ac-2e61-4728-aaa3-896d9d0a9f0e}\\Elements\\1600007a", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    BootAnimTog.IsOn = false;
                }
                else
                {
                    BootAnimTog.IsOn = true;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpbootscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ) == string.Empty)
                {
                    BootImageTog.IsOn = false;
                }
                else
                {
                    BootImageTog.IsOn = true;
                }
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpshutdownscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ) == string.Empty)
                {
                    ShutdownImageTog.IsOn = false;
                }
                else
                {
                    ShutdownImageTog.IsOn = true;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "SoftwareModeEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001") SoftNavTog.IsOn = true;
                else SoftNavTog.IsOn = false;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsDoubleTapOffEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001") DoubleTapTog.IsOn = true;
                else DoubleTapTog.IsOn = false;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsAutoHideEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001") AutoHideTog.IsOn = true;
                else AutoHideTog.IsOn = false;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsSwipeUpToHideEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001") SwipeUpTog.IsOn = true;
                else SwipeUpTog.IsOn = false;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsUserManaged", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001") UserManagedTog.IsOn = true;
                else UserManagedTog.IsOn = false;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsBurnInProtectionEnabled", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000001") BurninProtTog.IsOn = true;
                else BurninProtTog.IsOn = false;
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionIdleTimerTimeout", Helper.RegistryHelper.RegistryType.REG_DWORD) != string.Empty)
                    BurninTimeoutBox.Text = Convert.ToInt32(Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionIdleTimerTimeout", Helper.RegistryHelper.RegistryType.REG_DWORD), 16).ToString();
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionIconsOpacity", Helper.RegistryHelper.RegistryType.REG_DWORD) != string.Empty)
                    OpacitySlide.Value = Convert.ToInt32(Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionIconsOpacity", Helper.RegistryHelper.RegistryType.REG_DWORD), 16);

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Shell\\Start", "TileColumnSize", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000004") TileCombo.SelectedIndex = 0;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Shell\\Start", "TileColumnSize", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000006") TileCombo.SelectedIndex = 1;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Shell\\Start", "TileColumnSize", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000008") TileCombo.SelectedIndex = 2;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Shell\\Start", "TileColumnSize", Helper.RegistryHelper.RegistryType.REG_DWORD) == "0000000a") TileCombo.SelectedIndex = 3;
                else if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Shell\\Start", "TileColumnSize", Helper.RegistryHelper.RegistryType.REG_DWORD) == "0000000c") TileCombo.SelectedIndex = 4;

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\NOKIA\\Camera\\Barc", "DNGDisabled", Helper.RegistryHelper.RegistryType.REG_DWORD) == "00000000") DngTog.IsOn = true;

                VirtualMemBox.Text = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\CurrentControlSet\\Control\\Session Manager\\Memory Management", "PagingFiles", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ);

                switch (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps", "DumpType", Helper.RegistryHelper.RegistryType.REG_DWORD))
                {
                    case "00000000":
                        DumpTypeCombo.SelectedIndex = 0;
                        break;
                    case "00000001":
                        DumpTypeCombo.SelectedIndex = 1;
                        break;
                    case "00000002":
                        DumpTypeCombo.SelectedIndex = 2;
                        break;
                    default:
                        DumpTypeCombo.SelectedIndex = 0;
                        break;
                }
                switch (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps", "DumpCount", Helper.RegistryHelper.RegistryType.REG_DWORD))
                {
                    case "":
                        DumpCountCombo.SelectedIndex = 0;
                        break;
                    case "00000001":
                        DumpCountCombo.SelectedIndex = 0;
                        break;
                    case "00000002":
                        DumpCountCombo.SelectedIndex = 1;
                        break;
                    case "00000003":
                        DumpCountCombo.SelectedIndex = 2;
                        break;
                    case "00000004":
                        DumpCountCombo.SelectedIndex = 3;
                        break;
                    case "00000005":
                        DumpCountCombo.SelectedIndex = 4;
                        break;
                    case "00000006":
                        DumpCountCombo.SelectedIndex = 5;
                        break;
                    case "00000007":
                        DumpCountCombo.SelectedIndex = 6;
                        break;
                    case "00000008":
                        DumpCountCombo.SelectedIndex = 7;
                        break;
                    case "00000009":
                        DumpCountCombo.SelectedIndex = 8;
                        break;
                    case "0000000a":
                        DumpCountCombo.SelectedIndex = 9;
                        break;
                    default:
                        //DumpCountCombo.Items.Add(Regex.Replace(Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps", "DumpCount", Helper.RegistryHelper.RegistryType.REG_DWORD), @"\s+", ""));
                        DumpCountCombo.SelectedIndex = 9;
                        break;
                }
                DumpFolderBox.Text = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps", "DumpFolder", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ);

                flag = true;
            }
            catch (Exception ex)
            {
                //Helper.ThrowException(ex);
            }
        }

        private void InitializeFolders()
        {
            MenuFlyoutItem CustomPath = new MenuFlyoutItem();
            CustomPath.Click += Items_Click;
            CustomPath.Text = "Custom Location";
            AddFlyMenu.Items.Add(CustomPath);
            if (!Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\FileExplorer\\Config", "NavigationRoots", Helper.RegistryHelper.RegistryType.REG_SZ).Contains("shell:::{679f85cb-0220-4080-b29b-5540cc05aab6}"))
            {
                MenuFlyoutItem Recent = new MenuFlyoutItem();
                Recent.Click += Items_Click;
                Recent.Text = "Recent";
                AddFlyMenu.Items.Add(Recent);
            }
            if (!Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\FileExplorer\\Config", "NavigationRoots", Helper.RegistryHelper.RegistryType.REG_SZ).Contains("knownfolder:{1C2AC1DC-4358-4B6C-9733-AF21156576F0}"))
            {
                MenuFlyoutItem ThisDevice = new MenuFlyoutItem();
                ThisDevice.Click += Items_Click;
                ThisDevice.Text = "This Device";
                AddFlyMenu.Items.Add(ThisDevice);
            }
            if (!Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\FileExplorer\\Config", "NavigationRoots", Helper.RegistryHelper.RegistryType.REG_SZ).Contains("::{20D04FE0-3AEA-1069-A2D8-08002B30309D}"))
            {
                MenuFlyoutItem ThisPC = new MenuFlyoutItem();
                ThisPC.Click += Items_Click;
                ThisPC.Text = "This PC";
                AddFlyMenu.Items.Add(ThisPC);
            }
            string[] NavigationRoots = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\FileExplorer\\Config", "NavigationRoots", Helper.RegistryHelper.RegistryType.REG_SZ).Split(';');
            if (NavigationRoots.Length <= 1)
            {
                RemoveBtn.IsEnabled = false;
                MoveUpBtn.IsEnabled = false;
            }
            else
            {
                RemoveBtn.IsEnabled = true;
                MoveUpBtn.IsEnabled = true;
            }
            Paths = new string[NavigationRoots.Length];
            for (int i = 0; i < NavigationRoots.Length; i++)
            {
                Paths[i] = NavigationRoots[i];
                if (NavigationRoots[i].Equals("shell:::{679f85cb-0220-4080-b29b-5540cc05aab6}", StringComparison.CurrentCultureIgnoreCase)) NavigationRoots[i] = "Recent";
                else if (NavigationRoots[i].Equals("knownfolder:{1C2AC1DC-4358-4B6C-9733-AF21156576F0}", StringComparison.CurrentCultureIgnoreCase)) NavigationRoots[i] = "This Device";
                else if (NavigationRoots[i].Equals("::{20D04FE0-3AEA-1069-A2D8-08002B30309D}", StringComparison.CurrentCultureIgnoreCase)) NavigationRoots[i] = "This PC";
                else if (NavigationRoots[i].Equals("C:", StringComparison.CurrentCultureIgnoreCase)) NavigationRoots[i] = "MainOS (C:)";
                else if (NavigationRoots[i].Equals("U:", StringComparison.CurrentCultureIgnoreCase)) NavigationRoots[i] = "Data (U:)";
                else if (NavigationRoots[i].Equals("D:", StringComparison.CurrentCultureIgnoreCase)) NavigationRoots[i] = "SD Card (D:)";
                FolderBox.Items.Add(NavigationRoots[i]);
                RootOrderList.Items.Add(NavigationRoots[i]);
                FolderPathCombo.Items.Add(NavigationRoots[i]);
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\FileExplorer\\Config", "DefaultFolder", Helper.RegistryHelper.RegistryType.REG_SZ) == Paths[i]) FolderBox.SelectedIndex = i;
            }
            if (FolderBox.SelectedIndex == -1)
            {
                try
                {
                    string DefaultPath = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\FileExplorer\\Config", "DefaultFolder", Helper.RegistryHelper.RegistryType.REG_SZ);
                    if (DefaultPath.Equals("shell:::{679f85cb-0220-4080-b29b-5540cc05aab6}", StringComparison.CurrentCultureIgnoreCase)) DefaultPath = "Recent";
                    else if (DefaultPath.Equals("knownfolder:{1C2AC1DC-4358-4B6C-9733-AF21156576F0}", StringComparison.CurrentCultureIgnoreCase)) DefaultPath = "This Device";
                    else if (DefaultPath.Equals("::{20D04FE0-3AEA-1069-A2D8-08002B30309D}", StringComparison.CurrentCultureIgnoreCase)) DefaultPath = "This PC";
                    else if (DefaultPath.Equals("C:", StringComparison.CurrentCultureIgnoreCase)) DefaultPath = "MainOS (C:)";
                    else if (DefaultPath.Equals("U:", StringComparison.CurrentCultureIgnoreCase)) DefaultPath = "Data (U:)";
                    else if (DefaultPath.Equals("D:", StringComparison.CurrentCultureIgnoreCase)) DefaultPath = "SD Card (D:)";
                    FolderBox.Items.Add(DefaultPath);
                    FolderBox.SelectedIndex = FolderBox.Items.Count - 1;
                }
                catch (Exception ex) { }
            }
            InstalledFont ifg = new InstalledFont();
            var Glyphs = ifg.GetCharacters("Segoe MDL2 Assets");
            foreach (var Glyph in Glyphs)
            {
                glyphUnicodes.Add(Glyph.UnicodeIndex);
                FolderIconCombo.Items.Add(Glyph.Char);
            }
            if (FolderIconCombo.SelectedIndex == -1) FolderPathCombo.SelectedIndex = 0;
        }

        private void FolderBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\FileExplorer\\Config", "DefaultFolder", Helper.RegistryHelper.RegistryType.REG_SZ, Paths[FolderBox.SelectedIndex]);
        }

        private void Items_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem clickedItem = (MenuFlyoutItem)sender;
            if (clickedItem.Text == "Custom Location")
            {
                CustomLocation();
                return;
            }
            else if (clickedItem.Text == "This PC")
            {
                rpc.RegSetValueW(1, "Software\\Microsoft\\Windows\\CurrentVersion\\FileExplorer\\Config\\FolderIconCharacters", "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}", 4, BitConverter.GetBytes(uint.Parse("57873")));
            }
            RootOrderList.Items.Add(clickedItem.Text);
            AddFlyMenu.Items.Remove(clickedItem);
            SaveLocation();
        }

        private async void CustomLocation()
        {
            ContentDialog addPath = new ContentDialog
            {
                Title = "Select Option",
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "Browse Location",
                SecondaryButtonText = "Enter Location"
            };
            Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
            var result = await addPath.ShowAsync();
            if (result == ContentDialogResult.None)
            {
                return;
            }
            else if (result == ContentDialogResult.Primary)
            {
                var folderPicker = new FolderPicker
                {
                    SuggestedStartLocation = PickerLocationId.ComputerFolder
                };
                folderPicker.FileTypeFilter.Add("*");
                StorageFolder CustomFolder = await folderPicker.PickSingleFolderAsync();
                if (CustomFolder != null)
                {
                    rpc.RegSetValueW(1, "Software\\Microsoft\\Windows\\CurrentVersion\\FileExplorer\\Config\\FolderIconCharacters", CustomFolder.Path, 4, BitConverter.GetBytes(uint.Parse("60737")));
                    string value;
                    if (CustomFolder.Path.Equals("shell:::{679f85cb-0220-4080-b29b-5540cc05aab6}", StringComparison.CurrentCultureIgnoreCase)) value = "MainOS (C:)";
                    if (CustomFolder.Path.Equals("::{20D04FE0-3AEA-1069-A2D8-08002B30309D}", StringComparison.CurrentCultureIgnoreCase)) value = "This PC";
                    if (CustomFolder.Path.Equals("C:\\", StringComparison.CurrentCultureIgnoreCase)) value = "MainOS (C:)";
                    else if (CustomFolder.Path.Equals("U:\\", StringComparison.CurrentCultureIgnoreCase)) value = "Data (U:)";
                    else if (CustomFolder.Path.Equals("D:\\", StringComparison.CurrentCultureIgnoreCase)) value = "SD Card (D:)";
                    else value = CustomFolder.Path;
                    RootOrderList.Items.Add(value);
                }
            }
            else
            {
                TextBox inputTextBox = new TextBox
                {
                    AcceptsReturn = false,
                    IsSpellCheckEnabled = false,
                    IsTextPredictionEnabled = false,
                    PlaceholderText = "C:\\Data\\Users\\Public\\Downloads"
                };
                ContentDialog customPath = new ContentDialog
                {
                    Title = "Add Location",
                    IsSecondaryButtonEnabled = true,
                    PrimaryButtonText = "Add",
                    SecondaryButtonText = "Cancel",
                    Content = inputTextBox
                };
                try
                {
                    Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                    if (await customPath.ShowAsync() == ContentDialogResult.Primary)
                    {
                        if (Directory.Exists(inputTextBox.Text) && inputTextBox.Text != string.Empty)
                        {
                            if (inputTextBox.Text[inputTextBox.Text.Length - 1].ToString() == "\\")
                            {
                                inputTextBox.Text = inputTextBox.Text.Remove(inputTextBox.Text.Length - 1);
                            }
                            rpc.RegSetValueW(1, "Software\\Microsoft\\Windows\\CurrentVersion\\FileExplorer\\Config\\FolderIconCharacters", inputTextBox.Text, 4, BitConverter.GetBytes(uint.Parse("60737")));
                            if (inputTextBox.Text.Equals("C:", StringComparison.CurrentCultureIgnoreCase)) inputTextBox.Text = "MainOS (C:)";
                            else if (inputTextBox.Text.Equals("U:", StringComparison.CurrentCultureIgnoreCase)) inputTextBox.Text = "Data (U:)";
                            else if (inputTextBox.Text.Equals("D:", StringComparison.CurrentCultureIgnoreCase)) inputTextBox.Text = "SD Card (D:)";
                            RootOrderList.Items.Add(inputTextBox.Text);
                        }
                        else
                        {
                            await Helper.MessageBox("The entered location doesn't exist.", Helper.SoundHelper.Sound.Error, "Missing Location");
                        }
                        inputTextBox.Text = string.Empty;
                    }
                }
                catch (Exception ex)
                {
                    if (inputTextBox.Text[inputTextBox.Text.Length - 1].ToString() == "\\")
                    {
                        inputTextBox.Text = inputTextBox.Text.Remove(inputTextBox.Text.Length - 1);
                    }
                    rpc.RegSetValueW(1, "Software\\Microsoft\\Windows\\CurrentVersion\\FileExplorer\\Config\\FolderIconCharacters", inputTextBox.Text, 4, BitConverter.GetBytes(uint.Parse("60737")));
                    if (inputTextBox.Text.Equals("C:", StringComparison.CurrentCultureIgnoreCase)) inputTextBox.Text = "MainOS (C:)";
                    else if (inputTextBox.Text.Equals("U:", StringComparison.CurrentCultureIgnoreCase)) inputTextBox.Text = "Data (U:)";
                    else if (inputTextBox.Text.Equals("D:", StringComparison.CurrentCultureIgnoreCase)) inputTextBox.Text = "SD Card (D:)";
                    RootOrderList.Items.Add(inputTextBox.Text);
                }
            }
            SaveLocation();
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (RootOrderList.SelectedIndex == -1)
            {
                return;
            }
            RootOrderList.Items.Remove(RootOrderList.SelectedItem);
            SaveLocation();
        }

        private void MoveUpBtn_Click(object sender, RoutedEventArgs e)
        {
            if (RootOrderList.SelectedIndex == 0 || RootOrderList.SelectedIndex == -1)
            {
                return;
            }
            int newIndex = RootOrderList.SelectedIndex - 1;
            object selectedIndex = RootOrderList.SelectedItem;
            RootOrderList.Items.Remove(selectedIndex);
            RootOrderList.Items.Insert(newIndex, selectedIndex);
            RootOrderList.SelectedIndex = newIndex;
            SaveLocation();
        }

        private void SaveLocation()
        {
            try
            {
                string folderRoots = string.Empty;
                for (int i = 0; i < RootOrderList.Items.Count; i++)
                {
                    if (RootOrderList.Items[i].ToString().Equals("Recent", StringComparison.CurrentCultureIgnoreCase)) RootOrderList.Items[i] = "shell:::{679f85cb-0220-4080-b29b-5540cc05aab6}";
                    else if (RootOrderList.Items[i].ToString().Equals("This Device", StringComparison.CurrentCultureIgnoreCase)) RootOrderList.Items[i] = "knownfolder:{1C2AC1DC-4358-4B6C-9733-AF21156576F0}";
                    else if (RootOrderList.Items[i].ToString().Equals("This PC", StringComparison.CurrentCultureIgnoreCase)) RootOrderList.Items[i] = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
                    else if (RootOrderList.Items[i].ToString().Equals("MainOS (C:)", StringComparison.CurrentCultureIgnoreCase)) RootOrderList.Items[i] = "C:";
                    else if (RootOrderList.Items[i].ToString().Equals("Data (U:)", StringComparison.CurrentCultureIgnoreCase)) RootOrderList.Items[i] = "U:";
                    else if (RootOrderList.Items[i].ToString().Equals("SD Card (D:)", StringComparison.CurrentCultureIgnoreCase)) RootOrderList.Items[i] = "D:";
                    folderRoots += RootOrderList.Items[i] + ";";
                }
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\FileExplorer\\Config", "NavigationRoots", Helper.RegistryHelper.RegistryType.REG_SZ, folderRoots.Remove(folderRoots.Length - 1, 1));
                FolderBox.Items.Clear();
                RootOrderList.Items.Clear();
                FolderPathCombo.Items.Clear();
                FolderIconCombo.Items.Clear();
                InitializeFolders();
            }
            catch (Exception ex)
            {
                if (ex.Message == "Index was outside the bounds of the array." || ex.Message == "Arg_IndexOutOfRangeException")
                {
                    AddFlyMenu.Items.Clear();
                    RootOrderList.Items.Clear();
                    FolderPathCombo.Items.Clear();
                    FolderIconCombo.Items.Clear();
                    InitializeFolders();
                }
                else
                {
                    Helper.ThrowException(ex);
                }
            }
        }

        private void FolderPathCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FolderPathCombo.SelectedIndex != -1)
            {
                string SelFoldUri = FolderPathCombo.Items[FolderPathCombo.SelectedIndex].ToString();
                if (SelFoldUri.Equals("Recent", StringComparison.CurrentCultureIgnoreCase)) SelFoldUri = "::{679f85cb-0220-4080-b29b-5540cc05aab6}";
                else if (SelFoldUri.Equals("This Device", StringComparison.CurrentCultureIgnoreCase)) SelFoldUri = "::{5b934b42-522b-4c34-bbfe-37a3ef7b9c90}";
                else if (SelFoldUri.Equals("This PC", StringComparison.CurrentCultureIgnoreCase)) SelFoldUri = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
                else if (SelFoldUri.Equals("MainOS (C:)", StringComparison.CurrentCultureIgnoreCase)) SelFoldUri = "C:";
                else if (SelFoldUri.Equals("Data (U:)", StringComparison.CurrentCultureIgnoreCase)) SelFoldUri = "U:";
                else if (SelFoldUri.Equals("SD Card (D:)", StringComparison.CurrentCultureIgnoreCase)) SelFoldUri = "D:";
                for (int i = 0; i < glyphUnicodes.Count; i++)
                {
                    if (glyphUnicodes[i] == Convert.ToInt32(Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\FileExplorer\\Config\\FolderIconCharacters", SelFoldUri, Helper.RegistryHelper.RegistryType.REG_DWORD), 16))
                    {
                        FolderIconCombo.SelectedIndex = i;
                        rpc.RegSetValueW(1, "Software\\Microsoft\\Windows\\CurrentVersion\\FileExplorer\\Config\\FolderIconCharacters", SelFoldUri, 4, BitConverter.GetBytes(uint.Parse(glyphUnicodes[i].ToString())));
                        break;
                    }
                    else
                    {
                        FolderIconCombo.SelectedIndex = -1;
                    }
                }
            }
        }

        private void FolderIconCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FolderIconCombo.SelectedIndex != -1)
            {
                string SelFoldUri = FolderPathCombo.Items[FolderPathCombo.SelectedIndex].ToString();
                if (SelFoldUri.Equals("Recent", StringComparison.CurrentCultureIgnoreCase)) SelFoldUri = "::{679f85cb-0220-4080-b29b-5540cc05aab6}";
                else if (SelFoldUri.Equals("This Device", StringComparison.CurrentCultureIgnoreCase)) SelFoldUri = "::{5b934b42-522b-4c34-bbfe-37a3ef7b9c90}";
                else if (SelFoldUri.Equals("This PC", StringComparison.CurrentCultureIgnoreCase)) SelFoldUri = "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
                else if (SelFoldUri.Equals("MainOS (C:)", StringComparison.CurrentCultureIgnoreCase)) SelFoldUri = "C:";
                else if (SelFoldUri.Equals("Data (U:)", StringComparison.CurrentCultureIgnoreCase)) SelFoldUri = "U:";
                else if (SelFoldUri.Equals("SD Card (D:)", StringComparison.CurrentCultureIgnoreCase)) SelFoldUri = "D:";
                rpc.RegSetValueW(1, "Software\\Microsoft\\Windows\\CurrentVersion\\FileExplorer\\Config\\FolderIconCharacters", SelFoldUri, 4, BitConverter.GetBytes(uint.Parse(glyphUnicodes[FolderIconCombo.SelectedIndex].ToString())));
            }
        }

        private async void PinFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder PinTile = await folderPicker.PickSingleFolderAsync();
            StorageFolder nd = PinTile;
            if (PinTile != null)
            {
                TextBox displayName = new TextBox
                {
                    AcceptsReturn = false,
                    IsSpellCheckEnabled = false,
                    IsTextPredictionEnabled = false,
                    PlaceholderText = "Downloads"
                };
                ContentDialog pinTextBox = new ContentDialog
                {
                    Title = "Display Name",
                    IsSecondaryButtonEnabled = true,
                    PrimaryButtonText = "OK",
                    SecondaryButtonText = "Cancel",
                    Content = displayName
                };
                Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                if (await pinTextBox.ShowAsync() == ContentDialogResult.Primary)
                {
                    try
                    {
                        if (displayName.Text != string.Empty)
                        {
                            StorageApplicationPermissions.FutureAccessList.AddOrReplace(displayName.Text, PinTile);
                            if (displayName.Text.ToUpper().Contains("DOCUMENT")) TileIcon = "ms-appx:///Assets/Icons/Folders/DocumentsFolderTileLogo.png";
                            else if (displayName.Text.ToUpper().Contains("DOWNLOAD")) TileIcon = "ms-appx:///Assets/Icons/Folders/DownloadsFolderTileLogo.png";
                            else if (displayName.Text.ToUpper().Contains("FAVORITE")) TileIcon = "ms-appx:///Assets/Icons/Folders/FavoritesFolderTileLogo.png";
                            else if (displayName.Text.ToUpper().Contains("GAME") || displayName.Text.ToUpper().Contains("APP")) TileIcon = "ms-appx:///Assets/Icons/Folders/GamesFolderTileLogo.png";
                            else if (displayName.Text.ToUpper().Contains("MUSIC") || displayName.Text.ToUpper().Contains("SONG")) TileIcon = "ms-appx:///Assets/Icons/Folders/MusicFolderTileLogo.png";
                            else if (displayName.Text.ToUpper().Contains("PICTURE") || displayName.Text.ToUpper().Contains("IMAGE") || displayName.Text.ToUpper().Contains("PHOTO")) TileIcon = "ms-appx:///Assets/Icons/Folders/PicturesFolderTileLogo.png";
                            else if (displayName.Text.ToUpper().Contains("USER") || displayName.Text.ToUpper().Contains("MY ")) TileIcon = "ms-appx:///Assets/Icons/Folders/UserFolderTileLogo.png";
                            else if (displayName.Text.ToUpper().Contains("VIDEO") || displayName.Text.ToUpper().Contains("MOVIE") || displayName.Text.ToUpper().Contains("FILM")) TileIcon = "ms-appx:///Assets/Icons/Folders/VideosFolderTileLogo.png";
                            else TileIcon = "ms-appx:///Assets/Icons/Folders/ExplorerFolderTileLogo.png";
                            var FolderTile = new SecondaryTile(Regex.Replace(displayName.Text, "[^A-Za-z0-9]", ""), displayName.Text, displayName.Text, new Uri(TileIcon), TileSize.Default);
                            FolderTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                            FolderTile.VisualElements.ShowNameOnWide310x150Logo = true;
                            await FolderTile.RequestCreateAsync();
                        }
                        else
                        {
                            _ = Helper.MessageBox("You haven't entered any display name.", Helper.SoundHelper.Sound.Error, "Missing Name");
                        }
                    }
                    catch (Exception ex) { Helper.ThrowException(ex); }

                }
            }
        }

        private void DisplayOrient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flag == true)
            {
                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    if (DisplayOrient.SelectedIndex == 0)
                    {
                        Helper.LocalSettingsHelper.SaveSettings("OrientSet", DisplayOrient.SelectedIndex);
                        //await Task.Delay(100);
                        _ = tClient.Send("setDisplayResolution.exe -Orientation:0");
                    }
                    else
                    {
                        Helper.LocalSettingsHelper.SaveSettings("OrientSet", DisplayOrient.SelectedIndex);
                        //await Task.Delay(100);
                        _ = tClient.Send("setDisplayResolution.exe -Orientation:180");
                    }
                }
                else
                {
                    _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                }
            }
        }

        private void BrightTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (BrightTog.IsOn)
            {
                rpc.RegSetValueW(1, "SOFTWARE\\OEM\\NOKIA\\Display\\ColorAndLight", "UserSettingNoBrightnessSettings", 4, BitConverter.GetBytes(uint.Parse("1")));
            }
            else
            {
                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    _ = tClient.Send("reg delete HKLM\\SOFTWARE\\OEM\\NOKIA\\Display\\ColorAndLight /v UserSettingNoBrightnessSettings /f");
                }
                else
                {
                    _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                }
            }
            if (flag == true)
            {
                BrightTogIndicator.Visibility = Visibility.Visible;
            }
        }

        private void UptTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (UptTog.IsOn)
            {
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\DeviceUpdate\\Agent\\Settings", "GuidOfCategoryToScan", Helper.RegistryHelper.RegistryType.REG_SZ, "00000000-0000-0000-0000-000000000000");
            }
            else
            {
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\DeviceUpdate\\Agent\\Settings", "GuidOfCategoryToScan", Helper.RegistryHelper.RegistryType.REG_SZ, "F1E8E1CD-9819-4AC5-B0A7-2AFF3D29B46E");
            }
        }

        private async void SystemUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    await tClient.Send($"reg export HKLM\\System\\Platform\\DeviceTargetingInfo {Helper.localFolder.Path}\\DeviceTargetingInfo.reg");
                }
                string path = "System\\Platform\\DeviceTargetingInfo";
                Button button = sender as Button;
                if (button.Content as string == "Anniversary Update")
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, path, "PhoneManufacturer", Helper.RegistryHelper.RegistryType.REG_SZ, "NOKIA");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, path, "PhoneManufacturerModelName", Helper.RegistryHelper.RegistryType.REG_SZ, "RM-1045_1083");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, path, "PhoneModelName", Helper.RegistryHelper.RegistryType.REG_SZ, "Lumia 930");
                }
                else if (button.Content as string == "Creator Update")
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, path, "PhoneManufacturer", Helper.RegistryHelper.RegistryType.REG_SZ, "NOKIA");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, path, "PhoneManufacturerModelName", Helper.RegistryHelper.RegistryType.REG_SZ, "RM-1096_1002");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, path, "PhoneModelName", Helper.RegistryHelper.RegistryType.REG_SZ, "RM-1096");
                }
                else if (button.Content as string == "Fall Creator Update")
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, path, "PhoneManufacturer", Helper.RegistryHelper.RegistryType.REG_SZ, "MicrosoftMDG");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, path, "PhoneManufacturerModelName", Helper.RegistryHelper.RegistryType.REG_SZ, "RM-1116_11258");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, path, "PhoneModelName", Helper.RegistryHelper.RegistryType.REG_SZ, "Lumia 950 XL");
                }
                await Helper.MessageBox($"Now you can able to update to {button.Content as string} from the Windows Update settings.", Helper.SoundHelper.Sound.Alert, "Success");
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private async void GlanceTog_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (flag == true && GlanceTog.IsOn)
                {
                    if (!await Helper.IsCapabilitiesAllowed())
                    {
                        if (!await Helper.AskCapabilitiesPermission())
                        {
                            GlanceTog.IsOn = false;
                            return;
                        }
                    }
                    Directory.CreateDirectory("C:\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\GlanceScreen\\NsgGlance_NlpmService_4.1.12.4.dll", "C:\\Data\\SharedData\\OEM\\Public\\NsgGlance_NlpmService_4.1.12.4.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\GlanceScreen\\NsgGlance_NlpmServiceImpl_4.1.12.4.dll", "C:\\Data\\SharedData\\OEM\\Public\\NsgGlance_NlpmServiceImpl_4.1.12.4.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\GlanceScreen\\lpmFonts_4.1.12.4\\lpmFont_720P.bin", "C:\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_720P.bin");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\GlanceScreen\\lpmFonts_4.1.12.4\\lpmFont_720P_hi.bin", "C:\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_720P_hi.bin");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\GlanceScreen\\lpmFonts_4.1.12.4\\lpmFont_FHD.bin", "C:\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_FHD.bin");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\GlanceScreen\\lpmFonts_4.1.12.4\\lpmFont_FHD_hi.bin", "C:\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_FHD_hi.bin");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\GlanceScreen\\lpmFonts_4.1.12.4\\lpmFont_WQHD.bin", "C:\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_WQHD.bin");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\GlanceScreen\\lpmFonts_4.1.12.4\\lpmFont_WQHD_hi.bin", "C:\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_WQHD_hi.bin");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\GlanceScreen\\lpmFonts_4.1.12.4\\lpmFont_WVGA.bin", "C:\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_WVGA.bin");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\GlanceScreen\\lpmFonts_4.1.12.4\\lpmFont_WXGA.bin", "C:\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_WXGA.bin");
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\NokiaSvcHost\\Plugins\\NsgGlance\\NlpmService", "PluginPath", 1, Encoding.Unicode.GetBytes("\\Data\\SharedData\\OEM\\Public\\NsgGlance_NlpmServiceImpl_4.1.12.4.dll" + '\0'));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\NokiaSvcHost\\Plugins\\NsgGlance\\NlpmService", "Path", 1, Encoding.Unicode.GetBytes("C:\\Data\\SharedData\\OEM\\Public\\NsgGlance_NlpmService_4.1.12.4.dll" + '\0'));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\NokiaSvcHost\\Plugins\\NsgGlance\\NlpmService", "Version", 1, Encoding.Unicode.GetBytes("4.1.12.4" + '\0'));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\NokiaSvcHost\\Plugins\\NsgGlance\\NlpmService", "PluginVersion", 1, Encoding.Unicode.GetBytes("4.1.12.4" + '\0'));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\NokiaSvcHost\\Plugins\\NsgGlance\\NlpmService", "Enabled", 4, BitConverter.GetBytes(uint.Parse("1")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\NokiaSvcHost\\Plugins\\NsgGlance\\NlpmService", "UsingBeta", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\NokiaSvcHost\\Plugins\\NsgGlance\\NlpmService", "UseBeta", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "AlwaysOnInCharger", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "AppGraphicTimeout", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "BSSwitchOffTimeout", 4, BitConverter.GetBytes(uint.Parse("30")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "DarkMode", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "DarkModeElements", 4, BitConverter.GetBytes(uint.Parse("15")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "DarkModeEnd", 4, BitConverter.GetBytes(uint.Parse("420")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "DarkModeOverrideColor", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "DarkModeStart", 4, BitConverter.GetBytes(uint.Parse("1320")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "DarkModeThreshold", 4, BitConverter.GetBytes(uint.Parse("20000")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "DoubleTapEnabled", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "Enabled", 4, BitConverter.GetBytes(uint.Parse("1")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "MinimizeIcon", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "Mode", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "NormalModeElements", 4, BitConverter.GetBytes(uint.Parse("31")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "SwipeEnabled", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "SwitchOffTimeout", 4, BitConverter.GetBytes(uint.Parse("15")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "PanelType", 4, BitConverter.GetBytes(uint.Parse("1")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "ShowDetailedAppStatus", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "ShowSystemNotifications", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", 1, Encoding.Unicode.GetBytes("\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_wxga.bin" + '\0'));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "AppGraphicGestures", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "SingleTapWakeup", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "EnablePublicSDK", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "SupportedTouchEvents", 4, BitConverter.GetBytes(uint.Parse("0")));
                    rpc.RegSetValueW(1, "SYSTEM\\CurrentControlSet\\Services\\NlpmService", "ImagePath", 2, Encoding.Unicode.GetBytes("C:\\windows\\System32\\OEMServiceHost.exe -k NsgGlance" + '\0'));
                    FontFileBox.SelectedIndex = 3;
                    RestoreGlanceIndicator.Visibility = Visibility.Visible;
                }
                else
                {
                    GlanceTog.IsOn = true;
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private void FontFileBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontFileBox.SelectedIndex == 1) Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ, "\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_WVGA.bin");
            else if (FontFileBox.SelectedIndex == 2) Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ, "\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_720P.bin");
            else if (FontFileBox.SelectedIndex == 3) Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ, "\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_720P_hi.bin");
            else if (FontFileBox.SelectedIndex == 4) Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ, "\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_WXGA.bin");
            else if (FontFileBox.SelectedIndex == 5) Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ, "\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_FHD.bin");
            else if (FontFileBox.SelectedIndex == 6)
            {
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\GlanceScreen\\lpmFonts_4.1.12.4\\lpmFont_FHD_hi.bin", "C:\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_FHD_hi.bin");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ, "\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_FHD_hi.bin");
            }
            else if (FontFileBox.SelectedIndex == 7) Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ, "\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_WQHD.bin");
            else if (FontFileBox.SelectedIndex == 8) Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "FontFile", Helper.RegistryHelper.RegistryType.REG_SZ, "\\Data\\SharedData\\OEM\\Public\\lpmFonts_4.1.12.4\\lpmFont_WQHD_hi.bin");
        }

        private void FontColorTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (FontColorTog.IsOn)
            {
                rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", 4, BitConverter.GetBytes(uint.Parse("16711680")));
                RedRadio.IsChecked = true;
                GlanceColorStack.Visibility = Visibility.Visible;
            }
            else
            {
                rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", 4, BitConverter.GetBytes(uint.Parse("0")));
                GlanceAutoColor.SelectedIndex = 0;
                RedRadio.IsChecked = false;
                GreenRadio.IsChecked = false;
                BlueRadio.IsChecked = false;
                CyanRadio.IsChecked = false;
                MagentaRadio.IsChecked = false;
                YellowRadio.IsChecked = false;
                GlanceColorStack.Visibility = Visibility.Collapsed;
            }
        }

        private void GlanceAutoColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flag)
                Helper.LocalSettingsHelper.SaveSettings("GlanceAutoColorEnabled", GlanceAutoColor.SelectedIndex);
        }

        private void FontColor_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.Content as string == "Red") rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", 4, BitConverter.GetBytes(uint.Parse("16711680")));
            else if (rb.Content as string == "Green") rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", 4, BitConverter.GetBytes(uint.Parse("65280")));
            else if (rb.Content as string == "Blue") rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", 4, BitConverter.GetBytes(uint.Parse("255")));
            else if (rb.Content as string == "Cyan") rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", 4, BitConverter.GetBytes(uint.Parse("65535")));
            else if (rb.Content as string == "Magenta") rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", 4, BitConverter.GetBytes(uint.Parse("16711935")));
            else if (rb.Content as string == "Yellow") rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", 4, BitConverter.GetBytes(uint.Parse("16776960")));
        }

        private void MoveClockTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (MoveClockTog.IsOn)
            {
                rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "MoveClock", 4, BitConverter.GetBytes(uint.Parse("1")));
            }
            else
            {
                rpc.RegSetValueW(1, "SOFTWARE\\OEM\\Nokia\\lpm", "MoveClock", 4, BitConverter.GetBytes(uint.Parse("0")));
            }
        }

        private async void AutoWallTog_Toggled(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AutoWallTog.IsOn)
                {
                    if (flag == true)
                    {
                        Helper.LocalSettingsHelper.SaveSettings("BackupCurrentWall", Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Shell\\Wallpaper", "CurrentWallpaper", Helper.RegistryHelper.RegistryType.REG_SZ));
                        var currentLibrary = await StorageFolder.GetFolderFromPathAsync((await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[0]);
                        Helper.CopyFile((await currentLibrary.GetFilesAsync())[0].Path, $"{Helper.localFolder.Path}\\{System.IO.Path.GetFileName((await currentLibrary.GetFilesAsync())[0].Path)}");
                        UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                        await profileSettings.TrySetLockScreenImageAsync(await Helper.localFolder.GetFileAsync($"{System.IO.Path.GetFileName((await currentLibrary.GetFilesAsync())[0].Path)}"));
                        rpc.FileCopy(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\LiveLockscreen.bat", Helper.localFolder.Path + "\\LiveLockscreen.bat", 0);
                        _ = tClient.Send("start " + Helper.localFolder.Path + "\\LiveLockscreen.bat");
                    }
                }
                else
                {
                    if (File.Exists($"{Helper.localFolder.Path}\\LiveLockscreen.bat"))
                    {
                        File.Delete($"{Helper.localFolder.Path}\\LiveLockscreen.bat");
                    }
                    if (flag == true)
                    {
                        if (Helper.LocalSettingsHelper.LoadSettings("BackupCurrentWall", null) != null)
                        {
                            Helper.CopyFile(Helper.LocalSettingsHelper.LoadSettings("BackupCurrentWall", null), $"{Helper.localFolder.Path}\\{System.IO.Path.GetFileName(Helper.LocalSettingsHelper.LoadSettings("BackupCurrentWall", null))}");
                            UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                            await profileSettings.TrySetLockScreenImageAsync(await Helper.localFolder.GetFileAsync($"{System.IO.Path.GetFileName(Helper.LocalSettingsHelper.LoadSettings("BackupCurrentWall", null))}"));
                            Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Shell\\Wallpaper", "CurrentWallpaper", Helper.RegistryHelper.RegistryType.REG_SZ, Helper.LocalSettingsHelper.LoadSettings("BackupCurrentWall", null));
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }

        private async void AutoWallCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (flag == true)
                {
                    if (WallCollectionCombo.SelectedIndex == 3)
                    {
                        WallCollectionStack.Visibility = Visibility.Visible;
                        await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{Helper.installedLocation.Path}\\Assets\\Images\\Lockscreens\\Stripes\n15000\nFalse\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[3]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[4]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[5]}");
                    }
                    else
                    {
                        WallCollectionStack.Visibility = Visibility.Collapsed;
                        if (WallCollectionCombo.SelectedIndex == 0)
                        {
                            await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{Helper.installedLocation.Path}\\Assets\\Images\\Lockscreens\\RedMoon\n65\nTrue\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[3]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[4]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[5]}");
                        }
                        else if (WallCollectionCombo.SelectedIndex == 1)
                        {
                            await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{Helper.installedLocation.Path}\\Assets\\Images\\Lockscreens\\Flowers\n60\nTrue\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[3]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[4]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[5]}");
                        }
                        else if (WallCollectionCombo.SelectedIndex == 2)
                        {
                            await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{Helper.installedLocation.Path}\\Assets\\Images\\Lockscreens\\Timelapse\n70\nFalse\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[3]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[4]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[5]}");
                        }
                    }
                    if ((await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[0].Contains(Helper.localFolder.Path))
                    {
                        WallCollectionBox.Text = $"CMDInjector:\\Library\\{System.IO.Path.GetFileName((await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[0])}";
                    }
                    else if ((await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[0].Contains(Helper.installedLocation.Path))
                    {
                        WallCollectionBox.Text = $"CMDInjector:\\Assets\\Images\\Lockscreens\\{System.IO.Path.GetFileName((await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[0])}";
                    }
                    else
                    {
                        WallCollectionBox.Text = (await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[0];
                    }
                    WallIntervalBox.Text = (await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[1];
                    if ((await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[2] == "True")
                    {
                        WallRevLoopTog.IsOn = true;
                    }
                    else
                    {
                        WallRevLoopTog.IsOn = false;
                    }
                    if (AutoWallTog.IsOn)
                    {
                        var currentLibrary = await StorageFolder.GetFolderFromPathAsync((await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[0]);
                        Helper.CopyFile((await currentLibrary.GetFilesAsync())[0].Path, $"{Helper.localFolder.Path}\\{System.IO.Path.GetFileName((await currentLibrary.GetFilesAsync())[0].Path)}");
                        UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                        await profileSettings.TrySetLockScreenImageAsync(await Helper.localFolder.GetFileAsync($"{System.IO.Path.GetFileName((await currentLibrary.GetFilesAsync())[0].Path)}"));
                        await Helper.localFolder.CreateFileAsync("LockscreenBreak.txt", CreationCollisionOption.OpenIfExists);
                    }
                }
            }
            catch (Exception ex) { }
        }

        private async void LibraryBrowse_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WallCollectionBtn.IsEnabled = false;
                MenuFlyoutItem clickedItem = (MenuFlyoutItem)sender;
                if (clickedItem.Text == "Zip File")
                {
                    var filePicker = new FileOpenPicker
                    {
                        SuggestedStartLocation = PickerLocationId.ComputerFolder
                    };
                    filePicker.FileTypeFilter.Add(".zip");
                    StorageFile zipFile = await filePicker.PickSingleFileAsync();
                    if (zipFile != null)
                    {
                        ZipFileProg.Visibility = Visibility.Visible;
                        ZipFileProg.IsIndeterminate = true;
                        await Task.Run(async () =>
                        {
                            try
                            {
                                StorageFolder libraryFolder = await Helper.localFolder.CreateFolderAsync("Library", CreationCollisionOption.OpenIfExists);
                                bool isCacheExist = false;
                                IReadOnlyList<StorageFolder> oldCaches = null;
                                if (!Directory.Exists($"{libraryFolder.Path}\\{System.IO.Path.GetFileNameWithoutExtension(zipFile.Path)}"))
                                {
                                    oldCaches = await libraryFolder.GetFoldersAsync();
                                    isCacheExist = true;
                                }
                                using (Stream zipStream = await zipFile.OpenStreamForReadAsync())
                                using (ZipArchive archive = new ZipArchive(zipStream))
                                {
                                    foreach (var entry in archive.Entries)
                                    {
                                        if (System.IO.Path.GetExtension(entry.FullName).ToLower() == ".jpeg" || System.IO.Path.GetExtension(entry.FullName).ToLower() == ".jpg" || System.IO.Path.GetExtension(entry.FullName).ToLower() == ".png")
                                        {
                                            await libraryFolder.CreateFolderAsync($"{System.IO.Path.GetFileNameWithoutExtension(zipFile.Path)}", CreationCollisionOption.OpenIfExists);
                                            entry.ExtractToFile($"{libraryFolder.Path}\\{System.IO.Path.GetFileNameWithoutExtension(zipFile.Path)}\\{entry.FullName}", true);
                                        }
                                    }
                                }
                                if (Directory.Exists($"{libraryFolder.Path}\\{System.IO.Path.GetFileNameWithoutExtension(zipFile.Path)}"))
                                {
                                    await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{libraryFolder.Path}\\{System.IO.Path.GetFileNameWithoutExtension(zipFile.Path)}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[1]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[2]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[3]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[4]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[5]}");
                                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        WallCollectionBox.Text = $"CMDInjector:\\Library\\{System.IO.Path.GetFileNameWithoutExtension(zipFile.Path)}";
                                    });
                                    //archive.ExtractToDirectory($"{libraryFolder.Path}\\{System.IO.Path.GetFileNameWithoutExtension(zipFile.Path)}");
                                    await Helper.localFolder.CreateFileAsync("LockscreenBreak.txt", CreationCollisionOption.OpenIfExists);
                                    if (isCacheExist)
                                    {
                                        foreach (var oldCache in oldCaches)
                                        {
                                            Directory.Delete(oldCache.Path, true);
                                        }
                                    }
                                }
                                else
                                {
                                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        _ = Helper.MessageBox("The selected zip file does not contain any image files.", Helper.SoundHelper.Sound.Error);
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                                {
                                    if (ex.Message == "End of Central Directory record could not be found.")
                                    {
                                        _ = Helper.MessageBox("The selected file is corrupted or not a zip file.", Helper.SoundHelper.Sound.Error);
                                    }
                                    else
                                    {
                                        Helper.ThrowException(ex);
                                        await Helper.localFolder.CreateFileAsync("LockscreenBreak.txt", CreationCollisionOption.OpenIfExists);
                                    }
                                });
                            }
                        });
                        ZipFileProg.Visibility = Visibility.Collapsed;
                        ZipFileProg.IsIndeterminate = false;
                    }
                }
                else
                {
                    var folderPicker = new FolderPicker
                    {
                        SuggestedStartLocation = PickerLocationId.ComputerFolder
                    };
                    folderPicker.FileTypeFilter.Add(".jpeg");
                    folderPicker.FileTypeFilter.Add(".jpg");
                    folderPicker.FileTypeFilter.Add(".png");
                    StorageFolder wallFolder = await folderPicker.PickSingleFolderAsync();
                    if (wallFolder != null)
                    {
                        bool isImagePresent = false;
                        foreach (var file in await wallFolder.GetFilesAsync())
                        {
                            if (System.IO.Path.GetExtension(file.Path) == ".jpeg" || System.IO.Path.GetExtension(file.Path) == ".jpg" || System.IO.Path.GetExtension(file.Path) == ".png")
                            {
                                isImagePresent = true;
                                break;
                            }
                        }
                        if (isImagePresent)
                        {
                            WallCollectionBox.Text = wallFolder.Path;
                            await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{WallCollectionBox.Text}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[1]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[2]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[3]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[4]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[5]}");
                            await Helper.localFolder.CreateFileAsync("LockscreenBreak.txt", CreationCollisionOption.OpenIfExists);
                        }
                        else
                        {
                            _ = Helper.MessageBox("The selected folder does not contain any image files.", Helper.SoundHelper.Sound.Error);
                        }
                    }
                }
                WallCollectionBtn.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }

        private async void WallIntervalBtn_Click(object sender, RoutedEventArgs e)
        {
            await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[0]}\n{WallIntervalBox.Text}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[2]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[3]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[4]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[5]}");
            await Helper.localFolder.CreateFileAsync("LockscreenBreak.txt", CreationCollisionOption.OpenIfExists);
        }

        private void WallIntervalBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (WallIntervalBox.Text != string.Empty && !WallIntervalBox.Text.Contains('.') && WallIntervalBox.Text != "0" && WallIntervalBox.Text != "00" && WallIntervalBox.Text != "000" && WallIntervalBox.Text != "0000" && WallIntervalBox.Text != "00000" && Convert.ToInt32(WallIntervalBox.Text) >= 50 && Convert.ToInt32(WallIntervalBox.Text) <= 60000)
            {
                WallIntervalBtn.IsEnabled = true;
            }
            else
            {
                WallIntervalBtn.IsEnabled = false;
            }
        }

        private async void WallRevLoopTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (WallRevLoopTog.IsOn)
            {
                await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[0]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[1]}\nTrue\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[3]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[4]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[5]}");
            }
            else
            {
                await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[0]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[1]}\nFalse\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[3]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[4]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[5]}");
            }
            await Helper.localFolder.CreateFileAsync("LockscreenBreak.txt", CreationCollisionOption.OpenIfExists);
        }

        private async void ActiveHoursTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (flag == true)
            {
                if (ActiveHoursTog.IsOn)
                {
                    await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[0]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[1]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[2]}\n{new DateTime(StartTimePkr.Time.Ticks).ToString("HH:mm")}\n{new DateTime(StopTimePkr.Time.Ticks).ToString("HH:mm")}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[5]}");
                    await Helper.localFolder.CreateFileAsync("LockscreenBreak.txt", CreationCollisionOption.OpenIfExists);
                    Helper.LocalSettingsHelper.SaveSettings("ActiveHours", true);
                    ActiveHoursStack.Visibility = Visibility.Visible;
                }
                else
                {
                    await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[0]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[1]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[2]}\n00:00\n00:00\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[5]}");
                    await Helper.localFolder.CreateFileAsync("LockscreenBreak.txt", CreationCollisionOption.OpenIfExists);
                    Helper.LocalSettingsHelper.SaveSettings("ActiveHours", false);
                    ActiveHoursStack.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void LockscreenTimePkr_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (flag == true)
            {
                await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[0]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[1]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[2]}\n{new DateTime(StartTimePkr.Time.Ticks).ToString("HH:mm")}\n{new DateTime(StopTimePkr.Time.Ticks).ToString("HH:mm")}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[5]}");
                await Helper.localFolder.CreateFileAsync("LockscreenBreak.txt", CreationCollisionOption.OpenIfExists);
            }
        }

        private void BootAnimTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (BootAnimTog.IsOn)
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{7ea2e1ac-2e61-4728-aaa3-896d9d0a9f0e}\\Elements\\16000069", "Element", 3, ToBinary("00"));
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{7ea2e1ac-2e61-4728-aaa3-896d9d0a9f0e}\\Elements\\1600007a", "Element", 3, ToBinary("00"));
            }
            else
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{7ea2e1ac-2e61-4728-aaa3-896d9d0a9f0e}\\Elements\\16000069", "Element", 3, ToBinary("01"));
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{7ea2e1ac-2e61-4728-aaa3-896d9d0a9f0e}\\Elements\\1600007a", "Element", 3, ToBinary("01"));
            }
        }

        private void BootImageTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (BootImageTog.IsOn)
            {
                BootImageStack.Visibility = Visibility.Visible;
                if (flag == true)
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpbootscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ, Helper.installedLocation.Path + "\\Assets\\Images\\Bootscreens\\BootupImage.png");
                }
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpbootscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ).Contains(Helper.installedLocation.Path))
                {
                    BootImageBox.Text = $"CMDInjector:\\Assets\\Images\\Bootscreens\\BootupImage.png";
                }
                else
                {
                    BootImageBox.Text = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpbootscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ);
                }
            }
            else
            {
                BootImageStack.Visibility = Visibility.Collapsed;
                reg.DeleteValue(RegistryHive.HKLM, "System\\Shell\\OEM\\bootscreens", "wpbootscreenoverride");
            }
        }

        private async void BootImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var folderOpenPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderOpenPicker.FileTypeFilter.Add(".png");
            folderOpenPicker.FileTypeFilter.Add(".jpg");
            StorageFile bootImage = await folderOpenPicker.PickSingleFileAsync();
            if (bootImage == null)
            {
                return;
            }
            BootImageBox.Text = bootImage.Path;
            Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpbootscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ, bootImage.Path);
        }

        private void ShutdownImageTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (ShutdownImageTog.IsOn)
            {
                ShutdownImageStack.Visibility = Visibility.Visible;
                if (flag == true)
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpshutdownscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ, Helper.installedLocation.Path + "\\Assets\\Images\\Bootscreens\\ShutdownImage.png");
                }
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpshutdownscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ).Contains(Helper.installedLocation.Path))
                {
                    ShutdownImageBox.Text = $"CMDInjector:\\Assets\\Images\\Bootscreens\\ShutdownImage.png";
                }
                else
                {
                    ShutdownImageBox.Text = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpshutdownscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ);
                }
            }
            else
            {
                ShutdownImageStack.Visibility = Visibility.Collapsed;
                reg.DeleteValue(RegistryHive.HKLM, "System\\Shell\\OEM\\bootscreens", "wpshutdownscreenoverride");
            }
        }

        private async void ShutdownImageBtn_Click(object sender, RoutedEventArgs e)
        {
            var folderOpenPicker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderOpenPicker.FileTypeFilter.Add(".png");
            folderOpenPicker.FileTypeFilter.Add(".jpg");
            StorageFile shutdownImage = await folderOpenPicker.PickSingleFileAsync();
            if (shutdownImage == null)
            {
                return;
            }
            ShutdownImageBox.Text = shutdownImage.Path;
            Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "System\\Shell\\OEM\\bootscreens", "wpshutdownscreenoverride", Helper.RegistryHelper.RegistryType.REG_SZ, shutdownImage.Path);
        }

        private void SoftNavTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (SoftNavTog.IsOn)
            {
                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "SoftwareModeEnabled", 4, BitConverter.GetBytes(uint.Parse("1")));
            }
            else
            {
                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "SoftwareModeEnabled", 4, BitConverter.GetBytes(uint.Parse("0")));
            }
            if (flag == true)
            {
                SoftwareModeIndicator.Visibility = Visibility.Visible;
            }
        }

        private void DoubleTapTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (DoubleTapTog.IsOn)
            {

                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsDoubleTapOffEnabled", 4, BitConverter.GetBytes(uint.Parse("1")));
            }
            else
            {

                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsDoubleTapOffEnabled", 4, BitConverter.GetBytes(uint.Parse("0")));
            }
            if (flag == true)
            {
                DoubleTapIndicator.Visibility = Visibility.Visible;
            }
        }

        private void AutoHideTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (AutoHideTog.IsOn)
            {
                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsAutoHideEnabled", 4, BitConverter.GetBytes(uint.Parse("1")));
            }
            else
            {
                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsAutoHideEnabled", 4, BitConverter.GetBytes(uint.Parse("0")));
            }
            if (flag == true)
            {
                AutoHideIndicator.Visibility = Visibility.Visible;
            }
        }

        private void SwipeUpTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (SwipeUpTog.IsOn)
            {
                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsSwipeUpToHideEnabled", 4, BitConverter.GetBytes(uint.Parse("1")));
            }
            else
            {
                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsSwipeUpToHideEnabled", 4, BitConverter.GetBytes(uint.Parse("0")));
            }
            if (flag == true)
            {
                SwipeUpIndicator.Visibility = Visibility.Visible;
            }
        }

        private void UserManagedTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (UserManagedTog.IsOn)
            {
                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsUserManaged", 4, BitConverter.GetBytes(uint.Parse("1")));
            }
            else
            {
                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsUserManaged", 4, BitConverter.GetBytes(uint.Parse("0")));
            }
            if (flag == true)
            {
                UserManagedIndicator.Visibility = Visibility.Visible;
            }
        }

        private void BurninProtTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (BurninProtTog.IsOn)
            {
                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsBurnInProtectionEnabled", 4, BitConverter.GetBytes(uint.Parse("1")));
                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionMaskSwitchingInterval", 4, BitConverter.GetBytes(uint.Parse("1")));
            }
            else
            {
                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "IsBurnInProtectionEnabled", 4, BitConverter.GetBytes(uint.Parse("0")));
            }
            if (flag == true)
            {
                BurnInIndicator.Visibility = Visibility.Visible;
            }
        }

        private void BurninTimeoutBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BurninTimeoutBox.Text == string.Empty)
            {
                BurninTimeoutBtn.IsEnabled = false;
            }
            else
            {
                BurninTimeoutBtn.IsEnabled = true;
            }
        }

        private void BurninTimeoutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (BurninTimeoutBox.Text != string.Empty)
            {
                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionIdleTimerTimeout", 4, BitConverter.GetBytes(uint.Parse(BurninTimeoutBox.Text)));
                BurnInTimeoutIndicator.Visibility = Visibility.Visible;
            }
        }

        private void ColorPickCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flag == true && ColorPickCombo.SelectedIndex != 0)
            {
                ComboBoxItem cbi = ColorPickCombo.SelectedItem as ComboBoxItem;
                StackPanel stackPanel = cbi.Content as StackPanel;
                Brush selectedColor = (stackPanel.Children[0] as Rectangle).Fill;
                SolidColorBrush solidColor = (SolidColorBrush)selectedColor;
                string hexColor = solidColor.Color.ToString().Remove(0, 3);
                int decimalColor = Convert.ToInt32(hexColor, 16);
                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionBlackReplacementColor", 4, BitConverter.GetBytes(uint.Parse(decimalColor.ToString())));
                BurnInColorIndicator.Visibility = Visibility.Visible;
            }
        }

        private void OpacitySlide_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender is Slider slide)
            {
                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Shell\\NavigationBar", "BurnInProtectionIconsOpacity", 4, BitConverter.GetBytes(uint.Parse(slide.Value.ToString())));
            }
            if (flag == true)
            {
                BurnInOpacityIndicator.Visibility = Visibility.Visible;
            }
        }

        private void DumpTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps", "DumpType", Helper.RegistryHelper.RegistryType.REG_DWORD, "0000000" + DumpTypeCombo.SelectedIndex);
        }

        private void DumpCountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps", "DumpCount", 4, BitConverter.GetBytes(uint.Parse(((ComboBoxItem)DumpCountCombo.SelectedItem).Content.ToString())));
        }

        private async void DumpFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder dumpFolder = await folderPicker.PickSingleFolderAsync();
            if (dumpFolder != null)
            {
                rpc.RegSetValueW(1, "SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps", "DumpFolder", 7, Encoding.Unicode.GetBytes(dumpFolder.Path + '\0'));
                DumpFolderBox.Text = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\LocalDumps", "DumpFolder", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ);
            }
        }

        private void TileCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TileCombo.SelectedIndex == 0) rpc.RegSetValueW(1, "Software\\Microsoft\\Shell\\Start", "TileColumnSize", 4, BitConverter.GetBytes(uint.Parse("4")));
            else if (TileCombo.SelectedIndex == 1) rpc.RegSetValueW(1, "Software\\Microsoft\\Shell\\Start", "TileColumnSize", 4, BitConverter.GetBytes(uint.Parse("6")));
            else if (TileCombo.SelectedIndex == 2) rpc.RegSetValueW(1, "Software\\Microsoft\\Shell\\Start", "TileColumnSize", 4, BitConverter.GetBytes(uint.Parse("8")));
            else if (TileCombo.SelectedIndex == 3) rpc.RegSetValueW(1, "Software\\Microsoft\\Shell\\Start", "TileColumnSize", 4, BitConverter.GetBytes(uint.Parse("10")));
            else if (TileCombo.SelectedIndex == 4) rpc.RegSetValueW(1, "Software\\Microsoft\\Shell\\Start", "TileColumnSize", 4, BitConverter.GetBytes(uint.Parse("12")));
            if (flag == true)
            {
                StartTileIndicator.Visibility = Visibility.Visible;
            }
        }

        private void VirtualMemBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (VirtualMemBox.Text == string.Empty || string.IsNullOrWhiteSpace(VirtualMemBox.Text))
            {
                VirtualMemBtn.IsEnabled = false;
            }
            else
            {
                VirtualMemBtn.IsEnabled = true;
            }
        }

        private void DngTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (DngTog.IsOn)
            {
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\NOKIA\\Camera\\Barc", "DNGDisabled", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000000");
            }
            else
            {
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\NOKIA\\Camera\\Barc", "DNGDisabled", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
            }
        }

        private void VirtualMemBtn_Click(object sender, RoutedEventArgs e)
        {
            if (VirtualMemBox.Text != string.Empty)
            {
                rpc.RegSetValueW(1, "System\\CurrentControlSet\\Control\\Session Manager\\Memory Management", "PagingFiles", 7, Encoding.Unicode.GetBytes(VirtualMemBox.Text + '\0'));
                VirtualMemoryIndicator.Visibility = Visibility.Visible;
            }
        }

        private byte[] ToBinary(string data)
        {
            data = data.Replace("-", "");
            return Enumerable.Range(0, data.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(data.Substring(x, 2), 16)).ToArray();
        }

        private void BackgModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BackgModeCombo.SelectedIndex == 0)
            {
                /*_ = tClient.Send("reg add HKLM\\Software\\Microsoft\\Windows\\Currentversion\\Themes\\Personalize /v SystemUsesLightTheme /t REG_DWORD /d 0 /f" +
                    "&reg add HKLM\\Software\\Microsoft\\Windows\\Currentversion\\Themes\\Personalize /v AppsUseLightTheme /t REG_DWORD /d 0 /f" +
                    "&reg add \"HKLM\\Software\\Microsoft\\Windows\\Currentversion\\Control Panel\\Theme\" /v CurrentTheme /t REG_DWORD /d 1 /f");*/
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "SystemUsesLightTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000000");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000000");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Control Panel\\Theme", "CurrentTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                if (AutoBackgCombo.SelectedIndex == 1)
                {
                    AutoBackgCombo.SelectedIndex = -1;
                    AutoBackgItem.Content = "Dark Untill Sunrise";
                    AutoBackgCombo.SelectedIndex = 1;
                }
                else
                {
                    AutoBackgItem.Content = "Dark Untill Sunrise";
                }
            }
            else
            {
                /*_ = tClient.Send("reg add HKLM\\Software\\Microsoft\\Windows\\Currentversion\\Themes\\Personalize /v SystemUsesLightTheme /t REG_DWORD /d 1 /f" +
                    "&reg add HKLM\\Software\\Microsoft\\Windows\\Currentversion\\Themes\\Personalize /v AppsUseLightTheme /t REG_DWORD /d 1 /f" +
                    "&reg add \"HKLM\\Software\\Microsoft\\Windows\\Currentversion\\Control Panel\\Theme\" /v CurrentTheme /t REG_DWORD /d 0 /f");*/
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "SystemUsesLightTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Control Panel\\Theme", "CurrentTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000000");
                if (AutoBackgCombo.SelectedIndex == 1)
                {
                    AutoBackgCombo.SelectedIndex = -1;
                    AutoBackgItem.Content = "Light Untill Sunset";
                    AutoBackgCombo.SelectedIndex = 1;
                }
                else
                {
                    AutoBackgItem.Content = "Light Untill Sunset";
                }
            }
            if (!Helper.LocalSettingsHelper.LoadSettings("ThemeSettings", false))
            {
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

        private void AutoBackgCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AutoBackgCombo.SelectedIndex == 0)
            {
                BackgShiftStack.Visibility = Visibility.Collapsed;
            }
            else if (AutoBackgCombo.SelectedIndex == 1)
            {
                BackgShiftStack.Visibility = Visibility.Collapsed;
                Helper.LocalSettingsHelper.SaveSettings("AutoThemeLight", "06:00");
                Helper.LocalSettingsHelper.SaveSettings("AutoThemeDark", "18:00");
                BackgStartTime.Time = new TimeSpan(06, 00, 00);
                BackgStopTime.Time = new TimeSpan(18, 00, 00);
            }
            else if (AutoBackgCombo.SelectedIndex == 2)
            {
                BackgShiftStack.Visibility = Visibility.Visible;
            }
            Helper.LocalSettingsHelper.SaveSettings("AutoThemeMode", AutoBackgCombo.SelectedIndex);
        }

        private void ThemeBackgTime_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            if (flag == true)
            {
                Helper.LocalSettingsHelper.SaveSettings("AutoThemeLight", new DateTime(BackgStartTime.Time.Ticks).ToString("HH:mm"));
                Helper.LocalSettingsHelper.SaveSettings("AutoThemeDark", new DateTime(BackgStopTime.Time.Ticks).ToString("HH:mm"));
            }
        }

        private async void AccentColorCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (AccentColorCombo.SelectedIndex != 0)
                {
                    var cbi = AccentColorCombo.SelectedItem as ComboBoxItem;
                    var stackPanel = cbi.Content as StackPanel;
                    var selectedColor = (stackPanel.Children[0] as Rectangle).Fill;
                    var solidColor = (SolidColorBrush)selectedColor;
                    var col = Color.FromArgb(255, solidColor.Color.R, solidColor.Color.G, solidColor.Color.B);
                    var CurrentAccentHex = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, @"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme", "CurrentAccent", Helper.RegistryHelper.RegistryType.REG_DWORD);
                    var CurrentAccent = int.Parse(CurrentAccentHex, System.Globalization.NumberStyles.HexNumber);
                    for (int i = 0; i <= 1; i++)
                    {
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, $@"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme\Themes\{i}\Accents\{CurrentAccent.ToString()}", "Color", Helper.RegistryHelper.RegistryType.REG_DWORD, col.ToString().Replace("#", string.Empty));
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, $@"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme\Themes\{i}\Accents\{CurrentAccent.ToString()}", "ComplementaryColor", Helper.RegistryHelper.RegistryType.REG_DWORD, col.ToString().Replace("#", string.Empty));
                        //Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Control Panel\Theme\Themes", "SpecialColor", Helper.RegistryHelper.RegistryType.REG_DWORD, Convert.ToInt32(col.R.ToString("X2") + col.G.ToString("X2") + col.B.ToString("X2"), 16).ToString());
                    }
                    var regvalue = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, @"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme", "AccentPalette", Helper.RegistryHelper.RegistryType.REG_BINARY);
                    var array = regvalue.ToCharArray();

                    array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(0), 24);
                    array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(1), 25);
                    array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(2), 26);
                    array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(3), 27);
                    array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(4), 28);
                    array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(5), 29);

                    var newpalette = string.Join("", array);
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, @"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme", "AccentPalette", Helper.RegistryHelper.RegistryType.REG_BINARY, newpalette);
                    await Task.Delay(200);
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
                    AccentColorTwoCombo.SelectedIndex = AccentColorCombo.SelectedIndex;
                    AccentColorCombo.IsEnabled = false;
                    AccentColorTwoCombo.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }

        private void AccentColorTwoCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (AccentColorTwoCombo.SelectedIndex == 0) return;
                var cbi = AccentColorTwoCombo.SelectedItem as ComboBoxItem;
                var stackPanel = cbi.Content as StackPanel;
                var selectedColor = (stackPanel.Children[0] as Rectangle).Fill;
                var solidColor = (SolidColorBrush)selectedColor;
                var col = Color.FromArgb(255, solidColor.Color.R, solidColor.Color.G, solidColor.Color.B);
                var CurrentAccentHex = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, @"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme", "CurrentAccent", Helper.RegistryHelper.RegistryType.REG_DWORD);
                var CurrentAccent = int.Parse(CurrentAccentHex, System.Globalization.NumberStyles.HexNumber);
                for (int i = 0; i <= 1; i++)
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, $@"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme\Themes\{i}\Accents\{CurrentAccent.ToString()}", "Color", Helper.RegistryHelper.RegistryType.REG_DWORD, col.ToString().Replace("#", string.Empty));
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, $@"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme\Themes\{i}\Accents\{CurrentAccent.ToString()}", "ComplementaryColor", Helper.RegistryHelper.RegistryType.REG_DWORD, col.ToString().Replace("#", string.Empty));
                    //Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Control Panel\Theme\Themes", "SpecialColor", 4, Convert.ToInt32(col.R.ToString("X2") + col.G.ToString("X2") + col.B.ToString("X2"), 16).ToString(), 0);
                }
                var regvalue = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, @"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme", "AccentPalette", Helper.RegistryHelper.RegistryType.REG_BINARY);
                var array = regvalue.ToCharArray();

                array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(0), 24);
                array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(1), 25);
                array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(2), 26);
                array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(3), 27);
                array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(4), 28);
                array.SetValue(col.ToString().Replace("#", string.Empty).ToCharArray().GetValue(5), 29);

                var newpalette = string.Join("", array);
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, @"Software\Microsoft\Windows\CurrentVersion\Control Panel\Theme", "AccentPalette", Helper.RegistryHelper.RegistryType.REG_BINARY, newpalette);
            }
            catch (Exception ex)
            {
                //Helper.ThrowException(ex);
            }
        }

        private async void AccentColorInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Helper.build < 15063)
            {
                await Helper.MessageBox("Two accent color properties are available:\n1. The primary color used by UWP apps.\n2. The secondary color used by Silverlight apps.\n\nThe secondary color will change as well when you change the primary color. To select a secondary color you must first select a primary color. Once the primary color is selected, you need to restart this App to change the primary color again.", Helper.SoundHelper.Sound.Alert, "Info");
            }
            else
            {
                TextBlock resultBlock = new TextBlock()
                {
                    Text = "Two accent color properties are available:\n1. The primary color used by UWP apps.\n2. The secondary color used by Silverlight apps.\n\nThe secondary color will change as well when you change the primary color. To select a secondary color you must first select a primary color. Once the primary color is selected, you need to restart this App to change the primary color again.",
                    TextWrapping = TextWrapping.Wrap
                };
                ScrollViewer resultScrollViewer = new ScrollViewer() { Content = resultBlock };
                ContentDialog resultDialog = new ContentDialog()
                {
                    Content = resultScrollViewer,
                    Title = "Info",
                    CloseButtonText = "OK",
                };
                Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                await resultDialog.ShowAsync();
            }
        }

        private void NightModeSlide_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider slider = sender as Slider;
            rpc.RegSetValueW(1, @"SOFTWARE\OEM\Nokia\Display\ColorAndLight", "UserSettingWhitePoint", 4, BitConverter.GetBytes(uint.Parse(Convert.ToInt32(Math.Round(slider.Value / 100 * (63 - 32) + (63 - 32) + 1).ToString(), 16).ToString())));
            rpc.RegSetValueW(1, @"SOFTWARE\OEM\Nokia\Display\ColorAndLight", "UserSettingNightLightPct", 1, Encoding.Unicode.GetBytes($"0,{Math.Round(slider.Value / 100 * (63 - 32) + (63 - 32) + 1)}" + '\0'));
        }

        private async Task Restart()
        {
            try
            {
                var result = await Helper.MessageBox("Are you sure you want to restart the device?", Helper.SoundHelper.Sound.Alert, "", "No", true, "Yes");
                if (result == 0)
                {
                    Helper.RebootSystem();
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private async Task Shutdown()
        {
            try
            {
                var result = await Helper.MessageBox("Are you sure you want to shutdown the device?", Helper.SoundHelper.Sound.Alert, "", "No", true, "Yes");
                if (result == 0)
                {
                    if (tClient.IsConnected && HomeHelper.IsConnected())
                    {
                        _ = tClient.Send("shutdown /s /t 0");
                    }
                    else
                    {
                        _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                    }
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private void ShutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (buttonOnHold)
            {
                buttonOnHold = false;
                return;
            }
            _ = Shutdown();
        }

        private void RestartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (buttonOnHold)
            {
                buttonOnHold = false;
                return;
            }
            _ = Restart();
        }

        private async void RestartBtn_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
                buttonOnHold = true;
                if (e.HoldingState != Windows.UI.Input.HoldingState.Started) return;
                SecondaryTile RestartTile = new SecondaryTile("RestartTileID", "Restart", "Restart", new Uri("ms-appx:///Assets/Icons/PowerOptions/RestartPowerOptionTileLogo.png"), TileSize.Default);
                RestartTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                RestartTile.VisualElements.ShowNameOnWide310x150Logo = true;
                RestartTile.VisualElements.ShowNameOnSquare310x310Logo = true;
                bool isPinned = await RestartTile.RequestCreateAsync();
                if (isPinned)
                {
                    TipText.Visibility = Visibility.Collapsed;
                    Helper.LocalSettingsHelper.SaveSettings("TipSettings", false);
                }
            }
            catch (Exception ex) { /*Helper.ThrowException(ex);*/ }
        }

        private async void ShutBtn_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
                buttonOnHold = true;
                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    SecondaryTile ShutdownTile = new SecondaryTile("ShutdownTileID", "Shutdown", "Shutdown", new Uri("ms-appx:///Assets/Icons/PowerOptions/ShutdownPowerOptionTileLogo.png"), TileSize.Default);
                    ShutdownTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                    ShutdownTile.VisualElements.ShowNameOnWide310x150Logo = true;
                    ShutdownTile.VisualElements.ShowNameOnSquare310x310Logo = true;
                    bool isPinned = await ShutdownTile.RequestCreateAsync();
                    if (isPinned)
                    {
                        TipText.Visibility = Visibility.Collapsed;
                        Helper.LocalSettingsHelper.SaveSettings("TipSettings", false);
                    }
                }
                else
                {
                    _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                }
            }
            catch (Exception ex) { /*Helper.ThrowException(ex);*/ }
        }

        private async void FFULoaderBtn_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
                buttonOnHold = true;
                if (e.HoldingState != Windows.UI.Input.HoldingState.Started) return;
                if (tClient.IsConnected && HomeHelper.IsConnected() && File.Exists(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe"))
                {
                    SecondaryTile FFULoaderTile = new SecondaryTile("FFULoaderTileID", "FFU Loader", "FFULoader", new Uri("ms-appx:///Assets/Icons/PowerOptions/FFULoaderPowerOptionTileLogo.png"), TileSize.Default);
                    FFULoaderTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                    FFULoaderTile.VisualElements.ShowNameOnWide310x150Logo = true;
                    FFULoaderTile.VisualElements.ShowNameOnSquare310x310Logo = true;
                    bool isPinned = await FFULoaderTile.RequestCreateAsync();
                    if (isPinned)
                    {
                        TipText.Visibility = Visibility.Collapsed;
                        Helper.LocalSettingsHelper.SaveSettings("TipSettings", false);
                    }
                }
                else
                {
                    _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                }
            }
            catch (Exception ex) { /*Helper.ThrowException(ex);*/ }
        }

        private void LockscreenBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (buttonOnHold)
                {
                    buttonOnHold = false;
                    return;
                }
                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    _ = tClient.Send("powertool -screenoff");
                    Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Lock);
                }
                else
                {
                    _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private async void FFULoaderBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (buttonOnHold)
                {
                    buttonOnHold = false;
                    return;
                }
                var result = await Helper.MessageBox("Are you sure you want to reboot the device to FFU Loader?", Helper.SoundHelper.Sound.Alert, "", "No", true, "Yes");
                if (result == 0)
                {
                    if (tClient.IsConnected && HomeHelper.IsConnected() && File.Exists(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe"))
                    {
                        _ = tClient.Send(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\RebootToFlashingMode.bat");
                        if (Helper.build < 14393)
                        {
                            await Helper.MessageBox("Rebooting to FFU Loader...", Helper.SoundHelper.Sound.Alert);
                        }
                        else
                        {
                            TextBlock textBlock = new TextBlock
                            {
                                Text = "Rebooting to FFU Loader...",
                            };
                            StackPanel stackPanel = new StackPanel();
                            stackPanel.Children.Add(textBlock);
                            stackPanel.VerticalAlignment = VerticalAlignment.Bottom;
                            ContentDialog contentDialog = new ContentDialog
                            {
                                Content = stackPanel,
                                Margin = new Thickness(0, -50, 0, 0)
                            };
                            Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                            await contentDialog.ShowAsync();
                        }
                    }
                    else
                    {
                        _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                    }
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private async void LockscreenBtn_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
                buttonOnHold = true;
                if (e.HoldingState != Windows.UI.Input.HoldingState.Started) return;
                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    SecondaryTile LockscreenTile = new SecondaryTile("LockscreenTileID", "Lockscreen", "Lockscreen", new Uri("ms-appx:///Assets/Icons/PowerOptions/LockscreenPowerOptionTileLogo.png"), TileSize.Default);
                    LockscreenTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                    LockscreenTile.VisualElements.ShowNameOnWide310x150Logo = true;
                    LockscreenTile.VisualElements.ShowNameOnSquare310x310Logo = true;
                    bool isPinned = await LockscreenTile.RequestCreateAsync();
                    if (isPinned)
                    {
                        TipText.Visibility = Visibility.Collapsed;
                        Helper.LocalSettingsHelper.SaveSettings("TipSettings", false);
                    }
                }
                else
                {
                    _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                }
            }
            catch (Exception ex) { /*Helper.ThrowException(ex);*/ }
        }

        private void VolumeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (buttonOnHold)
            {
                buttonOnHold = false;
                return;
            }
            if (tClient.IsConnected && HomeHelper.IsConnected())
            {
                var button = sender as Button;
                if (button.Content.ToString() == "Volume Up")
                {
                    _ = tClient.Send("SendKeys -v \"0xAF 0xAF\"");
                }
                else if (button.Content.ToString() == "Volume Down")
                {
                    _ = tClient.Send("SendKeys -v \"0xAE 0xAE\"");
                }
                else if (button.Content.ToString() == "Mute/Unmute")
                {
                    _ = tClient.Send("SendKeys -v \"0xAD\"");
                }
            }
            else
            {
                _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
            }
        }

        private void RestoreNDTKTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (secondFlag == false) return;
            if (RestoreNDTKTog.IsOn)
            {
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\NokiaSvcHost\\Plugins\\NsgExtA\\NdtkSvc", "Path", Helper.RegistryHelper.RegistryType.REG_SZ, "C:\\Windows\\System32\\NdtkSvc.dll");
                NDTKIndicator.Visibility = Visibility.Visible;
            }
            else
            {
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\NokiaSvcHost\\Plugins\\NsgExtA\\NdtkSvc", "Path", Helper.RegistryHelper.RegistryType.REG_SZ, "NdtkSvc.dll");
                NDTKIndicator.Visibility = Visibility.Visible;
            }
        }

        private async void SecMgrPatchBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = await Helper.MessageBox("Are you sure you want to patch the driver?", Helper.SoundHelper.Sound.Alert, "", "No", true, "Yes");
            if (result == 0)
            {
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Drivers\\PatchedSecMgr.sys", @"C:\Windows\System32\Drivers\SecMgr.sys");
                SecMgrIndicator.Visibility = Visibility.Visible;
            }
        }

        private async void SecMgrRestoreBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = await Helper.MessageBox("Are you sure you want to restore the driver?", Helper.SoundHelper.Sound.Alert, "", "No", true, "Yes");
            if (result == 0)
            {
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Drivers\\OriginalSecMgr.sys", @"C:\Windows\System32\Drivers\SecMgr.sys");
                SecMgrIndicator.Visibility = Visibility.Visible;
            }
        }
        private void UfpEnableBtn_Click(object sender, RoutedEventArgs e)
        {
            Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{01de5a27-8705-40db-bad6-96fa5187d4a6}\\Elements\\25000209", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "0100000000000000");
            Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{01de5a27-8705-40db-bad6-96fa5187d4a6}\\Elements\\26000207", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "01");
            Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{0ff5f24a-3785-4aeb-b8fe-4226215b88c4}\\Elements\\25000209", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "0100000000000000");
            Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{0ff5f24a-3785-4aeb-b8fe-4226215b88c4}\\Elements\\26000207", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY, "01");
        }

        private void UfpDisableBtn_Click(object sender, RoutedEventArgs e)
        {
            if (tClient.IsConnected && HomeHelper.IsConnected())
            {
                _ = tClient.Send("reg delete hklm\\BCD00000001\\Objects\\{01de5a27-8705-40db-bad6-96fa5187d4a6}\\Elements\\25000209 /f" +
                        "&reg delete hklm\\BCD00000001\\Objects\\{01de5a27-8705-40db-bad6-96fa5187d4a6}\\Elements\\26000207 /f" +
                        "&reg delete hklm\\BCD00000001\\Objects\\{0ff5f24a-3785-4aeb-b8fe-4226215b88c4}\\Elements\\25000209 /f" +
                        "&reg delete hklm\\BCD00000001\\Objects\\{0ff5f24a-3785-4aeb-b8fe-4226215b88c4}\\Elements\\26000207 /f");
            }
            else
            {
                _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
            }
        }

        private async void SearchPressAppsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (secondFlag == false || SearchPressAppsCombo.SelectedIndex == 0) return;
                if (!await Helper.IsCapabilitiesAllowed())
                {
                    var isAllowed = await Helper.AskCapabilitiesPermission();
                    if (!isAllowed)
                    {
                        SearchPressAppsCombo.SelectedIndex = 0;
                        return;
                    }
                }
                if (SearchPressAppsCombo.SelectedIndex == 1)
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Input\\WEH\\Buttons\\WEHButton4\\Press", "AppID", Helper.RegistryHelper.RegistryType.REG_SZ, "");
                }
                else if (SearchPressAppsCombo.SelectedIndex == 2)
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Input\\WEH\\Buttons\\WEHButton4\\Press", "AppID", Helper.RegistryHelper.RegistryType.REG_SZ, "{None}");
                }
                else
                {
                    SearchPressParaCombo.SelectedIndex = 1;
                    if (Packages[SearchPressAppsCombo.SelectedIndex - 3].DisplayName == "CMD Injector") SearchPressParaCombo.Visibility = Visibility.Visible;
                    else SearchPressParaCombo.Visibility = Visibility.Collapsed;
                    var manifest = await Packages[SearchPressAppsCombo.SelectedIndex - 3].InstalledLocation.GetFileAsync("AppxManifest.xml");
                    var tags = XElement.Load(manifest.Path).Elements().Where(i => i.Name.LocalName == "PhoneIdentity");
                    var attributes = tags.Attributes().Where(i => i.Name.LocalName == "PhoneProductId");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Input\\WEH\\Buttons\\WEHButton4\\Press", "AppID", Helper.RegistryHelper.RegistryType.REG_SZ, $"{{{attributes.First().Value}}}");
                }
                var isRemapped = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Features", "ButtonRemapping", Helper.RegistryHelper.RegistryType.REG_SZ);
                if (isRemapped != "WEHButtonRouter.dll") SearchOptIndicator.Visibility = Visibility.Visible;
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Features", "ButtonRemapping", Helper.RegistryHelper.RegistryType.REG_SZ, "WEHButtonRouter.dll");
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }

        private async void SearchHoldAppsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (secondFlag == false || SearchHoldAppsCombo.SelectedIndex == 0) return;
                if (!await Helper.IsCapabilitiesAllowed())
                {
                    var isAllowed = await Helper.AskCapabilitiesPermission();
                    if (!isAllowed)
                    {
                        SearchHoldAppsCombo.SelectedIndex = 0;
                        return;
                    }
                }
                if (SearchHoldAppsCombo.SelectedIndex == 1)
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Input\\WEH\\Buttons\\WEHButton4\\PressAndHold", "AppID", Helper.RegistryHelper.RegistryType.REG_SZ, "");
                }
                else if (SearchHoldAppsCombo.SelectedIndex == 2)
                {
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Input\\WEH\\Buttons\\WEHButton4\\PressAndHold", "AppID", Helper.RegistryHelper.RegistryType.REG_SZ, "{None}");
                }
                else
                {
                    SearchHoldParaCombo.SelectedIndex = 1;
                    if (Packages[SearchHoldAppsCombo.SelectedIndex - 3].DisplayName == "CMD Injector") SearchHoldParaCombo.Visibility = Visibility.Visible;
                    else SearchHoldParaCombo.Visibility = Visibility.Collapsed;
                    var manifest = await Packages[SearchHoldAppsCombo.SelectedIndex - 3].InstalledLocation.GetFileAsync("AppxManifest.xml");
                    var tags = XElement.Load(manifest.Path).Elements().Where(i => i.Name.LocalName == "PhoneIdentity");
                    var attributes = tags.Attributes().Where(i => i.Name.LocalName == "PhoneProductId");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Input\\WEH\\Buttons\\WEHButton4\\PressAndHold", "AppID", Helper.RegistryHelper.RegistryType.REG_SZ, $"{{{attributes.First().Value}}}");
                }
                var isRemapped = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Features", "ButtonRemapping", Helper.RegistryHelper.RegistryType.REG_SZ);
                if (isRemapped != "WEHButtonRouter.dll") SearchOptIndicator.Visibility = Visibility.Visible;
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Features", "ButtonRemapping", Helper.RegistryHelper.RegistryType.REG_SZ, "WEHButtonRouter.dll");
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }

        private void StartWallTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (StartWallTog.IsOn)
            {
                Helper.LocalSettingsHelper.SaveSettings("StartWallSwitch", true);
                WallSwitchExtraStack.Visibility = Visibility.Visible;
            }
            else
            {
                Helper.LocalSettingsHelper.SaveSettings("StartWallSwitch", false);
                WallSwitchExtraStack.Visibility = Visibility.Collapsed;
            }
            Helper.LocalSettingsHelper.SaveSettings("StartWallImagePosition", 0);
        }

        private async void StartWallLibraryBtn_Click(object sender, RoutedEventArgs e)
        {
            var library = await TweakBoxHelper.SetWallpaperLibrary();
            if (library != null)
            {
                if (library.Path.Contains(Helper.installedLocation.Path)) StartWallLibPathBox.Text = "CMDInjector:\\Assets\\Images\\Lockscreens\\Stripes";
                else StartWallLibPathBox.Text = library.Path;
                Helper.LocalSettingsHelper.SaveSettings("StartWallImagePosition", 0);
            }
        }

        private void StartWallTrigCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StartWallTrigCombo.SelectedIndex == 0)
            {
                StartWallInterCombo.Visibility = Visibility.Collapsed;
            }
            else
            {
                StartWallInterCombo.Visibility = Visibility.Visible;
            }
            Helper.LocalSettingsHelper.SaveSettings("StartWallTrigger", StartWallTrigCombo.SelectedIndex);
        }

        private async void VolDownBtn_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
                buttonOnHold = true;
                if (e.HoldingState != Windows.UI.Input.HoldingState.Started) return;
                SecondaryTile VolDownTile = new SecondaryTile("VolDownTileID", "Volume down", "VolDown", new Uri("ms-appx:///Assets/Icons/VolumeOptions/DownVolumeOptionTileLogo.png"), TileSize.Default);
                VolDownTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                VolDownTile.VisualElements.ShowNameOnWide310x150Logo = true;
                VolDownTile.VisualElements.ShowNameOnSquare310x310Logo = true;
                bool isPinned = await VolDownTile.RequestCreateAsync();
                if (isPinned)
                {
                    TipText.Visibility = Visibility.Collapsed;
                    Helper.LocalSettingsHelper.SaveSettings("TipSettings", false);
                }
            }
            catch (Exception ex) { /*Helper.ThrowException(ex);*/ }
        }

        private async void VolUpBtn_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
                buttonOnHold = true;
                SecondaryTile VolUpTile = new SecondaryTile("VolUpTileID", "Volume up", "VolUp", new Uri("ms-appx:///Assets/Icons/VolumeOptions/UpVolumeOptionTileLogo.png"), TileSize.Default);
                VolUpTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                VolUpTile.VisualElements.ShowNameOnWide310x150Logo = true;
                VolUpTile.VisualElements.ShowNameOnSquare310x310Logo = true;
                bool isPinned = await VolUpTile.RequestCreateAsync();
                if (isPinned)
                {
                    TipText.Visibility = Visibility.Collapsed;
                    Helper.LocalSettingsHelper.SaveSettings("TipSettings", false);
                }
            }
            catch (Exception ex) { /*Helper.ThrowException(ex);*/ }
        }

        private async void VolMuteBtn_Holding(object sender, HoldingRoutedEventArgs e)
        {
            try
            {
                buttonOnHold = true;
                if (e.HoldingState != Windows.UI.Input.HoldingState.Started) return;
                SecondaryTile VolMuteTile = new SecondaryTile("VolMuteTileID", "Volume mute/unmute", "VolMute", new Uri("ms-appx:///Assets/Icons/VolumeOptions/MuteVolumeOptionTileLogo.png"), TileSize.Default);
                VolMuteTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                VolMuteTile.VisualElements.ShowNameOnWide310x150Logo = true;
                VolMuteTile.VisualElements.ShowNameOnSquare310x310Logo = true;
                bool isPinned = await VolMuteTile.RequestCreateAsync();
                if (isPinned)
                {
                    TipText.Visibility = Visibility.Collapsed;
                    Helper.LocalSettingsHelper.SaveSettings("TipSettings", false);
                }
            }
            catch (Exception ex) { /*Helper.ThrowException(ex);*/ }
        }

        private async void WallDisBatSavTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (WallDisBatSavTog.IsOn)
            {
                await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[0]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[1]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[2]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[3]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[4]}\nTrue");
            }
            else
            {
                await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"), $"{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[0]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[1]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[2]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[3]}\n{(await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Lockscreen.dat"))).Split('\n')[4]}\nFalse");
            }
            await Helper.localFolder.CreateFileAsync("LockscreenBreak.txt", CreationCollisionOption.OpenIfExists);
        }

        private void SearchPressParaCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string value = "";
            if (SearchPressParaCombo.SelectedIndex == 1) value = "";
            else if (SearchPressParaCombo.SelectedIndex == 4) value = "HomePage";
            else if (SearchPressParaCombo.SelectedIndex == 5) value = "TerminalPage";
            else if (SearchPressParaCombo.SelectedIndex == 6) value = "StartupPage";
            else if (SearchPressParaCombo.SelectedIndex == 7) value = "PacManPage";
            else if (SearchPressParaCombo.SelectedIndex == 8) value = "SnapperPage";
            else if (SearchPressParaCombo.SelectedIndex == 9) value = "BootConfigPage";
            else if (SearchPressParaCombo.SelectedIndex == 10) value = "TweakBoxPage";
            else if (SearchPressParaCombo.SelectedIndex == 11) value = "SettingsPage";
            else if (SearchPressParaCombo.SelectedIndex == 12) value = "HelpPage";
            else if (SearchPressParaCombo.SelectedIndex == 13) value = "AboutPage";
            else if (SearchPressParaCombo.SelectedIndex == 16) value = "Shutdown";
            else if (SearchPressParaCombo.SelectedIndex == 17) value = "Restart";
            else if (SearchPressParaCombo.SelectedIndex == 18) value = "Lockscreen";
            else if (SearchPressParaCombo.SelectedIndex == 19) value = "FFULoader";
            else if (SearchPressParaCombo.SelectedIndex == 22) value = "VolDown";
            else if (SearchPressParaCombo.SelectedIndex == 23) value = "VolUp";
            else if (SearchPressParaCombo.SelectedIndex == 24) value = "VolMute";
            else value = "";
            Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Input\\WEH\\Buttons\\WEHButton4\\Press", "AppParam", Helper.RegistryHelper.RegistryType.REG_SZ, value);
        }

        private void SearchHoldParaCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string value = "";
            if (SearchHoldParaCombo.SelectedIndex == 1) value = "";
            else if (SearchHoldParaCombo.SelectedIndex == 4) value = "HomePage";
            else if (SearchHoldParaCombo.SelectedIndex == 5) value = "TerminalPage";
            else if (SearchHoldParaCombo.SelectedIndex == 6) value = "StartupPage";
            else if (SearchHoldParaCombo.SelectedIndex == 7) value = "PacManPage";
            else if (SearchHoldParaCombo.SelectedIndex == 8) value = "SnapperPage";
            else if (SearchHoldParaCombo.SelectedIndex == 9) value = "BootConfigPage";
            else if (SearchHoldParaCombo.SelectedIndex == 10) value = "TweakBoxPage";
            else if (SearchHoldParaCombo.SelectedIndex == 11) value = "SettingsPage";
            else if (SearchHoldParaCombo.SelectedIndex == 12) value = "HelpPage";
            else if (SearchHoldParaCombo.SelectedIndex == 13) value = "AboutPage";
            else if (SearchHoldParaCombo.SelectedIndex == 16) value = "Shutdown";
            else if (SearchHoldParaCombo.SelectedIndex == 17) value = "Restart";
            else if (SearchHoldParaCombo.SelectedIndex == 18) value = "Lockscreen";
            else if (SearchHoldParaCombo.SelectedIndex == 19) value = "FFULoader";
            else if (SearchHoldParaCombo.SelectedIndex == 22) value = "VolDown";
            else if (SearchHoldParaCombo.SelectedIndex == 23) value = "VolUp";
            else if (SearchHoldParaCombo.SelectedIndex == 24) value = "VolMute";
            else value = "";
            Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\Input\\WEH\\Buttons\\WEHButton4\\PressAndHold", "AppParam", Helper.RegistryHelper.RegistryType.REG_SZ, value);
        }

        private void StartWallInterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (StartWallInterCombo.SelectedIndex)
            {
                case 0:
                    Helper.LocalSettingsHelper.SaveSettings("StartWallInterval", 15);
                    break;
                case 1:
                    Helper.LocalSettingsHelper.SaveSettings("StartWallInterval", 30);
                    break;
                case 2:
                    Helper.LocalSettingsHelper.SaveSettings("StartWallInterval", 60);
                    break;
                case 3:
                    Helper.LocalSettingsHelper.SaveSettings("StartWallInterval", 720);
                    break;
                case 4:
                    Helper.LocalSettingsHelper.SaveSettings("StartWallInterval", 1440);
                    break;
                default:
                    Helper.LocalSettingsHelper.SaveSettings("StartWallInterval", 15);
                    break;
            }
            Helper.LocalSettingsHelper.SaveSettings("StartWallInterItem", StartWallInterCombo.SelectedIndex);
        }
    }
}
