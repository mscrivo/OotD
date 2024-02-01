Log("Disabling Telemetry...")
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\DataCollection" -Name "AllowTelemetry" -Type DWord -Value 0
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Policies\DataCollection" -Name "AllowTelemetry" -Type DWord -Value 0
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\DataCollection" -Name "AllowTelemetry" -Type DWord -Value 0
    Disable-ScheduledTask -TaskName "Microsoft\Windows\Application Experience\Microsoft Compatibility Appraiser" | Out-Null
    Disable-ScheduledTask -TaskName "Microsoft\Windows\Application Experience\ProgramDataUpdater" | Out-Null
    Disable-ScheduledTask -TaskName "Microsoft\Windows\Autochk\Proxy" | Out-Null
    Disable-ScheduledTask -TaskName "Microsoft\Windows\Customer Experience Improvement Program\Consolidator" | Out-Null
    Disable-ScheduledTask -TaskName "Microsoft\Windows\Customer Experience Improvement Program\UsbCeip" | Out-Null
    Disable-ScheduledTask -TaskName "Microsoft\Windows\DiskDiagnostic\Microsoft-Windows-DiskDiagnosticDataCollector" | Out-Null
    Log("Disabling Application suggestions...")
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" -Name "ContentDeliveryAllowed" -Type DWord -Value 0
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" -Name "OemPreInstalledAppsEnabled" -Type DWord -Value 0
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" -Name "PreInstalledAppsEnabled" -Type DWord -Value 0
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" -Name "PreInstalledAppsEverEnabled" -Type DWord -Value 0
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" -Name "SilentInstalledAppsEnabled" -Type DWord -Value 0
    Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" -Name "SubscribedContent-338387Enabled" -Type DWord -Value 0
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" -Name "SubscribedContent-338388Enabled" -Type DWord -Value 0
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" -Name "SubscribedContent-338389Enabled" -Type DWord -Value 0
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" -Name "SubscribedContent-353698Enabled" -Type DWord -Value 0
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager" -Name "SystemPaneSuggestionsEnabled" -Type DWord -Value 0
    If (!(Test-Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\CloudContent")) {
        New-Item -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\CloudContent" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\CloudContent" -Name "DisableWindowsConsumerFeatures" -Type DWord -Value 1
    Log("Disabling Activity History...")
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\System" -Name "EnableActivityFeed" -Type DWord -Value 0
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\System" -Name "PublishUserActivities" -Type DWord -Value 0
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\System" -Name "UploadUserActivities" -Type DWord -Value 0
    Log("Disabling Location Tracking...")
    If (!(Test-Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location")) {
        New-Item -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location" -Name "Value" -Type String -Value "Deny"
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Sensor\Overrides\{BFA794E4-F964-4FDB-90F6-51056BFE4B44}" -Name "SensorPermissionState" -Type DWord -Value 0
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\lfsvc\Service\Configuration" -Name "Status" -Type DWord -Value 0
    Log("Disabling automatic Maps updates...")
    Set-ItemProperty -Path "HKLM:\SYSTEM\Maps" -Name "AutoUpdateEnabled" -Type DWord -Value 0
    Log("Disabling Feedback...")
    If (!(Test-Path "HKCU:\SOFTWARE\Microsoft\Siuf\Rules")) {
        New-Item -Path "HKCU:\SOFTWARE\Microsoft\Siuf\Rules" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Siuf\Rules" -Name "NumberOfSIUFInPeriod" -Type DWord -Value 0
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\DataCollection" -Name "DoNotShowFeedbackNotifications" -Type DWord -Value 1
    Disable-ScheduledTask -TaskName "Microsoft\Windows\Feedback\Siuf\DmClient" -ErrorAction SilentlyContinue | Out-Null
    Disable-ScheduledTask -TaskName "Microsoft\Windows\Feedback\Siuf\DmClientOnScenarioDownload" -ErrorAction SilentlyContinue | Out-Null
    Log("Disabling Tailored Experiences...")
    If (!(Test-Path "HKCU:\SOFTWARE\Policies\Microsoft\Windows\CloudContent")) {
        New-Item -Path "HKCU:\SOFTWARE\Policies\Microsoft\Windows\CloudContent" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Policies\Microsoft\Windows\CloudContent" -Name "DisableTailoredExperiencesWithDiagnosticData" -Type DWord -Value 1
    Log("Disabling Advertising ID...")
    If (!(Test-Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo")) {
        New-Item -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo" | Out-Null
    }
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo" -Name "DisabledByGroupPolicy" -Type DWord -Value 1
    Log("Disabling Error reporting...")
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\Windows Error Reporting" -Name "Disabled" -Type DWord -Value 1
    Disable-ScheduledTask -TaskName "Microsoft\Windows\Windows Error Reporting\QueueReporting" | Out-Null
    Log("Stopping and disabling Diagnostics Tracking Service...")
    Stop-Service "DiagTrack" -WarningAction SilentlyContinue
    Set-Service "DiagTrack" -StartupType Disabled
    Log("Stopping and disabling WAP Push Service...")
    Stop-Service "dmwappushservice" -WarningAction SilentlyContinue
    Set-Service "dmwappushservice" -StartupType Disabled
    Log("Disabling Remote Assistance...")
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\Remote Assistance" -Name "fAllowToGetHelp" -Type DWord -Value 0
    #Log("Disabling Storage Sense...")
    #Remove-Item -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy" -Recurse -ErrorAction SilentlyContinue
    Log("Stopping and disabling Superfetch service...")
    Stop-Service "SysMain" -WarningAction SilentlyContinue
    Set-Service "SysMain" -StartupType Disabled
    Log("Showing file operations details...")
    If (!(Test-Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\OperationStatusManager")) {
        New-Item -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\OperationStatusManager" | Out-Null
    }
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\OperationStatusManager" -Name "EnthusiastMode" -Type DWord -Value 1
    Log("Hiding Task View button...")
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name "ShowTaskViewButton" -Type DWord -Value 0
    Log("Hiding People icon...")
    If (!(Test-Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced\People")) {
        New-Item -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced\People" | Out-Null
    }
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced\People" -Name "PeopleBand" -Type DWord -Value 0
    Log("Changing default Explorer view to This PC...")
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced" -Name "LaunchTo" -Type DWord -Value 1
    Log("Hiding 3D Objects icon from This PC...")
    Remove-Item -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}" -Recurse -ErrorAction SilentlyContinue
    #Network Tweaks (Most of the tweaks were provided to me by emilwojcik93) | https://github.com/emilwojcik93/sc-cmd#introduction
	#Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters" -Name "IRPStackSize" -Type DWord -Value 20 # Causes problems with Mapped Drives and File Sharing Programs!
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters" -Name "SizReqBuf" -Type DWord -Value 17424
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters" -Name DefaultTTL -Value 64
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters" -Name TCP1323Opts -Value 1
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters" -Name MaxFreeTcbs -Value 65536
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters" -Name MaxUserPort -Value 65534
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters" -Name GlobalMaxTcpWindowSize -Value 65535
	#SVCHost Tweak
	Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control" -Name "SvcHostSplitThresholdInKB" -Type DWord -Value 4194304
    Log("Disable News and Interests")
    if (!(Test-Path "HKCU:\SOFTWARE\Policies\Microsoft\Windows\Windows Feeds")){
    New-Item -Path "HKCU:\SOFTWARE\Policies\Microsoft\Windows\Windows Feeds" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Policies\Microsoft\Windows\Windows Feeds" -Name "EnableFeeds" -Type DWord -Value 0
    #Remove news and interest from taskbar
    Set-ItemProperty -Path  "HKCU:\Software\Microsoft\Windows\CurrentVersion\Feeds" -Name "ShellFeedsTaskbarViewMode" -Type DWord -Value 2
    #Remove meet now button from taskbar
    If (!(Test-Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer")) {
        New-Item -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer" -Name "HideSCAMeetNow" -Type DWord -Value 1

    #Disable Advertising ID
    If (!(Test-Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo")) {
	    New-Item -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo" | Out-Null
    }
    Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo" -Name "Enabled" -Type DWord -Value 0
    Log("Advertising ID has been disabled")
	
	 #Disable SmartScreen
    if (!(Test-Path "HKLM:\Software\Microsoft\Windows\CurrentVersion\Explorer")){
        New-Item -Path "HKLM:\Software\Microsoft\Windows\CurrentVersion\Explorer" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKLM:\Software\Microsoft\Windows\CurrentVersion\Explorer" -Name "SmartScreenEnabled" -Type String -Value "Off"
    if (!(Test-Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\AppHost")){
        New-Item -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\AppHost" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\AppHost" -Name "EnableWebContentEvaluation" -Value 0
    Log("SmartScreen has been disabled")

    #Disable Hand Writing Reports
    if (!(Test-Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports")){
        New-Item -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\HandwritingErrorReports" -Name "PreventHandwritingErrorReports" -Type DWord -Value 1

    #Disable Location Tracking...
    if (!(Test-Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors")){
        New-Item -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors" -Name "DisableLocation" -Type DWord -Value 1
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors" -Name "DisableLocationScripting" -Type DWord -Value 1
    #Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors" -Name "DisableSensors" -Type DWord -Value 1
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors" -Name "DisableWindowsLocationProvider" -Type DWord -Value 1

    #Disable Auto Map Downloading/Updating
    if (!(Test-Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Maps")){
        New-Item -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Maps" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Maps" -Name "AutoDownloadAndUpdateMapData" -Type DWord -Value 0

    #Disable Windows Feeds
    if (!(Test-Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Feeds")){
        New-Item -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Feeds" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Feeds" -Name "EnableFeeds" -Type DWord -Value 0

    #Disable Game DVR
    if (!(Test-Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\GameDVR")){
        New-Item -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\GameDVR" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\GameDVR" -Name "AllowGameDVR" -Type DWord -Value 0

    #Disable Keyboard BS
    Set-ItemProperty -Path "HKCU:\Control Panel\Accessibility\StickyKeys" -Name "Flags" -Value "506"
    Set-ItemProperty -Path "HKCU:\Control Panel\Accessibility\Keyboard Response" -Name "Flags" -Value "122"
    Set-ItemProperty -Path "HKCU:\Control Panel\Accessibility\ToggleKeys" -Name "Flags" -Value "58"

    #Disable Mitigations
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management' -Name FeatureSettingsOverride -Value 3
    Set-ItemProperty -Path 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management' -Name FeatureSettingsOverrideMask -Value 3

    # Clean Taskbar
    Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Name TaskbarDa -Value 0 -Force
    Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Name TaskbarMn -Value 0 -Force
    Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Name TaskbarAl -Value 0 -Force
	
	Log("Disabling Bing Search in Start Menu...")
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Search" -Name "BingSearchEnabled" -Type DWord -Value 0
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Search" -Name "CortanaConsent" -Type DWord -Value 0
    If (!(Test-Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Search")) {
        New-Item -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Search" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Search" -Name "DisableWebSearch" -Type DWord -Value 1
    Log("Hiding Taskbar Search icon / box...")
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Search" -Name "SearchboxTaskbarMode" -Type DWord -Value 0
    Log("Disabling Cortana...")
    If (!(Test-Path "HKCU:\SOFTWARE\Microsoft\Personalization\Settings")) {
        New-Item -Path "HKCU:\SOFTWARE\Microsoft\Personalization\Settings" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\Personalization\Settings" -Name "AcceptedPrivacyPolicy" -Type DWord -Value 0
    If (!(Test-Path "HKCU:\SOFTWARE\Microsoft\InputPersonalization")) {
        New-Item -Path "HKCU:\SOFTWARE\Microsoft\InputPersonalization" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\InputPersonalization" -Name "RestrictImplicitTextCollection" -Type DWord -Value 1
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\InputPersonalization" -Name "RestrictImplicitInkCollection" -Type DWord -Value 1
    If (!(Test-Path "HKCU:\SOFTWARE\Microsoft\InputPersonalization\TrainedDataStore")) {
        New-Item -Path "HKCU:\SOFTWARE\Microsoft\InputPersonalization\TrainedDataStore" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKCU:\SOFTWARE\Microsoft\InputPersonalization\TrainedDataStore" -Name "HarvestContacts" -Type DWord -Value 0
    If (!(Test-Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Search")) {
        New-Item -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Search" -Force | Out-Null
    }
    Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Search" -Name "AllowCortana" -Type DWord -Value 0
    Stop-Process -Name SearchApp -Force
    Stop-Process -Name explorer -Force
    Log("Disabled Cortana")
	
	