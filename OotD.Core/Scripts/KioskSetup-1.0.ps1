#Modifies Windows settings to prepare a device to be used for kiosk mode
#The intent of this script is to:
#1.  Disable Ease of Access Center on logon screen
#2.  Use keyboard filter to block potentially dangerous keyboard combinations

#Disable Ease of Access Center on logon screen 
    #Create new registry key HKLM:\SOFTWARE\Microsoft\Windows Embedded\EmbeddedLogon\
    New-Item -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\" -Name "EmbeddedLogon"

    #Add BrandingNeutral to HKLM:\SOFTWARE\Microsoft\Windows Embedded\EmbeddedLogon\ and set its value to 8
    New-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\EmbeddedLogon\" -Name "BrandingNeutral" -Value 8 -PropertyType "Dword"

#Use keyboard filter to block potentially dangerous keyboard combinations
    #Block potentially dangerous key combinations using KeyboardFilter
    
    New-Item -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter\CustomFilters" -type Directory
    New-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter\CustomFilters" -Name "Alt+Esc" -Value "Blocked" -PropertyType "String"
    New-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter\CustomFilters" -Name "Ctrl+Alt+Esc" -Value "Blocked" -PropertyType "String"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "Alt+F4" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "Alt+Space" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "Alt+Tab" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "BrowserHome" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "BrowserSearch" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "Ctrl+Alt+Del" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "Ctrl+Esc" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "Ctrl+F4" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "Shift+Ctrl+Esc" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "Ctrl+Tab" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "LaunchApp1" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "LaunchApp2" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "LaunchMail" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "Windows" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "LShift+LAlt+PrintScrn" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "LShift+LAlt+NumLock" -Value "Blocked"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows Embedded\KeyboardFilter" -Name "Win+U" -Value "Blocked"