# Quick Reference: Publish Commands

## Windows Deployment

### Windows x64 (64-bit) - Recommended
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o ./publish/win-x64
```

### Windows x86 (32-bit)
```powershell
dotnet publish -c Release -r win-x86 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o ./publish/win-x86
```

### Windows ARM64
```powershell
dotnet publish -c Release -r win-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o ./publish/win-arm64
```

## Using the Provided Scripts

### Windows (PowerShell)
```powershell
.\publish-windows.ps1
```

## Command Parameters Explained

- `-c Release` - Build in Release configuration (optimized)
- `-r <RID>` - Runtime Identifier (win-x64, win-x86, win-arm64)
- `--self-contained true` - Include .NET runtime (users don't need .NET installed)
- `-p:PublishSingleFile=true` - Create a single executable file
- `-p:IncludeNativeLibrariesForSelfExtract=true` - Include native libraries in the single file
- `-p:EnableCompressionInSingleFile=true` - Compress the single file to reduce size
- `-o ./publish/<platform>` - Output directory

