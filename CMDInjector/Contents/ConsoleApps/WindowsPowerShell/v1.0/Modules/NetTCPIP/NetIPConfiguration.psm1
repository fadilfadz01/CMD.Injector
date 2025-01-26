class NetIPConfiguration
{
    [string] $ComputerName
    [string] $InterfaceAlias
    [int] $InterfaceIndex
    [string] $InterfaceDescription
    [int] $CompartmentId
    [Microsoft.Management.Infrastructure.CimInstance] $NetAdapter
    [Microsoft.Management.Infrastructure.CimInstance] $NetCompartment
    [Microsoft.Management.Infrastructure.CimInstance] $NetIPv6Interface
    [Microsoft.Management.Infrastructure.CimInstance] $NetIPv4Interface
    [Microsoft.Management.Infrastructure.CimInstance] $NetProfile
    [Microsoft.Management.Infrastructure.CimInstance[]] $AllIPAddresses
    [Microsoft.Management.Infrastructure.CimInstance[]] $IPv6Address
    [Microsoft.Management.Infrastructure.CimInstance[]] $IPv6TemporaryAddress
    [Microsoft.Management.Infrastructure.CimInstance[]] $IPv6LinkLocalAddress
    [Microsoft.Management.Infrastructure.CimInstance[]] $IPv4Address
    [Microsoft.Management.Infrastructure.CimInstance[]] $IPv6DefaultGateway
    [Microsoft.Management.Infrastructure.CimInstance[]] $IPv4DefaultGateway
    [Microsoft.Management.Infrastructure.CimInstance[]] $DNSServer
    [bool] $Detailed = $false
}

# .Link
# http://go.microsoft.com/fwlink/?LinkId=253567
# .ExternalHelp NetIPConfiguration.psm1-help.xml
function Get-NetIPConfiguration
{
    [CmdletBinding(DefaultParametersetName="Alias" )]
    Param(
        [Parameter(ParameterSetName = "Alias", Mandatory = $false, ValueFromPipelineByPropertyName = $true, Position = 0)] [Alias('ifAlias')] [string]$InterfaceAlias = "*",
        [Parameter(ParameterSetName = "Index", Mandatory = $true, ValueFromPipelineByPropertyName = $true)] [Alias('ifIndex')] [int]$InterfaceIndex = -1,
        [Parameter(ParameterSetName = "All", Mandatory = $true)] [Alias('IncludeAllInterfaces')] [switch]$All = $false,
        [Parameter(Mandatory = $false)] [Alias('IncludeAllCompartments')] [switch]$AllCompartments = $false,
        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)] [int]$CompartmentId = -1,
        [Parameter(Mandatory = $false)] [switch]$Detailed = $false,
        [Parameter(Mandatory = $false, ValueFromPipelineByPropertyName = $true)] [Alias('PSComputerName','ComputerName')] [Microsoft.Management.Infrastructure.CimSession]$CimSession = $null
        )
    Process
    {
        #
        # Session defaults to localhost.
        #
        if ($CimSession -eq $null)
        {
            $CimSession = New-CimSession
            $ComputerName = (Get-CimInstance win32_ComputerSystem).name
        }
        else
        {
            $ComputerName = $CimSession.ComputerName
        }

        #
        # Return hidden interfaces if the user query is for a specific interface.
        #
        if (($InterfaceAlias -ne "*") -or ($InterfaceIndex -ne -1))
        {
            $All = $true
        }

        #
        # CompartmentId requires the AllCompartments switch.
        #
        if ($CompartmentId -ne -1)
        {
            $AllCompartments = $true;
            $Compartments = Get-NetCompartment -CompartmentId $CompartmentId -CimSession $CimSession
        }
        else
        {
            $Compartments = Get-NetCompartment -CimSession $CimSession
        }

        #
        # Retrieve NetAdapters and NetIPInterfaces matching the user query.
        #
        if ($InterfaceIndex -ne -1)
        {
            $Adapters = Get-NetAdapter -InterfaceIndex $InterfaceIndex -IncludeHidden -CimSession $CimSession -ErrorAction SilentlyContinue
            $IPInterfaces = Get-NetIPInterface -InterfaceIndex $InterfaceIndex -IncludeAllCompartments:$AllCompartments -PolicyStore ActiveStore -CimSession $CimSession
        }
        elseif ($InterfaceAlias -ne "*")
        {
            $Adapters = Get-NetAdapter -Name $InterfaceAlias -IncludeHidden -CimSession $CimSession -ErrorAction SilentlyContinue
            $IPInterfaces = Get-NetIPInterface -InterfaceAlias $InterfaceAlias -IncludeAllCompartments:$AllCompartments -PolicyStore ActiveStore -CimSession $CimSession
        }
        else
        {
            $Adapters = Get-NetAdapter -IncludeHidden -CimSession $CimSession
            $IPInterfaces = Get-NetIPInterface -IncludeAllCompartments:$AllCompartments -PolicyStore ActiveStore -CimSession $CimSession
        }

        #
        # Filter out other compartments if the user query is for a specific compartment.
        #
        if ($CompartmentId -ne -1)
        {
            $IPInterfaces = $IPInterfaces | where CompartmentId -eq $CompartmentId;
        }

        #
        # Get the indices of all interfaces that are bound to IP.
        #
        $IfIndices = $IPInterfaces | select -ExpandProperty ifIndex -Unique

        #
        # Create a NetIPConfiguration for each interface index.
        #
        $IPConfigs = @()

        #
        # Check if the Get-NetConnectionProfile cmdlet is available.
        #
        $NetConnectionCmdletExists = Get-Command Get-NetConnectionProfile -ErrorAction SilentlyContinue

        #
        # Check if the Get-DnsClientServerAddress cmdlet is available.
        #
        $DNSClientCmdletExists = Get-Command Get-DnsClientServerAddress -ErrorAction SilentlyContinue

        foreach ($IfIndex in $IfIndices)
        {
            $IPConfig = [NetIPConfiguration]::New()
            $IPConfig.ComputerName = $ComputerName
            $IPConfig.InterfaceIndex = $IfIndex
            $IPConfig.Detailed = $Detailed

            #
            # Link up the NetAdapter and NetIPInterface objects.
            #
            $IPConfig.NetAdapter = $Adapters | where InterfaceIndex -eq $IfIndex
            $IPConfig.NetIPv4Interface = $IPInterfaces | where {($_.InterfaceIndex -eq $IfIndex ) -and ($_.AddressFamily -eq "IPv4")}
            $IPConfig.NetIPv6Interface = $IPInterfaces | where {($_.InterfaceIndex -eq $IfIndex ) -and ($_.AddressFamily -eq "IPv6")}

            #
            # Filter out hidden objects if necessary.
            #
            if ((-not $All) -and ($IPConfig.NetAdapter.Hidden))
            {
                continue
            }

            #
            # Filter out objects with no underlying NetAdapter, with two exceptions:
            # - VPN interfaces have a LowestIfNetLuid, but should not be linked to a NetAdapter.
            # - Isolation interfaces also have an IsolationId and should be linked to a NetAdapter.
            #
            if ($IPConfig.NetAdapter -eq $null)
            {
                if ($IPConfig.NetIPv6Interface.LowestIfNetLuid -ne 0)
                {
                    if ($IPConfig.NetIPv6Interface.IsolationId -ne 0)
                    {
                        $IPConfig.NetAdapter = $Adapters | where NetLuid -eq $IPConfig.NetIPv6Interface.LowestIfNetLuid
                    }
                }
                elseif ($IPConfig.NetIPv4Interface.LowestIfNetLuid -ne 0)
                {
                    if ($IPConfig.NetIPv4Interface.IsolationId -ne 0)
                    {
                        $IPConfig.NetAdapter = $Adapters | where NetLuid -eq $IPConfig.NetIPv4Interface.LowestIfNetLuid
                    }
                }
                else
                {
                    continue
                }
            }

            #
            # Find the best object to source the interface alias and compartment ID.
            #
            if ($IPConfig.NetIPv6Interface -ne $null)
            {
                $IPConfig.InterfaceAlias = $IPConfig.NetIPv6Interface.InterfaceAlias
                $IPConfig.CompartmentId = $IPConfig.NetIPv6Interface.CompartmentId
            }
            elseif ($IPConfig.NetIPv4Interface -ne $null)
            {
                $IPConfig.InterfaceAlias = $IPConfig.NetIPv4Interface.InterfaceAlias
                $IPConfig.CompartmentId = $IPConfig.NetIPv4Interface.CompartmentId
            }

            #
            # Escape wilcard characters in interface alias.
            #
            $IfAlias = [System.Management.Automation.WildcardPattern]::Escape($IPConfig.InterfaceAlias)

            #
            # Get NetAdapter properties.
            #
            if ($IPConfig.NetAdapter -ne $null)
            {
                $IPConfig.InterfaceDescription = $IPConfig.NetAdapter.InterfaceDescription
            }

            $IPConfig.NetCompartment = $Compartments | where CompartmentId -eq $IPConfig.CompartmentId
            if ($NetConnectionCmdletExists)
            {
                $IPConfig.NetProfile = Get-NetConnectionProfile -InterfaceAlias $IfAlias -CimSession $CimSession -ErrorAction SilentlyContinue
            }

            # Begin Verbose Section
            write-verbose "----------------------------------------"
            write-verbose "Interface $IfAlias"
            write-verbose "----------------------------------------"
            write-verbose "InterfaceAlias: Get-NetAdapter -Name '$IfAlias' -CimSession $ComputerName | select InterfaceAlias"
            write-verbose "InterfaceIndex: Get-NetAdapter -Name '$IfAlias' -CimSession $ComputerName | select InterfaceIndex"
            write-verbose "InterfaceDescription: Get-NetAdapter -Name '$IfAlias' -CimSession $ComputerName | select InterfaceDescription"
            write-verbose "NetCompartment: Get-NetCompartment -CompartmentId '$CompartmentId' -CimSession $ComputerName"
            if ($NetConnectionCmdletExists)
            {
                write-verbose "NetProfile: Get-NetConnectionProfile -InterfaceAlias '$IfAlias' -CimSession $ComputerName"
            }
            write-verbose "NetIPv6Interface: Get-NetIPInterface -InterfaceAlias '$IfAlias' -AddressFamily IPv6 -PolicyStore ActiveStore -CimSession $ComputerName"
            write-verbose "NetIPv4Interface: Get-NetIPInterface -InterfaceAlias '$IfAlias' -AddressFamily IPv4 -PolicyStore ActiveStore -CimSession $ComputerName"
            # End Verbose Section

            #
            # Get IPv6 addresses.
            #
            $Addresses = Get-NetIPAddress -InterfaceAlias $IfAlias -AddressFamily IPv6 -IncludeAllCompartments:$AllCompartments -PolicyStore ActiveStore -CimSession $CimSession -ErrorAction SilentlyContinue
            write-verbose "IPv6Address: Get-NetIPAddress -InterfaceAlias '$IfAlias' -AddressFamily IPv6 -PolicyStore ActiveStore -CimSession $ComputerName"

            $IPv6Addresses = @()
            $IPv6LinkLocalAddresses = @()
            $IPv6TemporaryAddresses = @()

            foreach ($Address in $Addresses)
            {
                if (($Address.PrefixOrigin -eq "RouterAdvertisement") -and ($Address.SuffixOrigin -eq "Random"))
                {
                    $IPv6TemporaryAddresses += $Address
                }
                elseif (($Address.PrefixOrigin -eq "WellKnown") -and ($Address.SuffixOrigin -eq "Link"))
                {
                    $IPv6LinkLocalAddresses += $Address
                }
                else
                {
                    $IPv6Addresses += $Address
                }
            }

            $IPConfig.IPv6Address = $IPv6Addresses
            $IPConfig.IPv6TemporaryAddress = $IPv6TemporaryAddresses
            $IPConfig.IPv6LinkLocalAddress = $IPv6LinkLocalAddresses

            #
            # Get IPv4 addresses.
            #
            $IPConfig.IPv4Address = Get-NetIPAddress -InterfaceAlias $IfAlias -AddressFamily IPv4 -IncludeAllCompartments:$AllCompartments -PolicyStore ActiveStore -CimSession $CimSession -ErrorAction SilentlyContinue
            write-verbose "IPv4Address: Get-NetIPAddress -InterfaceAlias '$IfAlias' -AddressFamily IPv4 -PolicyStore ActiveStore -CimSession $ComputerName"

            $IPConfig.AllIPAddresses = $IPConfig.IPv4Address + $IPv6Addresses + $IPv6TemporaryAddresses + $IPv6LinkLocalAddresses

            #
            # Get default gateway.
            #
            $IPConfig.IPv6DefaultGateway = Get-NetRoute -DestinationPrefix "::/0" -InterfaceAlias $IfAlias -IncludeAllCompartments:$AllCompartments -PolicyStore ActiveStore -CimSession $CimSession -ErrorAction SilentlyContinue
            $IPConfig.IPv4DefaultGateway = Get-NetRoute -DestinationPrefix "0.0.0.0/0" -InterfaceAlias $IfAlias -IncludeAllCompartments:$AllCompartments -PolicyStore ActiveStore -CimSession $CimSession -ErrorAction SilentlyContinue
            write-verbose "IPv6DefaultGateway: Get-NetRoute -DestinationPrefix '::/0' -InterfaceAlias '$IfAlias' -PolicyStore ActiveStore -CimSession $ComputerName | select NextHop"
            write-verbose "IPv4DefaultGateway: Get-NetRoute -DestinationPrefix '0.0.0.0/0' -InterfaceAlias '$IfAlias' -PolicyStore ActiveStore -CimSession $ComputerName | select NextHop"

            #
            # Get DNS servers.
            #
            if ($DNSClientCmdletExists)
            {
                $IPConfig.DNSServer = Get-DnsClientServerAddress -InterfaceAlias $IfAlias -CimSession $CimSession -ErrorAction SilentlyContinue | Sort AddressFamily -Descending
                write-verbose "DNSServer: Get-DnsClientServerAddress -InterfaceAlias '$IfAlias' -CimSession $ComputerName | select ServerAddresses"
            }

            $IPConfigs += $IPConfig
        }

        #
        # Group the results by compartment, where each group is sorted as:
        # - first the interfaces with connected NetAdapters sorted by media type,
        # - then the interfaces without a NetAdapter,
        # - then the interfaces with disconnected NetAdapters sorted by media type.
        #
        $Return = @()
        $CompartmentIds = $IPInterfaces | select -ExpandProperty CompartmentId -Unique | sort {$_.CompartmentId}

        foreach ($CompartmentId in $CompartmentIds)
        {
            $IPConfigsInCompartment = $IPConfigs | where {$_.CompartmentId -eq $CompartmentId}
            $Return += $IPConfigsInCompartment | where {$_.NetAdapter.Status -eq "Up" } | sort {$_.NetAdapter.MediaType, $_.InterfaceIndex}
            $Return += $IPConfigsInCompartment | where {$_.NetAdapter -eq $null} | sort {$_.InterfaceIndex}
            $Return += $IPConfigsInCompartment | where {($_.NetAdapter -ne $null) -and ($_.NetAdapter.Status -ne "Up")} | sort {$_.NetAdapter.MediaType} | sort {$_.NetAdapter.Status} -Descending
        }

        return $Return
    }
}

New-Alias gip Get-NetIPConfiguration
Export-ModuleMember -Alias gip -Function Get-NetIPConfiguration
