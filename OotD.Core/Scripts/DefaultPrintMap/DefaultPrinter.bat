@ECHO OFF
Set printer="%1"
Set printer=%printer:"=%
SET printerName="\\PRINTSERVER.sfmc.net\%printer%"
start /wait explorer.exe "%printerName%"
time /t 2 > NUL
taskkill /f /fi "WindowTitle eq %printer%*"
time /t 2 > NUL
start /wait /b RUNDLL32.exe PRINTUI.DLL,PrintUIEntry /y /n%printerName%