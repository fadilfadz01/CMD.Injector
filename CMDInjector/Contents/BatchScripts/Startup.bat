:: ///////////////////////////////////////////////////
:: Startup
:: This batch script is part of the CMD Injector App
:: https://github.com/fadilfadz01/CMD.Injector
:: Copyright (c) Fadil Fadz - @fadilfadz01
:: ///////////////////////////////////////////////////

REM Welcome message on phone bootup.
Start cmd.exe /c MessageDialog "Hey there! This message is a popup for you by the Startup feature of CMD Injector." "Welcome"

REM Keep developer unlocked.
Reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\AppModelUnlock" /v AllowAllTrustedApps /t REG_DWORD /d 1 /f
Reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\AppModelUnlock" /v AllowDevelopmentWithoutDevLicense /t REG_DWORD /d 1 /f

REM Keep interop unlocked.
Reg add "HKLM\Software\Microsoft\.NETCompactFramework\Managed Debugger" /v AttachEnabled /t REG_DWORD /d 1 /f
Reg add "HKLM\Software\Microsoft\.NETCompactFramework\Managed Debugger" /v Enabled /t REG_DWORD /d 0 /f
Reg add "HKLM\Software\Microsoft\DeviceReg" /v PortalUrlInt /t REG_SZ /d https://127.0.0.1 /f
Reg add "HKLM\Software\Microsoft\DeviceReg" /v PortalUrlProd /t REG_SZ /d https://127.0.0.1 /f
Reg add "HKLM\Software\Microsoft\DeviceReg\Install" /v MaxUnsignedApp /t REG_DWORD /d 65539 /f
Reg add "HKLM\Software\Microsoft\PackageManager" /v EnableAppLicenseCheck /t REG_DWORD /d 0 /f
Reg add "HKLM\Software\Microsoft\PackageManager" /v EnableAppProvisioning /t REG_DWORD /d 0 /f
Reg add "HKLM\Software\Microsoft\PackageManager" /v EnableAppSignatureCheck /t REG_DWORD /d 0 /f
Reg add "HKLM\Software\Microsoft\SecurityManager" /v DeveloperUnlockState /t REG_DWORD /d 1 /f
Reg add "HKLM\Software\Microsoft\SecurityManager\AuthorizationRules\Capability\CAPABILITY_RULE_ISV_DEVELOPER_UNLOCK" /v CapabilityClass /t REG_SZ /d CAPABILITY_CLASS_DEVELOPER_UNLOCK /f
Reg add "HKLM\Software\Microsoft\SecurityManager\AuthorizationRules\Capability\CAPABILITY_RULE_ISV_DEVELOPER_UNLOCK" /v PrincipalClass /t REG_SZ /d PRINCIPAL_CLASS_ISV_DEVELOPER_UNLOCK /f
for /f "skip=2 tokens=1,2,3" %%a in ('reg query "HKLM\Software\Microsoft\SecurityManager\CapabilityClasses" /v /f * ^| findstr /iv /c:CAPABILITY_CLASS_THIRD_PARTY_APPLICATIONS') do if %%c neq search: reg add "HKLM\software\Microsoft\SecurityManager\CapabilityClasses" /v %%a /t %%b /d %%c\0CAPABILITY_CLASS_THIRD_PARTY_APPLICATIONS /f
Reg add "HKLM\Software\Microsoft\SecurityManager\CapabilityClasses" /v ID_CAP_CHAMBER_PROFILE_CODE_RW /t REG_MULTI_SZ /d CAPABILITY_CLASS_FIRST_PARTY_APPLICATIONS\0CAPABILITY_CLASS_THIRD_PARTY_APPLICATIONS /f
Reg add "HKLM\Software\Microsoft\SecurityManager\CapabilityClasses" /v ID_CAP_CHAMBER_PROFILE_DATA_RW /t REG_MULTI_SZ /d CAPABILITY_CLASS_FIRST_PARTY_APPLICATIONS\0CAPABILITY_CLASS_THIRD_PARTY_APPLICATIONS /f
Reg add "HKLM\Software\Microsoft\SecurityManager\CapabilityClasses" /v ID_CAP_PUBLIC_FOLDER_FULL /t REG_MULTI_SZ /d CAPABILITY_CLASS_FIRST_PARTY_APPLICATIONS\0CAPABILITY_CLASS_THIRD_PARTY_APPLICATIONS /f
Reg add "HKLM\Software\Microsoft\SecurityManager\CapabilityClasses\Inheritance" /v CAPABILITY_CLASS_ENTERPRISE_OEM_HIGH_ACCESS_APPLICATIONS /t REG_MULTI_SZ /d CAPABILITY_CLASS_ENTERPRISE_OEM_VERY_HIGH_ACCESS_APPLICATIONS /f
Reg add "HKLM\Software\Microsoft\SecurityManager\CapabilityClasses\Inheritance" /v CAPABILITY_CLASS_ENTERPRISE_OEM_LOW_ACCESS_APPLICATIONS /t REG_MULTI_SZ /d CAPABILITY_CLASS_ENTERPRISE_OEM_MED_ACCESS_APPLICATIONS /f
Reg add "HKLM\Software\Microsoft\SecurityManager\CapabilityClasses\Inheritance" /v CAPABILITY_CLASS_ENTERPRISE_OEM_MED_ACCESS_APPLICATIONS /t REG_MULTI_SZ /d CAPABILITY_CLASS_ENTERPRISE_OEM_HIGH_ACCESS_APPLICATIONS /f
Reg add "HKLM\Software\Microsoft\SecurityManager\CapabilityClasses\Inheritance" /v CAPABILITY_CLASS_SECOND_PARTY_APPLICATIONS /t REG_MULTI_SZ /d CAPABILITY_CLASS_FIRST_PARTY_APPLICATIONS /f
Reg add "HKLM\Software\Microsoft\SecurityManager\CapabilityClasses\Inheritance" /v CAPABILITY_CLASS_THIRD_PARTY_APPLICATIONS /t REG_MULTI_SZ /d CAPABILITY_CLASS_SECOND_PARTY_APPLICATIONS\0CAPABILITY_CLASS_ENTERPRISE_APPLICATIONS\0CAPABILITY_CLASS_DEVELOPER_UNLOCK /f
Reg add "HKLM\Software\Microsoft\SecurityManager\DeveloperUnlock" /v DeveloperUnlockState /t REG_DWORD /d 1 /f
Reg add "HKLM\Software\Microsoft\Silverlight\Debugger" /v WaitForAttach /t REG_DWORD /d 1 /f
Reg add "HKLM\System\controlset001\Control\CI" /v CI_DEVELOPERMODE /t REG_DWORD /d 1 /f

REM Keep capabilities unlocked.
Reg add "HKLM\Software\Microsoft\SecurityManager\AuthorizationRules\Capability\capabilityRule_DevUnlock" /v CapabilityClass /t REG_SZ /d capabilityClass_DevUnlock_Internal /f
Reg add "HKLM\Software\Microsoft\SecurityManager\AuthorizationRules\Capability\capabilityRule_DevUnlock" /v PrincipalClass /t REG_SZ /d principalClass_DevUnlock_Internal /f

