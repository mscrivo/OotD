echo "Creating Boot Media Folders"
echo "c:\winpe_x86 and c:\winpe_amd64"
mkdir c:\winpe_x86
mkdir c:\winpe_amd64
mkdir c:\winpe_x86\mount
mkdir c:\winpe_amd64\mount
cd "C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment"
copy x86\en-us\winpe.wim c:\winpe_x86\
copy amd64\en-us\winpe.wim c:\winpe_amd64\

echo "Mount x86 WinPE"
dism /mount-image /imagefile:"c:\winpe_x86\winpe.wim" /index:1 /mountdir:"c:\winpe_x86\mount"

echo "Adding HTA"
dism /add-package /image:"c:\winpe_x86\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\x86\WinPE_OCs\WinPE-HTA.cab"
dism /add-package /image:"c:\winpe_x86\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\x86\WinPE_OCs\en-us\WinPE-HTA_en-us.cab"

echo "Adding WMI"
dism /add-package /image:"c:\winpe_x86\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\x86\WinPE_OCs\WinPE-WMI.cab"
dism /add-package /image:"c:\winpe_x86\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\x86\WinPE_OCs\en-us\WinPE-WMI_en-us.cab"

echo "Adding NetFx"
dism /add-package /image:"c:\winpe_x86\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\x86\WinPE_OCs\WinPE-NetFX.cab"
dism /add-package /image:"c:\winpe_x86\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\x86\WinPE_OCs\en-us\WinPE-NetFX_en-us.cab"

echo "Adding Scripting"
dism /add-package /image:"c:\winpe_x86\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\x86\WinPE_OCs\WinPE-Scripting.cab"
dism /add-package /image:"c:\winpe_x86\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\x86\WinPE_OCs\en-us\WinPE-Scripting_en-us.cab"

echo "Adding PowerShell"
dism /add-package /image:"c:\winpe_x86\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\x86\WinPE_OCs\WinPE-PowerShell.cab"
dism /add-package /image:"c:\winpe_x86\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\x86\WinPE_OCs\en-us\WinPE-PowerShell_en-us.cab"

echo "Adding Manage-bde - bitlocker support"
dism /add-package /image:"c:\winpe_x86\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\x86\WinPE_OCs\WinPE-SecureStartup.cab"
dism /add-package /image:"c:\winpe_x86\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\x86\WinPE_OCs\en-us\WinPE-SecureStartup_en-us.cab"

echo "Updating required PATH environment variables"
reg load HKLM\1 c:\winpe_x86\mount\Windows\System32\config\system
 reg add "HKLM\1\ControlSet001\Control\Session Manager\Environment" /v Path /t REG_EXPAND_SZ /d %%SystemRoot%%\system32;%%SystemRoot%%;%%SystemRoot%%\System32\Wbem;%%SystemDrive%%\cba8;%%SystemDrive%%\ldclient;%%SystemRoot%%\System32\WindowsPowerShell\v1.0\ /f
reg unload HKLM\1

echo Resealing wim"
dism /unmount-image /mountdir:"c:\winpe_x86\mount" /commit

echo "Mount amd64 WinPE"
dism /mount-image /imagefile:"c:\winpe_amd64\winpe.wim" /index:1 /mountdir:"c:\winpe_amd64\mount"

echo "Adding HTA"
dism /add-package /image:"c:\winpe_amd64\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\WinPE-HTA.cab"
dism /add-package /image:"c:\winpe_amd64\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\en-us\WinPE-HTA_en-us.cab"

echo "Adding WMI"
dism /add-package /image:"c:\winpe_amd64\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\WinPE-WMI.cab"
dism /add-package /image:"c:\winpe_amd64\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\en-us\WinPE-WMI_en-us.cab"

echo "Adding NetFx"
dism /add-package /image:"c:\winpe_amd64\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\WinPE-NetFX.cab"
dism /add-package /image:"c:\winpe_amd64\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\en-us\WinPE-NetFX_en-us.cab"

echo "Adding Scripting"
dism /add-package /image:"c:\winpe_amd64\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\WinPE-Scripting.cab"
dism /add-package /image:"c:\winpe_amd64\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\en-us\WinPE-Scripting_en-us.cab"

echo "Adding PowerShell"
dism /add-package /image:"c:\winpe_amd64\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\WinPE-PowerShell.cab"
dism /add-package /image:"c:\winpe_amd64\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\en-us\WinPE-PowerShell_en-us.cab"

echo "Adding Manage-bde - bitlocker support"
dism /add-package /image:"c:\winpe_amd64\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\WinPE-SecureStartup.cab"
dism /add-package /image:"c:\winpe_amd64\mount" /packagepath:"C:\Program Files (x86)\Windows Kits\10\Assessment and Deployment Kit\Windows Preinstallation Environment\amd64\WinPE_OCs\en-us\WinPE-SecureStartup_en-us.cab"

echo "Updating required PATH environment variables"
reg load HKLM\1 c:\winpe_amd64\mount\Windows\System32\config\system
 reg add "HKLM\1\ControlSet001\Control\Session Manager\Environment" /v Path /t REG_EXPAND_SZ /d %%SystemRoot%%\system32;%%SystemRoot%%;%%SystemRoot%%\System32\Wbem;%%SystemDrive%%\cba8;%%SystemDrive%%\ldclient;%%SystemRoot%%\System32\WindowsPowerShell\v1.0\ /f
reg unload HKLM\1

echo "Resealing wim"
dism /unmount-image /mountdir:"c:\winpe_amd64\mount" /commit

echo "Rename files"
rename "c:\winpe_x86\winpe.wim" boot.wim
rename "c:\winpe_amd64\winpe.wim" boot_x64.wim

echo "Output: c:\winpe_x86\boot.wim and c:\winpe_amd64\boot_x64.wim"