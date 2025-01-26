:: ///////////////////////////////////////////////////
:: Launch Application
:: This batch script is part of the CMD Injector App
:: https://github.com/fadilfadz01/CMD.Injector
:: Copyright (c) Fadil Fadz - @fadilfadz01
:: ///////////////////////////////////////////////////

set IsXap=%1
set ApplicationID=%2

if %IsXap%==True (
	for /f "tokens=4" %%a in ('th.exe ^| findstr /ri /c:".*%ApplicationID%.*"') do TestNavigationApi.exe LaunchSession %%a
) else (
	for /f "tokens=4" %%a in ('th.exe ^| findstr /ri /c:".*%ApplicationID%.*"') do TestNavigationApi.exe LaunchModernApplication %%a null	
)
goto :EOF