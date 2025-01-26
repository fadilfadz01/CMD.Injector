:: ///////////////////////////////////////////////////
:: Non System-Wide
:: This batch script is part of the CMD Injector App
:: https://github.com/fadilfadz01/CMD.Injector
:: Copyright (c) Fadil Fadz - @fadilfadz01
:: ///////////////////////////////////////////////////

for /f %%a in ('dir %SystemDrive%\data\programs\windowsapps\CMDInjector_*_arm__kqyng60eng17c /b') do set "InstalledLocation=%SystemDrive%\data\programs\windowsapps\%%a"
if not exist "%InstalledLocation%" set "InstalledLocation=%SystemDrive%\Data\SharedData\PhoneTools\AppxLayouts\CMDInjectorVS.Debug_ARM.Fadil_Fadz"

if exist "%InstalledLocation%" (
	start telnetd.exe cmd.exe 9999

	if exist "%SystemRoot%\System32\CMDInjectorFirstLaunch.dat" del "%SystemRoot%\System32\CMDInjectorFirstLaunch.dat"
	if exist "%SystemRoot%\System32\CMDInjectorTempSetup.dat" (
		xcopy /cey "%InstalledLocation%\Contents\ConsoleApps\CoreClrPowerShellExt" "%SystemRoot%\system32\CoreClrPowerShellExt\"
		xcopy /cey "%InstalledLocation%\Contents\ConsoleApps\DotNetCore" "%SystemRoot%\system32\DotNetCore\"
		xcopy /cey "%InstalledLocation%\Contents\ConsoleApps\WindowsPowerShell" "%SystemRoot%\system32\WindowsPowerShell\"
		del "%SystemRoot%\System32\CMDInjectorTempSetup.dat"
	)
)

if exist "%SystemRoot%\System32\CMDUninjector.dat" (

	CheckNetIsolation.exe loopbackexempt -d -n=UniversalUpdater_kqyng60eng17c
	CheckNetIsolation.exe loopbackexempt -d -n=CODE-Mobile_kqyng60eng17c
	CheckNetIsolation.exe loopbackexempt -d -n=WinUniveralTool_eyr0bca9nc39y
	CheckNetIsolation.exe loopbackexempt -d -n=W10MGroupMegaRepo_eyr0bca9nc39y
	CheckNetIsolation.exe loopbackexempt -d -n=62223BasharAstifan.WindowsUniversalTool_hj8xdmm31zwmc
	CheckNetIsolation.exe loopbackexempt -d -n=62223BasharAstifan.UniversalToolWUT_hj8xdmm31zwmc
	CheckNetIsolation.exe loopbackexempt -d -n=WinUniversalTool_eyr0bca9nc39y
	CheckNetIsolation.exe loopbackexempt -d -n=wut-mini-c6a2_44kb9g2z2a90y
	CheckNetIsolation.exe loopbackexempt -d -n=immobile-c789_44kb9g2z2a90y
	CheckNetIsolation.exe loopbackexempt -d -n=ChoungNetworksUS.68307A65C913_vvzc8y2tzcnsr
	CheckNetIsolation.exe loopbackexempt -d -n=WPCommandPrompt_6dg21qtxnde1e
	CheckNetIsolation.exe loopbackexempt -d -n=WPCommandPrompt_g5rj6pc6gbtrg

	del "%SystemRoot%\System32\CMDInjector.dat"
	del "%SystemRoot%\System32\CMDInjectorVersion.dat"
	rd /q /s "%SystemRoot%\System32\Dism"
	::rd /q /s "%SystemRoot%\System32\CoreClrPowerShellExt"
	::rd /q /s "%SystemRoot%\System32\DotNetCore"
	::rd /q /s "%SystemRoot%\System32\WindowsPowerShell"
	del "%SystemRoot%\System32\en-US\attrib.exe.mui"
	del "%SystemRoot%\System32\en-US\bcdboot.exe.mui"
	del "%SystemRoot%\System32\en-US\cmdkey.exe.mui"
	del "%SystemRoot%\System32\en-US\cscript.exe.mui"
	del "%SystemRoot%\System32\en-US\Dism.exe.mui"
	del "%SystemRoot%\System32\en-US\find.exe.mui"
	del "%SystemRoot%\System32\en-US\finger.exe.mui"
	del "%SystemRoot%\System32\en-US\ftp.exe.mui"
	del "%SystemRoot%\System32\en-US\help.exe.mui"
	del "%SystemRoot%\System32\en-US\hostname.exe.mui"
	del "%SystemRoot%\System32\en-US\ipconfig.exe.mui"
	del "%SystemRoot%\System32\en-US\label.exe.mui"
	del "%SystemRoot%\System32\en-US\logman.exe.mui"
	del "%SystemRoot%\System32\en-US\mountvol.exe.mui"
	del "%SystemRoot%\System32\en-US\neth.dll.mui"
	del "%SystemRoot%\System32\en-US\netsh.exe.mui"
	del "%SystemRoot%\System32\en-US\nslookup.exe.mui"
	del "%SystemRoot%\System32\en-US\ping.exe.mui"
	del "%SystemRoot%\System32\en-US\recover.exe.mui"
	del "%SystemRoot%\System32\en-US\regsvr32.exe.mui"
	del "%SystemRoot%\System32\en-US\replace.exe.mui"
	del "%SystemRoot%\System32\en-US\sc.exe.mui"
	del "%SystemRoot%\System32\en-US\setx.exe.mui"
	del "%SystemRoot%\System32\en-US\tzutil.exe.mui"
	del "%SystemRoot%\System32\en-US\VaultCmd.exe.mui"
	del "%SystemRoot%\System32\accesschk.exe"
	del "%SystemRoot%\System32\attrib.exe"
	del "%SystemRoot%\System32\bcdboot.exe"
	del "%SystemRoot%\System32\certmgr.exe"
	del "%SystemRoot%\System32\chkdsk.exe"
	del "%SystemRoot%\System32\cmdkey.exe"
	del "%SystemRoot%\System32\cscript.exe"
	del "%SystemRoot%\System32\dcopy.exe"
	del "%SystemRoot%\System32\depends.exe"
	del "%SystemRoot%\System32\DeployAppx.exe"
	del "%SystemRoot%\System32\DeployUtil.exe"
	del "%SystemRoot%\System32\devcon.exe"
	del "%SystemRoot%\System32\DevToolsLauncher.exe"
	del "%SystemRoot%\System32\DIALTESTWP8.exe"
	del "%SystemRoot%\System32\Dism.exe"
	del "%SystemRoot%\System32\fc.exe"
	del "%SystemRoot%\System32\find.exe"
	del "%SystemRoot%\System32\finger.exe"
	del "%SystemRoot%\System32\FolderPermissions.exe"
	del "%SystemRoot%\System32\format.com"
	del "%SystemRoot%\System32\ftp.exe"
	del "%SystemRoot%\System32\ftpd.exe"
	del "%SystemRoot%\System32\FveEnable.exe"
	del "%SystemRoot%\System32\gbot.exe"
	del "%SystemRoot%\System32\gse.dll"
	del "%SystemRoot%\System32\help.exe"
	del "%SystemRoot%\System32\HOSTNAME.exe"
	del "%SystemRoot%\System32\imagex.exe"
	del "%SystemRoot%\System32\IoTSettings.exe"
	del "%SystemRoot%\System32\IoTShell.exe"
	del "%SystemRoot%\System32\iotstartup.exe"
	del "%SystemRoot%\System32\ipconfig.exe"
	del "%SystemRoot%\System32\kill.exe"
	del "%SystemRoot%\System32\label.exe"
	del "%SystemRoot%\System32\logman.exe"
	del "%SystemRoot%\System32\minshutdown.exe"
	del "%SystemRoot%\System32\mountvol.exe"
	del "%SystemRoot%\System32\msnap.exe"
	del "%SystemRoot%\System32\mwkdbgctrl.exe"
	del "%SystemRoot%\System32\net.exe"
	del "%SystemRoot%\System32\net1.exe"
	del "%SystemRoot%\System32\neth.dll"
	del "%SystemRoot%\System32\neth.exe"
	del "%SystemRoot%\System32\netsh.exe"
	del "%SystemRoot%\System32\nslookup.exe"
	del "%SystemRoot%\System32\pacman_ierror.dll"
	del "%SystemRoot%\System32\pacmanerr.dll"
	del "%SystemRoot%\System32\ping.exe"
	del "%SystemRoot%\System32\PMTestErrorLookup.exe"
	del "%SystemRoot%\System32\ProvisioningTool.exe"
	del "%SystemRoot%\System32\recover.exe"
	del "%SystemRoot%\System32\regsvr32.exe"
	del "%SystemRoot%\System32\replace.exe"
	del "%SystemRoot%\System32\sc.exe"
	del "%SystemRoot%\System32\setbootoption.exe"
	del "%SystemRoot%\System32\setcomputername.exe"
	del "%SystemRoot%\System32\SetDisplayResolution.exe"
	del "%SystemRoot%\System32\setx.exe"
	del "%SystemRoot%\System32\sfpdisable.exe"
	del "%SystemRoot%\System32\SirepController.exe"
	del "%SystemRoot%\System32\TaskSchUtil.exe"
	del "%SystemRoot%\System32\TestDeploymentInfo.dll"
	del "%SystemRoot%\System32\TH.exe"
	del "%SystemRoot%\System32\tlist.exe"
	del "%SystemRoot%\System32\TouchRecorder.exe"
	del "%SystemRoot%\System32\tracelog.exe"
	del "%SystemRoot%\System32\tree.com"
	del "%SystemRoot%\System32\tzutil.exe"
	del "%SystemRoot%\System32\VaultCmd.exe"
	del "%SystemRoot%\System32\WPConPlatDev.exe"

	del "%SystemRoot%\servicing\Packages\FadilFadz.CMDInjector.Permanent~628844477771337a~arm~~1.0.0.0.mum"
	del "%SystemRoot%\servicing\Packages\FadilFadz.CMDInjector.Permanent~628844477771337a~arm~~1.0.0.0.cat"
	del "%SystemRoot%\system32\CATROOT\{F750E6C3-38EE-11D1-85E5-00C04FC295EE}\FadilFadz.CMDInjector.Permanent~628844477771337a~arm~~1.0.0.0.cat"
	del "%SystemRoot%\WinSxS\Manifests\arm_fadilfadz.cmdinject..ermanent.deployment_628844477771337a_1.0.0.0_none_3812df19b195b968.manifest"
	del "%SystemRoot%\WinSxS\Manifests\arm_fadilfadz.cmdinjector.permanent0_628844477771337a_1.0.0.0_none_a7a677916de429b9.manifest"
	del "%SystemRoot%\WinSxS\Manifests\arm_fadilfadz.cmdinjector.permanent1_628844477771337a_1.0.0.0_none_a7a660c96de4435a.manifest"
	rd /s /q "%SystemRoot%\WinSxS\arm_fadilfadz.cmdinjector.permanent0_628844477771337a_1.0.0.0_none_a7a677916de429b9"
	if exist "%SystemRoot%\system32\TemporaryInjection.reg" (
		reg load "HKLM\COMPONENTS" "%SystemRoot%\system32\Config\COMPONENTS"
		for /f "delims=\ tokens=5" %%a in ('reg query HKEY_LOCAL_MACHINE\COMPONENTS\DerivedData\VersionedIndex') do %SystemRoot%\system32\WindowsPowerShell\v1.0\powershell -c "gc '%SystemRoot%\system32\TemporaryInjection.reg'" ^| Foreach-object {$_ -replace '{Build}', '%%a'}  >%SystemRoot%\system32\CMDInjectorInjection.reg
		reg import "%SystemRoot%\system32\CMDInjectorInjection.reg"
		reg unload "HKLM\COMPONENTS"
		del "%SystemRoot%\system32\CMDInjectorInjection.reg"
		del "%SystemRoot%\system32\TemporaryInjection.reg"
	)

	del "%SystemRoot%\System32\CMDUninjector.dat"
	Start cmd.exe /c MessageDialog "Successfully applied the CMD un-injection changes." "CMD Injector"
)
goto :EOF