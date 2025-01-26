:: ///////////////////////////////////////////////////
:: Message Dialog
:: This batch script is part of the CMD Injector App
:: https://github.com/fadilfadz01/CMD.Injector
:: Copyright (c) Fadil Fadz - @fadilfadz01
:: ///////////////////////////////////////////////////

set Content=%1
set Title=%2

if not defined Content set Content=Content
if not defined Title set Title=Title

%SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe -noexit -exec bypass -c "&{$obj = [Windows.UI.Popups.MessageDialog, Windows.UI.Popups, ContentType = WindowsRuntime]::new(\"%Content%\", \"%Title%\");$obj.ShowAsync();sleep 30;exit}"
goto :EOF