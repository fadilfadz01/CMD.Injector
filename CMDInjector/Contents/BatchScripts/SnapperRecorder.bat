:: ///////////////////////////////////////////////////
:: Snapper Records
:: This batch script is part of the CMD Injector App
:: https://github.com/fadilfadz01/CMD.Injector
:: Copyright (c) Fadil Fadz - @fadilfadz01
:: ///////////////////////////////////////////////////

set Amount=%1
set Delay=%2
set LocalFolder=%3
set ClipsFolder=%4
set Num=0

del "%LocalFolder%\RecordStop.txt"
del "%LocalFolder%\RecordEnd.txt"

:Loop
set /a Num=%Num%+1
sleep.exe %Delay%
start ScreenCapture.exe "%ClipsFolder%\SnapperShot_%Num%.jpg"
if exist "%LocalFolder%\RecordStop.txt" goto Stop
if %Num%==%Amount% (goto End) else (goto Loop)

:Stop
del "%LocalFolder%\RecordStop.txt"
goto :EOF

:End
echo. >"%LocalFolder%\RecordEnd.txt"
goto :EOF