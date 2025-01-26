<#
Copyright (C) Microsoft. All rights reserved.
#>

#requires -version 3.0

#.ExternalHelp Microsoft.Windows.Appx.PackageManager.Commands.dll-help.xml
Function Get-AppxLastError()
{
    [CmdletBinding(HelpUri="http://go.microsoft.com/fwlink/?LinkId=246401")]
    param()

    Import-LocalizedData -bindingVariable msgTable

    # Search for last Add-AppxPackage related error
    foreach ($err in $global:Error)
    {
        if ($err.FullyQualifiedErrorId -match
            "DeploymentError,Microsoft.Windows.Appx.PackageManager.Commands.AddAppxPackageCommand")
        {
            $command = (Get-History | ? {$_.CommandLine -imatch 
                "Add-AppxPackage"} |Select-Object -Last 1).CommandLine;

            # Read ActivityId from the Exception record
            if ($err.Exception -imatch 
                "\[ActivityId\] ([0-9a-f]{8}-([0-9a-f]{4}-){3}[0-9a-f]{12})")
            {
                Write-Host($msgTable.EventLogsFor + ' "' + $command + '"');
                # Retrieve Event Log messages for ActivityId                
                Get-WinEvent -LogName Microsoft-Windows-Appx* -Oldest |
                    Where-Object {$_.Activityid -eq $matches[1]} |
                    ForEach-Object{
                        $null = $_.pstypenames.clear()
                        $null = $_.pstypenames.add('AppxDeploymentEventLog')
                        $_
                    }
                break;
            }
        }
    }
}

#.ExternalHelp Microsoft.Windows.Appx.PackageManager.Commands.dll-help.xml
Function Get-AppxLog()
{
    [CmdletBinding(DefaultParameterSetName="All",
        HelpUri="http://go.microsoft.com/fwlink/?LinkId=246400")]
    param([switch][parameter(parametersetname="All")]$All,
          [string][parameter(parametersetname="ActivityId")]$ActivityId)

    switch($PsCmdlet.ParameterSetName)
    {
    "All"
    {
        if ($All)
        {
            # Get all deployment events
            $logevents = Get-WinEvent -log Microsoft-Windows-AppxDeploy*

            # Select only unique activity IDs of deployment events
            $activityids = $logevents |% {$_.activityid} | select -unique

            # Grab all logs associated with Appx Deployment
            $logevents = Get-WinEvent -log Microsoft-Windows-Appx*
            [array]::Reverse($logevents)
            foreach($id in $activityids)
            {
                $logevents |
                    Where-Object {$_.activityid -eq $id} |
                    ForEach-Object{
                        $null = $_.pstypenames.clear()
                        $null = $_.pstypenames.add('AppxDeploymentEventLog')
                        $_
                    }
            }
        }
        else
        {
            $ActivityId = get-winevent -log Microsoft-Windows-AppxDeploy* -max 1 |% {$_.ActivityId}
            Get-WinEvent -log Microsoft-Windows-Appx* -oldest |
                Where-Object {$_.activityid -eq $ActivityId} |
                ForEach-Object{
                        $null = $_.pstypenames.clear()
                        $null = $_.pstypenames.add('AppxDeploymentEventLog')
                        $_
                }

        }
    }
    "ActivityId" { Get-WinEvent -log Microsoft-Windows-Appx* -oldest |
            Where-Object {$_.activityid -eq $ActivityId} |
            ForEach-Object{
                    $null = $_.pstypenames.clear()
                    $null = $_.pstypenames.add('AppxDeploymentEventLog')
                    $_
            }
       }

    }
}

Microsoft.PowerShell.Core\Export-ModuleMember -Function 'Get-AppxLastError'
Microsoft.PowerShell.Core\Export-ModuleMember -Function 'Get-AppxLog'

