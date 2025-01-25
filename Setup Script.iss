﻿#include "Inno Plugins\idp.iss"

#define MyAppName "Outlook on the Desktop"
#define MyAppVersion GetVersionNumbersString('OotD.Launcher\bin\Release\net9.0-windows7.0\OotD.Launcher.exe')
#define MyAppVerName "Outlook on the Desktop {#MyAppVersion}"
#define MyAppPublisher "Michael Scrivo"
#define MyAppURL "https://outlookonthedesktop.com"
#define MyAppExeName "OotD.Launcher.exe"
#define MyAppCopyright "©2006-2025 Michael Scrivo"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultGroupName={#MyAppName}
DefaultDirName={pf}\{#MyAppName}
VersionInfoCompany={#MyAppPublisher}
VersionInfoCopyright={#MyAppCopyright}
AppCopyright={#MyAppCopyright}
OutputBaseFilename=ootd-{#MyAppVersion}
SolidCompression=true
VersionInfoVersion={#MyAppVersion}
VersionInfoDescription={#MyAppName}
AppID={{6D9785D9-FF53-4C06-9C2A-E4173D41A2FD}
ShowLanguageDialog=yes
OutputDir=ServerStaging
MinVersion=0,6.1.7600
AllowUNCPath=false
UninstallLogMode=append
UninstallDisplayIcon={app}\OotD.Launcher.exe
PrivilegesRequired=none
DisableDirPage=auto
DisableReadyMemo=True
DisableProgramGroupPage=yes
UsePreviousGroup=False
WizardStyle=modern

[Languages]
Name: eng; MessagesFile: compiler:Default.isl

[Run]
Filename: "{app}\{#MyAppExeName}"; Parameters: "-s"; WorkingDir: "{app}"; Flags: postinstall waituntilterminated runasoriginaluser runhidden; Description: "Run on Startup"
Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Flags: postinstall skipifsilent nowait runasoriginaluser; Description: "{cm:LaunchProgram,{#MyAppName}}"

[Files]
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\AxInterop.Microsoft.Office.Interop.OutlookViewCtl.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\CommandLine.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\MACTrackBarLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\MarkdownSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\Microsoft.Win32.TaskScheduler.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\System.Reflection.MetadataLoadContext.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\NetSparkle.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\NLog.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\NLog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\OLXLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\OotD.Launcher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\OotD.Launcher.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\OotD.Launcher.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\OotD.Launcher.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\OotD.x64.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\OotD.x64.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\OotD.x64.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\OotD.x64.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\OotD.x86.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\OotD.x86.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\OotD.x86.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\OotD.x86.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\net9.0-windows7.0\OotDScheduledTaskDefinition.xml"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: {group}\{#MyAppName}; Filename: {app}\{#MyAppExeName}; WorkingDir: {app}
Name: {group}\{cm:ProgramOnTheWeb,{#MyAppName}}; Filename: {#MyAppURL}
Name: {group}\{cm:UninstallProgram,{#MyAppName}}; Filename: {uninstallexe}

[Registry]
Root: "HKCU"; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "OutlookOnDesktop"; ValueType: string; Flags: deletevalue;
Root: "HKCU"; Subkey: "Software\SMR Computer Services\Outlook On The Desktop"; Flags: createvalueifdoesntexist uninsdeletekey
Root: "HKCU"; Subkey: "Software\SMR Computer Services\Outlook On The Desktop\"; ValueType: string; ValueName: "FirstRun"; ValueData: "True"
Root: "HKCU"; Subkey: "Software\SMR Computer Services\Outlook On The Desktop\AutoUpdate"; Flags: createvalueifdoesntexist uninsdeletekey

[Dirs]
Name: "{app}\logs"; Permissions: everyone-modify

[UninstallRun]
Filename: "taskkill"; Parameters: "/f /im OotD.x86.exe"; Flags: runhidden
Filename: "taskkill"; Parameters: "/f /im OotD.x64.exe"; Flags: runhidden
Filename: "schtasks"; Parameters: "/DELETE /F /TN ""Outlook on the Desktop"""; Flags: runhidden

[Code]
const
  dotnetRuntimex64DesktopUrl = 'https://download.visualstudio.microsoft.com/download/pr/685792b6-4827-4dca-a971-bce5d7905170/1bf61b02151bc56e763dc711e45f0e1e/windowsdesktop-runtime-9.0.0-win-x64.exe';
  dotnetRuntimex86DesktopUrl = 'https://download.visualstudio.microsoft.com/download/pr/8dfbde7b-c316-418d-934a-d3246253f342/69c6a35b77a4f01b95588e1df2bddf9a/windowsdesktop-runtime-9.0.0-win-x86.exe';
  dotnetRuntimex64DesktopFilename = 'windowsdesktop-runtime-9.0.0-win-x64.exe';
  dotnetRuntimex86DesktopFilename = 'windowsdesktop-runtime-9.0.0-win-x86.exe';

procedure InitializeWizard;
begin
  idpAddFile(dotnetRuntimex64DesktopUrl, ExpandConstant('{tmp}\$dotnetRuntimex64DesktopFilename'));
  idpAddFile(dotnetRuntimex86DesktopUrl, ExpandConstant('{tmp}\$dotnetRuntimex86DesktopFilename'));
  idpDownloadAfter(wpReady);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  nCode: Integer;
begin
  Exec(ExpandConstant('{tmp}\$dotnetRuntimex64DesktopFilename'),'/install /passive /quiet /norestart','',SW_SHOW,ewWaitUntilTerminated,nCode);
  Exec(ExpandConstant('{tmp}\$dotnetRuntimex86DesktopFilename'),'/install /passive /quiet /norestart','',SW_SHOW,ewWaitUntilTerminated,nCode);
end;

procedure TaskKill(FileName: String);
var
  ResultCode: Integer;
begin
  Exec(ExpandConstant('taskkill.exe'), '/f /im ' + '"' + FileName + '"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

function HasOldx64Version(): Boolean;
begin
  Result := FileExists('C:\Program Files\Outlook on the Desktop\unins000.exe');
end;

function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
  // Return Values:
  // 2 - error executing the UnInstallString
  // 3 - successfully executed the UnInstallString

  // default return value
  Result := 0;

  Exec('taskkill', '/f /im OutlookDesktop.exe', '', SW_HIDE,  ewWaitUntilTerminated, iResultCode);

  if Exec('C:\Program Files\Outlook on the Desktop\unins000.exe', '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
    Result := 3
  else
    Result := 2;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep=ssInstall) then
  begin
    if (HasOldx64Version()) then
    begin
      UnInstallOldVersion();
    end;
  end;
end;

[UninstallDelete]
Name: {app}\*; Type: filesandordirs
Name: {app}; Type: dirifempty

[InstallDelete]
Name: C:\{app}\*; Type: filesandordirs
Name: C:\{app}; Type: dirifempty
