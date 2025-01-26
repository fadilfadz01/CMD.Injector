:: ///////////////////////////////////////////////////
:: Reboot To Flashing Mode
:: This batch script is part of the CMD Injector App
:: https://github.com/fadilfadz01/CMD.Injector
:: Copyright (c) Fadil Fadz - @fadilfadz01
:: ///////////////////////////////////////////////////

%SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe -exec bypass -c "&{$RebootToFlashingMode = '[DllImport(\"WPCoreUtil.dll\", CharSet = CharSet.Unicode, SetLastError = true)]public static extern int RebootToFlashingMode();';$WPCoreUtil = Add-Type -MemberDefinition $RebootToFlashingMode -Name 'WPCoreUtil' -Namespace 'Win32' -PassThru;$WPCoreUtil::RebootToFlashingMode();}"
goto :EOF