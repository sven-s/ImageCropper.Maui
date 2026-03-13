# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build entire solution
dotnet build ImageCropper.Maui.sln

# Build specific project (library only)
dotnet build ImageCropper.Maui/ImageCropper.Maui.csproj

# Build for specific platform
dotnet build ImageCropper.Maui/ImageCropper.Maui.csproj -f net10.0-ios
dotnet build ImageCropper.Maui/ImageCropper.Maui.csproj -f net10.0-android

# Pack NuGet
dotnet pack ImageCropper.Maui/ImageCropper.Maui.csproj -c Release
```

No test projects exist in this solution.

## Architecture

This is a .NET MAUI plugin (NuGet: `ImageCropper.Maui.V2`) for cropping/rotating photos on Android and iOS. Fork of jmbowman1107/ImageCropper.Maui (via odapmoa) with iOS fixes. It wraps two native libraries via binding projects.

### Project Structure (3 layers)

1. **ImageCropper.Maui** — Cross-platform library. Entry point is `ImageCropper.cs` which uses `DependencyService` to resolve `IImageCropperWrapper` per platform. Targets `net10.0;net10.0-android;net10.0-ios`.

2. **Native binding projects** (referenced conditionally by platform):
   - **Com.Vanniktech.AndroidImageCropper.Maui** — Android Java binding for [CanHub/Android-Image-Cropper](https://github.com/CanHub/Android-Image-Cropper). Contains AAR + metadata transforms. Target: `net10.0-android`.
   - **TOCropView.Maui** — iOS ObjC binding for [TimOliver/TOCropViewController](https://github.com/TimOliver/TOCropViewController). Uses `ApiDefinition.cs`/`StructsAndEnums.cs` + xcframework. Target: `net10.0-ios`.

3. **ImageCropperDemo** — Sample MAUI app (net10.0-android/ios). Not part of the NuGet package.

### Key Pattern

Platform implementations live in `ImageCropper.Maui/Platforms/{Android,iOS}/PlatformImageCropper.cs`. Both implement `IImageCropperWrapper.ShowFromFile()`. The Android path launches `CropImageActivity` via an activity result launcher; iOS presents `TOCropViewController` modally.

### iOS Safe Area

The toolbar is positioned at **top** (`TOCropViewControllerToolbarPosition.Top`) to avoid home indicator overlap on notched iPhones. This was a deliberate fix — don't move it back to bottom.

### NuGet Package IDs

- `ImageCropper.Maui.V2`
- `Com.Vanniktech.AndroidImageCropper.Maui.V2`
- `TOCropView.Maui.V2`

### Gotcha

Long file paths break TOCropView.Maui NuGet restore in Visual Studio — use `dotnet restore` from CLI instead.
