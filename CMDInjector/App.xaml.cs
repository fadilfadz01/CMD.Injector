using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.Storage.AccessCache;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using System.Threading;
using XamlBrewer.Uwp.Controls;
using CMDInjectorHelper;
using Windows.ApplicationModel.Background;
using MinimalisticTelnet;
using WinUniversalTool;
using Windows.Security.Credentials.UI;

namespace CMDInjector
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>

        Frame rootFrame;
        TelnetConnection tc;
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient tClient = new TelnetClient(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        bool isRootFrame = false;

        private void Connect()
        {
            string IP = "127.0.0.1";
            int Port = Int32.Parse("9999");
            tc = new TelnetConnection(IP, Port);
            long i = 0;
            while (tc.IsConnected == false && i < 1000000)
            {
                i++;
            }

            _ = tClient.Connect();
            long j = 0;
            while (tClient.IsConnected == false && j < 1000000)
            {
                j++;
            }
        }

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            Connect();
            Helper.Init();
            Helper.SoundHelper.Init();
            if (!Helper.BackgroundTaskHelper.IsThemeTaskActivated()) _ = Helper.BackgroundTaskHelper.RegisterThemeTask(15, new SystemTrigger(SystemTriggerType.UserAway, false));
            if (!Helper.BackgroundTaskHelper.IsWallpaperTaskActivated()) _ = Helper.BackgroundTaskHelper.RegisterWallpaperTask(15, new SystemTrigger(SystemTriggerType.UserPresent, false));
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (e.Arguments == "Restart")
            {
                Helper.RebootSystem();
            }
            else if (e.Arguments == "Shutdown" || e.Arguments == "Lockscreen" || e.Arguments == "FFULoader" || e.Arguments == "VolUp" || e.Arguments == "VolDown" || e.Arguments == "VolMute")
            {
                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    switch (e.Arguments)
                    {
                        case "Shutdown":
                            _ = tClient.Send("shutdown /s /t 0");
                            break;
                        case "Lockscreen":
                            Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Lock);
                            _ = tClient.Send("powertool -screenoff");
                            break;
                        case "FFULoader":
                            _ = tClient.Send("start " + Helper.installedLocation.Path + "\\Contents\\BatchScripts\\RebootToFlashingMode.bat");
                            await Helper.MessageBox("Rebooting to FFU Loader...");
                            break;
                        case "VolUp":
                            _ = tClient.Send("SendKeys -v \"0xAF 0xAF\"");
                            break;
                        case "VolDown":
                            _ = tClient.Send("SendKeys -v \"0xAE 0xAE\"");
                            break;
                        case "VolMute":
                            _ = tClient.Send("SendKeys -v \"0xAD\"");
                            break;
                    }
                    if (e.PreviousExecutionState == ApplicationExecutionState.NotRunning)
                    {
                        CoreApplication.Exit();
                    }
                    else if (e.PreviousExecutionState != ApplicationExecutionState.Running)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2));
                        _ = tClient.Send("SendKeys -v \"0x5B\"");
                    }
                    return;
                }
                else
                {
                    _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                }
            }
            else if (e.Arguments != "" && StorageApplicationPermissions.FutureAccessList.ContainsItem(e.Arguments))
            {
                try
                {
                    await Launcher.LaunchFolderAsync(await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(e.Arguments));
                    if (e.PreviousExecutionState == ApplicationExecutionState.NotRunning) CoreApplication.Exit();
                }
                catch (Exception) { _ = Helper.MessageBox("The folder you are trying to open is no longer exist.", Helper.SoundHelper.Sound.Error, "Missing Folder"); }
                return;
            }

            Helper.rect = e.SplashScreen.ImageLocation;

            rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                /*if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }*/

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;

                if (Helper.LocalSettingsHelper.LoadSettings("ThemeSettings", false))
                {
                    if (Helper.LocalSettingsHelper.LoadSettings("Theme", 0) == 0)
                    {
                        ((Frame)Window.Current.Content).RequestedTheme = ElementTheme.Dark;
                        Helper.color = Colors.Black;
                    }
                    else
                    {
                        ((Frame)Window.Current.Content).RequestedTheme = ElementTheme.Light;
                        Helper.color = Colors.White;
                    }
                }
                else
                {
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

                try
                {
                    await Task.Run(async () =>
                    {
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            if (Helper.LocalSettingsHelper.LoadSettings("AccentSettings", false))
                            {
                                int count = 0;
                                foreach (var color in typeof(Colors).GetRuntimeProperties())
                                {
                                    if (color.Name != "AliceBlue" && color.Name != "AntiqueWhite" && color.Name != "Azure" && color.Name != "Beige" && color.Name != "Bisque" && color.Name != "Black" && color.Name != "BlanchedAlmond" && color.Name != "Cornsilk" && color.Name != "FloralWhite" && color.Name != "Gainsboro" && color.Name != "GhostWhite" && color.Name != "Honeydew" && color.Name != "Ivory" && color.Name != "Lavender" && color.Name != "LavenderBlush" && color.Name != "LemonChiffon"
                                    && color.Name != "LightCyan" && color.Name != "LightGoldenrodYellow" && color.Name != "LightGray" && color.Name != "LightYellow" && color.Name != "Linen" && color.Name != "MintCream" && color.Name != "MistyRose" && color.Name != "Moccasin" && color.Name != "OldLace" && color.Name != "PapayaWhip" && color.Name != "SeaShell" && color.Name != "Snow" && color.Name != "Transparent" && color.Name != "White" && color.Name != "WhiteSmoke")
                                    {
                                        if (Helper.LocalSettingsHelper.LoadSettings("Accent", 11) == count)
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
                                            break;
                                        }
                                        count++;
                                    }
                                }
                            }
                            else if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                            {
                                var statusBar = StatusBar.GetForCurrentView();
                                if (statusBar != null)
                                {
                                    var accentColor = new UISettings().GetColorValue(UIColorType.Accent);
                                    statusBar.ForegroundColor = accentColor;
                                }
                            }
                        });
                    });
                }
                catch (Exception ex) { /*CommonHelper.ThrowException(ex);*/ }

                bool flag = false;
                try
                {
                    if (!Helper.NotificationHelper.IsToastAlreadyThere("CheckUpdateTag"))
                    {
                        var latestRelease = await AboutHelper.IsNewUpdateAvailable();
                        if (latestRelease != null)
                        {
                            flag = true;
                            Helper.NotificationHelper.PushNotification("A new version of CMD Injector is available.", "Update Available", "DownloadUpdate", "CheckUpdateTag", 0);
                            Helper.LocalSettingsHelper.SaveSettings("UpdateNotifyTime", DateTime.Now.AddHours(12));
                        }
                    }
                }
                catch (Exception ex)
                {
                    //CommonHelper.ThrowException(ex);
                }
                if (!flag && File.Exists(@"C:\Windows\System32\CMDInjectorVersion.dat"))
                {
                    if (!Helper.NotificationHelper.IsToastAlreadyThere("Re-InjectTag") && Convert.ToInt32(Helper.InjectedBatchVersion) < Helper.currentBatchVersion)
                    {
                        if (File.Exists(@"C:\Windows\System32\CMDInjector.dat")) Helper.NotificationHelper.PushNotification("The App has been updated, required re-injection in order to work the app fine.", "Re-injection required", "ReinjectionRequired", "Re-InjectTag", 1800);
                        else Helper.NotificationHelper.PushNotification("The App has been updated, required re-injection in order to work the App fine.", "Re-injection required", "ReinjectionRequired", "Re-InjectTag", 1800, "Re-Inject", "InjectCMD", "Re-Inject");
                    }
                }
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    if (Helper.LocalSettingsHelper.LoadSettings("SplashScreen", true))
                    {
                        ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                    }
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    if (Helper.LocalSettingsHelper.LoadSettings("LoginTogReg", true) && Helper.LocalSettingsHelper.LoadSettings("SplashScreen", true) && Helper.build >= 10572) (rootFrame.Content as Windows.UI.Xaml.Controls.Page).OpenFromSplashScreen(e.SplashScreen.ImageLocation, Helper.color, new Uri("ms-appx:///Assets/SplashScreen.png"));
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
            Type frame = null;
            if (string.IsNullOrEmpty(e.Arguments) == false)
            {
                isRootFrame = true;
                //var rootFrame = new Frame();
                if (e.Arguments == "HomePage")
                {
                    frame = typeof(Home);
                }
                else if (e.Arguments.Contains("TerminalPage"))
                {
                    frame = typeof(Terminal);
                }
                else if (e.Arguments == "StartupPage")
                {
                    frame = typeof(Startup);
                }
                else if (e.Arguments == "PacManPage")
                {
                    frame = typeof(PacMan);
                }
                else if (e.Arguments == "SnapperPage")
                {
                    frame = typeof(Snapper);
                }
                else if (e.Arguments == "BootConfigPage")
                {
                    frame = typeof(BootConfig);
                }
                else if (e.Arguments == "TweakBoxPage")
                {
                    frame = typeof(TweakBox);
                }
                else if (e.Arguments == "SettingsPage")
                {
                    frame = typeof(Settings);
                }
                else if (e.Arguments == "HelpPage")
                {
                    frame = typeof(Help);
                }
                else if (e.Arguments == "AboutPage")
                {
                    frame = typeof(About);
                }
            }

            if (Helper.LocalSettingsHelper.LoadSettings("SplashScreen", true) && !Helper.splashScreenDisplayed)
            {
                if (e.PreviousExecutionState != ApplicationExecutionState.Running)
                {
                    bool loadState = (e.PreviousExecutionState == ApplicationExecutionState.Terminated);
                    SplashAnimation extendedSplash = new SplashAnimation(e, loadState);
                    rootFrame.Content = extendedSplash;
                    Window.Current.Content = rootFrame;
                    while (true)
                    {
                        await Task.Delay(200);
                        if (Helper.splashScreenDisplayed) break;
                    }
                    ApplicationView.GetForCurrentView().ExitFullScreenMode();
                }
            }
            if (!await CallLoginPage())
            {
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            if (frame != null)
            {
                if (frame.Name == "Home" || frame.Name == "Terminal") rootFrame.Navigate(frame, e.Arguments);
                else rootFrame.Navigate(frame);
            }
            if (!Helper.LocalSettingsHelper.LoadSettings("LoginTogReg", true) && Helper.LocalSettingsHelper.LoadSettings("SplashScreen", true) && Helper.build >= 10572) (rootFrame.Content as Windows.UI.Xaml.Controls.Page).OpenFromSplashScreen(e.SplashScreen.ImageLocation, Helper.color, new Uri("ms-appx:///Assets/SplashScreen.png"));

            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        protected override async void OnActivated(IActivatedEventArgs e)
        {
            base.OnActivated(e);
            rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;

                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    if (e.Kind == ActivationKind.ToastNotification)
                    {
                        var toastActivationArgs = e as ToastNotificationActivatedEventArgs;
                        if (toastActivationArgs.Argument == "OpenSnapper" || toastActivationArgs.Argument == "StopSnapper=StopCapturing;OpenSnapper" || toastActivationArgs.Argument == "StopSnapper=StopRecording;OpenSnapper" || toastActivationArgs.Argument == "OpenImage" || toastActivationArgs.Argument == "OpenVideo")
                        {
                            rootFrame.Navigate(typeof(MainPage));
                            rootFrame.Navigate(typeof(Snapper));
                        }
                        else if (toastActivationArgs.Argument == "Updation" || toastActivationArgs.Argument == "DownloadUpdate")
                        {
                            rootFrame.Navigate(typeof(MainPage));
                            rootFrame.Navigate(typeof(About), toastActivationArgs.Argument);
                        }
                        else if (toastActivationArgs.Argument == "ReinjectionRequired" || toastActivationArgs.Argument == "InjectCMD=Re-Inject;ReinjectionRequired")
                        {
                            rootFrame.Navigate(typeof(MainPage));
                            rootFrame.Navigate(typeof(Home), toastActivationArgs.Argument);
                        }
                        else
                        {
                            rootFrame.Navigate(typeof(MainPage), toastActivationArgs.Argument);
                        }
                    }
                    else if (e.Kind == ActivationKind.Protocol)
                    {
                        var protocolArgs = e as ProtocolActivatedEventArgs;
                        var AbsoluteURI = protocolArgs.Uri.AbsoluteUri;
                        if (AbsoluteURI == "cmdinjector:")
                        {
                            rootFrame.Navigate(typeof(MainPage));
                        }
                        else
                        {
                            var cleanToken = AbsoluteURI.Replace("cmdinjector::", "").Replace("cmdinjector:", "");
                            var strFilePath = await SharedStorageAccessManager.RedeemTokenForFileAsync(cleanToken);
                            if (Path.GetExtension(strFilePath.Path.ToString().ToLower()) == ".xap" || Path.GetExtension(strFilePath.Path.ToString().ToLower()) == ".appx" || Path.GetExtension(strFilePath.Path.ToString().ToLower()) == ".appxbundle" || Path.GetExtension(strFilePath.Path.ToString().ToLower()) == ".xml" || Path.GetExtension(strFilePath.Path).ToLower() == ".pmlog")
                            {
                                rootFrame.Navigate(typeof(MainPage));
                                rootFrame.Navigate(typeof(PacMan), cleanToken);
                            }
                            else
                            {
                                rootFrame.Navigate(typeof(MainPage), protocolArgs);
                            }
                        }
                    }
                }

                await CallLoginPage();

                // Ensure the current window is active
                Window.Current.Activate();
            }
            else
            {
                if (isRootFrame)
                {
                    if (e.Kind == ActivationKind.ToastNotification)
                    {
                        var toastActivationArgs = e as ToastNotificationActivatedEventArgs;
                        if (toastActivationArgs.Argument == "OpenSnapper" || toastActivationArgs.Argument == "StopSnapper=StopCapturing;OpenSnapper" || toastActivationArgs.Argument == "StopSnapper=StopRecording;OpenSnapper" || toastActivationArgs.Argument == "OpenImage" || toastActivationArgs.Argument == "OpenVideo")
                        {
                            rootFrame.Navigate(typeof(Snapper), toastActivationArgs.Argument);
                        }
                        else if (toastActivationArgs.Argument == "Updation" || toastActivationArgs.Argument == "DownloadUpdate")
                        {
                            rootFrame.Navigate(typeof(About), toastActivationArgs.Argument);
                        }
                        else if (toastActivationArgs.Argument == "ReinjectionRequired" || toastActivationArgs.Argument == "InjectCMD=Re-Inject;ReinjectionRequired")
                        {
                            rootFrame.Navigate(typeof(Home), toastActivationArgs.Argument);
                        }
                    }
                    else if (e.Kind == ActivationKind.Protocol)
                    {
                        var protocolArgs = e as ProtocolActivatedEventArgs;
                        var AbsoluteURI = protocolArgs.Uri.AbsoluteUri;
                        if (AbsoluteURI == "cmdinjector:")
                        {
                            rootFrame.Navigate(typeof(MainPage), protocolArgs);
                        }
                        else
                        {
                            var cleanToken = AbsoluteURI.Replace("cmdinjector::", "").Replace("cmdinjector:", "");
                            var strFilePath = await SharedStorageAccessManager.RedeemTokenForFileAsync(cleanToken);
                            if (Path.GetExtension(strFilePath.Path.ToString().ToLower()) == ".xap" || Path.GetExtension(strFilePath.Path.ToString().ToLower()) == ".appx" || Path.GetExtension(strFilePath.Path.ToString().ToLower()) == ".appxbundle" || Path.GetExtension(strFilePath.Path.ToString().ToLower()) == ".xml" || Path.GetExtension(strFilePath.Path).ToLower() == ".pmlog")
                            {
                                rootFrame.Navigate(typeof(PacMan), cleanToken);
                            }
                            else
                            {
                                rootFrame.Navigate(typeof(MainPage), protocolArgs);
                            }
                        }
                    }
                }
                else
                {
                    if (e.Kind == ActivationKind.ToastNotification)
                    {
                        var toastActivationArgs = e as ToastNotificationActivatedEventArgs;
                        rootFrame.Navigate(typeof(MainPage), toastActivationArgs.Argument);
                    }
                    else if (e.Kind == ActivationKind.Protocol)
                    {
                        var protocolArgs = e as ProtocolActivatedEventArgs;
                        rootFrame.Navigate(typeof(MainPage), protocolArgs);
                    }
                }
                await CallLoginPage();
            }
        }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            base.OnFileActivated(args);
            rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;

                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage));
                    await CallLoginPage();
                    rootFrame.Navigate(typeof(PacMan), args.Files[0]);
                }

                // Ensure the current window is active
                Window.Current.Activate();
            }
            else
            {
                await CallLoginPage();
                if (isRootFrame)
                {
                    rootFrame.Navigate(typeof(PacMan), args.Files[0]);
                }
                else
                {
                    rootFrame.Navigate(typeof(MainPage), args);
                }
            }
        }

        private async Task<bool> CallLoginPage()
        {
            if (Helper.build >= 10586 && (await UserConsentVerifier.CheckAvailabilityAsync()) == UserConsentVerifierAvailability.Available && Helper.LocalSettingsHelper.LoadSettings("LoginTogReg", true) && !Helper.userVerified)
            {
                rootFrame.Navigate(typeof(Login));
                while (true)
                {
                    await Task.Delay(200);
                    if (Helper.userVerified) break;
                }
                return true;
            }
            return false;
        }
    }
}
