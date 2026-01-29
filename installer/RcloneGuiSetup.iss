; Inno Setup Script for Rclone GUI
; Requires Inno Setup 6.0 or later

#define MyAppName "Rclone GUI"
#define MyAppVersion "0.0.1"
#define MyAppPublisher "Thorved"
#define MyAppURL "https://github.com/thorved/rclone-gui"
#define MyAppExeName "RcloneGui.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
AppId={{8F3B2A1C-5D7E-4F9A-B6C8-2E1D0A3F5B7C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
; Output settings
OutputDir=..\installer\output
OutputBaseFilename=RcloneGui-Setup-{#MyAppVersion}
; Compression
Compression=lzma2/ultra64
SolidCompression=yes
; Require admin for WinFsp installation
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
; Modern installer look
WizardStyle=modern
; Architecture
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; Uninstaller
UninstallDisplayIcon={app}\{#MyAppExeName}
; Minimum Windows version (Windows 10 1809)
MinVersion=10.0.17763

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startupicon"; Description: "Start {#MyAppName} when Windows starts"; GroupDescription: "Startup:"; Flags: unchecked

[Files]
; WinFsp installer (bundled prerequisite)
Source: "..\binary\winfsp-2.0.23075.msi"; DestDir: "{tmp}"; Flags: ignoreversion deleteafterinstall; Check: not IsWinFspInstalled

; Main application files (from Release build output)
Source: "..\src\RcloneGui\bin\x64\Release\net9.0-windows10.0.22621.0\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Note: rclone.exe is already included in build output via csproj Content Include

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
; Start with Windows option
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: startupicon

[Run]
; Install WinFsp if not present
Filename: "msiexec.exe"; Parameters: "/i ""{tmp}\winfsp-2.0.23075.msi"" /qb REBOOT=ReallySuppress"; StatusMsg: "Installing WinFsp driver (required for mounting)..."; Flags: waituntilterminated; Check: not IsWinFspInstalled

; Launch application after install
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Note: We don't uninstall WinFsp as other apps may depend on it

[Code]
// Check if WinFsp is installed by checking registry
function IsWinFspInstalled: Boolean;
var
  InstallDir: String;
begin
  Result := False;
  
  // Check 64-bit registry
  if RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\WinFsp', 'InstallDir', InstallDir) then
  begin
    Result := True;
    Exit;
  end;
  
  // Check 32-bit registry (WOW6432Node)
  if RegQueryStringValue(HKEY_LOCAL_MACHINE, 'SOFTWARE\WOW6432Node\WinFsp', 'InstallDir', InstallDir) then
  begin
    Result := True;
    Exit;
  end;
  
  // Check common installation paths
  if DirExists(ExpandConstant('{pf}\WinFsp')) then
  begin
    Result := True;
    Exit;
  end;
  
  if DirExists(ExpandConstant('{pf32}\WinFsp')) then
  begin
    Result := True;
    Exit;
  end;
end;

// Show WinFsp installation message
function InitializeSetup: Boolean;
begin
  Result := True;
  
  if not IsWinFspInstalled then
  begin
    if MsgBox('WinFsp is required for mounting remote drives but is not installed on your system.' + #13#10 + #13#10 +
              'The installer will now install WinFsp automatically.' + #13#10 + #13#10 +
              'Do you want to continue?', mbConfirmation, MB_YESNO) = IDNO then
    begin
      Result := False;
    end;
  end;
end;

// Verify WinFsp was installed successfully
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Check if WinFsp installation was needed and verify it succeeded
    if not IsWinFspInstalled then
    begin
      MsgBox('Warning: WinFsp installation may have failed. Mount functionality may not work.' + #13#10 + #13#10 +
             'You can manually install WinFsp from: https://winfsp.dev/rel/', mbError, MB_OK);
    end;
  end;
end;
