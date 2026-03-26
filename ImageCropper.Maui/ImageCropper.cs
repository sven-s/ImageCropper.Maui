using Microsoft.Extensions.Logging;

namespace ImageCropper.Maui;

public class ImageCropper
{
    /// <summary>
    /// Logger instance. Auto-resolved from MAUI's DI container; can be overridden manually.
    /// </summary>
    public ILogger Logger { get; set; }

    public static ImageCropper Current { get; set; }

    public ImageCropper()
    {
        Current = this;
        Logger = ResolveLogger();
    }

    private static ILogger ResolveLogger()
    {
        try
        {
            var factory = IPlatformApplication.Current?.Services.GetService<ILoggerFactory>();
            return factory?.CreateLogger<ImageCropper>();
        }
        catch
        {
            return null;
        }
    }

    public enum CropShapeType
    {
        Rectangle,
        Oval
    };

    public CropShapeType CropShape { get; set; } = CropShapeType.Rectangle;

    public int AspectRatioX { get; set; } = 0;

    public int AspectRatioY { get; set; } = 0;

    public string PageTitle { get; set; } = null;

    public string SelectSourceTitle { get; set; } = "Select source";

    public string TakePhotoTitle { get; set; } = "Take Photo";

    public string PhotoLibraryTitle { get; set; } = "Photo Library";

    public string CancelButtonTitle { get; set; } = "Cancel";

    /// <summary>
    /// Boton para realizar el recorte
    /// </summary>
    public string CropButtonTitle { get; set; } = "Crop";

    /// <summary>
    /// Padding ratio around the initial crop window (0.0 - 0.5). Higher values add more margin.
    /// Default is 0.1. Increase if the crop rectangle extends beyond the touchable area on some devices.
    /// </summary>
    public float InitialCropWindowPaddingRatio { get; set; } = 0.1f;

    public Action<string> Success { get; set; }

    public Action Failure { get; set; }

    public MediaPickerOptions MediaPickerOptions { get; set; } = new();

    public async Task Show(Page page, string imageFile = null)
    {
        if (imageFile == null)
        {
            FileResult file = null;
            string newFile = null;

            var action = await page.DisplayActionSheetAsync(SelectSourceTitle, CancelButtonTitle, null,
                TakePhotoTitle, PhotoLibraryTitle);
            try
            {
                if (action == TakePhotoTitle)
                {
                    file = await MediaPicker.Default.CapturePhotoAsync(MediaPickerOptions).ConfigureAwait(false);
                }
                else if (action == PhotoLibraryTitle)
                {
                    var pickResult = await FilePicker.PickAsync(new PickOptions
                    {
                        PickerTitle = PhotoLibraryTitle,
                        FileTypes = FilePickerFileType.Images
                    }).ConfigureAwait(false);
                    if (pickResult != null)
                        file = new FileResult(pickResult.FullPath);
                }
                else
                {
                    MainThread.BeginInvokeOnMainThread(() => Failure?.Invoke());
                    return;
                }

                if (file != null)
                {
                    // save the file into local storage
                    newFile = Path.Combine(FileSystem.CacheDirectory, file.FileName);

                    //Mover a cache local
                    if (File.Exists(newFile))
                    {
                        File.Delete(newFile);
                    }

                    //Copiarlo llevaba mucho trabajo
                    await using var stream = await file.OpenReadAsync().ConfigureAwait(false);
                    await using var newStream = File.OpenWrite(newFile);
                    await stream.CopyToAsync(newStream).ConfigureAwait(false);
                    stream.Close();
                    newStream.Close();
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Failed to capture or pick photo");
            }

            if (file == null || newFile == null)
            {
                Failure?.Invoke();
                return;
            }

            imageFile = newFile;
        }
        else
        {
            // Copy the provided file to cache so the cropper doesn't need write access to the original
            var cachedFile = Path.Combine(FileSystem.CacheDirectory, $"crop_input_{Guid.NewGuid()}{Path.GetExtension(imageFile)}");
            File.Copy(imageFile, cachedFile, true);
            imageFile = cachedFile;
        }

        // small delay
        await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
        MainThread.BeginInvokeOnMainThread(() =>
            DependencyService.Get<IImageCropperWrapper>().ShowFromFile(this, imageFile));
    }
}