@ECHO OFF >NUL
pushd ""%~dp0"
SETLOCAL enableextensions disabledelayedexpansion
set "model=nothing assigned yet"
for /f "tokens=1* delims==" %%a in (
  'wmic computersystem get model /value'
  ) do for /f "delims=" %%c in ("%%~b") do (
      set "model=%%c"
    )
echo %model%|find "G5" > nul
if not errorlevel 1 (
echo Found model containing "G5", using G3/G4 bios settings
goto :G5Bios
)
	
echo %model%|find "G4" > nul
if not errorlevel 1 (
echo Found model containing "G4", using G3/G4 bios settings
goto :G4Bios
)

echo %model%|find "G3 DM" > nul
if not errorlevel 1 (
echo Found model containing "G3 DM", using G3/G4 bios settings
goto :G3DMBios
)

echo %model%|find "G3" > nul
if not errorlevel 1 (
echo Found model containing "G3", using G3/G4 bios settings
goto :G3Bios
)

echo %model%|find "8300" > nul
if not errorlevel 1 (
echo Found model containing "8300", using 8300 bios settings
goto :8300Bios
)

echo %model%|find "G2 DM" > nul
if not errorlevel 1 (
echo Found model containing "G2 DM", using G2 bios settings
goto :G2Bios
)

echo %model%|find "G2" > nul
if not errorlevel 1 (
echo Found model containing "G2", using G2 bios settings
goto :G2Bios
)

echo %model%|find "G1" > nul
if not errorlevel 1 (
echo Found model containing "G1", using G1 bios settings
goto :G1Bios
)

exit /B 22

ENDLOCAL
goto :eof

:G5Bios
echo Running G5 bios file...
HpFirmwareUpdRec\HpFirmwareUpdRec64.exe" -s -fQ08_020901.bin -r -b
echo Running G5 bios settings file...
"%~dp0\BiosConfigUtility64.exe" /set:BIOSConfig(G3-G4).txt /verbose
goto :eof

:G4Bios
echo Running G4 bios file...
"%~dp0\HPBIOSUPDREC\HPBIOSUPDREC.exe" -s -fP08_0231.bin -r -wmi
echo Running G4 bios settings file...
"%~dp0\BiosConfigUtility64.exe" /set:BIOSConfig(G3-G4).txt /verbose
goto :eof

:G3DMBios
echo Running G3 DM bios file...
"%~dp0\HPBIOSUPDREC\HPBIOSUPDREC.exe" -s -fP22_0231.bin -r -wmi
echo Running G3 bios settings file...
"%~dp0\BiosConfigUtility64.exe" /set:BIOSConfig(G3-G4).txt /verbose
goto :eof

:G3Bios
echo Running G3 bios file...
"%~dp0\HPBIOSUPDREC\HPBIOSUPDREC.exe" -s -fP01_0231.bin -r -wmi
echo Running G3 bios settings file...
"%~dp0\BiosConfigUtility64.exe" /set:BIOSConfig(G3-G4).txt /verbose
goto :eof

:G2DMBios
echo Running G2 DM bios file...
"%~dp0\HPBIOSUPDREC\HPBIOSUPDREC.exe" -s -fN21_0242.bin -r -wmi
echo Running G2 bios settings file...
"%~dp0\BiosConfigUtility64.exe" /set:BIOSConfig(G2).txt /verbose
goto :eof

:G2Bios
echo Running G2 bios file...
"%~dp0\HPBIOSUPDREC\HPBIOSUPDREC.exe" -s -fN01_0244.bin -r -wmi
echo Running G2 bios settings file...
"%~dp0\BiosConfigUtility64.exe" /set:BIOSConfig(G2).txt /verbose
goto :eof

:G1Bios
echo Running G1 bios file...
"%~dp0\HPQFlash\hpqFlash.exe" -s -r -fL01_0277.BIN
echo Running G1 bios settings file...
"%~dp0\BiosConfigUtility64.exe" /set:BIOSConfig(8300-G1).txt /verbose
goto :eof

:8300Bios
echo Running 8300 bios file...
"%~dp0\HPQFlash\hpqFlash.exe" -s -r -fK01_0308.BIN
echo Running 8300 bios settings file...
"%~dp0\BiosConfigUtility64.exe" /set:BIOSConfig(8300-G1).txt /verbose
goto :eof

