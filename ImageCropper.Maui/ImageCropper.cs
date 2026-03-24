using Microsoft.Extensions.Logging;

namespace ImageCropper.Maui;

public class ImageCropper
{
    /// <summary>
    /// Optional logger. When set, errors are logged via ILogger (picked up by Sentry, etc.).
    /// </summary>
    public ILogger Logger { get; set; }

    public static ImageCropper Current { get; set; }

    public ImageCropper()
    {
        Current = this;
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

    public MediaPickerOptions MediaPickerOptions { get; set; } = new MediaPickerOptions();

    public async void Show(Page page, string imageFile = null)
    {
        if (imageFile == null)
        {
            FileResult file = null;
            string newFile = null;

            string action = await page.DisplayActionSheet(SelectSourceTitle, CancelButtonTitle, null,
                TakePhotoTitle, PhotoLibraryTitle);
            try
            {
                if (action == TakePhotoTitle)
                {
                    file = await MediaPicker.CapturePhotoAsync(MediaPickerOptions);
                }
                else if (action == PhotoLibraryTitle)
                {
                    file = await MediaPicker.PickPhotoAsync(MediaPickerOptions);
                }
                else
                {
                    Failure?.Invoke();
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
                    using (var stream = await file.OpenReadAsync())
                    using (var newStream = File.OpenWrite(newFile))
                    {
                        await stream.CopyToAsync(newStream);
                        stream.Close();
                        newStream.Close();
                    }
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

        // small delay
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        DependencyService.Get<IImageCropperWrapper>().ShowFromFile(this, imageFile);
    }
}