#Script to disable Windows PowerShell 2.0

Disable-WindowsOptionalFeature -Online -FeatureName "MicrosoftWindowsPowerShellV2" -NoRestart
Disable-WindowsOptionalFeature -Online -FeatureName "MicrosoftWindowsPowerShellV2Root" -NoRestart