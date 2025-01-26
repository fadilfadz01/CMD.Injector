:: ///////////////////////////////////////////////////
:: Live Lockscreen
:: This batch script is part of the CMD Injector App
:: https://github.com/fadilfadz01/CMD.Injector
:: Copyright (c) Fadil Fadz - @fadilfadz01
:: ///////////////////////////////////////////////////

setlocal enabledelayedexpansion

set "Loop=False"
set "Sort=False"

:Loop
if exist "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\LockscreenBreak.txt" (
	del "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\LockscreenBreak.txt"
	set "Sort=False"
	goto Loop
)
if not exist "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\Lockscreen.dat" exit
if not exist "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\LiveLockscreen.bat" exit
for /f %%a in ('tlist -p LockApp.exe') do if %%a==-1 goto Loop
if %Sort%==False (
	for /f "tokens=*" %%a in ('more "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\Lockscreen.dat"') do set "WallpaperPath=%%a"&goto WallpaperPath
	:WallpaperPath
	if not exist "%WallpaperPath%" exit
	for /f "skip=1" %%a in ('more "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\Lockscreen.dat"') do set "TimeInterval=%%a"&goto TimeInterval
	:TimeInterval
	for /f "skip=2" %%a in ('more "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\Lockscreen.dat"') do set "ReverseLoop=%%a"&goto ReverseLoop
	:ReverseLoop
	for /f "skip=3" %%a in ('more "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\Lockscreen.dat"') do set "StartTime=%%a"&goto StartTime
	:StartTime
	for /f "skip=4" %%a in ('more "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\Lockscreen.dat"') do set "StopTime=%%a"&goto StopTime
	:StopTime
	for /f "skip=5" %%a in ('more "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\Lockscreen.dat"') do set "BatterySaver=%%a"&goto BatterySaver
	:BatterySaver
	cd "%WallpaperPath%"
	if exist "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\Lockscreen.txt" del "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\Lockscreen.txt"
	for /r %%a in (*jpeg;*.jpg;*.png) do echo %%a >>"%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\Lockscreen.txt"
	set "Sort=True"
)

set "CurrentTime=%Time:~0,5%"
set "CurrentTime=%CurrentTime: =0%"

if %StartTime::=% lss %StopTime::=% (
	if %CurrentTime::=% lss %StartTime::=% (goto Loop) else (if %CurrentTime::=% geq %StopTime::=% goto Loop)
) else (
	if !StopTime!==00:00 set "StopTime=24:00"
	if %CurrentTime::=% lss %StartTime::=% if %CurrentTime::=% geq %StopTime::=% goto Loop
)

if %ReverseLoop%==True (
	if %Loop%==True (
		for /f "tokens=* skip=2" %%a in ('sort /r "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\Lockscreen.txt"') do (
			if exist "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\LockscreenBreak.txt" (
				del "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\LockscreenBreak.txt"
				set "Sort=False"
				goto Loop
			)
			if not exist "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\Lockscreen.dat" exit
			if not exist "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\LiveLockscreen.bat" exit
			for /f %%b in ('tlist -p LockApp.exe') do if %%b==-1 goto Loop
			if %BatterySaver%==True (
				for /f "tokens=3" %%c in ('reg query "HKLM\Software\OEM\Nokia\Display" /v PowerSaveState') do if %%c==0x1 goto Loop
			)
			reg add "HKLM\Software\Microsoft\Shell\Wallpaper" /v CurrentWallpaper /t REG_SZ /d "%%a" /f
			sleep -m %TimeInterval%
		)
		set "Loop=False"y
	) else (goto Else)
) else (
	:Else
	for /f "tokens=* skip=2" %%a in ('more "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\Lockscreen.txt"') do (
		if exist "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\LockscreenBreak.txt" (
			del "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\LockscreenBreak.txt"
			set "Sort=False"
			goto Loop
		)
		if not exist "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\Lockscreen.dat" exit
		if not exist "%SystemDrive%\Data\USERS\DefApps\APPDATA\LOCAL\Packages\CMDInjector_kqyng60eng17c\LocalState\LiveLockscreen.bat" exit
		for /f %%b in ('tlist -p LockApp.exe') do if %%b==-1 goto Loop
		if %BatterySaver%==True (
			for /f "tokens=3" %%c in ('reg query "HKLM\Software\OEM\Nokia\Display" /v PowerSaveState') do if %%c==0x1 goto Loop
		)
		reg add "HKLM\Software\Microsoft\Shell\Wallpaper" /v CurrentWallpaper /t REG_SZ /d "%%a" /f
		sleep -m %TimeInterval%
	)
	set "Loop=True"
)
goto Loop