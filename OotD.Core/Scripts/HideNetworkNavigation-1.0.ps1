#Script to hide Network on the navigation pane in file explorer

New-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\NonEnum" -Name "{F02C1A0D-BE21-4350-88B0-7367FC96EF3C}" -Value 1 -PropertyType "Dword"