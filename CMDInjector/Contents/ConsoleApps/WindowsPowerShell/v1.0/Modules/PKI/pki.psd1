@{
GUID="{cf094c6b-63d1-4dda-bf70-15a602c4eb2b}"
Author="Microsoft Corporation"
CompanyName="Microsoft Corporation"
Copyright="© Microsoft Corporation. All rights reserved."
ModuleVersion="1.0.0.0"
NestedModules="Microsoft.CertificateServices.PKIClient.Cmdlets"
TypesToProcess = 'pki.types.ps1xml'
HelpInfoUri="http://go.microsoft.com/fwlink/?linkid=390811"
PowerShellVersion='3.0'
CLRVersion='4.0'
CmdletsToExport = @(
    'Import-PfxCertificate', 'Export-PfxCertificate', 'Export-Certificate', 'Import-Certificate')
}
