@{
    GUID = "{AEEF2BEF-EBA9-4A1D-A3D2-D0B52DF76DEB}"
    AliasesToExport = @()
    Author = "Microsoft Corporation"
    CLRVersion = '4.0'
    CmdletsToExport = 'Add-AppxPackage', 'Get-AppxPackage', 'Get-AppxPackageManifest', 'Remove-AppxPackage', 'Get-AppxVolume', 'Add-AppxVolume', 'Remove-AppxVolume', 'Mount-AppxVolume', 'Dismount-AppxVolume', 'Move-AppxPackage', 'Get-AppxDefaultVolume', 'Set-AppxDefaultVolume', 'Invoke-CommandInDesktopPackage'
    CompanyName = "Microsoft Corporation"
    Copyright = "© Microsoft Corporation. All rights reserved."
    FunctionsToExport = 'Get-AppxLastError', 'Get-AppxLog'
    HelpInfoUri="http://go.microsoft.com/fwlink/?LinkId=525624"
    ModuleVersion = "2.0.0.0"
    ModuleToProcess = 'Microsoft.Windows.Appx.PackageManager.Commands'
    FormatsToProcess = 'Appx.format.ps1xml'
    NestedModules = 'Appx.psm1'
    PowerShellVersion = '3.0'
}
