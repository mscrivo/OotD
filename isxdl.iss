[Files]
Source: isxdl.dll; DestDir: {tmp}; Flags: dontcopy

[Code]
function isxdl_Download(hWnd: Integer; URL, Filename: PChar): Integer;
external 'isxdl_Download@files:isxdl.dll stdcall';

procedure isxdl_AddFile(URL, Filename: PChar);
external 'isxdl_AddFile@files:isxdl.dll stdcall';

procedure isxdl_AddFileSize(URL, Filename: PChar; Size: Cardinal);
external 'isxdl_AddFileSize@files:isxdl.dll stdcall';

function isxdl_DownloadFiles(hWnd: Integer): Integer;
external 'isxdl_DownloadFiles@files:isxdl.dll stdcall';

procedure isxdl_ClearFiles;
external 'isxdl_ClearFiles@files:isxdl.dll stdcall';

function isxdl_IsConnected: Integer;
external 'isxdl_IsConnected@files:isxdl.dll stdcall';

function isxdl_SetOption(Option, Value: PChar): Integer;
external 'isxdl_SetOption@files:isxdl.dll stdcall';

function isxdl_GetFileName(URL: PChar): PChar;
external 'isxdl_GetFileName@files:isxdl.dll stdcall';

