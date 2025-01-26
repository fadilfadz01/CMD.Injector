using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ndtklib;
using Windows.UI.Popups;
using Windows.Storage;
using Windows.ApplicationModel;
using Windows.System.Profile;
using System.Text;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Xaml.Documents;
using CMDInjectorHelper;
using System.Threading;
using WinUniversalTool;
using System.Xml;
using Windows.ApplicationModel.Core;
using Windows.System;
using XamlBrewer.Uwp.Controls;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Security.Credentials.UI;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CMDInjector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CMDInjector : Page
    {
        bool buttonOnHold = false;

        public CMDInjector()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Initialize();
        }

        private async void Initialize()
        {
            //Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\KeepWiFiOnSvc", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000002");
            Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment", "Path", Helper.RegistryHelper.RegistryType.REG_EXPAND_SZ, "%SystemRoot%\\system32;%SystemRoot%;%SystemDrive%\\Programs\\CommonFiles\\System;%SystemDrive%\\wtt;%SystemDrive%\\data\\test\\bin;%SystemRoot%\\system32\\WindowsPowerShell\\v1.0;");
            Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\SecurityManager\\PrincipalClasses\\PRINCIPAL_CLASS_TCB", "Directories", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, "C:\\ ");
            await Helper.WaitAppLaunch();
            bool isFirstLaunch = false;
            if (!File.Exists(@"C:\Windows\System32\Startup.bat"))
            {
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\Startup.bat", @"C:\Windows\System32\Startup.bat");
            }
            if (Helper.IsStrAGraterThanStrB(Helper.currentVersion, Helper.LocalSettingsHelper.LoadSettings("InitialLaunch", "0.0.0.0"), '.'))
            {
                if (Convert.ToInt32(Helper.InjectedBatchVersion) < 3800)
                {
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\Startup.bat", @"C:\Windows\System32\Startup.bat");
                }
                await Changelog.DisplayLog();
                Helper.LocalSettingsHelper.SaveSettings("InitialLaunch", Helper.currentVersion);
                Helper.LocalSettingsHelper.SaveSettings("TempInjection", true);
                Helper.LocalSettingsHelper.SaveSettings("AskCapPermission", true);
            }
            if ((!HomeHelper.IsCMDInjected() && !File.Exists(@"C:\Windows\System32\CMDInjector.dat")) || !File.Exists(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe"))
            {
                var isInjected = await OperationInjection();
                if (isInjected)
                {
                    if (Helper.LocalSettingsHelper.LoadSettings("FirstLaunch", true) && !File.Exists(@"C:\Windows\System32\CMDInjectorFirstLaunch.dat")/* && build >= 14393*/)
                    {
                        isFirstLaunch = true;
                        var result = await Helper.MessageBox("A system reboot is required to initialize the App.", Helper.SoundHelper.Sound.Alert, "First Launch", "Cancel", true, "Reboot");
                        Helper.LocalSettingsHelper.SaveSettings("FirstLaunch", false);
                        Helper.CopyFile(Helper.localFolder.Path + "\\CMDInjector.dat", @"C:\Windows\System32\CMDInjectorFirstLaunch.dat");
                        if (result == 0)
                        {
                            Helper.RebootSystem();
                        }
                    }
                }
                else
                {
                    ContentDialog contentDialog = new ContentDialog
                    {
                        Title = "CMD Injector",
                        Content = "Please restore the NDTKSvc in order to use this App.",
                        PrimaryButtonText = "How",
                        SecondaryButtonText = "Exit"
                    };
                    Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                    var result = await contentDialog.ShowAsync();
                    if (result != ContentDialogResult.Primary)
                    {
                        CoreApplication.Exit();
                    }
                    else
                    {
                        Uri uri = new Uri("https://www.google.com/search?q=How+to+interop+unlock+Windows+10+Mobile%3F");
                        try
                        {
                            await Launcher.LaunchUriAsync(uri);
                        }
                        catch (Exception ex)
                        {
                            Helper.ThrowException(ex);
                        }
                        CoreApplication.Exit();
                        return;
                    }
                }
            }
            else
            {
                Helper.LocalSettingsHelper.SaveSettings("FirstLaunch", false);
            }
            if (Helper.LocalSettingsHelper.LoadSettings("TempInjection", true))
            {
                _ = OperationInjection();
                Helper.LocalSettingsHelper.SaveSettings("TempInjection", false);
            }
            if (Helper.LocalSettingsHelper.LoadSettings("AskCapPermission", true) && !isFirstLaunch && !File.Exists(@"C:\Windows\System32\CMDInjectorFirstLaunch.dat"))
            {
                try
                {
                    if (!await Helper.IsCapabilitiesAllowed())
                    {
                        await Helper.AskCapabilitiesPermission();
                        Helper.LocalSettingsHelper.SaveSettings("AskCapPermission", false);
                    }
                }
                catch (Exception ex)
                {
                    Helper.ThrowException(ex);
                }
            }
        }

        private async Task<bool> OperationInjection()
        {
            await Task.Run(async () =>
            {
                await FileIO.WriteTextAsync(await Helper.localFolder.CreateFileAsync("CMDInjector.dat", CreationCollisionOption.ReplaceExisting), Helper.currentBatchVersion.ToString());
                Helper.CopyFile(Helper.localFolder.Path + "\\CMDInjector.dat", @"C:\Windows\System32\CMDInjectorTempSetup.dat");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\MessageDialog.bat", @"C:\Windows\System32\MessageDialog.bat");
                if (!File.Exists(@"C:\Windows\System32\CMDInjector.bat")) Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\NonSystemWide.bat", @"C:\Windows\System32\CMDInjector.bat");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\Setup.bat", @"C:\Windows\System32\CMDInjectorSetup.bat");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Bootsh\\bootshsvc.dll", @"C:\Windows\System32\bootshsvc.dll");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Bootsh\\bootshsvc.dll.mui", @"C:\Windows\System32\en-US\bootshsvc.dll.mui");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Startup\\startup.bsc", @"C:\Windows\System32\Boot\startup.bsc");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\bcdedit.exe.mui", @"C:\Windows\System32\en-US\bcdedit.exe.mui");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\CheckNetIsolation.exe.mui", @"C:\Windows\System32\en-US\CheckNetIsolation.exe.mui");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\cmd.exe.mui", @"C:\Windows\System32\en-US\cmd.exe.mui");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\findstr.exe.mui", @"C:\Windows\System32\en-US\findstr.exe.mui");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\ICacls.exe.mui", @"C:\Windows\System32\en-US\ICacls.exe.mui");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\reg.exe.mui", @"C:\Windows\System32\en-US\reg.exe.mui");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\sort.exe.mui", @"C:\Windows\System32\en-US\sort.exe.mui");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\takeown.exe.mui", @"C:\Windows\System32\en-US\takeown.exe.mui");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\AppXTest.Common.Feature.DeployAppx.dll", @"C:\Windows\System32\AppXTest.Common.Feature.DeployAppx.dll");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\bcdedit.exe", @"C:\Windows\System32\bcdedit.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\CheckNetIsolation.exe", @"C:\Windows\System32\CheckNetIsolation.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\cmd.exe", @"C:\Windows\System32\cmd.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\findstr.exe", @"C:\Windows\System32\findstr.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\icacls.exe", @"C:\Windows\System32\icacls.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\InputProcessorClient.dll", @"C:\Windows\System32\InputProcessorClient.dll");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\MinDeployAppX.exe", @"C:\Windows\System32\MinDeployAppX.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\more.com", @"C:\Windows\System32\more.com");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\PowerTool.exe", @"C:\Windows\System32\PowerTool.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\reg.exe", @"C:\Windows\System32\reg.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\ScreenCapture.exe", @"C:\Windows\System32\ScreenCapture.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\SendKeys.exe", @"C:\Windows\System32\SendKeys.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\shutdown.exe", @"C:\Windows\System32\shutdown.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\sleep.exe", @"C:\Windows\System32\sleep.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\sort.exe", @"C:\Windows\System32\sort.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\takeown.exe", @"C:\Windows\System32\takeown.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\telnetd.exe", @"C:\Windows\System32\telnetd.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\TestNavigationApi.exe", @"C:\Windows\System32\TestNavigationApi.exe");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\xcopy.exe", @"C:\Windows\System32\xcopy.exe");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\CI", "UMCIAuditMode", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "Type", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000010");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000002");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "ServiceSidType", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "ErrorControl", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "ImagePath", Helper.RegistryHelper.RegistryType.REG_EXPAND_SZ, "%SystemRoot%\\system32\\svchost.exe -k Bootshsvc");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "DisplayName", Helper.RegistryHelper.RegistryType.REG_SZ, "@bootshsvc.dll,-1");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "Description", Helper.RegistryHelper.RegistryType.REG_SZ, "@bootshsvc.dll,-2");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "ObjectName", Helper.RegistryHelper.RegistryType.REG_SZ, "LocalSystem");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "DependOnService", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, "Afd lmhosts keyiso ");
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "FailureActions", Helper.RegistryHelper.RegistryType.REG_BINARY, "80510100000000000000000003000000140000000100000060EA00000100000060EA00000000000000000000");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh", "RequiredPrivileges", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, "SeAssignPrimaryTokenPrivilege SeAuditPrivilege SeSecurityPrivilege SeChangeNotifyPrivilege SeCreateGlobalPrivilege SeDebugPrivilege SeImpersonatePrivilege SeIncreaseQuotaPrivilege SeTcbPrivilege SeBackupPrivilege SeRestorePrivilege SeShutdownPrivilege SeSystemProfilePrivilege SeSystemtimePrivilege SeManageVolumePrivilege SeCreatePagefilePrivilege SeCreatePermanentPrivilege SeCreateSymbolicLinkPrivilege SeIncreaseBasePriorityPrivilege SeIncreaseWorkingSetPrivilege SeLoadDriverPrivilege SeLockMemoryPrivilege SeProfileSingleProcessPrivilege SeSystemEnvironmentPrivilege SeTakeOwnershipPrivilege SeTimeZonePrivilege ");
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh\\Parameters", "ServiceDll", Helper.RegistryHelper.RegistryType.REG_EXPAND_SZ, "%SystemRoot%\\system32\\bootshsvc.dll");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh\\Parameters", "ServiceDllUnloadOnStop", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                /*Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh\\Parameters\\Commands", "Loopback", Helper.RegistryHelper.RegistryType.REG_SZ, "CheckNetIsolation.exe loopbackexempt -a -n=CMDInjector_kqyng60eng17c");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh\\Parameters\\Commands", "Telnetd", Helper.RegistryHelper.RegistryType.REG_SZ, "start telnetd.exe cmd.exe 9999");*/
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Svchost", "bootshsvc", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, "bootsh");
            });
            return HomeHelper.IsCMDInjected();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (buttonOnHold)
            {
                buttonOnHold = false;
                return;
            }
            var button = sender as Button;
            var stackPanel = button.Content as StackPanel;
            var textBlock = stackPanel.Children[1] as TextBlock;
            var image = stackPanel.Children[0] as Image;
            var bitmapImage = image.Source as BitmapImage;
            if (textBlock.Text == "Home")
            {
                Helper.pageNavigation.Invoke(1, null);
            }
            else if (textBlock.Text == "Terminal")
            {
                Helper.pageNavigation.Invoke(2, null);
            }
            else if (textBlock.Text == "Startup")
            {
                Helper.pageNavigation.Invoke(3, null);
            }
            else if (textBlock.Text == "PacMan")
            {
                Helper.pageNavigation.Invoke(4, null);
            }
            else if (textBlock.Text == "Snapper")
            {
                Helper.pageNavigation.Invoke(5, null);
            }
            else if (textBlock.Text == "BootConfig")
            {
                Helper.pageNavigation.Invoke(6, null);
            }
            else if (textBlock.Text == "TweakBox")
            {
                Helper.pageNavigation.Invoke(7, null);
            }
            else if (textBlock.Text == "Settings")
            {
                Helper.pageNavigation.Invoke(8, null);
            }
            else if (textBlock.Text == "Help")
            {
                Helper.pageNavigation.Invoke(9, null);
            }
            else if (textBlock.Text == "About")
            {
                Helper.pageNavigation.Invoke(10, null);
            }
            if (Helper.LocalSettingsHelper.LoadSettings("MenuTransition", true) && Helper.build >= 10572) (Frame.Content as Page).OpenFromSplashScreen(Helper.rect, Helper.color, bitmapImage.UriSource);
        }

        private async void Button_Holding(object sender, HoldingRoutedEventArgs e)
        {
            buttonOnHold = true;
            if (e.HoldingState != Windows.UI.Input.HoldingState.Started) return;
            var button = sender as Button;
            var stackPanel = button.Content as StackPanel;
            var textBlock = stackPanel.Children[1] as TextBlock;
            if (textBlock.Text == "Terminal")
            {
                try
                {
                    ContentDialog argumentTerm = new ContentDialog();
                    argumentTerm.Title = "Terminal";
                    argumentTerm.IsSecondaryButtonEnabled = true;
                    argumentTerm.PrimaryButtonText = "Pin";
                    argumentTerm.SecondaryButtonText = "Cancel";
                    TextBlock terminalTextblock = new TextBlock
                    {
                        Text = "Argument (Optional)"
                    };
                    TextBox terminalTextBox = new TextBox
                    {
                        AcceptsReturn = false,
                        IsSpellCheckEnabled = false,
                        IsTextPredictionEnabled = false,
                        PlaceholderText = "Echo Hello World!"
                    };
                    StackPanel terminalStackpanel = new StackPanel();
                    terminalStackpanel.Children.Add(terminalTextblock);
                    terminalStackpanel.Children.Add(terminalTextBox);
                    argumentTerm.Content = terminalStackpanel;
                    Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                    if (await argumentTerm.ShowAsync() == ContentDialogResult.Primary)
                    {
                        if (terminalTextBox.Text == string.Empty)
                        {
                            var CMDInjectorTile = new SecondaryTile(textBlock.Text + "PageID", textBlock.Text, textBlock.Text + "Page", new Uri("ms-appx:///Assets/Icons/Menus/" + textBlock.Text + "MenuTileLogo.png"), Windows.UI.StartScreen.TileSize.Default);
                            CMDInjectorTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                            CMDInjectorTile.VisualElements.ShowNameOnWide310x150Logo = true;
                            CMDInjectorTile.VisualElements.ShowNameOnSquare310x310Logo = true;
                            await CMDInjectorTile.RequestCreateAsync();
                        }
                        else
                        {
                            var cmd = TerminalHelper.EscapeSymbols(terminalTextBox.Text, false);
                            var CMDInjectorTile = new SecondaryTile(textBlock.Text + "PageID" + cmd.Replace(" ", "_"), textBlock.Text + $" ({terminalTextBox.Text})", textBlock.Text + "Page" + $" {cmd}", new Uri("ms-appx:///Assets/Icons/Menus/" + textBlock.Text + "MenuTileLogo.png"), Windows.UI.StartScreen.TileSize.Default);
                            CMDInjectorTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                            CMDInjectorTile.VisualElements.ShowNameOnWide310x150Logo = true;
                            CMDInjectorTile.VisualElements.ShowNameOnSquare310x310Logo = true;
                            await CMDInjectorTile.RequestCreateAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Helper.ThrowException(ex);
                }
            }
            else if (textBlock.Text == "Home" || textBlock.Text == "Settings" || textBlock.Text == "Help" || textBlock.Text == "About")
            {
                var CMDInjectorTile = new SecondaryTile(textBlock.Text + "PageID", textBlock.Text + " (CMD Injector)", textBlock.Text + "Page", new Uri("ms-appx:///Assets/Icons/Menus/" + textBlock.Text + "MenuTileLogo.png"), Windows.UI.StartScreen.TileSize.Default);
                CMDInjectorTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                CMDInjectorTile.VisualElements.ShowNameOnWide310x150Logo = true;
                CMDInjectorTile.VisualElements.ShowNameOnSquare310x310Logo = true;
                await CMDInjectorTile.RequestCreateAsync();
            }
            else
            {
                var CMDInjectorTile = new SecondaryTile(textBlock.Text + "PageID", textBlock.Text, textBlock.Text + "Page", new Uri("ms-appx:///Assets/Icons/Menus/" + textBlock.Text + "MenuTileLogo.png"), Windows.UI.StartScreen.TileSize.Default);
                CMDInjectorTile.VisualElements.ShowNameOnSquare150x150Logo = true;
                CMDInjectorTile.VisualElements.ShowNameOnWide310x150Logo = true;
                CMDInjectorTile.VisualElements.ShowNameOnSquare310x310Logo = true;
                await CMDInjectorTile.RequestCreateAsync();
            }
        }
    }
}
