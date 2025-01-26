using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CMDInjector
{
    public enum CustomContentDialogResult
    {
        App,
        Data,
        AppData,
        Cancel,
        Nothing
    }

    public sealed partial class CustomContentDialog : ContentDialog
    {
        public CustomContentDialogResult Result { get; set; }

        public CustomContentDialog(string title, bool isAppExist, bool isDataExist, bool isAppInstalled = true)
        {
            this.InitializeComponent();
            TitleBox.Text = title;
            this.Result = CustomContentDialogResult.Nothing;
            if (!isAppExist)
            {
                Btn1.IsEnabled = false;
                Btn3.IsEnabled = false;
            }
            if (!isDataExist)
            {
                Btn2.IsEnabled = false;
                Btn3.IsEnabled = false;
            }
            if (!isAppInstalled)
            {
                Btn2.IsEnabled = false;
            }
        }

        // Handle the button clicks from dialog
        private void Btn1_Click(object sender, RoutedEventArgs e)
        {
            this.Result = CustomContentDialogResult.App;
            // Close the dialog
            dialog.Hide();
        }

        private void Btn2_Click(object sender, RoutedEventArgs e)
        {
            this.Result = CustomContentDialogResult.Data;
            // Close the dialog
            dialog.Hide();
        }

        private void Btn3_Click(object sender, RoutedEventArgs e)
        {
            this.Result = CustomContentDialogResult.AppData;
            // Close the dialog
            dialog.Hide();
        }

        private void Btn4_Click(object sender, RoutedEventArgs e)
        {
            this.Result = CustomContentDialogResult.Cancel;
            // Close the dialog
            dialog.Hide();
        }
    }
}