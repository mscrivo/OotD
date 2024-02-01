Remove-AppxProvisionedPackage -Online -PackageName Microsoft.549981C3F5F10_1.1911.21713.0_neutral_~_8wekyb3d8bbwe
Get-AppxPackage -allusers *Microsoft.549981C3F5F10* | Remove-AppxPackage -allUsers