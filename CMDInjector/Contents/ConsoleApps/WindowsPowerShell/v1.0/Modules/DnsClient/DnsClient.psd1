@{
    GUID = "5696d5ef-fa2d-4997-94f1-0bc13daa2ac5"
    Author = "Microsoft Corporation"
    CompanyName = "Microsoft Corporation"
    Copyright = "© Microsoft Corporation. All rights reserved."
    ModuleVersion = "1.0.0.0"
    PowerShellVersion = "3.0"
    ClrVersion = "4.0"
    NestedModules = @(
        "dnslookup",
        "MSFT_DnsClient.cdxml",
        "MSFT_DnsClientCache.cdxml",
        "MSFT_DnsClientGlobalSetting.cdxml",
        "MSFT_DnsClientServerAddress.cdxml"
        )
    TypesToProcess = @(
        "DnsCmdlets.Types.ps1xml",
        "DnsConfig.Types.ps1xml"
        )
    FormatsToProcess = @(
        "DnsCmdlets.Format.ps1xml",
        "DnsConfig.Format.ps1xml"
        )
   FunctionsToExport = @(
        "Clear-DnsClientCache",
        "Get-DnsClient",
        "Get-DnsClientCache",
        "Get-DnsClientGlobalSetting",
        "Get-DnsClientServerAddress",
        "Register-DnsClient",
        "Set-DnsClient",
        "Set-DnsClientGlobalSetting",
        "Set-DnsClientServerAddress"
       )
    CmdletsToExport = @(
        "Resolve-DnsName"
        )
   AliasesToExport = @()
   HelpInfoUri = "http://go.microsoft.com/fwlink/?linkid=390768"
}
