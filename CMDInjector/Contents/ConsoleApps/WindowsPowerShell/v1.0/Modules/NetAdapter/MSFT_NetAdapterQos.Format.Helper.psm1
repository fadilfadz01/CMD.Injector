<#

Copyright (c) 2011 Microsoft Corporation. All Rights Reserved.

Module Name:

    MSFT_NetAdapterQos.Format.Helper.psm1

Description:

    Provides helper routines for formatting the output of NetAdapterQos cmdlets.

#>

#
# This function accepts a PRE-SORTED array of integers and returns a formatted
# string that groups these numbers as follows:
#
#    In:  {0,1,2,4,6,7}
#    Out: "0-2, 4, 6-7"
#
function Format-NetAdapterQosIntegerArray(
    $array
)
{
    $out = ""
    $first = $array[0]
    $previous = $array[0]

    for ($i = 1; $i -le $array.count; $i++)
    {
        $previous++

        if ($i -eq $array.count -or $array[$i] -ne $previous)
        {
            if ($out[0]) { $out += "," }

            if ($previous - $first -gt 1)
            {
                $out += "$($first)-$($previous - 1)"
            }
            else
            {
                $out += "$($first)"
            }

            if ($i -ne $array.count)
            {
                $first = $array[$i]
                $previous = $array[$i]
            }
        }
    }

    return $out
}

#
# This function accepts an MSFT_NetAdapter_QosSettings and generates formatted
# output using the information from the PriorityAssignmentTable,
# BandwidthAssignmentTable, and TsaAssignmentTable fields.
#
function Format-NetAdapterQosTrafficClass(
    $settings
)
{
    $template = "{0,2} {1,-6} {2,-9} {3}`n"

    $out  = ""
    $out += $template -f "TC", "TSA", "Bandwidth", "Priorities"
    $out += $template -f "--", "---", "---------", "----------"

    $tcprimap = @(@(),@(),@(),@(),@(),@(),@(),@())
    for ($tc = 0; $tc -lt $tcprimap.count; $tc++)
    {
        $tcprimap[$tc] = @()
    }

    for ($pri = 0; $pri -lt $settings.PriorityAssignmentTable.count; $pri++)
    {
        $tc = $settings.PriorityAssignmentTable[$pri]
        $tcprimap[$tc] += @($pri)
    }

    $actual_cnt = 0

    for ($tc = 0; $tc -lt $settings.TsaAssignmentTable.count; $tc++)
    {
        if ($tcprimap[$tc].count -eq 0) { continue }

        $actual_cnt++

        $tsa = [Microsoft.PowerShell.Cmdletization.GeneratedTypes.NetAdapterQos.Tsa]$settings.TsaAssignmentTable[$tc]
        $temp = Format-NetAdapterQosIntegerArray $tcprimap[$tc]

        $bw_str = ""
        if ($tsa -eq [Microsoft.PowerShell.Cmdletization.GeneratedTypes.NetAdapterQos.Tsa]::ETS)
        {
            $bw = $settings.BandwidthAssignmentTable[$tc]
            $bw_str = "$($bw)%"
        }

        $out += $template -f $tc, $tsa, $bw_str, $temp
    }

    if ($actual_cnt -eq 0)
    {
        $out = ""
    }

    return $out
}

#
# This function accepts an MSFT_NetAdapter_QosSettings and generates formatted
# output using the information from the PriorityFlowControlEnableArray field.
#
function Format-NetAdapterQosFlowControl(
    $settings
)
{
    $enablecnt = 0
    $enablearray = @()

    for ($pri = 0; $pri -lt $settings.PriorityFlowControlEnableArray.count; $pri++)
    {
        if ($settings.PriorityFlowControlEnableArray[$pri])
        {
            $enablecnt++
            $enablearray += @($pri)
        }
    }

    if ($enablecnt -eq $settings.PriorityFlowControlEnableArray.count) { $out = "All Priorities Enabled" }
    elseif ($enablecnt -eq 0) { $out = "All Priorities Disabled" }
    elseif ($enablecnt -eq 1) { $out = "Priority $($enablearray[0]) Enabled" }
    else { $out = "Priorities $(Format-NetAdapterQosIntegerArray $enablearray) Enabled" }

    return $out
}

#
# This function accepts an MSFT_NetAdapter_QosSettings and generates formatted
# output using the information from the ClassificationTable field.
#
function Format-NetAdapterQosClassification(
    $settings
)
{
    if ($settings.ClassificationTable.count -eq 0)
    {
        return ""
    }

    $template = "{0,-9} {1,-9} {2}`n"

    $out  = ""
    $out += $template -f "Protocol", "Port/Type", "Priority"
    $out += $template -f "--------", "---------", "--------"

    foreach ($element in $settings.ClassificationTable)
    {
        switch ($element.ProtocolSelector)
        {
            0 { $protocol_str = "Reserved";                             $port_str = "" }
            1 { $protocol_str = "Default";                              $port_str = "" }
            2 { $protocol_str = "TCP";                                  $port_str = "$($element.ProtocolSpecificValue)" }
            3 { $protocol_str = "UDP";                                  $port_str = "$($element.ProtocolSpecificValue)" }
            4 { $protocol_str = "TCP/UDP";                              $port_str = "$($element.ProtocolSpecificValue)" }
            5 { $protocol_str = "Ethertype";                            $port_str = "0x{0:X0}" -f $element.ProtocolSpecificValue }
            6 { $protocol_str = "NetDirect";                            $port_str = "$($element.ProtocolSpecificValue)" }
            default { $protocol_str = "($($element.ProtocolSelector))"; $port_str = "0x{0:X0}" -f $element.ProtocolSpecificValue }
        }

        $out += $template -f $protocol_str, $port_str, $element.Priority
    }

    return $out
}
