# Changelog

## 1.4.0

### Added
- ILogger auto-resolves from MAUI's DI container — no manual setup needed
- Info logging when crop activity is launched and when crop completes (both platforms)
- Android: theme-aware toolbar colors — adapts to light/dark mode for readable status bar
- Android: circular mask applied to oval crop output (CanHub only outputs rectangles natively)
- Android: oval crop now forces 1:1 aspect ratio to ensure a circle, not an ellipse

### Fixed
- Android: replaced `MediaPicker.PickPhotoAsync` with `FilePicker` to fix `ActivityResultLauncher` registration crash on Android 13+
- Input files passed to `Show()` are now copied to cache before cropping, preventing file lock issues

### Changed
- `Show()` return type changed from `async void` to `async Task`
- Demo app updated to use DI for `ILogger` and `MainPage`

## 1.3.2.0

### Added
- Optional `ILogger` property on `ImageCropper` — integrates with standard .NET logging (Sentry, etc.)

### Fixed
- Android: crop activity errors (`result.Error`) were silently swallowed — now logged via `ILogger`
- Android: `GetUriFilePath` returning null on some devices (scoped storage) now triggers `Failure` callback with a log instead of silently calling `Success(null)`
- Android: unhandled exceptions in crop result handler now caught and logged
- iOS: save failure logging switched from `Debug.WriteLine` to `ILogger`
- Replaced all `Debug.WriteLine`/`Console.WriteLine` with `ILogger` calls

## 1.3.1.0

### Added
- `InitialCropWindowPaddingRatio` property (Android only) — controls padding around the initial crop rectangle (0.0–0.5, default 0.1). Increase if crop handles extend beyond the touchable screen area on some devices.
