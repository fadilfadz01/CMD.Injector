using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Management.Deployment;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace CMDInjectorHelper
{
    public static class PacManHelper
    {
        public static bool installOnProcess = false;

        public class AppsDetails
        {
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string FamilyName { get; set; }
            public string FullName { get; set; }
            public string Publisher { get; set; }
            public string Architecture { get; set; }
            public string Type { get; set; }
            public string Version { get; set; }
            public string PublisherID { get; set; }
            public string InstalledDate { get; set; }
            public Package Package { get; set; }
            public StorageFolder InstalledLocation { get; set; }
            public bool IsBundle { get; set; }
            public bool IsDevelopmentMode { get; set; }
            public bool IsFramework { get; set; }
            public bool IsResourcePackage { get; set; }
            public bool IsXap { get; set; }
        }

        public class BackupInfo
        {
            public string Title { get; set; }
            public string Name { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
            public string FamilyName { get; set; }
            public string FullName { get; set; }
            public string Publisher { get; set; }
            public string Architecture { get; set; }
            public string Type { get; set; }
            public string Version { get; set; }
            public string PublisherID { get; set; }
            public string CreatedDate { get; set; }
        }

        public static object CreateJsonObject(AppsDetails package, string title)
        {
            var appInfo = new BackupInfo
            {
                Title = title,
                Name = package.Name,
                DisplayName = package.DisplayName,
                Description = package.Description,
                FamilyName = package.FamilyName,
                FullName = package.FullName,
                Publisher = package.Publisher,
                Architecture = package.Architecture,
                Type = package.Type,
                Version = package.Version,
                PublisherID = package.PublisherID,
                CreatedDate = DateTime.Now.ToString()
            };

            return appInfo;
        }

        public static async Task<ObservableCollection<AppsDetails>> GetPackagesByType(PackageTypes type, bool excludeCMDApp = true, IProgress<double> progress = null)
        {
            ObservableCollection<AppsDetails> appsDetails = new ObservableCollection<AppsDetails>();
            IEnumerable<Package> packages = new PackageManager().FindPackagesForUserWithPackageTypes("", type);
            foreach (var package in packages)
            {
                var displayName = string.Empty;
                var desciption = string.Empty;
                StorageFolder installedLocation = null;
                var installedDate = string.Empty;
                var isOptional = false;
                var isXap = false;
                AppListEntry app = null;
                if (excludeCMDApp && package.Id.Name == "CMDInjector") continue;
                try
                {
                    var appEntries = await package.GetAppListEntriesAsync();
                    app = appEntries.FirstOrDefault();
                }
                catch (Exception ex) { }
                if (app == null || app.DisplayInfo.DisplayName == string.Empty || app.DisplayInfo.DisplayName.Length == 1) displayName = package.Id.Name;
                else displayName = app.DisplayInfo.DisplayName;
                if (app == null) desciption = package.Description;
                else desciption = app.DisplayInfo.Description;
                if (type.ToString() == "Xap")
                {
                    isXap = true;
                }
                try
                {
                    installedLocation = package.InstalledLocation;
                }
                catch (Exception ex) { }
                if (Helper.build >= 14393)
                {
                    installedDate = package.InstalledDate.DateTime.ToString();
                    isOptional = package.IsOptional;
                }
                appsDetails.Where(l => l.Type != type.ToString()).ToList().All(i => appsDetails.Remove(i));
                appsDetails.Add(new AppsDetails()
                {
                    Name = package.Id.Name,
                    DisplayName = displayName,
                    Description = desciption,
                    FamilyName = package.Id.FamilyName,
                    FullName = package.Id.FullName,
                    Publisher = package.Id.Publisher,
                    Architecture = package.Id.Architecture.ToString(),
                    Type = type.ToString(),
                    Version = string.Format("{0}.{1}.{2}.{3}", package.Id.Version.Major, package.Id.Version.Minor, package.Id.Version.Build, package.Id.Version.Revision),
                    PublisherID = package.Id.PublisherId,
                    InstalledDate = installedDate,
                    Package = package,
                    InstalledLocation = installedLocation,
                    IsBundle = package.IsBundle,
                    IsDevelopmentMode = package.IsDevelopmentMode,
                    IsFramework = package.IsFramework,
                    IsResourcePackage = package.IsResourcePackage,
                    IsXap = isXap
                });
                if (progress != null) progress.Report(1);
            }
            return appsDetails;
        }

        public static async Task<ObservableCollection<BackupInfo>> GetBackups(string packageFullName)
        {
            ObservableCollection<BackupInfo> appsInfo = new ObservableCollection<BackupInfo>();
            appsInfo.Where(l => true).ToList().All(i => appsInfo.Remove(i));
            var backupFolder = await GetBackupFolder();
            if (backupFolder == null) return null;
            StorageFolder packageFolder;
            try
            {
                packageFolder = await backupFolder.GetFolderAsync(packageFullName);
                if (packageFolder == null) return null;
            }
            catch (Exception ex)
            {
                return null;
            }
            var backupFiles = await packageFolder.GetFilesAsync();
            foreach (var file in backupFiles)
            {
                if (!file.Name.EndsWith(".pmbak", StringComparison.OrdinalIgnoreCase) || !Helper.Archive.CheckFileExist(file.Path, "AppInfo.json")) continue;
                var appInfoText = await Helper.Archive.ReadTextFromZip(file.Path, "AppInfo.json");
                appsInfo.Add(JsonConvert.DeserializeObject<BackupInfo>(appInfoText));
            }
            return appsInfo;
        }

        public static string InstallResultFile
        {
            get { return $"{Helper.localFolder.Path}\\PacManInstallResult.txt"; }
        }

        public static string MoveResultFile
        {
            get { return $"{Helper.localFolder.Path}\\PacManMoveResult.txt"; }
        }

        public static string InstallEndFile
        {
            get { return $"{Helper.localFolder.Path}\\PacManInstallEnd.txt"; }
        }

        public static string MoveEndFile
        {
            get { return $"{Helper.localFolder.Path}\\PacManMoveEnd.txt"; }
        }

        public static string LogFileName
        {
            get { return "PacMan_Installer.pmlog"; }
        }

        public static string XapAppDataInternal
        {
            get
            {
                return "C:\\Data\\Users\\DefApps\\AppData";
            }
        }

        public static string XapAppDataExternal
        {
            get
            {
                return "D:\\WPSystem\\AppData";
            }
        }

        public static string AppDataInternal
        {
            get
            {
                return "C:\\Data\\Users\\DefApps\\AppData\\Local\\Packages";
            }
        }

        public static string AppDataExternal
        {
            get
            {
                return "D:\\WPSystem\\AppData\\Local\\Packages";
            }
        }

        public static string[] GetBackupFile
        {
            get
            {
                string[] backupFiles = new string[3];
                backupFiles[0] = $"{Helper.cacheFolder.Path}\\Backup\\Package.zip";
                backupFiles[1] = $"{Helper.cacheFolder.Path}\\Backup\\Data.zip";
                backupFiles[2] = $"{Helper.cacheFolder.Path}\\Backup\\AppInfo.json";
                return backupFiles;
            }
        }

        public static async Task<StorageFile> GetInstallResultFile()
        {
            return await Helper.localFolder.GetFileAsync("PacManInstallResult.txt");
        }

        public static async Task<StorageFile> GetMoveResultFile()
        {
            return await Helper.localFolder.GetFileAsync("PacManMoveResult.txt");
        }

        public static async Task<StorageFolder> GetLogPath()
        {
            if (!Helper.LocalSettingsHelper.LoadSettings("PMLogPath", false))
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PMLogPath", KnownFolders.DocumentsLibrary);
                Helper.LocalSettingsHelper.SaveSettings("PMLogPath", true);
            }
            return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("PMLogPath");
        }

        public static async void CreateLogFile()
        {
            await (await GetLogPath()).CreateFileAsync(LogFileName, CreationCollisionOption.ReplaceExisting);
        }

        public static async Task<string> ReadLogFile()
        {
            return await FileIO.ReadTextAsync(await (await GetLogPath()).GetFileAsync(LogFileName));
        }

        public static async Task WriteLogFile(string text)
        {
            await FileIO.AppendTextAsync(await (await GetLogPath()).GetFileAsync(LogFileName), text);
        }

        public static async Task<bool> IsBackupFolderSelected()
        {
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("PMBackupFolder"))
            {
                try
                {
                    await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("PMBackupFolder");
                    return true;
                }
                catch (Exception ex)
                {
                    //Helper.ThrowException(ex);
                }
            }
            return false;
        }

        public static async Task<StorageFolder> GetBackupFolder()
        {
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("PMBackupFolder"))
            {
                try
                {
                    return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("PMBackupFolder");
                }
                catch (Exception ex)
                {
                    //Helper.ThrowException(ex);
                }
            }
            var result = await Helper.MessageBox("Please select a parent folder to save all the PacMan packages backup.", Helper.SoundHelper.Sound.Alert, "", "Browse");
            if (result == 0)
            {
                var folderPicker = new FolderPicker
                {
                    SuggestedStartLocation = PickerLocationId.Downloads
                };
                folderPicker.FileTypeFilter.Add("*");
                StorageFolder DownloadsFolderTest = await folderPicker.PickSingleFolderAsync();
                if (DownloadsFolderTest != null)
                {
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace("PMBackupFolder", DownloadsFolderTest);
                    return DownloadsFolderTest;
                }
            }
            return null;
        }

        public static async Task<bool> SetBackupFolder()
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder DownloadsFolderTest = await folderPicker.PickSingleFolderAsync();
            if (DownloadsFolderTest != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PMBackupFolder", DownloadsFolderTest);
                return true;
            }
            return false;
        }
    }
}
