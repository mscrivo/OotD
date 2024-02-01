#Script to disable services related to Xbox
Set-Service XboxGipSvc -StartupType Disabled
Set-Service XblAuthManager -StartupType Disabled
Set-Service XblGameSave -StartupType Disabled
Set-Service XboxNetApiSvc -StartupType Disabled
