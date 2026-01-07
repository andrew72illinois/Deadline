# Deployment Guide for Deadline Application

## Publishing the Application

The application has been published as a self-contained executable. This means users don't need to install .NET to run it.

### Files Created

After running the publish command, you'll find the executable in the `publish` folder:
- `Deadline.exe` - The main executable file
- Supporting DLL files (if not using single-file mode)

### Distribution Options

#### Option 1: Single Executable (Recommended)
The application is published as a single-file executable that includes everything needed to run.

**To distribute:**
1. Navigate to the `publish` folder
2. Zip the `Deadline.exe` file (or the entire folder if needed)
3. Upload to your hosting service (GitHub Releases, Google Drive, Dropbox, etc.)
4. Share the download link

#### Option 2: Full Folder Distribution
If you need to include additional files or prefer a folder structure:
1. Zip the entire `publish` folder
2. Users extract and run `Deadline.exe` from the extracted folder

### Re-publishing (If Needed)

#### Windows Deployment

**For Windows x64 (64-bit):**
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o ./publish/win-x64
```

**For Windows x86 (32-bit):**
```bash
dotnet publish -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o ./publish/win-x86
```

**For Windows ARM64:**
```bash
dotnet publish -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o ./publish/win-arm64
```

### Publishing for Different Architectures

- **64-bit Windows (most common):** `win-x64`
- **32-bit Windows:** `win-x86`
- **ARM64 Windows:** `win-arm64`

### File Size

The single-file executable will be approximately 50-100 MB because it includes the .NET runtime. This is normal for self-contained applications.

### Security Considerations

- Windows may show a SmartScreen warning on first run (unknown publisher)
- To avoid this, you would need to code-sign the executable (requires a certificate)
- Users can click "More info" and then "Run anyway" if they trust your application

### Hosting Suggestions

1. **GitHub Releases** - Free, version control, easy distribution
2. **Google Drive / Dropbox** - Simple file sharing
3. **OneDrive** - Microsoft's cloud storage
4. **Your own website** - Direct download link

### User Instructions

For end users:
1. Download the `Deadline.exe` file
2. Run the executable (no installation required)
3. The application will create a data folder in `%LocalAppData%\YearDeadline` to store goals (note: the folder name remains YearDeadline for backward compatibility)

