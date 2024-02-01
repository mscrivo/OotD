#Disables power management of NICs

New-ItemProperty "HKLM:\SYSTEM\CurrentControlSet\Control\Power" -Name "PlatformAoAcOverride" -Value "0" -PropertyType DWord

 $adapters = Get-NetAdapter -Physical | Get-NetAdapterPowerManagement
    foreach ($adapter in $adapters)
        {
        $adapter.AllowComputerToTurnOffDevice = 'Disabled'
        $adapter | Set-NetAdapterPowerManagement
        }