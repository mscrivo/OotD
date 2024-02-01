#Remove old task
Unregister-ScheduledTask -TaskName "Daily Reboot" -Confirm:$false
# Create task action
$taskAction = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument 'Restart-Computer -Force'
# Create a trigger (Daily at 2:00am)
$taskTrigger = New-ScheduledTaskTrigger -Daily -At 2:00am
# The user to run the task
$taskUser = New-ScheduledTaskPrincipal -UserId "LOCALSERVICE" -LogonType ServiceAccount
# The name of the scheduled task.
$taskName = "Daily Reboot"
# Describe the scheduled task.
$description = "Forcibly reboot the computer at 2:00am daily"
# Register the scheduled task
Register-ScheduledTask -TaskName $taskName -Action $taskAction -Trigger $taskTrigger -Principal $taskUser -Description $description