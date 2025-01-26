using CMDInjectorHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Popups;

namespace BackgroundTasks
{
    public sealed class ThemeUpdater : IBackgroundTask
    {
        private BackgroundTaskDeferral _Deferral { get; set; }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _Deferral = taskInstance.GetDeferral();

            Helper.Init();

            if (Helper.LocalSettingsHelper.LoadSettings("AutoThemeMode", 0) != 0)
            {
                var lightTime = Helper.LocalSettingsHelper.LoadSettings("AutoThemeLight", "06:00");
                var darkTime = Helper.LocalSettingsHelper.LoadSettings("AutoThemeDark", "18:00");
                var currentTime = DateTime.Now.ToString("HH:mm");

                if (Helper.IsStrAGraterThanStrB(darkTime, lightTime, ':'))
                {
                    if (Helper.IsStrAGraterThanStrB(currentTime, lightTime, ':') && Helper.IsStrAGraterThanStrB(darkTime, currentTime, ':'))
                    {
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "SystemUsesLightTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Control Panel\\Theme", "CurrentTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000000");
                    }
                    else
                    {
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "SystemUsesLightTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000000");
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000000");
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Control Panel\\Theme", "CurrentTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                    }
                }
                else
                {
                    if (Helper.IsStrAGraterThanStrB(currentTime, darkTime, ':') && Helper.IsStrAGraterThanStrB(lightTime, currentTime, ':'))
                    {
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "SystemUsesLightTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000000");
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000000");
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Control Panel\\Theme", "CurrentTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                    }
                    else
                    {
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "SystemUsesLightTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize", "AppsUseLightTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000001");
                        Helper.RegistryHelper.SetRegValue(Helper.RegistryHelper.RegistryHive.HKEY_LOCAL_MACHINE, "Software\\Microsoft\\Windows\\CurrentVersion\\Control Panel\\Theme", "CurrentTheme", Helper.RegistryHelper.RegistryType.REG_DWORD, "00000000");
                    }
                }
            }

            _Deferral.Complete();
        }
    }
}
