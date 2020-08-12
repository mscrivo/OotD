#include "Inno Plugins\idp.iss"

#define MyAppName "Outlook on the Desktop"
#define MyAppVersion GetFileVersion('OotD.Launcher\bin\Release\netcoreapp3.1\OotD.Launcher.exe')
#define MyAppVerName "Outlook on the Desktop {#MyAppVersion}"
#define MyAppPublisher "Michael Scrivo"
#define MyAppURL "https://outlookonthedesktop.com"
#define MyAppExeName "OotD.Launcher.exe"
#define MyAppCopyright "©2006-2020 Michael Scrivo"

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
Compression=lzma
SolidCompression=true
VersionInfoVersion={#MyAppVersion}
VersionInfoDescription={#MyAppName}
WizardImageFile=Inno Plugins\WizModernImage-IS.bmp
WizardSmallImageFile=Inno Plugins\WizModernSmallImage-IS.bmp
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

[Languages]
Name: eng; MessagesFile: compiler:Default.isl

[Run]
Filename: "{app}\{#MyAppExeName}"; Parameters: "-s"; WorkingDir: "{app}"; Flags: postinstall waituntilterminated runasoriginaluser runhidden; Description: "Run on Startup"
Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Flags: postinstall skipifsilent nowait runasoriginaluser; Description: "{cm:LaunchProgram,{#MyAppName}}"

[Files]
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\AxInterop.Microsoft.Office.Interop.OutlookViewCtl.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\Microsoft.Office.Interop.Outlook.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\CommandLine.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\MACTrackBarLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\MarkdownSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\Microsoft.Win32.TaskScheduler.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\NetSparkle.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\NLog.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\NLog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\OLXLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\OotD.Launcher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\OotD.Launcher.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\OotD.Launcher.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\OotD.Launcher.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\OotD.x64.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\OotD.x64.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\OotD.x64.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\OotD.x64.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\OotD.x86.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\OotD.x86.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\OotD.x86.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\OotD.x86.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\netcoreapp3.1\OotDScheduledTaskDefinition.xml"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: {group}\{#MyAppName}; Filename: {app}\{#MyAppExeName}; WorkingDir: {app}
Name: {group}\{cm:ProgramOnTheWeb,{#MyAppName}}; Filename: {#MyAppURL}
Name: {group}\{cm:UninstallProgram,{#MyAppName}}; Filename: {uninstallexe}

[Registry]
Root: "HKCU"; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "OutlookOnDesktop"; ValueType: string; Flags: deletevalue;
Root: "HKCU"; Subkey: "Software\SMR Computer Services\Outlook On The Desktop"; Flags: createvalueifdoesntexist uninsdeletekey
Root: "HKCU"; Subkey: "Software\SMR Computer Services\Outlook On The Desktop\"; ValueType: string; ValueName: "First Run"; ValueData: "True"
Root: "HKCU"; Subkey: "Software\SMR Computer Services\Outlook On The Desktop\AutoUpdate"; Flags: createvalueifdoesntexist uninsdeletekey

[Dirs]
Name: "{app}\logs"; Permissions: everyone-modify

[UninstallRun]
Filename: "taskkill"; Parameters: "/f /im OotD.x86.exe"; Flags: runhidden
Filename: "taskkill"; Parameters: "/f /im OotD.x64.exe"; Flags: runhidden
Filename: "schtasks"; Parameters: "/DELETE /F /TN ""Outlook on the Desktop"""; Flags: runhidden

[Code]
const
  dotnetCore3x64DesktopUrl = 'https://download.visualstudio.microsoft.com/download/pr/5e4695fb-da51-4fa8-a090-07a64480888c/65aa842670d2280b5d05b8a070a9f495/windowsdesktop-runtime-3.1.7-win-x64.exe';
  dotnetCore3x86DesktopUrl = 'https://download.visualstudio.microsoft.com/download/pr/3e6c8a13-9d89-4991-b683-b6bb279bc096/d1c44ba0c34f2be8878c36d27287e1a5/windowsdesktop-runtime-3.1.7-win-x86.exe';
  dotnetCore3x64DesktopFilename = 'windowsdesktop-runtime-3.1.7-win-x64.exe';
  dotnetCore3x86DesktopFilename = 'windowsdesktop-runtime-3.1.7-win-x86.exe';

procedure InitializeWizard;
begin
  idpAddFile(dotnetCore3x64DesktopUrl, ExpandConstant('{tmp}\$dotnetCore3x64DesktopFilename'));
  idpAddFile(dotnetCore3x86DesktopUrl, ExpandConstant('{tmp}\$dotnetCore3x86DesktopFilename'));
  idpDownloadAfter(wpReady);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  nCode: Integer;
begin
  Exec(ExpandConstant('{tmp}\$dotnetCore3x64DesktopFilename'),'/install /passive /quiet /norestart','',SW_SHOW,ewWaitUntilTerminated,nCode);
  Exec(ExpandConstant('{tmp}\$dotnetCore3x86DesktopFilename'),'/install /passive /quiet /norestart','',SW_SHOW,ewWaitUntilTerminated,nCode);
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