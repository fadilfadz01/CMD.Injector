using CMDInjectorHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.System.UserProfile;

namespace BackgroundTasks
{
    public sealed class WallpaperUpdater : IBackgroundTask
    {
        private BackgroundTaskDeferral _Deferral { get; set; }

        readonly uint[] colors = { 16711680, 65280, 255, 65535, 16711935, 16776960 };

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _Deferral = taskInstance.GetDeferral();

            Helper.Init();

            if (Helper.LocalSettingsHelper.LoadSettings("GlanceAutoColorEnabled", 0) == 1)
            {
                Helper.RegistryHelper.SetRegValueEx(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "SOFTWARE\\OEM\\Nokia\\lpm", "ClockAndIndicatorsCustomColor", Helper.RegistryHelper.RegistryType.REG_DWORD, colors[Helper.LocalSettingsHelper.LoadSettings("GlanceColorIndex", 0)].ToString());
                if (Helper.LocalSettingsHelper.LoadSettings("GlanceColorIndex", 0) < 5)
                {
                    Helper.LocalSettingsHelper.SaveSettings("GlanceColorIndex", Helper.LocalSettingsHelper.LoadSettings("GlanceColorIndex", 0) + 1);
                }
                else
                {
                    Helper.LocalSettingsHelper.SaveSettings("GlanceColorIndex", 0);
                }
            }

            if (Helper.LocalSettingsHelper.LoadSettings("StartWallSwitch", false))
            {
                if (Helper.LocalSettingsHelper.LoadSettings("StartWallTrigger", 0) == 1)
                {
                    int interval = Helper.LocalSettingsHelper.LoadSettings("StartWallInterval", 15);
                    DateTime dateTime = DateTime.ParseExact(Helper.LocalSettingsHelper.LoadSettings("StartWallTime", $"{DateTime.Now.Subtract(TimeSpan.FromMinutes(interval)).ToString("dd/MM/yy HH:mm:ss")}"), "dd/MM/yy HH:mm:ss", CultureInfo.InvariantCulture);
                    DateTime currentTime = DateTime.Now;
                    if ((currentTime - dateTime).Minutes < interval)
                    {
                        goto Update;
                    }
                    Helper.LocalSettingsHelper.SaveSettings("StartWallTime", $"{DateTime.Now.ToString("dd/MM/yy HH:mm:ss")}");
                }

                var library = await TweakBoxHelper.GetWallpaperLibrary();
                var files = await library.GetFilesAsync();

                if (Helper.LocalSettingsHelper.LoadSettings("StartWallImagePosition", 0) >= files.Count)
                {
                    Helper.LocalSettingsHelper.SaveSettings("StartWallImagePosition", 0);
                }

                int j = 0;
                for (int i = 0; i < files.Count; i++)
                {
                    if (Path.GetExtension(files[i].Name).ToLower() == ".jpg" || Path.GetExtension(files[i].Name).ToLower() == ".jpeg" || Path.GetExtension(files[i].Name).ToLower() == ".png")
                    {
                        if(Helper.LocalSettingsHelper.LoadSettings("StartWallImagePosition", 0) == j)
                        {
                            var newFile = await files[i].CopyAsync(Helper.localFolder, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(files[i].Path));
                            UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                            var result = await profileSettings.TrySetWallpaperImageAsync(newFile);
                            Helper.LocalSettingsHelper.SaveSettings("StartWallImagePosition", Helper.LocalSettingsHelper.LoadSettings("StartWallImagePosition", 0) + 1);
                            await newFile.DeleteAsync();
                            break;
                        }

                        j++;
                    }
                }
            }

            Update:
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                var currentTime = DateTime.Now;
                if (currentTime >= Helper.LocalSettingsHelper.LoadSettings("UpdateNotifyTime", currentTime))
                {
                    bool isAlreadyExist = Helper.NotificationHelper.IsToastAlreadyThere("CheckUpdateTag");
                    if (!isAlreadyExist)
                    {
                        var isAvailable = await AboutHelper.IsNewUpdateAvailable();
                        if (isAvailable != null)
                        {
                            Helper.NotificationHelper.PushNotification("A new version of CMD Injector is available.", "Update Available", "DownloadUpdate", "CheckUpdateTag", 0);
                            Helper.LocalSettingsHelper.SaveSettings("UpdateNotifyTime", currentTime.AddHours(12));
                        }
                    }
                }
            }

            _Deferral.Complete();
        }
    }
}
