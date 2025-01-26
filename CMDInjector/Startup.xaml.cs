using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using Windows.UI.Popups;
using ndtklib;
using CMDInjectorHelper;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CMDInjector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Startup : Page
    {
        public Startup()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Initialize();
            CommandBox.TextWrapping = Helper.LocalSettingsHelper.LoadSettings("CommandsTextWrap", false) ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }

        private async void Initialize()
        {
            try
            {
                if (HomeHelper.IsCMDInjected())
                {
                    if (File.Exists(@"C:\Windows\System32\Startup.bat"))
                    {
                        Helper.CopyFile(@"C:\Windows\System32\Startup.bat", Helper.localFolder.Path + "\\Startup.bat");
                    }
                    else
                    {
                        Helper.CopyFile(Helper.installedLocation.Path + "\\Contents\\BatchScripts\\Startup.bat", Helper.localFolder.Path + "\\Startup.bat");
                    }
                    var text = await FileIO.ReadTextAsync(await Helper.localFolder.GetFileAsync("Startup.bat"), Windows.Storage.Streams.UnicodeEncoding.Utf8);
                    CommandBox.Text = text;
                    CommandBox.Text += "\r";
                    CommandBox.Text = CommandBox.Text.Remove(CommandBox.Text.LastIndexOf("\r"));
                }
                else
                {
                    CommandBox.IsReadOnly = true;
                    _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                }
            }
            catch (Exception ex)
            {
                CommandBox.IsReadOnly = false;
                Helper.ThrowException(ex);
            }
        }

        private async void CommandBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await FileIO.WriteTextAsync(await Helper.localFolder.GetFileAsync("Startup.bat"), CommandBox.Text.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n"));
                Helper.CopyFile(Helper.localFolder.Path + "\\Startup.bat", @"C:\Windows\System32\Startup.bat");
                _ = Helper.MessageBox("The changes applied successfully.", Helper.SoundHelper.Sound.Alert, "Completed");
            }
            catch (Exception ex)
            {
                Helper.ThrowException(ex);
            }
        }
    }
}
