<#

Copyright (c) 2011 Microsoft Corporation. All Rights Reserved.

Module Name:

    MSFT_NetAdapterPowerManagement.Format.Helper.psm1

Description:

    Provides helper routines for formatting the output of
    NetAdapterPowerManagement CmdLets.

#>

function Format-NdisConfigurationValue()

<#

Function Description:

    Converts an enumerated configuration value into human readable text.

Arguments:

    $ConfigurationValue - Supplies the configuration value to format.

Return Value:

    Returns a string corresponding to the integer configuration value.

#>

{
    param ($ConfigurationValue)

    switch ($ConfigurationValue)
    {
        0 { $temp = "Unsupported" }
        1 { $temp = "Disabled" }
        2 { $temp = "Enabled" }
        3 { $temp = "Inactive" }
        default { $temp = "Invalid ($($ConfigurationValue))" }
    }

    return $temp
}

function Format-WakePatternType()

<#

Function Description:

    Converts a wake pattern type into a friendly name.

Arguments:

    $WakePatternType - Supplies an NDIS_PM_WOL_PACKET enumeration value.

Return Value:

    Returns a friendly string representation of the wake pattern type.

#>

{
    param ($WakePatternType)

    switch ($WakePatternType)
    {
        1 { $temp = "Bitmap Pattern" }
        2 { $temp = "Magic Packet" }
        3 { $temp = "IPv4 TCP SYN" }
        4 { $temp = "IPv6 TCP SYN" }
        5 { $temp = "EAPOL Request Id Message" }
        6 { $temp = "Wildcard" }
        7 { $temp = "WFD Invitation Request" }
        default { $temp = "Invalid ($($ConfigurationValue))" }
    }

    return $temp
}

function Format-BitmapWakePattern()

<#

Function Description:

    Converts a Pattern and Mask array to a string.

Arguments:

    $Pattern - Supplies an array of bytes defining a pattern match.

    $Mask - Supplies an array of bytes masking the pattern. The nth bit of the
        $Mask array indicates whether the nth byte in the Pattern should be
        matched (1) or ignored (0).

Return Value:

    Returns a string containing the bytes of the pattern match. Bytes that are
    ignored are represented by "--".

#>

{
    param(
        [byte[]] ${Pattern},
        [byte[]] ${Mask}
        )

    $Width = 16
    $out = ""

    $PatternLength = $Pattern.Length
    $Bit = 1
    $MaskIndex = 0
    for ($i = 0; $i -lt $PatternLength; $i++)
    {
        #
        # Print offset at the beginning of each line
        #

        if (($i % $Width) -eq 0)
        {
            $out += "{0:x2} : " -f $i
        }

        #
        # Check whether the pattern byte is masked
        #

        if ($Mask[$MaskIndex] -band $Bit)
        {
            $out += "{0:x2} " -f $Pattern[$i]
        }
        else
        {
            $out += "-- " -f $Pattern[$i]
        }

        #
        # Newline if this is the last element on the line.
        #

        if (($i % $Width) -eq ($Width - 1))
        {
            $out += "`n"
        }

        #
        # Walk through the mask bits.
        #

        $Bit = $Bit * 2
        if ($Bit -gt 128)
        {
            $MaskIndex++
            $Bit = 1
        }
    }

    $out
}
