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
using System.Threading;
using Windows.UI.Popups;
using Windows.Storage;
using Windows.System;
using Windows.System.Profile;
using System.Text.RegularExpressions;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Documents;
using Windows.UI.Text;
using CMDInjectorHelper;
using WinUniversalTool;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CMDInjector
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Terminal : Page
    {
        static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        TelnetClient tClient = new TelnetClient(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);
        ulong revision = (ulong.Parse(AnalyticsInfo.VersionInfo.DeviceFamilyVersion) & 0x000000000000FFFFL);
        List<string> savedCommands = new List<string>();
        int selectedCommand;
        bool flag = false;

        public Terminal()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            Initialize();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ConsoleBox.FontSize = TerminalHelper.FontSize;
            DirLabel.FontSize = TerminalHelper.FontSize;
            TelnetCommand.FontSize = TerminalHelper.FontSize;
            if (Helper.build < 14393)
            {
                ConsoleBoxGrid.Padding = new Thickness(2, 0, 0, 100);
                TelnetCommand.Visibility = Visibility.Collapsed;
                CommandBtn.Visibility = Visibility.Collapsed;
                RowMinHig.MinHeight = 100;
            }
            else
            {
                TelnetCommandBox.Visibility = Visibility.Collapsed;
                CommandSendBtn.Visibility = Visibility.Collapsed;
                if (Helper.LocalSettingsHelper.LoadSettings("ConKeyBtnSet", false))
                {
                    ConsoleBoxGrid.Padding = new Thickness(2, 0, 0, 200);
                    CommandBtn.Visibility = Visibility.Visible;
                    RowMinHig.MinHeight = 100;
                }
                else
                {
                    ConsoleBoxGrid.Padding = new Thickness(2, 0, 0, 270);
                    CommandBtn.Visibility = Visibility.Collapsed;
                    RowMinHig.MinHeight = 30;
                }
            }
            if (e.Parameter != null)
            {
                if (e.Parameter.ToString() == "TerminalPage")
                {
                    return;
                }
                if (tClient.IsConnected && HomeHelper.IsConnected())
                {
                    try
                    {
                        var cmd = TerminalHelper.EscapeSymbols(e.Parameter.ToString(), true);
                        if (Helper.LocalSettingsHelper.LoadSettings("TerminalRunArg", true))
                        {
                            TextBlock textBlockAsk = new TextBlock
                            {
                                Text = "\nDo you want to run the argument?"
                            };
                            TextBlock textBlockArg = new TextBlock();
                            textBlockArg.Inlines.Add(new Run() { Text = cmd.Substring(cmd.IndexOf(" ") + 1), FontStyle = FontStyle.Italic });
                            CheckBox checkBox = new CheckBox
                            {
                                Content = "Don't ask again"
                            };
                            StackPanel stackPanel = new StackPanel();
                            stackPanel.Children.Add(textBlockArg);
                            stackPanel.Children.Add(textBlockAsk);
                            stackPanel.Children.Add(checkBox);
                            ContentDialog contentDialog = new ContentDialog
                            {
                                Title = "Terminal",
                                Content = stackPanel,
                                IsSecondaryButtonEnabled = true,
                                PrimaryButtonText = "Run",
                                SecondaryButtonText = "Cancel"
                            };
                            Helper.SoundHelper.PlaySound(Helper.SoundHelper.Sound.Alert);
                            if (await contentDialog.ShowAsync() != ContentDialogResult.Primary)
                            {
                                return;
                            }
                            if (checkBox.IsChecked == true)
                            {
                                Helper.LocalSettingsHelper.SaveSettings("TerminalRunArg", false);
                            }
                        }
                        while (flag == false)
                        {
                            await Task.Delay(100);
                        }
                        if (Helper.build < 14393)
                        {
                            TelnetCommandBox.Text = cmd.Substring(cmd.IndexOf(" ") + 1);
                            CommandRun(TelnetCommandBox, CommandSendBtn);
                        }
                        else
                        {
                            TelnetCommand.Text = cmd.Substring(cmd.IndexOf(" ") + 1);
                            CommandRun(TelnetCommand, CommandBtn);
                        }
                    }
                    catch (Exception ex)
                    {
                        //Helper.ThrowException(ex);
                    }
                }
            }
        }

        private async void Initialize()
        {
            if (HomeHelper.IsCMDInjected())
            {
                await Connect();
                File.Delete(TerminalHelper.EndFile);
                await Task.Delay(200);
                _ = SendCommand($"echo %cd%^> >{TerminalHelper.DirectoryFile} 2>&1&echo. >{TerminalHelper.EndFile}");
                while (File.Exists(TerminalHelper.EndFile) == false)
                {
                    await Task.Delay(100);
                }
                ConsoleBox.Text = $"Microsoft Windows [Version 10.0.{Helper.build}.{revision}]\r\n(c) Microsoft Corporation. All rights reserved.\r\n";
                DirLabel.Text = Regex.Replace(File.ReadLines(TerminalHelper.DirectoryFile).First(), @"\s+", "");
                //TelnetCommand.Text = "";
                TelnetCommand.Focus(FocusState.Keyboard);
            }
            else
            {
                _ = Helper.MessageBox(HomeHelper.GetTelnetTroubleshoot(), Helper.SoundHelper.Sound.Error, "Error");
                TelnetCommand.IsHitTestVisible = false;
                TelnetCommandBox.IsReadOnly = true;
                CommandBtn.IsEnabled = false;
                CommandSendBtn.IsEnabled = false;
            }
            flag = true;
        }

        private async Task Connect()
        {
            ConsoleBox.Text = "Connecting...";
            _ = tClient.Connect();
            long i = 0;
            while (tClient.IsConnected == false && i < 150)
            {
                await Task.Delay(100);
                i++;
            }
            if (tClient.IsConnected && HomeHelper.IsConnected())
            {
                ConsoleBox.Text = "Connected.";
            }
            else
            {
                ConsoleBox.Text = HomeHelper.GetTelnetTroubleshoot();
                TelnetCommand.IsHitTestVisible = false;
                TelnetCommandBox.IsReadOnly = true;
                CommandBtn.IsEnabled = false;
                CommandSendBtn.IsEnabled = false;
            }
        }

        private async Task SendCommand(string command)
        {
            await Task.Run(() =>
            {
                tClient.Write(command);
            });
        }

        private void CommandBtn_Click(object sender, RoutedEventArgs e)
        {
            ConsoleScroll.ChangeView(0.0f, ConsoleScroll.ScrollableHeight, 1.0f);
            TelnetCommand.Focus(FocusState.Keyboard);
        }

        private void TelnetCommand_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && !string.IsNullOrWhiteSpace(TelnetCommand.Text))
            {
                CommandRun(TelnetCommand, CommandBtn);
            }
            else if (e.Key == VirtualKey.Up && selectedCommand > 0)
            {
                --selectedCommand;
                TelnetCommand.Text = savedCommands[selectedCommand];
                TelnetCommand.Select(TelnetCommand.Text.Length, 0);
            }
            else if (e.Key == VirtualKey.Down && selectedCommand < savedCommands.Count - 1)
            {
                ++selectedCommand;
                TelnetCommand.Text = savedCommands[selectedCommand];
                TelnetCommand.Select(TelnetCommand.Text.Length, 0);
            }
        }

        private void CommandSendBtn_Click(object sender, RoutedEventArgs e)
        {
            CommandRun(TelnetCommandBox, CommandSendBtn);
        }

        private void TelnetCommandBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TelnetCommandBox.Text == string.Empty || string.IsNullOrWhiteSpace(TelnetCommandBox.Text))
            {
                CommandSendBtn.IsEnabled = false;
            }
            else
            {
                CommandSendBtn.IsEnabled = true;
            }
        }

        private async void CommandRun(TextBox textBox, Button button)
        {
            InputPane.GetForCurrentView().TryHide();
            ConsoleScroll.ChangeView(0.0f, ConsoleScroll.ScrollableHeight, 1.0f);
            savedCommands.Add(TelnetCommand.Text);
            selectedCommand = savedCommands.Count;
            if (textBox.Text.ToLower() == "exit")
            {
                textBox.Text = string.Empty;
                DirLabel.Text = string.Empty;
                textBox.IsHitTestVisible = false;
                button.IsEnabled = false;
                ConsoleBox.Text = "An existing connection was forcibly closed by user.\n\nSession closed.";
                tClient.Dispose();
            }
            else if (textBox.Text.ToLower() == "cls")
            {
                ConsoleBox.Text = string.Empty;
                DirLabel.Text = Regex.Replace(File.ReadLines(TerminalHelper.DirectoryFile).First(), @"\s+", "");
                textBox.Text = string.Empty;
            }
            else if (textBox.Text.ToLower() == "cmd")
            {
                ConsoleBox.Text += "\r\n" + DirLabel.Text + textBox.Text + $"\r\nMicrosoft Windows [Version 10.0.{Helper.build}.{revision}]\r\n(c) Microsoft Corporation. All rights reserved.\r\n";
                DirLabel.Text = Regex.Replace(File.ReadLines(TerminalHelper.DirectoryFile).First(), @"\s+", "");
                textBox.Text = string.Empty;
            }
            else if (textBox.Text.ToLower() == "powershell")
            {
                ConsoleBox.Text += "\r\n" + DirLabel.Text + textBox.Text + "\r\nPowershell is unsupported by the Terminal.\r\n";
                DirLabel.Text = Regex.Replace(File.ReadLines(TerminalHelper.DirectoryFile).First(), @"\s+", "");
                textBox.Text = string.Empty;
            }
            else if (textBox.Text.ToLower() == "cmdinjector -unlock")
            {
                Helper.LocalSettingsHelper.SaveSettings("UnlockHidden", true);
                TelnetCommand.Text = string.Empty;
                TelnetCommandBox.Text = string.Empty;
            }
            else
            {
                try
                {
                    ConsoleBox.Text += "\r\n" + DirLabel.Text + textBox.Text;
                    DirLabel.Text = string.Empty;
                    File.Delete(TerminalHelper.ResultFile);
                    _ = SendCommand(textBox.Text + $" >>{TerminalHelper.ResultFile} 2>&1&echo Executed successfully. >>{TerminalHelper.ResultFile}");
                    textBox.Text = string.Empty;
                    textBox.IsHitTestVisible = false;
                    button.IsEnabled = false;
                    var temp = string.Empty;
                    await Task.Run(async () =>
                    {
                        while (true)
                        {
                            if (File.Exists(TerminalHelper.ResultFile)) break;
                        }
                        while (true)
                        {
                            using (var fs = new FileStream(TerminalHelper.ResultFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (var sr = new StreamReader(fs))
                            {
                                var output = sr.ReadToEnd();
                                if (temp != output)
                                {
                                    temp = output;
                                    var result = string.Empty;
                                    if (output.Split('\n').Length >= 2)
                                    {
                                        result = output.Split('\n')[output.Split('\n').Length - 2];
                                    }
                                    if (result.Contains("Executed successfully."))
                                    {
                                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                        {
                                            try
                                            {
                                                TempBox.Text = output.Remove(output.TrimEnd().LastIndexOf(Environment.NewLine));
                                            }
                                            catch (Exception ex)
                                            {
                                                //Helper.ThrowException(ex);
                                            }
                                        });
                                        break;
                                    }
                                    else
                                    {
                                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                        {
                                            TempBox.Text = output;
                                        });
                                    }
                                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        TempBox.Visibility = Visibility.Visible;
                                        ConsoleScroll.ChangeView(0.0f, ConsoleScroll.ScrollableHeight, 1.0f);
                                    });
                                }
                            }
                        }
                    });
                    ConsoleBox.Text += "\r\n" + TempBox.Text;
                    if (TempBox.Text != string.Empty) ConsoleBox.Text += "\r\n";
                    TempBox.Visibility = Visibility.Collapsed;
                    TempBox.Text = string.Empty;
                    ConsoleScroll.ChangeView(0.0f, ConsoleScroll.ScrollableHeight, 1.0f);
                    File.Delete(TerminalHelper.EndFile);
                    _ = SendCommand($"echo %cd%^> >{TerminalHelper.DirectoryFile} 2>&1&echo. >{TerminalHelper.EndFile}");
                    while (File.Exists(TerminalHelper.EndFile) == false)
                    {
                        await Task.Delay(100);
                    }
                    DirLabel.Text = Regex.Replace(File.ReadLines(TerminalHelper.DirectoryFile).First(), @"\s+", "");
                    ConsoleScroll.ChangeView(0.0f, ConsoleScroll.ScrollableHeight, 1.0f);
                    textBox.IsHitTestVisible = true;
                    if (Helper.build >= 14393) button.IsEnabled = true;
                }
                catch (Exception ex) { Helper.ThrowException(ex); }
            }
        }

        private void ConsoleScroll_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Helper.build >= 14393 && TelnetCommand.IsHitTestVisible)
            {
                ConsoleScroll.ChangeView(0.0f, ConsoleScroll.ScrollableHeight, 1.0f);
                TelnetCommand.Focus(FocusState.Keyboard);
            }
        }
    }
}
