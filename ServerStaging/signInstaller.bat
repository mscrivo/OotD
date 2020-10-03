@echo off
set /p path="Enter path to installer: "
"C:\Program Files (x86)\Windows Kits\10\App Certification Kit\signtool.exe" sign /n "Open Source Developer, Michael Scrivo" /t http://timestamp.comodoca.com/authenticode /fd SHA256 %path%
set /p throwaway=Hit ENTER to continue...