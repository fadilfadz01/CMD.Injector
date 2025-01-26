using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Text;
using ndtklib;
using Windows.System.Profile;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Notifications;
using CMDInjectorHelper;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CMDInjector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Page
    {
        bool isRootFrame = false;

        public Home()
        {
            this.InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                /*if (build < 14393 && Helper.IsSecureBootPolicyInstalled() && !File.Exists(@"C:\Windows\System32\CMDInjectorVersion.dat"))
                {
                    UnlockBLBox.Visibility = Visibility.Visible;
                }*/
                if (File.Exists(@"C:\Windows\servicing\Packages\FadilFadz.CMDInjector.Permanent~628844477771337a~arm~~1.0.0.0.mum"))
                {
                    InjectionTypeCombo.SelectedIndex = 1;
                }
                else
                {
                    InjectionTypeCombo.SelectedIndex = 0;
                }
                if (File.Exists(@"C:\Windows\System32\CMDInjector.dat") || File.Exists(@"C:\Windows\System32\CMDUninjector.dat"))
                {
                    InjectBtn.IsEnabled = false;
                    UnInjectBtn.IsEnabled = false;
                    if (File.Exists(@"C:\Windows\System32\CMDInjector.dat"))
                    {
                        InjectBtn.Content = "Injected";
                        reInjectionReboot.Text = "A previous injection reboot is pending. Please reboot your device to apply the changes.";
                    }
                    else
                    {
                        UnInjectBtn.Content = "Un-Injected";
                        reInjectionReboot.Text = "A previous un-injection reboot is pending. Please reboot your device to apply the changes.";
                    }
                    UnInjectBtn.Visibility = Visibility.Visible;
                    reInjectionReboot.Visibility = Visibility.Visible;
                }
                else if (HomeHelper.IsCMDInjected() && File.Exists(@"C:\Windows\System32\CMDInjectorVersion.dat"))
                {
                    if (Convert.ToInt32(Helper.InjectedBatchVersion) > Helper.currentBatchVersion)
                    {
                        InjectBtn.IsEnabled = false;
                        InjectBtn.Content = "Injected";
                        reInjectionNote.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        if (Convert.ToInt32(Helper.InjectedBatchVersion) < Helper.currentBatchVersion)
                        {
                            reInjectionBox.Visibility = Visibility.Visible;
                        }
                        InjectBtn.Content = "Re-Inject";
                    }
                    UnInjectBtn.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
            {
                if (e.Parameter.ToString() == "HomePage")
                {
                    isRootFrame = true;
                }
                else if (e.Parameter.ToString() == "InjectCMD=Re-Inject;ReinjectionRequired")
                {
                    while (true)
                    {
                        await Task.Delay(200);
                        if (Helper.userVerified) break;
                    }
                    InjectBtn_Click(null, null);
                }
            }
        }

        private async Task Reboot()
        {
            try
            {
                var result = await Helper.MessageBox("To apply the changes made, you have to reboot your device.", Helper.SoundHelper.Sound.Alert, "Reboot Required", "Reboot later", true, "Reboot now");
                if (result == 0)
                {
                    Helper.RebootSystem();
                }
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private async void InjectBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                /*if (UnlockBLBox.Visibility == Visibility.Visible)
                {
                    MessageDialog showDialog = new MessageDialog("Are you sure you want to inject?", "Confirmation");
                    showDialog.Commands.Add(new UICommand("Yes")
                    {
                        Id = 0
                    });
                    showDialog.Commands.Add(new UICommand("No")
                    {
                        Id = 1
                    });
                    showDialog.DefaultCommandIndex = 0;
                    showDialog.CancelCommandIndex = 1;
                    var result = await showDialog.ShowAsync();
                    if ((int)result.Id == 1)
                    {
                        return;
                    }
                }*/
                InjectBtn.IsEnabled = false;
                UnInjectBtn.IsEnabled = false;
                InjectBtn.Content = "Injecting";
                await Task.Run(async () =>
                {
                    if (Helper.build < 14393)
                    {
                        //CommonHelper.CopyFile(CommonHelper.installFolder.Path + "\\Contents\\Drivers\\PatchedSecMgr.sys", @"C:\Windows\System32\Drivers\SecMgr.sys");

                        if (File.Exists(@"C:\Windows\System32\CMDInjectorVersion.dat"))
                        {
                            if (Convert.ToInt32(Helper.InjectedBatchVersion) <= 3550)
                            {
                                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Drivers\\OriginalSecMgr.sys", @"C:\Windows\System32\Drivers\SecMgr.sys");
                            }
                        }
                    }
                    await FileIO.WriteTextAsync(await Helper.localFolder.CreateFileAsync("CMDInjector.dat", CreationCollisionOption.ReplaceExisting), Helper.currentBatchVersion.ToString());
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        if (InjectionTypeCombo.SelectedIndex == 0)
                        {
                            Helper.CopyFile(Helper.localFolder.Path + "\\CMDInjector.dat", @"C:\Windows\System32\CMDInjectorTemporary.dat");
                        }
                        else
                        {
                            Helper.CopyFile(Helper.localFolder.Path + "\\CMDInjector.dat", @"C:\Windows\System32\CMDInjectorPermanent.dat");
                        }
                    });
                    Helper.CopyFile(Helper.localFolder.Path + "\\CMDInjector.dat", @"C:\Windows\System32\CMDInjector.dat");
                    Helper.CopyFile(Helper.localFolder.Path + "\\CMDInjector.dat", @"C:\Windows\System32\CMDInjectorVersion.dat");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\MessageDialog.bat", @"C:\Windows\System32\MessageDialog.bat");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\SystemWide.bat", @"C:\Windows\System32\CMDInjector.bat");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\Setup.bat", @"C:\Windows\System32\CMDInjectorSetup.bat");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Bootsh\\bootshsvc.dll", @"C:\Windows\System32\bootshsvc.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Bootsh\\bootshsvc.dll.mui", @"C:\Windows\System32\en-US\bootshsvc.dll.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\Startup\\startup.bsc", @"C:\Windows\System32\Boot\startup.bsc");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\attrib.exe.mui", @"C:\Windows\System32\en-US\attrib.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\bcdboot.exe.mui", @"C:\Windows\System32\en-US\bcdboot.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\bcdedit.exe.mui", @"C:\Windows\System32\en-US\bcdedit.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\CheckNetIsolation.exe.mui", @"C:\Windows\System32\en-US\CheckNetIsolation.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\cmd.exe.mui", @"C:\Windows\System32\en-US\cmd.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\cmdkey.exe.mui", @"C:\Windows\System32\en-US\cmdkey.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\cscript.exe.mui", @"C:\Windows\System32\en-US\cscript.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\Dism.exe.mui", @"C:\Windows\System32\en-US\Dism.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\find.exe.mui", @"C:\Windows\System32\en-US\find.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\findstr.exe.mui", @"C:\Windows\System32\en-US\findstr.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\finger.exe.mui", @"C:\Windows\System32\en-US\finger.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\ftp.exe.mui", @"C:\Windows\System32\en-US\ftp.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\help.exe.mui", @"C:\Windows\System32\en-US\help.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\hostname.exe.mui", @"C:\Windows\System32\en-US\hostname.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\ICacls.exe.mui", @"C:\Windows\System32\en-US\ICacls.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\ipconfig.exe.mui", @"C:\Windows\System32\en-US\ipconfig.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\label.exe.mui", @"C:\Windows\System32\en-US\label.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\logman.exe.mui", @"C:\Windows\System32\en-US\logman.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\mountvol.exe.mui", @"C:\Windows\System32\en-US\mountvol.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\neth.dll.mui", @"C:\Windows\System32\en-US\neth.dll.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\netsh.exe.mui", @"C:\Windows\System32\en-US\netsh.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\nslookup.exe.mui", @"C:\Windows\System32\en-US\nslookup.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\ping.exe.mui", @"C:\Windows\System32\en-US\ping.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\recover.exe.mui", @"C:\Windows\System32\en-US\recover.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\reg.exe.mui", @"C:\Windows\System32\en-US\reg.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\regsvr32.exe.mui", @"C:\Windows\System32\en-US\regsvr32.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\replace.exe.mui", @"C:\Windows\System32\en-US\replace.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\sc.exe.mui", @"C:\Windows\System32\en-US\sc.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\setx.exe.mui", @"C:\Windows\System32\en-US\setx.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\sort.exe.mui", @"C:\Windows\System32\en-US\sort.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\takeown.exe.mui", @"C:\Windows\System32\en-US\takeown.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\tzutil.exe.mui", @"C:\Windows\System32\en-US\tzutil.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\en-US\\VaultCmd.exe.mui", @"C:\Windows\System32\en-US\VaultCmd.exe.mui");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\accesschk.exe", @"C:\Windows\System32\accesschk.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\AppXTest.Common.Feature.DeployAppx.dll", @"C:\Windows\System32\AppXTest.Common.Feature.DeployAppx.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\attrib.exe", @"C:\Windows\System32\attrib.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\bcdboot.exe", @"C:\Windows\System32\bcdboot.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\bcdedit.exe", @"C:\Windows\System32\bcdedit.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\certmgr.exe", @"C:\Windows\System32\certmgr.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\CheckNetIsolation.exe", @"C:\Windows\System32\CheckNetIsolation.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\chkdsk.exe", @"C:\Windows\System32\chkdsk.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\cmd.exe", @"C:\Windows\System32\cmd.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\cmdkey.exe", @"C:\Windows\System32\cmdkey.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\cscript.exe", @"C:\Windows\System32\cscript.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\dcopy.exe", @"C:\Windows\System32\dcopy.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\depends.exe", @"C:\Windows\System32\depends.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\DeployAppx.exe", @"C:\Windows\System32\DeployAppx.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\DeployUtil.exe", @"C:\Windows\System32\DeployUtil.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\devcon.exe", @"C:\Windows\System32\devcon.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\DevToolsLauncher.exe", @"C:\Windows\System32\DevToolsLauncher.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\DIALTESTWP8.exe", @"C:\Windows\System32\DIALTESTWP8.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\Dism.exe", @"C:\Windows\System32\Dism.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\fc.exe", @"C:\Windows\System32\fc.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\find.exe", @"C:\Windows\System32\find.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\findstr.exe", @"C:\Windows\System32\findstr.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\finger.exe", @"C:\Windows\System32\finger.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\FolderPermissions.exe", @"C:\Windows\System32\FolderPermissions.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\format.com", @"C:\Windows\System32\format.com");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\ftp.exe", @"C:\Windows\System32\ftp.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\ftpd.exe", @"C:\Windows\System32\ftpd.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\FveEnable.exe", @"C:\Windows\System32\FveEnable.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\gbot.exe", @"C:\Windows\System32\gbot.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\gse.dll", @"C:\Windows\System32\gse.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\help.exe", @"C:\Windows\System32\help.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\HOSTNAME.EXE", @"C:\Windows\System32\HOSTNAME.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\icacls.exe", @"C:\Windows\System32\icacls.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\imagex.exe", @"C:\Windows\System32\imagex.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\InputProcessorClient.dll", @"C:\Windows\System32\InputProcessorClient.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\IoTSettings.exe", @"C:\Windows\System32\IoTSettings.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\IoTShell.exe", @"C:\Windows\System32\IoTShell.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\iotstartup.exe", @"C:\Windows\System32\iotstartup.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\ipconfig.exe", @"C:\Windows\System32\ipconfig.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\kill.exe", @"C:\Windows\System32\kill.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\label.exe", @"C:\Windows\System32\label.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\logman.exe", @"C:\Windows\System32\logman.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\MinDeployAppX.exe", @"C:\Windows\System32\MinDeployAppX.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\minshutdown.exe", @"C:\Windows\System32\minshutdown.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\more.com", @"C:\Windows\System32\more.com");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\mountvol.exe", @"C:\Windows\System32\mountvol.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\msnap.exe", @"C:\Windows\System32\msnap.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\mwkdbgctrl.exe", @"C:\Windows\System32\mwkdbgctrl.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\net.exe", @"C:\Windows\System32\net.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\net1.exe", @"C:\Windows\System32\net1.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\neth.dll", @"C:\Windows\System32\neth.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\neth.exe", @"C:\Windows\System32\neth.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\netsh.exe", @"C:\Windows\System32\netsh.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\nslookup.exe", @"C:\Windows\System32\nslookup.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\pacman_ierror.dll", @"C:\Windows\System32\pacman_ierror.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\pacmanerr.dll", @"C:\Windows\System32\pacmanerr.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\ping.exe", @"C:\Windows\System32\ping.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\PMTestErrorLookup.exe", @"C:\Windows\System32\PMTestErrorLookup.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\PowerTool.exe", @"C:\Windows\System32\PowerTool.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\ProvisioningTool.exe", @"C:\Windows\System32\ProvisioningTool.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\recover.exe", @"C:\Windows\System32\recover.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\reg.exe", @"C:\Windows\System32\reg.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\regsvr32.exe", @"C:\Windows\System32\regsvr32.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\replace.exe", @"C:\Windows\System32\replace.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\sc.exe", @"C:\Windows\System32\sc.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\ScreenCapture.exe", @"C:\Windows\System32\ScreenCapture.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\SendKeys.exe", @"C:\Windows\System32\SendKeys.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\setbootoption.exe", @"C:\Windows\System32\setbootoption.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\setcomputername.exe", @"C:\Windows\System32\setcomputername.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\SetDisplayResolution.exe", @"C:\Windows\System32\SetDisplayResolution.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\setx.exe", @"C:\Windows\System32\setx.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\sfpdisable.exe", @"C:\Windows\System32\sfpdisable.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\shutdown.exe", @"C:\Windows\System32\shutdown.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\SirepController.exe", @"C:\Windows\System32\SirepController.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\sleep.exe", @"C:\Windows\System32\sleep.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\sort.exe", @"C:\Windows\System32\sort.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\takeown.exe", @"C:\Windows\System32\takeown.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\TaskSchUtil.exe", @"C:\Windows\System32\TaskSchUtil.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\telnetd.exe", @"C:\Windows\System32\telnetd.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\TestDeploymentInfo.dll", @"C:\Windows\System32\TestDeploymentInfo.dll");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\TestNavigationApi.exe", @"C:\Windows\System32\TestNavigationApi.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\TH.exe", @"C:\Windows\System32\TH.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\tlist.exe", @"C:\Windows\System32\tlist.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\TouchRecorder.exe", @"C:\Windows\System32\TouchRecorder.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\tracelog.exe", @"C:\Windows\System32\tracelog.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\tree.com", @"C:\Windows\System32\tree.com");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\tzutil.exe", @"C:\Windows\System32\tzutil.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\VaultCmd.exe", @"C:\Windows\System32\VaultCmd.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\WPConPlatDev.exe", @"C:\Windows\System32\WPConPlatDev.exe");
                    Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\ConsoleApps\\xcopy.exe", @"C:\Windows\System32\xcopy.exe");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\CI", "UMCIAuditMode", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                    //Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\KeepWiFiOnSvc", "Start", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000002");
                    Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment", "Path", Helper.RegistryHelper.RegistryType.REG_EXPAND_SZ, "%SystemRoot%\\system32;%SystemRoot%;%SystemDrive%\\Programs\\CommonFiles\\System;%SystemDrive%\\wtt;%SystemDrive%\\data\\test\\bin;%SystemRoot%\\system32\\WindowsPowerShell\\v1.0;");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\SecurityManager\\PrincipalClasses\\PRINCIPAL_CLASS_TCB", "Directories", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, "C:\\ ");
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
                    /*Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh\\Parameters\\Commands", "Loopback", 1, "CheckNetIsolation.exe loopbackexempt -a -n=CMDInjector_kqyng60eng17c");
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\services\\BootSh\\Parameters\\Commands", "Telnetd", 1, "telnetd.exe cmd.exe 9999");*/
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Svchost", "bootshsvc", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, "bootsh ");
                });
                reInjectionReboot.Text = "An injection reboot is pending. Please reboot your device as soon as possible to apply the changes.";
                reInjectionBox.Visibility = Visibility.Collapsed;
                reInjectionReboot.Visibility = Visibility.Visible;
                InjectBtn.Content = "Injected";
                ToastNotificationManager.History.Remove("Re-InjectTag");
                _ = Reboot();
            }
            catch (Exception ex) { Helper.ThrowException(ex); }
        }

        private void FaqHelp_Click(Windows.UI.Xaml.Documents.Hyperlink sender, Windows.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            if (isRootFrame)
            {
                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame == null)
                {
                    rootFrame = new Frame();
                }
                rootFrame.Navigate(typeof(Help));
            }
            else
            {
                Helper.pageNavigation.Invoke(9, null);
            }
        }

        private async void InjectionInfoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Helper.build < 15063)
            {
                _ = Helper.MessageBox("Two types of injections are available:\n1. Temporary injection that wipes out on hard reset.\n2. Permanent injection that remains even after hard reset.\n\n" +
                    "Using permanent injection the CMD can work without re-injecting on hard reset and the Startup feature can now execute the commands even after a hard reset. It helps to keep the registry changes and much more.\n\n" +
                    "You can switch between the injection type any time by a re-inject. Builds below 14393 still require re-injection even if the CMD files persists.", Helper.SoundHelper.Sound.Alert, "Info");
            }
            else
            {
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Info",
                    Content = "Two types of injections are available:\n1. Temporary injection that wipes out on hard reset.\n2. Permanent injection that remains even after hard reset.\n\n" +
                    "Using permanent injection the CMD can work without re-injecting on hard reset and the Startup feature can now execute the commands even after a hard reset. It helps to keep the registry changes and much more.\n\n" +
                    "You can switch between the injection type any time by a re-inject. Builds below 14393 still require re-injection even if the CMD files persists.",
                    CloseButtonText = "Close"
                };
                Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                await contentDialog.ShowAsync();
            }
        }

        private async void UnInjectBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = await Helper.MessageBox("Are you sure you want to un-inject?\n\nThis will remove CMD from the system, but the App will still remain functional.", Helper.SoundHelper.Sound.Alert, "Confirmation", "No", true, "Yes");
            if (result == 0)
            {
                InjectBtn.IsEnabled = false;
                UnInjectBtn.IsEnabled = false;
                await FileIO.WriteTextAsync(await Helper.localFolder.CreateFileAsync("CMDInjector.dat", CreationCollisionOption.ReplaceExisting), Helper.currentBatchVersion.ToString());
                Helper.CopyFile(Helper.localFolder.Path + "\\CMDInjector.dat", @"C:\Windows\System32\CMDUninjector.dat");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\NonSystemWide.bat", @"C:\Windows\System32\CMDInjector.bat");
                Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\InjectionType\\TemporaryInjection.reg", @"C:\Windows\System32\TemporaryInjection.reg");
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment", "Path", Helper.RegistryHelper.RegistryType.REG_EXPAND_SZ, $"%SystemRoot%\\system32;%SystemRoot%;%SystemDrive%\\Programs\\CommonFiles\\System;%SystemDrive%\\wtt;%SystemDrive%\\data\\test\\bin;%SystemRoot%\\system32\\WindowsPowerShell\\v1.0;{Helper.localFolder.Path}\\CMDInjector;");
                UnInjectBtn.Content = "Un-Injected";
                reInjectionReboot.Text = "An un-injection reboot is pending. Please reboot your device as soon as possible to apply the changes.";
                reInjectionBox.Visibility = Visibility.Collapsed;
                reInjectionNote.Visibility = Visibility.Collapsed;
                reInjectionReboot.Visibility = Visibility.Visible;
                _ = Reboot();
            }
        }
    }
}
