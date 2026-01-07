# Windows Deployment Script for Deadline Application
# This script publishes the application for Windows platforms

Write-Host "Publishing Deadline for Windows x64..." -ForegroundColor Green
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o ./publish/win-x64

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nWindows x64 publish completed successfully!" -ForegroundColor Green
    Write-Host "Output location: ./publish/win-x64/Deadline.exe" -ForegroundColor Cyan
} else {
    Write-Host "`nWindows x64 publish failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "`nTo publish for other Windows architectures, use:" -ForegroundColor Yellow
Write-Host "  win-x86   - 32-bit Windows" -ForegroundColor Yellow
Write-Host "  win-arm64 - ARM64 Windows" -ForegroundColor Yellow

