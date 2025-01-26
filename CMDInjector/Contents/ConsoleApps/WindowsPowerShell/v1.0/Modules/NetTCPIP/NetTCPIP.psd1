@{
    GUID = "{2d0b6c7f-16a0-4185-843f-ae47b6db4551}"
    Author = "Microsoft Corporation"
    CompanyName = "Microsoft Corporation"
    Copyright = "© Microsoft Corporation. All rights reserved."
    HelpInfoUri = "http://go.microsoft.com/fwlink/?linkid=390802"
    ModuleVersion = "1.0.0.0"
    PowerShellVersion = "3.0"

    NestedModules = @(
        "MSFT_NetCompartment",
        "MSFT_NetIPAddress",
        "MSFT_NetIPInterface",
        "MSFT_NetIPv4Protocol",
        "MSFT_NetIPv6Protocol",
        "MSFT_NetNeighbor",
        "MSFT_NetOffloadGlobalSetting",
        "MSFT_NetPrefixPolicy",
        "MSFT_NetRoute",
        "MSFT_NetTCPConnection",
        "MSFT_NetTCPSetting",
        "MSFT_NetTransportFilter",
        "MSFT_NetUDPEndpoint",
        "MSFT_NetUDPSetting",
        "NetIPConfiguration",
        "Test-NetConnection"
        )

    FormatsToProcess = @('Tcpip.Format.ps1xml')
    TypesToProcess = @('Tcpip.Types.ps1xml')

    FunctionsToExport = @(
        "Get-NetIPAddress",
        "Get-NetIPInterface",
        "Get-NetIPv4Protocol",
        "Get-NetIPv6Protocol",
        "Get-NetNeighbor",
        "Get-NetOffloadGlobalSetting",
        "Get-NetPrefixPolicy",
        "Get-NetRoute",
        "Get-NetTCPConnection",
        "Get-NetTCPSetting",
        "Get-NetTransportFilter",
        "Get-NetUDPEndpoint",
        "Get-NetUDPSetting",
        "Get-NetCompartment",
        "Get-NetIPConfiguration"
        "New-NetIPAddress",
        "New-NetNeighbor",
        "New-NetRoute",
        "New-NetTransportFilter",
        "Remove-NetIPAddress",
        "Remove-NetNeighbor",
        "Remove-NetRoute",
        "Remove-NetTransportFilter",
        "Find-NetRoute",
        "Set-NetIPAddress",
        "Set-NetIPInterface",
        "Set-NetIPv4Protocol",
        "Set-NetIPv6Protocol",
        "Set-NetNeighbor",
        "Set-NetOffloadGlobalSetting",
        "Set-NetRoute",
        "Set-NetTCPSetting",
        "Set-NetUDPSetting",
        "Test-NetConnection"
        )

    AliasesToExport = @(
        "gip",
        "TNC"
        )

    CmdletsToExport = @()
}
