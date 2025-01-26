:: ///////////////////////////////////////////////////
:: System-Wide
:: This batch script is part of the CMD Injector App
:: https://github.com/fadilfadz01/CMD.Injector
:: Copyright (c) Fadil Fadz - @fadilfadz01
:: ///////////////////////////////////////////////////

::start ftpd.exe
start telnetd.exe cmd.exe 9999
start telnetd.exe cmd.exe 23
start telnetd.exe %SystemRoot%\system32\WindowsPowerShell\v1.0\powershell.exe 24

CheckNetIsolation.exe loopbackexempt -a -n=UniversalUpdater_kqyng60eng17c
CheckNetIsolation.exe loopbackexempt -a -n=CODE-Mobile_kqyng60eng17c
CheckNetIsolation.exe loopbackexempt -a -n=WinUniveralTool_eyr0bca9nc39y
CheckNetIsolation.exe loopbackexempt -a -n=W10MGroupMegaRepo_eyr0bca9nc39y
CheckNetIsolation.exe loopbackexempt -a -n=62223BasharAstifan.WindowsUniversalTool_hj8xdmm31zwmc
CheckNetIsolation.exe loopbackexempt -a -n=62223BasharAstifan.UniversalToolWUT_hj8xdmm31zwmc
CheckNetIsolation.exe loopbackexempt -a -n=WinUniversalTool_eyr0bca9nc39y
CheckNetIsolation.exe loopbackexempt -d -n=wut-mini-c6a2_44kb9g2z2a90y
CheckNetIsolation.exe loopbackexempt -d -n=immobile-c789_44kb9g2z2a90y
CheckNetIsolation.exe loopbackexempt -a -n=ChoungNetworksUS.68307A65C913_vvzc8y2tzcnsr
CheckNetIsolation.exe loopbackexempt -a -n=WPCommandPrompt_6dg21qtxnde1e
CheckNetIsolation.exe loopbackexempt -a -n=WPCommandPrompt_g5rj6pc6gbtrg

reg add "HKLM\SOFTWARE\Microsoft\SecurityManager\PrincipalClasses\PRINCIPAL_CLASS_TCB" /v Directories /t REG_MULTI_SZ /d %SystemDrive%\\0 /f
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" /v Path /t REG_EXPAND_SZ /d ^%%SystemRoot^%%\system32;^%%SystemRoot^%%;^%%SystemDrive^%%\Programs\CommonFiles\System;^%%SystemDrive^%%\wtt;^%%SystemDrive^%%\data\test\bin;^%%SystemRoot^%%\system32\WindowsPowerShell\v1.0; /f
::reg add "HKLM\SYSTEM\CurrentControlSet\services\KeepWiFiOnSvc" /v Start /t REG_DWORD /d 2 /f

if exist "%SystemRoot%\System32\CMDInjectorFirstLaunch.dat" del "%SystemRoot%\System32\CMDInjectorFirstLaunch.dat"
if exist "%SystemRoot%\System32\CMDInjectorTempSetup.dat" del "%SystemRoot%\System32\CMDInjectorTempSetup.dat"

for /f %%a in ('dir %SystemDrive%\data\programs\windowsapps\CMDInjector_*_arm__kqyng60eng17c /b') do set "InstalledLocation=%SystemDrive%\data\programs\windowsapps\%%a"
if not exist "%InstalledLocation%" set "InstalledLocation=%SystemDrive%\Data\SharedData\PhoneTools\AppxLayouts\CMDInjectorVS.Debug_ARM.Fadil_Fadz"

if exist "%InstalledLocation%" (
	if exist "%SystemRoot%\system32\CMDInjector.dat" (
		xcopy /cey "%InstalledLocation%\Contents\ConsoleApps\Dism" "%SystemRoot%\system32\Dism\"
		xcopy /cey "%InstalledLocation%\Contents\ConsoleApps\CoreClrPowerShellExt" "%SystemRoot%\system32\CoreClrPowerShellExt\"
		xcopy /cey "%InstalledLocation%\Contents\ConsoleApps\DotNetCore" "%SystemRoot%\system32\DotNetCore\"
		xcopy /cey "%InstalledLocation%\Contents\ConsoleApps\WindowsPowerShell" "%SystemRoot%\system32\WindowsPowerShell\"

		if exist "%SystemRoot%\system32\CMDInjectorTemporary.dat" (
			del "%SystemRoot%\servicing\Packages\FadilFadz.CMDInjector.Permanent~628844477771337a~arm~~1.0.0.0.mum"
			del "%SystemRoot%\servicing\Packages\FadilFadz.CMDInjector.Permanent~628844477771337a~arm~~1.0.0.0.cat"
			del "%SystemRoot%\system32\CATROOT\{F750E6C3-38EE-11D1-85E5-00C04FC295EE}\FadilFadz.CMDInjector.Permanent~628844477771337a~arm~~1.0.0.0.cat"
			del "%SystemRoot%\WinSxS\Manifests\arm_fadilfadz.cmdinject..ermanent.deployment_628844477771337a_1.0.0.0_none_3812df19b195b968.manifest"
			del "%SystemRoot%\WinSxS\Manifests\arm_fadilfadz.cmdinjector.permanent0_628844477771337a_1.0.0.0_none_a7a677916de429b9.manifest"
			del "%SystemRoot%\WinSxS\Manifests\arm_fadilfadz.cmdinjector.permanent1_628844477771337a_1.0.0.0_none_a7a660c96de4435a.manifest"
			rd /s /q "%SystemRoot%\WinSxS\arm_fadilfadz.cmdinjector.permanent0_628844477771337a_1.0.0.0_none_a7a677916de429b9"
			reg load "HKLM\COMPONENTS" "%SystemRoot%\system32\Config\COMPONENTS"
			for /f "delims=\ tokens=5" %%a in ('reg query HKEY_LOCAL_MACHINE\COMPONENTS\DerivedData\VersionedIndex') do %SystemRoot%\system32\WindowsPowerShell\v1.0\powershell -c "gc '%InstalledLocation%\Contents\InjectionType\TemporaryInjection.reg'" ^| Foreach-object {$_ -replace '{Build}', '%%a'}  >%SystemRoot%\system32\CMDInjectorInjection.reg
			reg import "%SystemRoot%\system32\CMDInjectorInjection.reg"
			reg unload "HKLM\COMPONENTS"
			del %SystemRoot%\system32\CMDInjectorInjection.reg
			del %SystemRoot%\system32\CMDInjectorTemporary.dat
		) else if exist "%SystemRoot%\system32\CMDInjectorPermanent.dat" (
			takeown.exe /f %SystemRoot%\servicing\Packages /r
			takeown.exe /f %SystemRoot%\WinSxS\Manifests /r
			icacls.exe %SystemRoot%\servicing\Packages /grant everyone:f /t
			icacls.exe %SystemRoot%\WinSxS\Manifests /grant everyone:f /t
			xcopy /y "%InstalledLocation%\Contents\InjectionType\FadilFadz.CMDInjector.Permanent~628844477771337a~arm~~1.0.0.0.mum" "%SystemRoot%\servicing\Packages\"
			xcopy /y "%InstalledLocation%\Contents\InjectionType\FadilFadz.CMDInjector.Permanent~628844477771337a~arm~~1.0.0.0.cat" "%SystemRoot%\servicing\Packages\"
			xcopy /y "%InstalledLocation%\Contents\InjectionType\FadilFadz.CMDInjector.Permanent~628844477771337a~arm~~1.0.0.0.cat" "%SystemRoot%\system32\CATROOT\{F750E6C3-38EE-11D1-85E5-00C04FC295EE}\"
			xcopy /y "%InstalledLocation%\Contents\InjectionType\arm_fadilfadz.cmdinject..ermanent.deployment_628844477771337a_1.0.0.0_none_3812df19b195b968.manifest" "%SystemRoot%\WinSxS\Manifests\"
			xcopy /y "%InstalledLocation%\Contents\InjectionType\arm_fadilfadz.cmdinjector.permanent0_628844477771337a_1.0.0.0_none_a7a677916de429b9.manifest" "%SystemRoot%\WinSxS\Manifests\"
			xcopy /y "%InstalledLocation%\Contents\InjectionType\arm_fadilfadz.cmdinjector.permanent1_628844477771337a_1.0.0.0_none_a7a660c96de4435a.manifest" "%SystemRoot%\WinSxS\Manifests\"
			md "%SystemRoot%\WinSxS\arm_fadilfadz.cmdinjector.permanent0_628844477771337a_1.0.0.0_none_a7a677916de429b9\windows\System32\en-US"
			icacls.exe %SystemRoot%\servicing\Packages /grant everyone:f /t
			icacls.exe %SystemRoot%\WinSxS\Manifests /grant everyone:f /t
			xcopy /y "%InstalledLocation%\Contents\InjectionType\FadilFadz.CMDInjector.Permanent~628844477771337a~arm~~1.0.0.0.mum" "%SystemRoot%\servicing\Packages\"
			xcopy /y "%InstalledLocation%\Contents\InjectionType\FadilFadz.CMDInjector.Permanent~628844477771337a~arm~~1.0.0.0.cat" "%SystemRoot%\servicing\Packages\"
			xcopy /y "%InstalledLocation%\Contents\InjectionType\arm_fadilfadz.cmdinject..ermanent.deployment_628844477771337a_1.0.0.0_none_3812df19b195b968.manifest" "%SystemRoot%\WinSxS\Manifests\"
			xcopy /y "%InstalledLocation%\Contents\InjectionType\arm_fadilfadz.cmdinjector.permanent0_628844477771337a_1.0.0.0_none_a7a677916de429b9.manifest" "%SystemRoot%\WinSxS\Manifests\"
			xcopy /y "%InstalledLocation%\Contents\InjectionType\arm_fadilfadz.cmdinjector.permanent1_628844477771337a_1.0.0.0_none_a7a660c96de4435a.manifest" "%SystemRoot%\WinSxS\Manifests\"
			reg load "HKLM\COMPONENTS" "%SystemRoot%\system32\Config\COMPONENTS"
			for /f "delims=\ tokens=5" %%a in ('reg query HKEY_LOCAL_MACHINE\COMPONENTS\DerivedData\VersionedIndex') do %SystemRoot%\system32\WindowsPowerShell\v1.0\powershell -c "gc '%InstalledLocation%\Contents\InjectionType\PermanentInjection.reg'" ^| Foreach-object {$_ -replace '{Build}', '%%a'}  >%SystemRoot%\system32\CMDInjectorInjection.reg
			reg import "%SystemRoot%\system32\CMDInjectorInjection.reg"
			reg unload "HKLM\COMPONENTS"
			del %SystemRoot%\system32\CMDInjectorInjection.reg
			del %SystemRoot%\system32\CMDInjectorPermanent.dat
		)

		del "%SystemRoot%\system32\CMDInjector.dat"
		Start cmd.exe /c MessageDialog "Successfully applied the CMD injection changes." "CMD Injector"
	)
)
goto :EOF