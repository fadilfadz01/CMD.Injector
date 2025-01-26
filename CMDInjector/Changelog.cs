using CMDInjectorHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System.Profile;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace CMDInjector
{
    class Changelog
    {
        static readonly string currentVersion = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}";
        static readonly ulong build = (ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion) & 0x00000000FFFF0000L) >> 16;

        public static async Task DisplayLog()
        {
            if (build < 15063)
            {
                await Helper.MessageBox($"\nCMD Injector v{currentVersion}\n" +
                    " • Fixed an issue with capabilities not reading correctly.\n" +
                    " • Changed KeepWiFiOnSvc to an optional setting.\n\n\n", Helper.SoundHelper.Sound.Alert, "Changelog");
            }
            else
            {
                TextBlock textBlock = new TextBlock
                {
                    TextWrapping = TextWrapping.Wrap
                };
                textBlock.Inlines.Add(new Run { Text = $"CMD Injector v{currentVersion} (Current)\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Fixed an issue with capabilities not reading correctly.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Changed KeepWiFiOnSvc to an optional setting.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = $"CMD Injector v4.0.5.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Improved App compatibility with build 14320.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added Glance font color auto changer.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added new Glance font file FHD_HI.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed App crashing on build 10572 and below.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed capabilities issue that prevent the App from installing on build 10240.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = $"CMD Injector v4.0.2.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = $"CMD Injector v4.0.1.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = $"CMD Injector v4.0.0.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added transition effect for menu navigation.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = $"CMD Injector v3.9.8.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added interval trigger for start screen auto wallpaper changer.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added an option to disable live lockscreen on battery saver mode.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Able to pin Volume Options tile to start screen.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Improved Snapper screen capturer sound effects.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Assigning CMD Injector on Search Options will now allow passing parameters.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = $"CMD Injector v3.9.4.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added automatic start screen wallpaper changer.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed an error when restoring an app backup more than once.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Updated FAQ.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = $"CMD Injector v3.9.0.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added PacMan Manager restore option.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = $"CMD Injector v3.8.6.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added CMD un-injection option.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added search button options to assign & launch custom apps on actions.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = $"CMD Injector v3.8.3.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Fixed App crashes on build 10572 and below.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed auto theme switch not switches on assigned time sometimes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Improved Terminal console output.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = $"CMD Injector v3.8.0.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • App will now popup a completion message after injection reboot.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added an option to ask user consent on App launch.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added custom schedule for auto theme switch.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = $"CMD Injector v3.7.8.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added permanent injection option that remains even after hard reset.  (Builds below 14393 still require re-injection even if the CMD files persists)\n" });
                textBlock.Inlines.Add(new Run { Text = " • The Startup feature can now execute the commands even after a hard reset on permanent injection.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added options to Launch, Move and Backup Apps from the PacMan Manager.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Removed night light settings from the TweakBox as its not working.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = $"CMD Injector v3.7.5.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added animated Splashscreen and can be customized from the App settings.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added sound effects for certain actions.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added automatic system background theme switcher.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added night light settings.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = $"CMD Injector v3.7.0.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Re-Injection allows relocking bootloader safely for devices having below 14393 build that is already injected using CMD Injector v3.4 or lower.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added tweak to customize system accent colors.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added a power option to reboot device to FFU Loader directly from the OS.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added App changelog history.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v3.5.5.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Unlocked Bootloader is no longer required for any of the W10M builds.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed getting BSOD for some specific builds.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed PacMan manager not working for build 14393 and below.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed Terminal tile not pinning if the argument contains specific symbols.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Now you are able to express your thanks by clicking the Buy me a coffee in the About menu.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Updated FAQ.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v3.4.0.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Able to set command as argument during Terminal tile pinning.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Prevented live lockscreen being running under screen unlocked state.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v3.3.0.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Fixed App crashes on launch for random users.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed PacMan errors and bugs.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added more customizations for live lockscreen in TweakBox.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v3.0.0.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added auto lockscreen wallpaper changer.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added boot progression animation tweak.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed Startup bugs.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Notify on re-injection needed.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v2.9.0.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Fixed App crashing on protocol launch.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed App crashing on Snapper notification launch.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Able to add more packages to installation queue by holding the browse button in PacMan.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v2.8.2.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Able to select & install multiple Apps from PacMan in a queue.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Scroll between previously executed commands on Terminal by using keyboard pointing stick.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Accept and install App package or manifest file through the protocol \"cmdinjector\".\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v2.7.0.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Fixed UI scaling issues.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v2.6.8.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added App manager in PacMan.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added option to control Apps loopback exemption.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added more tweaks in TweakBox.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added option to disable Snapper notification.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v2.6.0.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Improved UI.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added some console applications.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed Terminal hangs after sending too many commands.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed PacMan Stuck on progress 99% sometimes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed Snapper stops capuring or recording when navigate to BootConfig.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Prevented screen locking while screen capturing and recording.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v2.4.7.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • CMD Injector App functions can now work without injecting the CMD on build 14393 and above.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added a warning indicator when CMD not re-injected after an App update.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Improved Terminal.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Improved Startup.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v2.4.0.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Able to restore old Glance screen and its required files with one click.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed auto enabling Glance screen font colour.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed auto enabling Navigation bar tweaks.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v2.3.8.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added TweakBox back.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added loopback exemption for Command Prompt by Empyreal96.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Able to pin File Explorer folders to start screen.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added System update, Glance screen, Navigation bar, and many other Tweaks.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Disabled camera button required settings from the BootConfig until camera button get pressed once from the BootConfig menu.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v2.2.5.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Improved overall App performance.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Improved App update checking.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Able to quick stop Snapper screen capturing and recoding from notification panel.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Updated FAQ.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v2.1.3.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added a warning instead of disabling inject button for build 10586 & lower having locked BL.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added loopback exemption for WUT Lite by Bashar Astifan.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed boot App Developer Menu not working for build 15063 & higher.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Improved stability for BootConfig.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Able to pin hamburger menu items tile to start screen.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Able to check the App update from About.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Updated FAQ.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v2.0.0.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added GUI for Bcdedit in BootConfig.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Improved Terminal.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Able to install boot App Developer Menu.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Able to use PacMan as default App installer.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Able to pin Power Options tile to start screen.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v1.9.1.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Check bootloader state and disable Inject button for locked bootloader to avoid getting BSOD.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Able to install, update or register signed/unsigned XAP & APPX/BUNDLE packages from PacMan.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Able to install App packages to SD card.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added screen recorder in Snapper.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added some tweaks in TweakBox.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added FAQ in Help.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v1.8.4.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added App support from build 10240.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added XAP package installation feature.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added advanced screenshot capturing feature.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added loopback exemption for Windows Universal Tool (WUT) by Bashar Astifan.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added various settings.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v1.7.6.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added built-in CMD shell.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed App crashes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v1.6.0.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added PowerShell support.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Added custom startup commands feature.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Removed permanent injection as it's not working.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v1.5.2.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added some console applications.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Able to Re-Inject CMD on already injected device.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Able to switch to permanent injection from temporary injection.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Enabled loopback exemption for Token2Shell & Windows Universal Tool (W10M group app).\n" });
                textBlock.Inlines.Add(new Run { Text = " • Bug fixes.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Many other improvements.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v1.3.6.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Added temporary and permanent injection option.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Fixed UI for large scale display and for landscape mode.\n" });
                textBlock.Inlines.Add(new Run { Text = " • Removed step running \"Setup\" command on first CMD connection.\n\n\n" });
                textBlock.Inlines.Add(new Run { Text = "CMD Injector v1.1.0.0\n", FontWeight = Windows.UI.Text.FontWeights.Bold });
                textBlock.Inlines.Add(new Run { Text = " • Initial public release.\n\n\n" });
                ScrollViewer scrollViewer = new ScrollViewer
                {
                    Content = textBlock
                };
                ContentDialog contentDialog = new ContentDialog
                {
                    Title = "Changelog",
                    Content = scrollViewer,
                    CloseButtonText = "Close"
                };
                Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                await contentDialog.ShowAsync();
            }
        }
    }
}
