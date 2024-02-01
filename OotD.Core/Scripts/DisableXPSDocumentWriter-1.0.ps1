#Script to disable XPS Document Writer

Disable-WindowsOptionalFeature -Online -FeatureName "Printing-XPSServices-Features" -NoRestart