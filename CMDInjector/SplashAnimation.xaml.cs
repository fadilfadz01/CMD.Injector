using CMDInjectorHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Security.Credentials.UI;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CMDInjector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SplashAnimation : Page
    {
        internal Rect splashImageRect; // Rect to store splash screen image coordinates
        internal bool dismissed = false; // Variable to track splash screen dismissal status

        private SplashScreen splash; // Variable to hold the splash screen object
        private double ScaleFactor; //Variable to hold the device scale factor

        public SplashAnimation(LaunchActivatedEventArgs e, bool loadState)
        {
            this.InitializeComponent();
            if (IsSystemThemeDark())
            {
                ExtendedSplashImage.Source = new BitmapImage(new Uri($"ms-appx:///Assets/Images/Splashscreens/{Helper.LocalSettingsHelper.LoadSettings("SplashAnim", "Glitch")}_Dark.gif"));
                SplashProgressRing.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                ExtendedSplashImage.Source = new BitmapImage(new Uri($"ms-appx:///Assets/Images/Splashscreens/{Helper.LocalSettingsHelper.LoadSettings("SplashAnim", "Glitch")}_Light.gif"));
                SplashProgressRing.Foreground = new SolidColorBrush(Colors.Black);
            }
            DismissExtendedSplash();
            Window.Current.SizeChanged += Current_SizeChanged;
            ScaleFactor = (double)DisplayInformation.GetForCurrentView().ResolutionScale / 100;
            splash = e.SplashScreen;
            if (splash != null)
            {
                splash.Dismissed += Splash_Dismissed;
                splashImageRect = splash.ImageLocation;
                PositionImage();
            }
            RestoreStateAsync(loadState);
        }

        private async void RestoreStateAsync(bool loadState)
        {
            if (loadState)
                await SuspensionManager.RestoreAsync();
        }

        private void PositionImage()
        {
            ExtendedSplashImage.SetValue(Canvas.LeftProperty, splashImageRect.Left);
            ExtendedSplashImage.SetValue(Canvas.TopProperty, splashImageRect.Top);
            ExtendedSplashImage.Width = splashImageRect.Width / ScaleFactor;
            ExtendedSplashImage.Height = splashImageRect.Height / ScaleFactor;
        }

        private void Splash_Dismissed(SplashScreen sender, object args)
        {
            dismissed = true;
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            if (splash != null)
            {
                splashImageRect = splash.ImageLocation;
                PositionImage();
            }
        }

        private async void DismissExtendedSplash()
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            Helper.splashScreenDisplayed = true;
        }

        private bool IsSystemThemeDark()
        {
            if (Helper.LocalSettingsHelper.LoadSettings("ThemeSettings", false))
            {
                if (Helper.LocalSettingsHelper.LoadSettings("Theme", 0) == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (Application.Current.RequestedTheme == ApplicationTheme.Light)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
