@{
    GUID = "7E984F2F-35DA-48A2-A3C1-40CE59930A7C"
    Author = "Microsoft Corporation"
    CompanyName = "Microsoft Corporation"
    Copyright = "© Microsoft Corporation. All rights reserved."
    ModuleVersion = "1.0.0.0"
    PowerShellVersion = "3.0"
    HelpInfoUri = "http://go.microsoft.com/fwlink/?linkid=532774"

    NestedModules = @(
        'MSFT_NetEventSession.cdxml',
        'MSFT_NetEventProvider.cdxml',
        'MSFT_NetEventPacketCaptureProvider.cdxml',
        'MSFT_NetEventNetworkAdapter.cdxml',
        'MSFT_NetEventVmSwitch.cdxml',
        'MSFT_NetEventVmNetworkAdatper.cdxml',
        'MSFT_NetEventWFPCaptureProvider.cdxml',
        'MSFT_NetEventVFPProvider.cdxml',
        'MSFT_NetEventVmSwitchProvider.cdxml')

    FormatsToProcess = @(
        'MSFT_NetEventSession.format.ps1xml',
        'MSFT_NetEventProvider.format.ps1xml',
        'MSFT_NetEventPacketCaptureProvider.format.ps1xml',
        'MSFT_NetEventNetworkAdapter.format.ps1xml',
        'MSFT_NetEventVmSwitch.format.ps1xml',
        'MSFT_NetEventVmNetworkAdatper.format.ps1xml',
        'MSFT_NetEventVFPProvider.format.ps1xml',
        'MSFT_NetEventVmSwitchProvider.format.ps1xml')

    TypesToProcess = @('NetEventPacketCapture.Types.ps1xml')

    FunctionsToExport = @(
        'New-NetEventSession',
        'Remove-NetEventSession',
        'Get-NetEventSession',
        'Set-NetEventSession',
        'Start-NetEventSession',
        'Stop-NetEventSession',
        'Add-NetEventProvider',
        'Remove-NetEventProvider',
        'Get-NetEventProvider',
        'Set-NetEventProvider',
        'Add-NetEventPacketCaptureProvider',
        'Remove-NetEventPacketCaptureProvider',
        'Get-NetEventPacketCaptureProvider',
        'Set-NetEventPacketCaptureProvider',
        'Add-NetEventNetworkAdapter',
        'Remove-NetEventNetworkAdapter',
        'Get-NetEventNetworkAdapter',
        'Add-NetEventVmNetworkAdapter',
        'Remove-NetEventVmNetworkAdapter',
        'Get-NetEventVmNetworkAdapter',
        'Add-NetEventVmSwitch',
        'Remove-NetEventVmSwitch',
        'Get-NetEventVmSwitch',
        'Add-NetEventWFPCaptureProvider',
        'Remove-NetEventWFPCaptureProvider',
        'Get-NetEventWFPCaptureProvider',
        'Set-NetEventWFPCaptureProvider',
        'Add-NetEventVFPProvider',
        'Remove-NetEventVFPProvider',
        'Get-NetEventVFPProvider',
        'Set-NetEventVFPProvider',
        'Add-NetEventVmSwitchProvider',
        'Remove-NetEventVmSwitchProvider',
        'Get-NetEventVmSwitchProvider',
        'Set-NetEventVmSwitchProvider')
        
    CmdletsToExport = @()
    
    AliasesToExport = @()
}
