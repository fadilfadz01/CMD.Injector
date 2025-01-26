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
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.Storage;
using System.Text.RegularExpressions;
using Windows.Foundation.Metadata;
using Windows.UI.ViewManagement;
using ndtklib;
using System.Text;
using Windows.System.Profile;
using Windows.ApplicationModel;
using Windows.Phone.UI.Input;
using System.Threading;
using CMDInjectorHelper;
using WinUniversalTool;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CMDInjector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BootConfig : Page
    {
        NRPC rpc = new NRPC();
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient tClient = new TelnetClient(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        StatusBarProgressIndicator Indicator { get; set; }
        string[] Identifier = new string[100];
        int flag = 0;

        private void Connect()
        {
            _ = tClient.Connect();
            long i = 0;
            while (tClient.IsConnected == false && i < 1000000)
            {
                i++;
            }
        }

        private async Task SendCommand(string command)
        {
            await Task.Delay(200);
            await Task.Run(() =>
            {
                _ = tClient.Send(command);
            });
        }

        private byte[] ToBinary(string data)
        {
            data = data.Replace("-", "");
            return Enumerable.Range(0, data.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(data.Substring(x, 2), 16)).ToArray();
        }

        /*private string[] ToMultiSZ(string data)
        {
            string[] MultiSZ = new string[0];
            MultiSZ[0] = data;
            return MultiSZ;
        }*/

        private async Task GetEntries()
        {
            try
            {
                flag = 1;
                if (!Helper.LocalSettingsHelper.LoadSettings("HaveCamera", false) && !Helper.LocalSettingsHelper.LoadSettings("UnlockHidden", false))
                {
                    VolUpBox.Items.Add(new ComboBoxItem { Content = "None (Use for navigation)", IsEnabled = false });
                    VolDownBox.Items.Add(new ComboBoxItem { Content = "None (Use for navigation)", IsEnabled = false });
                }
                else
                {
                    VolUpBox.Items.Add(new ComboBoxItem { Content = "None (Use for navigation)", IsEnabled = true });
                    VolDownBox.Items.Add(new ComboBoxItem { Content = "None (Use for navigation)", IsEnabled = true });
                }
                DescriptionBox.Text = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ);

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\24000001", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ) != string.Empty)
                {
                    string[] DisplayOrder = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\24000001", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ).Split(' ');
                    for (int i = 0; i < DisplayOrder.Length; i++)
                    {
                        DisplayOrderList.Items.Add(Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + DisplayOrder[i] + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ));
                    }
                }
                if (DisplayOrderList.Items.Count <= 1)
                {
                    RemoveBtn.IsEnabled = false;
                    MoveUpBtn.IsEnabled = false;
                }
                else
                {
                    RemoveBtn.IsEnabled = true;
                    MoveUpBtn.IsEnabled = true;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\16000049", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    ManTestSigningTog.IsOn = true;
                }
                else
                {
                    ManTestSigningTog.IsOn = false;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\16000048", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    ManNoIntegrityChecksTog.IsOn = true;
                }
                else
                {
                    ManNoIntegrityChecksTog.IsOn = false;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\25000004", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY) != string.Empty)
                {
                    string[] HexTimeout = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\25000004", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).ToCharArray().Select(c => c.ToString()).ToArray();
                    TimeoutBox.Text = Convert.ToString(Convert.ToInt32(HexTimeout[0] + HexTimeout[1], 16));
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\26000020", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    BootMenuTog.IsOn = true;
                }
                else
                {
                    BootMenuTog.IsOn = false;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\16000049", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    LoadTestSigningTog.IsOn = true;
                }
                else
                {
                    LoadTestSigningTog.IsOn = false;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\16000048", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    LoadNoIntegrityChecksTog.IsOn = true;
                }
                else
                {
                    LoadNoIntegrityChecksTog.IsOn = false;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\1600007e", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    LoadFlightSignTog.IsOn = true;
                }
                else
                {
                    LoadFlightSignTog.IsOn = false;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\250000c2", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY) != string.Empty)
                {
                    string[] BootMenuPol = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\250000c2", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).ToCharArray().Select(c => c.ToString()).ToArray();
                    BootMenuPolBox.SelectedIndex = Convert.ToInt32(BootMenuPol[1]);
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{6efb52bf-1766-41db-a6b3-0ee5eff72bd7}\\Elements\\16000040", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    AdvOptTog.IsOn = true;
                }
                else
                {
                    AdvOptTog.IsOn = false;
                }

                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{6efb52bf-1766-41db-a6b3-0ee5eff72bd7}\\Elements\\16000041", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                {
                    OptEditTog.IsOn = true;
                }
                else
                {
                    OptEditTog.IsOn = false;
                }

                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    File.Delete($"{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    File.Delete($"{Helper.localFolder.Path}\\BootConfigObjects.txt");
                    _ = SendCommand($"for /f \"delims=\\ tokens=4\" %a in ('reg query hklm\\bcd00000001\\objects') do echo %a >>{Helper.localFolder.Path}\\BootConfigObjects.txt&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    while (File.Exists($"{Helper.localFolder.Path}\\BootConfigEnd.txt") == false)
                    {
                        await Task.Delay(200);
                    }
                    //string[] Objects = File.ReadAllLines($"{Helper.localFolder.Path}\\BootConfigObjects.txt");
                    await Task.Delay(1500);
                    string[] Objects = await GetObjects();
                    /*string toDisplay = string.Join(Environment.NewLine, Objects);
                    _ = new MessageDialog(toDisplay).ShowAsync();*/
                    var foundDevMenu = false;
                    int j = 0;
                    for (int i = 0; i < Objects.Length; i++)
                    {
                        if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ) == string.Empty || Regex.Replace(Objects[i], @"\s+", "") == "{311b88b5-9b30-491d-bad9-167ca3e2d417}" || Regex.Replace(Objects[i], @"\s+", "") == "{01de5a27-8705-40db-bad6-96fa5187d4a6}" || Regex.Replace(Objects[i], @"\s+", "") == "{0ce4991b-e6b3-4b16-b23c-5e0d9250e5d9}" || Regex.Replace(Objects[i], @"\s+", "") == "{4636856e-540f-4170-a130-a84776f4c654}" || Regex.Replace(Objects[i], @"\s+", "") == "{6efb52bf-1766-41db-a6b3-0ee5eff72bd7}" || Regex.Replace(Objects[i], @"\s+", "") == "{7ea2e1ac-2e61-4728-aaa3-896d9d0a9f0e}" || Regex.Replace(Objects[i], @"\s+", "") == "{ae5534e0-a924-466c-b836-758539a3ee3a}" || Regex.Replace(Objects[i], @"\s+", "") == "{9dea862c-5cdd-4e70-acc1-f32b344d4795}")
                        {
                            continue;
                        }
                        Identifier[j] = Regex.Replace(Objects[i], @"\s+", "");
                        if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ) == "Developer Menu")
                        {
                            foundDevMenu = true;
                            DevMenuBtn.Content = "Uninstall";
                            if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\16000049", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                            {
                                DevTestSigningTog.IsOn = true;
                            }
                            if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\16000048", "Element", Helper.RegistryHelper.RegistryType.REG_BINARY).Contains("01"))
                            {
                                DevNoIntegrityChecksTog.IsOn = true;
                            }
                            if (!Helper.IsSecureBootPolicyInstalled() && Helper.LocalSettingsHelper.LoadSettings("UnlockHidden", false))
                            {
                                DevTestSigningTog.IsEnabled = true;
                                DevNoIntegrityChecksTog.IsEnabled = true;
                            }
                        }
                        else
                        {
                            if (!foundDevMenu)
                            {
                                DevMenuBtn.Content = "Install";
                                if (Helper.IsSecureBootPolicyInstalled() && !Helper.LocalSettingsHelper.LoadSettings("UnlockHidden", false))
                                {
                                    DevTestSigningTog.IsEnabled = false;
                                    DevNoIntegrityChecksTog.IsEnabled = false;
                                }
                            }
                        }
                        if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\24000001", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ).Contains(Regex.Replace(Objects[i], @"\s+", "")) == false)
                        {
                            MenuFlyoutItem Items = new MenuFlyoutItem();
                            Items.Click += Items_Click;
                            Items.Text = Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ);
                            AddFlyMenu.Items.Add(Items);
                        }
                        DefaultBox.Items.Add(Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ));
                        VolUpBox.Items.Add(Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ));
                        VolDownBox.Items.Add(Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Regex.Replace(Objects[i], @"\s+", "") + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ));
                        if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\23000003", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ) == Regex.Replace(Objects[i], @"\s+", ""))
                        {
                            DefaultBox.SelectedIndex = j;
                        }
                        if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\54000001", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ) == Regex.Replace(Objects[i], @"\s+", ""))
                        {
                            VolUpBox.SelectedIndex = j + 1;
                        }
                        if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\54000002", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ) == Regex.Replace(Objects[i], @"\s+", ""))
                        {
                            VolDownBox.SelectedIndex = j + 1;
                        }
                        j++;
                    }
                    if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\54000001", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ) == string.Empty)
                    {
                        VolUpBox.SelectedIndex = 0;
                    }
                    if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\54000002", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ) == string.Empty)
                    {
                        VolDownBox.SelectedIndex = 0;
                    }
                    DefaultBox.IsEnabled = true;
                    if (DisplayOrderList.Items.Count > 0) SaveBtn.IsEnabled = true;
                    VolUpBox.IsEnabled = true;
                    VolDownBox.IsEnabled = true;
                    DevMenuBtn.IsEnabled = true;
                    Indicator.ProgressValue = 0;
                    Indicator.Text = string.Empty;
                }
                flag = 0;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Index was outside the bounds of the array." || ex.Message == "Arg_IndexOutOfRangeException")
                {
                    DisplayOrderList.Items.Clear();
                    await GetEntries();
                }
                else if (ex.Message == "Object reference not set to an instance of an object." || ex.Message == "Arg_NullReferenceException")
                {
                    return;
                }
                else
                {

                }
            }
            Helper.LocalSettingsHelper.SaveSettings("UnlockHidden", false);
        }

        private async void StatusIndicator()
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                Indicator = statusBar.ProgressIndicator;
                Indicator.ProgressValue = null;
                Indicator.Text = "Reading...";
                await Indicator.ShowAsync();
            }
        }

        private async void StatusProgress()
        {
            File.Delete($"{Helper.localFolder.Path}\\BootConfigEnd.txt");
            Indicator.ProgressValue = null;
            Indicator.Text = "Writing...";
            while (File.Exists($"{Helper.localFolder.Path}\\BootConfigEnd.txt") == false)
            {
                await Task.Delay(200);
            }
            Indicator.ProgressValue = 0;
            Indicator.Text = string.Empty;
        }

        public BootConfig()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            HardwareButtons.CameraPressed += HardwareButtons_CameraPressed;
            FirstRun();
            Connect();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (tClient.IsConnected && HomeHelper.IsConnected())
            {
                StatusIndicator();
                DefaultBox.Items.Clear();
                AddFlyMenu.Items.Clear();
                VolUpBox.Items.Clear();
                VolDownBox.Items.Clear();
            }
            DisplayOrderList.Items.Clear();
            if (Helper.IsSecureBootPolicyInstalled() && !Helper.LocalSettingsHelper.LoadSettings("UnlockHidden", false))
            {
                ManNoIntegrityChecksTog.IsEnabled = false;
                ManTestSigningTog.IsEnabled = false;
                LoadNoIntegrityChecksTog.IsEnabled = false;
                LoadTestSigningTog.IsEnabled = false;
                DevTestSigningTog.IsEnabled = false;
                DevNoIntegrityChecksTog.IsEnabled = false;
            }
            else
            {
                ManNoIntegrityChecksTog.IsEnabled = true;
                ManTestSigningTog.IsEnabled = true;
                LoadNoIntegrityChecksTog.IsEnabled = true;
                LoadTestSigningTog.IsEnabled = true;
                DevTestSigningTog.IsEnabled = true;
                DevNoIntegrityChecksTog.IsEnabled = true;
            }
            if (!Helper.LocalSettingsHelper.LoadSettings("HaveCamera", false) && !Helper.LocalSettingsHelper.LoadSettings("UnlockHidden", false))
            {
                AdvOptTog.IsEnabled = false;
                OptEditTog.IsEnabled = false;
            }
            else
            {
                AdvOptTog.IsEnabled = true;
                OptEditTog.IsEnabled = true;
            }
            _ = GetEntries();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            StatusBar statusBar = StatusBar.GetForCurrentView();
            Indicator = statusBar.ProgressIndicator;
            Indicator.ProgressValue = 0;
            Indicator.Text = string.Empty;
        }

        private async void FirstRun()
        {
            if (Helper.LocalSettingsHelper.LoadSettings("BootConfigNote", true))
            {
                await Helper.MessageBox("It is recommended to install the Developer Menu and add it to the DisplayOrder or one of the Volume buttons to recover the BCD in case of corruption.", Helper.SoundHelper.Sound.Alert, "Warning");
                await Helper.MessageBox("Some of the settings are disabled by default. You have to press the Camera button from the BootConfig menu once to enable these settings. These settings are only for devices having Camera button.", Helper.SoundHelper.Sound.Alert, "Note");
                Helper.LocalSettingsHelper.SaveSettings("BootConfigNote", false);
            }
        }

        private void HardwareButtons_CameraPressed(object sender, CameraEventArgs e)
        {
            if (Helper.LocalSettingsHelper.LoadSettings("HaveCamera", false) == false)
            {
                Helper.LocalSettingsHelper.SaveSettings("HaveCamera", true);
                AdvOptTog.IsEnabled = true;
                OptEditTog.IsEnabled = true;
            }
        }

        private async void DefaultBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (flag == 0 && DefaultBox.SelectedItem != null)
            {
                //DefaultBox.IsEnabled = false;
                //statusProgress();
                //_ = sendCommand("bcdedit /set {bootmgr} default \"" + Identifier[DefaultBox.SelectedIndex] + $"\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\23000003", "Element", Helper.RegistryHelper.RegistryType.REG_SZ, Identifier[DefaultBox.SelectedIndex]);
                if (Identifier[DefaultBox.SelectedIndex] != "{7619dcc9-fafe-11d9-b411-000476eba25f}")
                {
                    await Helper.MessageBox("Make sure the Windows Loader is selected in DisplayOrder or any of the Volume buttons. Otherwise you won't be able to boot to Windows anymore.", Helper.SoundHelper.Sound.Alert, "Warning");
                }
                //DefaultBox.IsEnabled = true;
            }
        }

        private void ManTestSigningTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (ManTestSigningTog.IsOn)
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\16000049", "Element", 3, ToBinary("01"));
            }
            else
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\16000049", "Element", 3, ToBinary("00"));
            }
        }

        private void ManNoIntegrityChecksTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (ManNoIntegrityChecksTog.IsOn)
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\16000048", "Element", 3, ToBinary("01"));
            }
            else
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\16000048", "Element", 3, ToBinary("00"));
            }
        }

        private void TimeoutBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TimeoutBox.Text == string.Empty || TimeoutBox.Text.Contains("."))
            {
                TimeoutBtn.IsEnabled = false;
            }
            else
            {
                TimeoutBtn.IsEnabled = true;
            }
        }

        private void TimeoutBtn_Click(object sender, RoutedEventArgs e)
        {
            rpc.RegSetValueW(1, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\25000004", "Element", 3, BitConverter.GetBytes(Convert.ToInt32(TimeoutBox.Text)));
        }

        private void BootMenuTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (BootMenuTog.IsOn)
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\26000020", "Element", 3, ToBinary("01"));
            }
            else
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\26000020", "Element", 3, ToBinary("00"));
            }
        }

        private async void VolUpBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VolUpBox.IsEnabled = false;
            if (flag == 0 && VolUpBox.SelectedItem != null)
            {
                StatusProgress();
                if (VolUpBox.SelectedIndex != 0)
                {
                    if (VolDownBox.SelectedIndex != 0)
                    {
                        await SendCommand($"bcdedit /set {{bootmgr}} customactions \"0x1000048000001\" \"0x54000001\" \"0x1000050000001\" \"0x54000002\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                    else
                    {
                        await SendCommand($"bcdedit /set {{bootmgr}} customactions \"0x1000048000001\" \"0x54000001\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\54000001", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, Regex.Replace(Identifier[VolUpBox.SelectedIndex - 1], @"\s+", ""));
                }
                else
                {
                    if (VolDownBox.SelectedIndex != 0)
                    {
                        await SendCommand($"reg delete \"hklm\\bcd00000001\\objects\\{{9dea862c-5cdd-4e70-acc1-f32b344d4795}}\\elements\\54000001\" /f & bcdedit /set {{bootmgr}} customactions  \"0x1000050000001\" \"0x54000002\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                    else
                    {
                        await SendCommand($"reg delete \"hklm\\bcd00000001\\objects\\{{9dea862c-5cdd-4e70-acc1-f32b344d4795}}\\elements\\54000001\" /f & reg delete \"hklm\\bcd00000001\\objects\\{{9dea862c-5cdd-4e70-acc1-f32b344d4795}}\\elements\\27000030\" /f&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                }
            }
            VolUpBox.IsEnabled = true;
        }

        private async void VolDownBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VolDownBox.IsEnabled = false;
            if (flag == 0 && VolUpBox.SelectedItem != null)
            {
                StatusProgress();
                if (VolDownBox.SelectedIndex != 0)
                {
                    if (VolUpBox.SelectedIndex != 0)
                    {
                        await SendCommand($"bcdedit /set {{bootmgr}} customactions \"0x1000048000001\" \"0x54000001\" \"0x1000050000001\" \"0x54000002\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                    else
                    {
                        await SendCommand($"bcdedit /set {{bootmgr}} customactions \"0x1000050000001\" \"0x54000002\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                    Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\54000002", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, Regex.Replace(Identifier[VolDownBox.SelectedIndex - 1], @"\s+", ""));
                }
                else
                {
                    if (VolUpBox.SelectedIndex != 0)
                    {
                        await SendCommand($"reg delete \"hklm\\bcd00000001\\objects\\{{9dea862c-5cdd-4e70-acc1-f32b344d4795}}\\elements\\54000002\" /f & bcdedit /set {{bootmgr}} customactions  \"0x1000048000001\" \"0x54000001\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                    else
                    {
                        await SendCommand($"reg delete \"hklm\\bcd00000001\\objects\\{{9dea862c-5cdd-4e70-acc1-f32b344d4795}}\\elements\\54000002\" /f & reg delete \"hklm\\bcd00000001\\objects\\{{9dea862c-5cdd-4e70-acc1-f32b344d4795}}\\elements\\27000030\" /f&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                    }
                }
            }
            VolDownBox.IsEnabled = true;
        }

        private async void DescriptionBtn_Click(object sender, RoutedEventArgs e)
        {
            DescriptionBtn.IsEnabled = false;
            Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ, DescriptionBox.Text);
            Indicator.ProgressValue = null;
            Indicator.Text = "Writing";
            DefaultBox.Items.Clear();
            AddFlyMenu.Items.Clear();
            VolUpBox.Items.Clear();
            VolDownBox.Items.Clear();
            DisplayOrderList.Items.Clear();
            await GetEntries();
            DescriptionBtn.IsEnabled = true;
        }

        private void DescriptionBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DescriptionBox.Text == string.Empty || string.IsNullOrWhiteSpace(DescriptionBox.Text))
            {
                DescriptionBtn.IsEnabled = false;
            }
            else
            {
                DescriptionBtn.IsEnabled = true;
            }
        }

        private void LoadTestSigningTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (LoadTestSigningTog.IsOn)
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\16000049", "Element", 3, ToBinary("01"));
            }
            else
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\16000049", "Element", 3, ToBinary("00"));
            }
        }

        private void LoadNoIntegrityChecksTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (LoadNoIntegrityChecksTog.IsOn)
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\16000048", "Element", 3, ToBinary("01"));
            }
            else
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\16000048", "Element", 3, ToBinary("00"));
            }
        }

        private void LoadFlightSignTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (LoadFlightSignTog.IsOn)
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\1600007e", "Element", 3, ToBinary("01"));
                rpc.FileCopy(Helper.installedLocation.Path + "\\Contents\\Certificates\\SbcpFlightToken.p7b", "C:\\EFIESP\\efi\\Microsoft\\boot\\policies\\SbcpFlightToken.p7b", 0);
            }
            else
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\1600007e", "Element", 3, ToBinary("00"));
            }
        }

        private void BootMenuPolBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BootMenuPolBox.SelectedIndex == 0)
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\250000c2", "Element", 3, ToBinary("00"));
            }
            else
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{7619dcc9-fafe-11d9-b411-000476eba25f}\\Elements\\250000c2", "Element", 3, ToBinary("01"));
            }
        }

        private void AdvOptTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (AdvOptTog.IsOn)
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{6efb52bf-1766-41db-a6b3-0ee5eff72bd7}\\Elements\\16000040", "Element", 3, ToBinary("01"));
                if (flag == 0)
                {
                    _ = Helper.MessageBox("Make sure BootMenuPolicy is set to Legacy, otherwise you won't be able to boot to the Windows anymore.", Helper.SoundHelper.Sound.Alert, "Warning");
                }
            }
            else
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{6efb52bf-1766-41db-a6b3-0ee5eff72bd7}\\Elements\\16000040", "Element", 3, ToBinary("00"));
            }
        }

        private void OptEditTog_Toggled(object sender, RoutedEventArgs e)
        {
            if (OptEditTog.IsOn)
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{6efb52bf-1766-41db-a6b3-0ee5eff72bd7}\\Elements\\16000041", "Element", 3, ToBinary("01"));
            }
            else
            {
                rpc.RegSetValueW(1, "BCD00000001\\Objects\\{6efb52bf-1766-41db-a6b3-0ee5eff72bd7}\\Elements\\16000041", "Element", 3, ToBinary("00"));
            }
        }

        private void MoveUpBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayOrderList.SelectedIndex == 0 || DisplayOrderList.SelectedIndex == -1)
            {
                return;
            }
            int newIndex = DisplayOrderList.SelectedIndex - 1;
            object selectedIndex = DisplayOrderList.SelectedItem;
            DisplayOrderList.Items.Remove(selectedIndex);
            DisplayOrderList.Items.Insert(newIndex, selectedIndex);
            DisplayOrderList.SelectedIndex = newIndex;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            /*SaveBtn.IsEnabled = false;
            statusProgress();*/
            string orderList = string.Empty;
            for (int i = 0; i < DisplayOrderList.Items.Count; i++)
            {
                for (int j = 0; j < DefaultBox.Items.Count; j++)
                {
                    if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Identifier[j] + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ) == DisplayOrderList.Items[i].ToString())
                    {
                        orderList += /*"\"" +*/ Identifier[j] + " " /*+ "\" "*/;
                        break;
                    }
                }
            }
            //await sendCommand("bcdedit /set {bootmgr} displayorder " + orderList + $"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
            Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\{9dea862c-5cdd-4e70-acc1-f32b344d4795}\\Elements\\24000001", "Element", Helper.RegistryHelper.RegistryType.REG_MULTI_SZ, orderList);
            /*SaveBtn.IsEnabled = true;
            InputPane.GetForCurrentView().TryHide();*/
        }

        private void RemoveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayOrderList.SelectedIndex == -1)
            {
                return;
            }
            MenuFlyoutItem Items = new MenuFlyoutItem();
            Items.Click += Items_Click;
            Items.Text = DisplayOrderList.SelectedItem.ToString();
            AddFlyMenu.Items.Add(Items);
            DisplayOrderList.Items.Remove(DisplayOrderList.SelectedItem);
            if (DisplayOrderList.Items.Count <= 1)
            {
                RemoveBtn.IsEnabled = false;
                MoveUpBtn.IsEnabled = false;
            }
        }

        private void Items_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem clickedItem = (MenuFlyoutItem)sender;
            DisplayOrderList.Items.Add(clickedItem.Text);
            AddFlyMenu.Items.Remove(clickedItem);
            if (DisplayOrderList.Items.Count > 1)
            {
                RemoveBtn.IsEnabled = true;
                MoveUpBtn.IsEnabled = true;
            }
            SaveBtn.IsEnabled = true;
        }

        private async void DevMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            DevMenuBtn.IsEnabled = false;
            File.Delete($"{Helper.localFolder.Path}\\BootConfigEnd.txt");
            Indicator.ProgressValue = null;
            Indicator.Text = "Writing...";
            if ((DevMenuBtn.Content as string) == "Install")
            {
                if (Helper.build < 14393)
                {
                    rpc.FileCopy(Helper.installedLocation.Path + "\\Contents\\DeveloperMenu\\developermenu1607.efi", "C:\\EFIESP\\Windows\\System32\\Boot\\developermenu.efi", 0);
                    await SendCommand("bcdedit /create {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} /d \"Developer Menu\" /application \"bootapp\"" +
                    "&bcdedit /set {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} path \"\\windows\\system32\\BOOT\\developermenu.efi\"" +
                    "&bcdedit /set {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} device \"partition=%SystemDrive%\\Efiesp\"" +
                    "&bcdedit /set {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} inherit \"{bootloadersettings}\"" +
                    //"&bcdedit /set {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} isolatedcontext \"yes\"" +
                    //"&bcdedit /set {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} nointegritychecks \"yes\"" +
                    $"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                }
                else
                {
                    if (Helper.build == 14393)
                    {
                        rpc.FileCopy(Helper.installedLocation.Path + "\\Contents\\DeveloperMenu\\developermenu1607.efi", "C:\\EFIESP\\Windows\\System32\\Boot\\developermenu.efi", 0);
                    }
                    else
                    {
                        rpc.FileCopy(Helper.installedLocation.Path + "\\Contents\\DeveloperMenu\\developermenu1709.efi", "C:\\EFIESP\\Windows\\System32\\Boot\\developermenu.efi", 0);
                    }
                    await SendCommand("bcdedit /create {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} /d \"Developer Menu\" /application \"bootapp\"" +
                    "&bcdedit /set {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} path \"\\windows\\system32\\BOOT\\developermenu.efi\"" +
                    "&bcdedit /set {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} device \"partition=%SystemDrive%\\Efiesp\"" +
                    "&bcdedit /set {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} inherit \"{bootloadersettings}\"" +
                    "&bcdedit /set {dcc0bd7c-ed9d-49d6-af62-23a3d901117b} isolatedcontext \"yes\"" +
                    $"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                }
                rpc.FileCopy(Helper.installedLocation.Path + "\\Contents\\DeveloperMenu\\ui\\boot.ums.connected.bmpx", "C:\\EFIESP\\Windows\\System32\\Boot\\ui\\boot.ums.connected.bmpx", 0);
                rpc.FileCopy(Helper.installedLocation.Path + "\\Contents\\DeveloperMenu\\ui\\boot.ums.disconnected.bmpx", "C:\\EFIESP\\Windows\\System32\\Boot\\ui\\boot.ums.disconnected.bmpx", 0);
                rpc.FileCopy(Helper.installedLocation.Path + "\\Contents\\DeveloperMenu\\ui\\boot.ums.waiting.bmpx", "C:\\EFIESP\\Windows\\System32\\Boot\\ui\\boot.ums.waiting.bmpx", 0);
            }
            else
            {
                for (int i = 0; i < DefaultBox.Items.Count; i++)
                {
                    if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Identifier[i] + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ) == "Developer Menu")
                    {
                        await SendCommand("bcdedit /delete \"" + Identifier[i] + $"\"&echo. >{Helper.localFolder.Path}\\BootConfigEnd.txt");
                        break;
                    }
                }
            }
            while (File.Exists($"{Helper.localFolder.Path}\\BootConfigEnd.txt") == false)
            {
                await Task.Delay(200);
            }
            DefaultBox.Items.Clear();
            AddFlyMenu.Items.Clear();
            VolUpBox.Items.Clear();
            VolDownBox.Items.Clear();
            DisplayOrderList.Items.Clear();
            await GetEntries();
            DevMenuBtn.IsEnabled = true;
        }

        private void DevTestSigningTog_Toggled(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < DefaultBox.Items.Count; i++)
            {
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Identifier[i] + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ) == "Developer Menu")
                {
                    if (DevTestSigningTog.IsOn)
                    {
                        rpc.RegSetValueW(1, "BCD00000001\\Objects\\" + Identifier[i] + "\\Elements\\16000049", "Element", 3, ToBinary("01"));
                    }
                    else
                    {
                        rpc.RegSetValueW(1, "BCD00000001\\Objects\\" + Identifier[i] + "\\Elements\\16000049", "Element", 3, ToBinary("00"));
                    }
                    break;
                }
            }
        }

        private void DevNoIntegrityChecksTog_Toggled(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < DefaultBox.Items.Count; i++)
            {
                if (Helper.RegistryHelper.GetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "BCD00000001\\Objects\\" + Identifier[i] + "\\Elements\\12000004", "Element", Helper.RegistryHelper.RegistryType.REG_SZ) == "Developer Menu")
                {
                    if (DevNoIntegrityChecksTog.IsOn)
                    {
                        rpc.RegSetValueW(1, "BCD00000001\\Objects\\" + Identifier[i] + "\\Elements\\16000048", "Element", 3, ToBinary("01"));
                    }
                    else
                    {
                        rpc.RegSetValueW(1, "BCD00000001\\Objects\\" + Identifier[i] + "\\Elements\\16000048", "Element", 3, ToBinary("00"));
                    }
                    break;
                }
            }
        }

        private async Task<string[]> GetObjects()
        {
            using (var reader = File.OpenText($"{Helper.localFolder.Path}\\BootConfigObjects.txt"))
            {
                var Objects = await reader.ReadToEndAsync();
                return Objects.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            }
        }
    }
}
