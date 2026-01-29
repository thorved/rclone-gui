# Rclone GUI Installer

This folder contains the installer build scripts for Rclone GUI.

## Prerequisites

1. **.NET 10 SDK** - Required to build the application
2. **Inno Setup 6** - Required to create the installer
   - Download from: https://jrsoftware.org/isdl.php
   - Or install via winget: `winget install JRSoftware.InnoSetup`

## Building the Installer

### Option 1: Using PowerShell Script (Recommended)

```powershell
cd installer
.\build-installer.ps1
```

This will:
1. Build and publish the application in Release mode
2. Verify all prerequisites are in place
3. Create the installer using Inno Setup

### Option 2: Manual Build

1. **Publish the application:**
   ```powershell
   cd src\RcloneGui
   dotnet publish -c Release -p:PublishProfile=FolderProfile
   ```

2. **Compile the installer:**
   - Open `RcloneGuiSetup.iss` in Inno Setup
   - Press Ctrl+F9 to compile
   - Or run from command line:
     ```powershell
     & "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" RcloneGuiSetup.iss
     ```

## Output

The installer will be created in `installer\output\`:
- `RcloneGui-Setup-1.0.0.exe`

## What the Installer Does

1. **Checks for WinFsp** - Required for mounting remote drives
   - If not installed, prompts user and installs bundled `winfsp-2.0.23075.msi`
   
2. **Installs Rclone GUI** to `C:\Program Files\Rclone GUI`

3. **Creates shortcuts:**
   - Start Menu shortcut
   - Optional Desktop shortcut
   - Optional "Start with Windows" registry entry

4. **Bundles rclone.exe** in the installation folder

## Updating the Version

Edit the version number in `RcloneGuiSetup.iss`:
```iss
#define MyAppVersion "1.0.0"
```

## Files Structure

```
installer/
├── RcloneGuiSetup.iss    # Inno Setup script
├── build-installer.ps1   # Build automation script
├── README.md             # This file
└── output/               # Generated installers (created during build)
```
