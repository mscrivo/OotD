#Script to disable Microsoft Print to PDF

Disable-WindowsOptionalFeature -Online -FeatureName "Printing-PrintToPDFServices-Features" -NoRestart