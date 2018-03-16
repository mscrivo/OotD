#include <idp.iss>

#define MyAppName "Outlook on the Desktop"
#define MyAppVersion "3.6.0"
#define MyAppVerName "Outlook on the Desktop 3.6.0"
#define MyAppPublisher "Michael Scrivo"
#define MyAppURL "https://outlookonthedesktop.com"
#define MyAppExeName "OotD.Launcher.exe"
#define MyAppCopyright "©2006-2018 Michael Scrivo"

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
WizardImageFile=C:\Program Files (x86)\Inno Setup 5\WizModernImage-IS.bmp
WizardSmallImageFile=C:\Program Files (x86)\Inno Setup 5\WizModernSmallImage-IS.bmp
AppID={{6D9785D9-FF53-4C06-9C2A-E4173D41A2FD}
ShowLanguageDialog=yes
OutputDir=ServerStaging
MinVersion=0,6.0.6001sp2
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
Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Flags: postinstall skipifsilent nowait runasoriginaluser unchecked; Description: "{cm:LaunchProgram,{#MyAppName}}"
Filename: "schtasks"; \
    Parameters: "/F /Create /XML ""{app}\Outlook on the Desktop.xml"" /TN ""Outlook on the Desktop"""; \
    Flags: runhidden

[Tasks]
Name: "installdotnet"; Description: "Download and Install Microsoft .NET Framework 4.7.1"; Check: NeedsDotNetFramework

[Files]
Source: "OotD.Launcher\bin\Release\AxInterop.Microsoft.Office.Interop.OutlookViewCtl.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\HtmlRenderer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\HtmlRenderer.WinForms.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\MACTrackBarLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\MarkdownSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\NetSparkle.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\NLog.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\NLog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\OLXLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\OotD.Launcher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\OotD.Launcher.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\OotD.x64.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\OotD.x64.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\OotD.x86.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "OotD.Launcher\bin\Release\OotD.x86.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "Outlook on the Desktop.xml"; DestDir: "{app}"; Flags: ignoreversion

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
	dotnetURL = 'https://download.microsoft.com/download/8/E/2/8E2BDDE7-F06E-44CC-A145-56C6B9BBE5DD/NDP471-KB4033344-Web.exe';
const
  sFileName = 'AEAE0F3F-96E9-4711-AADA-5E35EF902306/NDP471-KB4033344-Web.exe';

function NeedsDotNetFramework(): Boolean;
var
	ReleaseVersion: Cardinal;
	tempResult: Boolean;
begin
	tempResult:= True;

	if RegKeyExists(HKLM,'Software\Microsoft\NET Framework Setup\NDP\v4\Full') then
	begin
		if RegQueryDWordValue(HKLM, 'Software\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', ReleaseVersion) then
		begin
      if (ReleaseVersion >= 461308) then
      begin
        tempResult := False;
      end;
		end;
	end;

	Result := tempResult;
end;

function NextButtonClick(CurPage: Integer): Boolean;
var
	hWnd: Integer;
	sTasks: String;
	nCode: Integer;
begin
	Result := true;

	if CurPage = wpSelectTasks then 
  begin
		hWnd := StrToInt(ExpandConstant('{wizardhwnd}'));

		sTasks := WizardSelectedTasks(false);

    idpClearFiles;

		if IsTaskSelected('installdotnet') then
		begin
      idpAddFile(dotnetURL, ExpandConstant('{tmp}\$sFileName'));
		end;

    idpDownloadAfter(wpSelectTasks);
	end;

  if CurPage = wpReady then 
  begin
      if FileExists(ExpandConstant('{tmp}\$sFileName')) then 
      begin 
        Exec(ExpandConstant('{tmp}\$sFileName'),'/passive /norestart','',SW_SHOW,ewWaitUntilTerminated,nCode)
      end  
  end;
end;

procedure TaskKill(FileName: String);
var
  ResultCode: Integer;
begin
    Exec(ExpandConstant('taskkill.exe'), '/f /im ' + '"' + FileName + '"', '', SW_HIDE,
     ewWaitUntilTerminated, ResultCode);
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