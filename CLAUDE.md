# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity TMP Parameter Mover is a WinUI 3 desktop application that processes Unity TextMeshPro (TMP) font asset JSON files. It copies specific parameters from a source file to multiple target files while preserving certain fields.

**Technology Stack:**
- .NET 9.0 with WinUI 3
- Windows App SDK 1.8
- System.Text.Json for JSON manipulation
- Framework-dependent deployment (single-file EXE)

## Build Commands

### Quick Build (Recommended)
```powershell
.\Build.ps1
```
This generates a single-file EXE (~38 MB) in the `Release` folder.

### Build for Different Platforms
```powershell
.\Build.ps1 -Platform x64   # Default
.\Build.ps1 -Platform x86
.\Build.ps1 -Platform ARM64
```

### Manual Build (Development)
```powershell
cd Unity-TMP-ParameterMover-WinUI
dotnet restore
dotnet build --configuration Release -p:Platform=x64
```

### Manual Publish
```powershell
dotnet publish `
    --configuration Release `
    --runtime win-x64 `
    --self-contained false `
    -p:Platform=x64 `
    -p:PublishSingleFile=true
```

## Architecture

### Project Structure
```
Unity-TMP-ParameterMover-WinUI/
├── App.xaml.cs              # Application entry point
├── MainWindow.xaml(.cs)     # Main UI and orchestration
├── Services/
│   └── TMPParameterService.cs  # Core business logic
├── Models/
│   └── ProcessResult.cs     # Data transfer object
└── Utilities/
    └── CustomJsonFormatting.cs  # JSON formatting helpers
```

### Core Architecture Pattern

**MVVM-lite approach:**
- `MainWindow.xaml.cs` handles UI logic and file selection
- `TMPParameterService` contains all parameter migration logic
- Callback-based logging via `OnLogMessage` delegate
- Uses `ObservableCollection<FileItem>` for file list binding

### TMPParameterService Processing Flow

The service processes JSON files in this order:
1. Parse both origin and source JSON files using `System.Text.Json.Nodes.JsonObject`
2. Process these TMP sections (copying from source to origin):
   - `m_fontInfo` (excluding `Name` field)
   - `m_glyphInfoList` (all fields)
   - `m_FaceInfo` (excluding `m_FamilyName` and `m_StyleName`)
   - `m_AtlasWidth` / `m_AtlasHeight`
   - `m_GlyphTable` (all fields)
   - `m_CharacterTable` (all fields)
   - `m_UsedGlyphRects` (all fields)
   - `m_FreeGlyphRects` (all fields)
3. Track changed fields using `JsonNode.DeepEquals()` comparison
4. Format output with `CustomJsonFormatting.Options` (indented, all fields preserved)
5. Write to `[origin-file-directory]/Moved_Parameters/[original-filename]`

## Critical Technical Details

### Project Name and Namespace Issue
The project folder name contains hyphens (`Unity-TMP-ParameterMover-WinUI`) which are invalid in C# namespaces. This is resolved via:
```xml
<AssemblyName>Unity_TMP_ParameterMover_WinUI</AssemblyName>
<RootNamespace>Unity_TMP_ParameterMover_WinUI</RootNamespace>
```
**Never remove these properties** or XAML compilation will fail.

### Single-File Deployment Setup
The app uses framework-dependent single-file publishing with these critical settings:
```xml
<PublishSingleFile>true</PublishSingleFile>
<SelfContained>false</SelfContained>
<WindowsAppSDKSelfContained>false</WindowsAppSDKSelfContained>
<EnableMsixTooling>true</EnableMsixTooling>
<PublishReadyToRun>False</PublishReadyToRun>  <!-- Disabled for size optimization -->
<PublishTrimmed>False</PublishTrimmed>         <!-- Must be false for WinUI apps -->
<NoWarn>$(NoWarn);WINSDK1001;WINSDK1002;WINSDK1003</NoWarn>  <!-- Suppress PublishSingleFile warnings -->
```

**Warning Suppression:**
The `NoWarn` property suppresses Windows App SDK warnings about using `PublishSingleFile` with framework-dependent deployment:
- `WINSDK1001`: Warns that PublishSingleFile is recommended only for WindowsAppSDKSelfContained apps
- `WINSDK1002`: Warns that PublishSingleFile is recommended only for SelfContained apps
- `WINSDK1003`: Warns about MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY requirement

These warnings are informational only - the configuration is intentional for size optimization (38 MB vs 276 MB self-contained).

And requires this environment variable in `App.xaml.cs` constructor:
```csharp
Environment.SetEnvironmentVariable("MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY", AppContext.BaseDirectory);
```

### Size Optimization History
- Original self-contained: 276 MB
- Framework-dependent with R2R: 54 MB
- Current (R2R disabled, System.Text.Json): 38 MB

**Do not re-enable `PublishReadyToRun`** or re-add `Newtonsoft.Json` without discussing file size impact.

### Runtime Dependencies (User Installation Required)
- .NET 9.0 Desktop Runtime: https://dotnet.microsoft.com/download/dotnet/9.0
- Windows App SDK Runtime: https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads

## UI Threading Model

All file processing happens on background thread via `Task.Run()`, with UI updates dispatched using:
```csharp
DispatcherQueue.TryEnqueue(() => { /* UI updates */ });
```

The service's `OnLogMessage` callback is invoked from background thread, so MainWindow wraps it with dispatcher.

## JSON Processing Notes

- Uses `System.Text.Json` (not Newtonsoft.Json)
- All JSON nodes are `JsonObject` from `System.Text.Json.Nodes`
- Field iteration: `foreach (var property in jsonObject)` where `property.Key` is field name
- Comparison: `JsonNode.DeepEquals(node1, node2)`
- Cloning: `node.DeepClone()` when copying values
- Empty array cleanup: `CustomJsonFormatting.CleanEmptyArrays()` uses regex to collapse `[ ]` spacing
