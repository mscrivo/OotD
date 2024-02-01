$ServiceName = 'CiscoAMP'
$ServiceStatus = Get-Service -Name $ServiceName

If ($ServiceStatus.Status -eq 'Running')
{ 
	Set-MpPreference -DisableRealtimeMonitoring $true
}
