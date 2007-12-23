#include "isxdl.iss"

#define MyAppName "Outlook on the Desktop"
#define MyAppVersion "1.3.6"
#define MyAppVerName "Outlook on the Desktop 1.3.6"
#define MyAppPublisher "Michael Scrivo"
#define MyAppURL "http://www.michaelscrivo.com/projects/outlookdesktop"
#define MyAppExeName "OutlookDesktop.exe"
#define MyAppCopyright "©2007 Michael Scrivo"

[Setup]
AppName={#MyAppName}
AppVerName={#MyAppVerName}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName=C:\{#MyAppName}
DefaultGroupName={#MyAppName}
VersionInfoCompany={#MyAppPublisher}
VersionInfoCopyright={#MyAppCopyright}
AppCopyright={#MyAppCopyright}
OutputBaseFilename=setup
Compression=lzma
SolidCompression=true
VersionInfoVersion={#MyAppVersion}
VersionInfoDescription={#MyAppName}
WizardImageFile=C:\Program Files\Inno Setup 5\WizModernImage-IS.bmp
WizardSmallImageFile=C:\Program Files\Inno Setup 5\WizModernSmallImage-IS.bmp
AppID={{6D9785D9-FF53-4C06-9C2A-E4173D41A2FD}
ShowLanguageDialog=yes
OutputDir=./
MinVersion=0,5.0.2195
AllowUNCPath=false
UninstallLogMode=append
UninstallDisplayIcon={app}\App.ico
PrivilegesRequired=none

[Languages]
Name: eng; MessagesFile: compiler:Default.isl

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}
Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked
Name: installdotnet; Description: Download and Install Microsoft .NET Framework 2.0; Check: NeedsDotNetFramework

[Files]
Source: OutlookDesktop\bin\x86\Release\OutlookDesktop.exe; DestDir: {app}; Flags: ignoreversion
Source: OutlookDesktop\RequiredFiles\OutlookDesktop.exe.manifest; DestDir: {app}; Flags: ignoreversion
Source: OutlookDesktop\bin\x86\Release\AxInterop.Microsoft.Office.Interop.OutlookViewCtl.dll; DestDir: {app}; Flags: ignoreversion
Source: OutlookDesktop\bin\x86\Release\Microsoft.Office.Interop.Outlook.dll; DestDir: {app}; Flags: ignoreversion
Source: OutlookDesktop\bin\x86\Release\Microsoft.Office.Interop.OutlookViewCtl.dll; DestDir: {app}; Flags: ignoreversion
Source: OutlookDesktop\bin\x86\Release\Office.dll; DestDir: {app}; Flags: ignoreversion

[Icons]
Name: {group}\{#MyAppName}; Filename: {app}\{#MyAppExeName}; WorkingDir: {app}
Name: {userdesktop}\{#MyAppName}; Filename: {app}\{#MyAppExeName}; Tasks: desktopicon; WorkingDir: {app}
Name: {userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}; Filename: {app}\{#MyAppExeName}; Tasks: quicklaunchicon; WorkingDir: {app}

[Run]
Filename: {app}\{#MyAppExeName}; Description: {cm:LaunchProgram,{#MyAppName}}; Flags: postinstall skipifsilent nowait; WorkingDir: {app}; OnlyBelowVersion: 0,6.0

[Registry]
Root: HKCU; Subkey: Software\Microsoft\Windows\CurrentVersion\Run; ValueType: string; ValueName: OutlookOnDesktop; ValueData: {app}\OutlookDesktop.exe; Flags: uninsdeletevalue
Root: HKCU; Subkey: Software\SMR Computer Services\Outlook On The Desktop; Flags: createvalueifdoesntexist uninsdeletekey

[Code]
const
	dotnetURL = 'http://www.microsoft.com/downloads/info.aspx?na=90&p=&SrcDisplayLang=en&SrcCategoryId=&SrcFamilyId=0856eacb-4362-4b0d-8edd-aab15c5e04f5&u=http%3a%2f%2fdownload.microsoft.com%2fdownload%2f5%2f6%2f7%2f567758a3-759e-473e-bf8f-52154438565a%2fdotnetfx.exe';
	dotnetURL64 = 'http://www.microsoft.com/downloads/info.aspx?na=90&p=&SrcDisplayLang=en&SrcCategoryId=&SrcFamilyId=b44a0000-acf8-4fa1-affb-40e78d788b00&u=http%3a%2f%2fdownload.microsoft.com%2fdownload%2fa%2f3%2ff%2fa3f1bf98-18f3-4036-9b68-8e6de530ce0a%2fNetFx64.exe';

function NeedsDotNetFramework(): Boolean;
begin
	Result := NOT RegKeyExists(HKLM,'SOFTWARE\Microsoft\.NETFramework\policy\v2.0');
end;

function HasWinXP64(): Boolean;
begin
	Result := RegKeyExists(HKLM,'SOFTWARE\Wow6432Node');
end;

function NextButtonClick(CurPage: Integer): Boolean;
var
	hWnd: Integer;
	sFileName: String;
	sTasks: String;
	nCode: Integer;
begin
	Result := true;

	if CurPage = wpReady then begin
		hWnd := StrToInt(ExpandConstant('{wizardhwnd}'));

		sTasks := WizardSelectedTasks(false);

		isxdl_ClearFiles;
		isxdl_SetOption('title', 'Downloading the Microsoft .NET Framework 2.0');
		isxdl_SetOption('description', 'Please wait while Setup downloads the Microsoft .NET Framework 2.0 to your computer.');

		if Not HasWinXP64() then
		begin
			sFileName := ExpandConstant('{tmp}\dotnetfx.exe');
		end else begin
			sFileName := ExpandConstant('{tmp}\NetFx64.exe');
		end;

		if IsTaskSelected('installdotnet') then
		begin
			if Not HasWinXP64() then
			begin
				isxdl_AddFile(dotnetURL, sFileName);
			end	else begin
				isxdl_AddFile(dotnetURL64, sFileName);
			end;
		end;

		if isxdl_DownloadFiles(hWnd) <> 0 then
		begin
			if FileExists(sFileName) then Exec(sFileName,'/q','',SW_SHOW,ewWaitUntilTerminated,nCode)
		end else
			Result := false;

	end;
end;

function CloseAllInstances(): Boolean;
var
	hWnd: Integer;
	j: Integer;
begin
	for j := 1 to 10 do
	begin
		hWnd := FindWindowByWindowName('Outlook on the Desktop');
		if hWnd > 0 then
		begin
			SendMessage(hWnd, 274, 61536, 0);
		end;
	end;
end;

function InitializeUninstall(): Boolean;
begin
	CloseAllInstances();
	Result:= true;
end;

procedure InitializeWizard();
begin
	CloseAllInstances();
end;
