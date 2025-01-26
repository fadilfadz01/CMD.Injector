using CMDInjectorHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.Security.Credentials.UI;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using XamlBrewer.Uwp.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CMDInjector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
            Helper.pageNavigation = (sender, args) =>
            {
                if ((int)sender == 45 || (int)sender == 0)
                {
                    MySplitView.CompactPaneLength = (int)sender;
                }
                else
                {
                    HamburgItems.SelectedIndex = (int)sender;
                }
            };
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            if (Helper.LocalSettingsHelper.LoadSettings("SplitView", false) == false)
            {
                MySplitView.CompactPaneLength = 0;
            }
            Init();
        }

        private async void Init()
        {
            await Helper.WaitAppLaunch();
            HamburgItems.SelectedIndex = 0;
            MyFrame.Navigate(typeof(CMDInjector));
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await Helper.WaitAppLaunch();
            base.OnNavigatedTo(e);
            if (e.Parameter == null)
            {
                return;
            }
            if (e.Parameter.ToString() == "OpenSnapper" || e.Parameter.ToString() == "StopSnapper=StopCapturing;OpenSnapper" || e.Parameter.ToString() == "StopSnapper=StopRecording;OpenSnapper" || e.Parameter.ToString() == "OpenImage" || e.Parameter.ToString() == "OpenVideo")
            {
                HamburgItems.SelectedIndex = 5;
                MyFrame.Navigate(typeof(Snapper), e.Parameter);
            }
            else if (e.Parameter.ToString() == "Updation" || e.Parameter.ToString() == "DownloadUpdate")
            {
                HamburgItems.SelectedIndex = 10;
                MyFrame.Navigate(typeof(About), e.Parameter);
            }
            else if (e.Parameter.ToString() == "ReinjectionRequired" || e.Parameter.ToString() == "InjectCMD=Re-Inject;ReinjectionRequired")
            {
                HamburgItems.SelectedIndex = 1;
                MyFrame.Navigate(typeof(Home), e.Parameter);
            }
            else if (e.Parameter is FileActivatedEventArgs || e.Parameter is ProtocolActivatedEventArgs)
            {
                StorageFile strFilePath;
                if (e.Parameter is FileActivatedEventArgs)
                {
                    strFilePath = (e.Parameter as FileActivatedEventArgs).Files[0] as StorageFile;
                }
                else
                {
                    var protocolArgs = e.Parameter as ProtocolActivatedEventArgs;
                    var AbsoluteURI = protocolArgs.Uri.AbsoluteUri;
                    var cleanToken = AbsoluteURI.Replace("cmdinjector::", "").Replace("cmdinjector:", "");
                    strFilePath = await SharedStorageAccessManager.RedeemTokenForFileAsync(cleanToken);
                }
                if (Path.GetExtension(strFilePath.Path.ToString().ToLower()) == ".xap" || Path.GetExtension(strFilePath.Path.ToString().ToLower()) == ".appx" || Path.GetExtension(strFilePath.Path.ToString().ToLower()) == ".appxbundle" || Path.GetExtension(strFilePath.Path.ToString().ToLower()) == ".xml" || Path.GetExtension(strFilePath.Path).ToLower() == ".pmlog" || Path.GetExtension(strFilePath.Path).ToLower() == ".pmbak")
                {
                    HamburgItems.SelectedIndex = 4;
                    MyFrame.Navigate(typeof(PacMan), e.Parameter);
                }
                else
                {
                    HamburgItems.SelectedIndex = 0;
                    MyFrame.Navigate(typeof(CMDInjector));
                }

            }
        }

        private async void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            //Code to disable the back button
            e.Handled = true;

            //Here you can add your own code and perfrom any task
            if (CMDInjectorPage.IsSelected || MyFrame.SourcePageType == typeof(Login))
            {
                try
                {
                    var result = await Helper.MessageBox("Are you sure you want to close the App?", Helper.SoundHelper.Sound.Alert, "CMD Injector", "No", true, "Yes");
                    if (result == 0)
                    {
                        CoreApplication.Exit();
                    }
                }
                catch (Exception ex) { Helper.ThrowException(ex); }
            }
            else
            {
                CMDInjectorPage.IsSelected = true;
                MyFrame.Navigate(typeof(CMDInjector));
                if (Helper.LocalSettingsHelper.LoadSettings("MenuTransition", true) && Helper.build >= 10572) (MyFrame.Content as Page).OpenFromSplashScreen(Helper.rect, Helper.color, new Uri("ms-appx:///Assets/Images/Transitions/Transparent.png"));
            }
        }

        private void HamburgBtn_Click(object sender, RoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void HamburgItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MySplitView.IsPaneOpen = false;

            if (CMDInjectorPage.IsSelected) MyFrame.Navigate(typeof(CMDInjector));
            else if (HomePage.IsSelected) MyFrame.Navigate(typeof(Home));
            else if (TerminalPage.IsSelected) MyFrame.Navigate(typeof(Terminal));
            else if (StartupPage.IsSelected) MyFrame.Navigate(typeof(Startup));
            else if (PacManPage.IsSelected) MyFrame.Navigate(typeof(PacMan));
            else if (SnapperPage.IsSelected) MyFrame.Navigate(typeof(Snapper));
            else if (BootConfigPage.IsSelected) MyFrame.Navigate(typeof(BootConfig));
            else if (TweaksPage.IsSelected) MyFrame.Navigate(typeof(TweakBox));
            else if (SettingsPage.IsSelected) MyFrame.Navigate(typeof(Settings));
            else if (HelpPage.IsSelected) MyFrame.Navigate(typeof(Help));
            else if (AboutPage.IsSelected) MyFrame.Navigate(typeof(About));
            else MyFrame.Navigate(typeof(CMDInjector));
        }
    }
}
