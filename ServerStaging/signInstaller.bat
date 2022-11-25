@echo off
set /p path="Enter path to installer: "
"C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe" sign /tr http://timestamp.digicert.com /td sha256 /fd sha256 /f "ootd.pfx" %path%
set /p throwaway=Hit ENTER to continue...