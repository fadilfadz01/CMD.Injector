<#++

Copyright (c) 2011 Microsoft Corporation

Module Name:

    NetAdapter.psd1

Abstract:

    This module is the container module definition for the MSFT_NetAdapter* CIM
    object CmdLets.

--#>

@{
    GUID = '1042b422-63a8-4016-a6d6-293e19e8f8a6'
    Author = "Microsoft Corporation"
    CompanyName = "Microsoft Corporation"
    Copyright = "© Microsoft Corporation. All rights reserved."
    HelpInfoUri = "http://go.microsoft.com/fwlink/?linkid=390794"
    ModuleVersion = "2.0.0.0"
    PowerShellVersion = "3.0"
    NestedModules = @(
        'MSFT_NetAdapter.cmdletDefinition.cdxml',
        'MSFT_NetAdapterAdvancedProperty.cmdletDefinition.cdxml',
        'MSFT_NetAdapterBinding.cmdletDefinition.cdxml',
        'MSFT_NetAdapterChecksumOffload.cdxml',
        'MSFT_NetAdapterEncapsulatedPacketTaskOffload.cdxml',
        'MSFT_NetAdapterHardwareInfo.cmdletDefinition.cdxml',
        'MSFT_NetAdapterIPsecOffload.cdxml',
        'MSFT_NetAdapterLso.cdxml',
        'MSFT_NetAdapterPowerManagement.cmdletDefinition.cdxml',
        'MSFT_NetAdapterQos.cdxml',
        'MSFT_NetAdapterRdma.cdxml',
        'MSFT_NetAdapterRsc.cdxml',
        'MSFT_NetAdapterRss.cmdletDefinition.cdxml',
        'MSFT_NetAdapterSriov.cdxml',
        'MSFT_NetAdapterSriovVf.cmdletDefinition.cdxml',
        'MSFT_NetAdapterStatistics.cmdletDefinition.cdxml',
        'MSFT_NetAdapterVmq.cmdletDefinition.cdxml',
        'MSFT_NetAdapterVmqQueue.cmdletDefinition.cdxml',
        'MSFT_NetAdapterVPort.cmdletDefinition.cdxml',
        'MSFT_NetAdapterPacketDirect.cdxml'
        )

    TypesToProcess = @('NetAdapter.Types.ps1xml')

    FormatsToProcess = @(
        'MSFT_NetAdapter.Format.ps1xml',
        'MSFT_NetAdapterAdvancedProperty.Format.ps1xml',
        'MSFT_NetAdapterBinding.Format.ps1xml',
        'MSFT_NetAdapterChecksumOffload.Format.ps1xml',
        'MSFT_NetAdapterEncapsulatedPacketTaskOffload.Format.ps1xml',
        'MSFT_NetAdapterHardwareInfo.Format.ps1xml',
        'MSFT_NetAdapterIPsecOffload.Format.ps1xml',
        'MSFT_NetAdapterLso.Format.ps1xml',
        'MSFT_NetAdapterPowerManagement.Format.ps1xml',
        'MSFT_NetAdapterQos.Format.ps1xml',
        'MSFT_NetAdapterRdma.Format.ps1xml',
        'MSFT_NetAdapterRsc.Format.ps1xml',
        'MSFT_NetAdapterRss.Format.ps1xml',
        'MSFT_NetAdapterSriov.Format.ps1xml',
        'MSFT_NetAdapterSriovVf.Format.ps1xml',
        'MSFT_NetAdapterStatistics.Format.ps1xml',
        'MSFT_NetAdapterVmq.Format.ps1xml',
        'MSFT_NetAdapterVmqQueue.Format.ps1xml',
        'MSFT_NetAdapterVPort.Format.ps1xml',
        'MSFT_NetAdapterPacketDirect.Format.ps1xml'
        )

    FunctionsToExport = @(
        'Disable-NetAdapter',
        'Disable-NetAdapterBinding',
        'Disable-NetAdapterChecksumOffload',
        'Disable-NetAdapterEncapsulatedPacketTaskOffload',
        'Disable-NetAdapterIPsecOffload',
        'Disable-NetAdapterLso',
        'Disable-NetAdapterPowerManagement',
        'Disable-NetAdapterQos',
        'Disable-NetAdapterRdma',
        'Disable-NetAdapterRsc',
        'Disable-NetAdapterRss',
        'Disable-NetAdapterSriov',
        'Disable-NetAdapterVmq',
        'Disable-NetAdapterPacketDirect',
        'Enable-NetAdapter',
        'Enable-NetAdapterBinding',
        'Enable-NetAdapterChecksumOffload',
        'Enable-NetAdapterEncapsulatedPacketTaskOffload',
        'Enable-NetAdapterIPsecOffload',
        'Enable-NetAdapterLso',
        'Enable-NetAdapterPowerManagement',
        'Enable-NetAdapterQos',
        'Enable-NetAdapterRdma',
        'Enable-NetAdapterRsc',
        'Enable-NetAdapterRss',
        'Enable-NetAdapterSriov',
        'Enable-NetAdapterVmq',
        'Enable-NetAdapterPacketDirect',
        'Get-NetAdapter',
        'Get-NetAdapterAdvancedProperty',
        'Get-NetAdapterBinding',
        'Get-NetAdapterChecksumOffload',
        'Get-NetAdapterEncapsulatedPacketTaskOffload',
        'Get-NetAdapterHardwareInfo',
        'Get-NetAdapterIPsecOffload',
        'Get-NetAdapterLso',
        'Get-NetAdapterPowerManagement',
        'Get-NetAdapterQos',
        'Get-NetAdapterRdma',
        'Get-NetAdapterRsc',
        'Get-NetAdapterRss',
        'Get-NetAdapterSriov',
        'Get-NetAdapterSriovVf',
        'Get-NetAdapterStatistics',
        'Get-NetAdapterVmq',
        'Get-NetAdapterVMQQueue',
        'Get-NetAdapterVPort',
        'Get-NetAdapterPacketDirect',
        'New-NetAdapterAdvancedProperty',
        'Remove-NetAdapterAdvancedProperty',
        'Rename-NetAdapter',
        'Reset-NetAdapterAdvancedProperty',
        'Restart-NetAdapter',
        'Set-NetAdapter',
        'Set-NetAdapterAdvancedProperty',
        'Set-NetAdapterBinding',
        'Set-NetAdapterChecksumOffload',
        'Set-NetAdapterEncapsulatedPacketTaskOffload',
        'Set-NetAdapterIPsecOffload',
        'Set-NetAdapterLso',
        'Set-NetAdapterPowerManagement',
        'Set-NetAdapterQos',
        'Set-NetAdapterRdma',
        'Set-NetAdapterRsc',
        'Set-NetAdapterRss',
        'Set-NetAdapterSriov',
        'Set-NetAdapterVmq',
        'Set-NetAdapterPacketDirect'
        )
        
    CmdletsToExport = @()
    
    AliasesToExport = @()
}
