#   Description:
# This script will remove and disable OneDrive integration.

<#
.SYNOPSIS
If the target registry key is already present, all values within that key are purged.
.DESCRIPTION
While `mkdir -force` works fine when dealing with regular folders, it behaves strange when using it at registry level.
If the target registry key is already present, all values within that key are purged.
.PARAMETER Path
Full path of the storage or registry folder
.EXAMPLE
New-FolderForced -Path "HKCU:\Printers\Defaults"
.EXAMPLE
New-FolderForced "HKCU:\Printers\Defaults"
.EXAMPLE
"HKCU:\Printers\Defaults" | New-FolderForced
.NOTES
Replacement for `force-mkdir` to uphold PowerShell conventions.
Thanks to raydric, this function should be used instead of `mkdir -force`.
#>
function New-FolderForced {
    [CmdletBinding(SupportsShouldProcess = $true)]
    param (
		[Parameter(Position = 0, Mandatory, ValueFromPipeline, ValueFromPipelineByPropertyName)]
		[string]
        $Path
    )

    process {
        if (-not (Test-Path $Path)) {
            Write-Verbose "-- Creating full path to:  $Path"
            New-Item -Path $Path -ItemType Directory -Force
        }
    }
}

function Takeown-Registry($key) {
    # TODO does not work for all root keys yet
    switch ($key.split('\')[0]) {
        "HKEY_CLASSES_ROOT" {
            $reg = [Microsoft.Win32.Registry]::ClassesRoot
            $key = $key.substring(18)
        }
        "HKEY_CURRENT_USER" {
            $reg = [Microsoft.Win32.Registry]::CurrentUser
            $key = $key.substring(18)
        }
        "HKEY_LOCAL_MACHINE" {
            $reg = [Microsoft.Win32.Registry]::LocalMachine
            $key = $key.substring(19)
        }
    }

    # get administraor group
    $admins = New-Object System.Security.Principal.SecurityIdentifier("S-1-5-32-544")
    $admins = $admins.Translate([System.Security.Principal.NTAccount])

    # set owner
    $key = $reg.OpenSubKey($key, "ReadWriteSubTree", "TakeOwnership")
    $acl = $key.GetAccessControl()
    $acl.SetOwner($admins)
    $key.SetAccessControl($acl)

    # set FullControl
    $acl = $key.GetAccessControl()
    $rule = New-Object System.Security.AccessControl.RegistryAccessRule($admins, "FullControl", "Allow")
    $acl.SetAccessRule($rule)
    $key.SetAccessControl($acl)
}

function Takeown-File($path) {
    takeown.exe /A /F $path
    $acl = Get-Acl $path

    # get administraor group
    $admins = New-Object System.Security.Principal.SecurityIdentifier("S-1-5-32-544")
    $admins = $admins.Translate([System.Security.Principal.NTAccount])

    # add NT Authority\SYSTEM
    $rule = New-Object System.Security.AccessControl.FileSystemAccessRule($admins, "FullControl", "None", "None", "Allow")
    $acl.AddAccessRule($rule)

    Set-Acl -Path $path -AclObject $acl
}

function Takeown-Folder($path) {
    Takeown-File $path
    foreach ($item in Get-ChildItem $path) {
        if (Test-Path $item -PathType Container) {
            Takeown-Folder $item.FullName
        } else {
            Takeown-File $item.FullName
        }
    }
}

function Elevate-Privileges {
    param($Privilege)
    $Definition = @"
    using System;
    using System.Runtime.InteropServices;
    public class AdjPriv {
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
            internal static extern bool AdjustTokenPrivileges(IntPtr htok, bool disall, ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr rele);
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
            internal static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);
        [DllImport("advapi32.dll", SetLastError = true)]
            internal static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
            internal struct TokPriv1Luid {
                public int Count;
                public long Luid;
                public int Attr;
            }
        internal const int SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const int TOKEN_QUERY = 0x00000008;
        internal const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        public static bool EnablePrivilege(long processHandle, string privilege) {
            bool retVal;
            TokPriv1Luid tp;
            IntPtr hproc = new IntPtr(processHandle);
            IntPtr htok = IntPtr.Zero;
            retVal = OpenProcessToken(hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok);
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            retVal = LookupPrivilegeValue(null, privilege, ref tp.Luid);
            retVal = AdjustTokenPrivileges(htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            return retVal;
        }
    }
"@
    $ProcessHandle = (Get-Process -id $pid).Handle
    $type = Add-Type $definition -PassThru
    $type[0]::EnablePrivilege($processHandle, $Privilege)
}

Write-Output "Kill OneDrive process"
taskkill.exe /F /IM "OneDrive.exe"
taskkill.exe /F /IM "explorer.exe"

Write-Output "Remove OneDrive"
if (Test-Path "$env:systemroot\System32\OneDriveSetup.exe") {
    & "$env:systemroot\System32\OneDriveSetup.exe" /uninstall
}

if (Test-Path "$env:systemroot\System32\OneDriveSetup.exe") {
    Takeown-File "$env:systemroot\System32\OneDriveSetup.exe"
    Remove-Item -Force -ErrorAction SilentlyContinue "$env:systemroot\System32\OneDriveSetup.exe"
}

if (Test-Path "$env:systemroot\SysWOW64\OneDriveSetup.exe") {
    & "$env:systemroot\SysWOW64\OneDriveSetup.exe" /uninstall
}

if (Test-Path "$env:systemroot\SysWOW64\OneDriveSetup.exe") {
    Takeown-File "$env:systemroot\SysWOW64\OneDriveSetup.exe"
    Remove-Item -Force -ErrorAction SilentlyContinue "$env:systemroot\SysWOW64\OneDriveSetup.exe"
}

Write-Output "Removing OneDrive leftovers"
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$env:localappdata\Microsoft\OneDrive"
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$env:programdata\Microsoft OneDrive"
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$env:systemdrive\OneDriveTemp"
# check if directory is empty before removing:
If ((Get-ChildItem "$env:userprofile\OneDrive" -Recurse | Measure-Object).Count -eq 0) {
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$env:userprofile\OneDrive"
}

Write-Output "Disable OneDrive via Group Policies"
New-FolderForced -Path "HKLM:\SOFTWARE\Wow6432Node\Policies\Microsoft\Windows\OneDrive"
Set-ItemProperty -Path "HKLM:\SOFTWARE\Wow6432Node\Policies\Microsoft\Windows\OneDrive" "DisableFileSyncNGSC" 1
New-FolderForced -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\OneDrive"
Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\OneDrive" "DisableFileSyncNGSC" 1

Write-Output "Remove Onedrive from explorer sidebar"
New-PSDrive -PSProvider "Registry" -Root "HKEY_CLASSES_ROOT" -Name "HKCR"
mkdir -Force "HKCR:\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}"
Set-ItemProperty -Path "HKCR:\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}" "System.IsPinnedToNameSpaceTree" 0
mkdir -Force "HKCR:\Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}"
Set-ItemProperty -Path "HKCR:\Wow6432Node\CLSID\{018D5C66-4533-4307-9B53-224DE2ED1FE6}" "System.IsPinnedToNameSpaceTree" 0
Remove-PSDrive "HKCR"

# Thank you Matthew Israelsson
Write-Output "Removing run hook for new users"
reg load "hku\Default" "C:\Users\Default\NTUSER.DAT"
reg delete "HKEY_USERS\Default\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" /v "OneDriveSetup" /f
reg unload "hku\Default"

Write-Output "Removing startmenu entry"
Remove-Item -Force -ErrorAction SilentlyContinue "$env:userprofile\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\OneDrive.lnk"

Write-Output "Removing scheduled task"
Get-ScheduledTask -TaskPath '\' -TaskName 'OneDrive*' -ea SilentlyContinue | Unregister-ScheduledTask -Confirm:$false

Write-Output "Restarting explorer"
Start-Process "explorer.exe"

Write-Output "Waiting for explorer to complete loading"
Start-Sleep 10

Write-Output "Removing additional OneDrive leftovers"
foreach ($item in (Get-ChildItem "$env:WinDir\WinSxS\*onedrive*")) {
    Takeown-Folder $item.FullName
    Remove-Item -Recurse -Force $item.FullName
}