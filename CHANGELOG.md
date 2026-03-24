# Changelog

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
