using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace CMDInjectorHelper
{
    public static class TweakBoxHelper
    {
        public static async Task<StorageFolder> SetWallpaperLibrary()
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            folderPicker.FileTypeFilter.Add(".jpeg");
            folderPicker.FileTypeFilter.Add(".jpg");
            folderPicker.FileTypeFilter.Add(".png");
            StorageFolder LibraryFolderTest = await folderPicker.PickSingleFolderAsync();
            if (LibraryFolderTest != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("WallpaperLibrary", LibraryFolderTest);
                return LibraryFolderTest;
            }
            return null;
        }

        public async static Task<StorageFolder> GetWallpaperLibrary()
        {
            if (StorageApplicationPermissions.FutureAccessList.ContainsItem("WallpaperLibrary"))
            {
                try
                {
                    return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("WallpaperLibrary");
                }
                catch (Exception ex)
                {
                    //Helper.ThrowException(ex);
                }
            }
            StorageFolder defaultLibrary = await StorageFolder.GetFolderFromPathAsync($"{Helper.installedLocation.Path}\\Assets\\Images\\Lockscreens\\Stripes");
            StorageApplicationPermissions.FutureAccessList.AddOrReplace("WallpaperLibrary", defaultLibrary);
            return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("WallpaperLibrary");
        }
    }
}
