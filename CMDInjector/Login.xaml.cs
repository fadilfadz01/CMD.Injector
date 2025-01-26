using CMDInjectorHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Credentials.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CMDInjector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        public Login()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Verify();
        }

        private void VerifyBtn_Click(object sender, RoutedEventArgs e)
        {
            Verify();
        }

        private async void Verify()
        {
            VerifyBtn.IsEnabled = false;
            UserConsentVerificationResult consentResult = await UserConsentVerifier.RequestVerificationAsync("Please enter your lockscreen PIN");
            if (consentResult == UserConsentVerificationResult.Canceled || consentResult == UserConsentVerificationResult.RetriesExhausted)
            {
                VerifyBtn.IsEnabled = true;
                return;
            }
            Helper.userVerified = true;
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }
    }
}
