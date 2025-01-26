<#

Copyright (c) 2011 Microsoft Corporation. All Rights Reserved.

Module Name:

    NetAdapter.Format.Helper.psm1

Description:

    Provides helper routines for formatting the output of
    NetAdapter CmdLets.

#>

function Format-MacAddress($MacAddress)
<#
Function Description:
    This function formats a MAC address

Arguments:
    $MacAddress

Return Value:
    Formatted MAC address
#>
{
    $out = ""
    if($MacAddress -ne $null)
    {
        for($i = 0; $i -lt $MacAddress.Length; )
        {
            $out += $MacAddress[$i++];
            if($i -eq $MacAddress.Length)
            {
              break;
            }
            $out += $MacAddress[$i++];
            if ($i -lt $MacAddress.Length)
            {
              $out += '-';
            }
        }
    }
    return $out
}

function Format-AdapterName($Name, $Width)
<#
Function Description:
    This function formats a network adapter name (ifAlias)
    by fitting the name in specified column width.
    If the name contains a number at the end, this function
    makes sure that the number is preserved.

Arguments:
    $Name
    $Width

Return Value:
    Formatted name
#>
{
    $out = $Name
    if ($Name.length -gt $width)
    {
        $idpresent = $Name -match "(\d+$)"
        if ($idpresent -and ($width -gt ($matches[0].length+3)))
        {
            $base = $Name.substring(0, $width - ($matches[0].length+3))
            $out=$base+"..."+$matches[0]
        }
    }

    return $out
}

function Format-AdapterInstanceID($InstanceID, $Width)
<#
Function Description:
    This function formats a network adapter InstanceID (ifDeesc)
    by fitting the InstanceID in specified column width.
    If the InstanceID contains a number at the end, this function
    makes sure that the number is preserved.

Arguments:
    $InstanceID
    $Width

Return Value:
    Formatted InstanceID
#>
{
    $out = $InstanceID
    if ($InstanceID.length -gt $width)
    {
        $idpresent = $InstanceID -match "(#\d+$)"
        if ($idpresent -and ($width -gt ($matches[0].length+3)))
        {
            $base = $InstanceID.substring(0, $width - ($matches[0].length+3))
            $out=$base+"..."+$matches[0]
        }
    }
    return $out
}

function Format-LinkSpeed([Decimal]$LinkSpeed, $Precision = 1, $AutoFormat = $true)
<#
Function Description:
    This function returns the link speed of a network adapter

Arguments:
    $LinkSpeed  - LinkSpeed
    $Precision  - Precision to use when rounding
    $AutoFormat - Automatically format the number and display units after

Return Value:
    Formatted link speed
#>
{
    if ($LinkSpeed -eq $null) {
        return "Unknown"
    }
    
    if ($AutoFormat -eq $true) {
        $Postfixes = @("", "K", "M", "G", "T")
        for ($i = 0; $LinkSpeed -ge 1000 -and $i -lt $Postfixes.Length - 1; $i++) {
            $LinkSpeed /= 1000
        }
        $ps = " " + $Postfixes[$i] + "bps"
    } else {
        $ps = $null
    }
    
    $LinkSpeed = [Math]::Round([Decimal]$LinkSpeed, $Precision)
    return "$LinkSpeed" + $ps
}
