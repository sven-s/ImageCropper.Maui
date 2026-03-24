using AndroidX.Activity.Result;
using Com.Canhub.Cropper;
using Microsoft.Extensions.Logging;
using Fragment = AndroidX.Fragment.App.Fragment;
using Object = Java.Lang.Object;

namespace ImageCropper.Maui;

public class Platform : Fragment, IActivityResultCallback
{
    public MauiAppCompatActivity AppActivity { get; set; }

    public void Init(MauiAppCompatActivity activity)
    {
        DependencyService.Register<IImageCropperWrapper, PlatformImageCropper>();
        AppActivity = activity;
        ImageCropperActivityResultLauncher = activity.RegisterForActivityResult(new CropImageContract(), this);
    }

    public static ActivityResultLauncher ImageCropperActivityResultLauncher { get; set; }

    public void OnActivityResult(Object cropImageResult)
    {
        var logger = ImageCropper.Current?.Logger;

        try
        {
            if (cropImageResult is not CropImage.ActivityResult result)
            {
                logger?.LogWarning("Crop activity returned unexpected result type: {Type}",
                    cropImageResult?.GetType().Name);
                ImageCropper.Current?.Failure?.Invoke();
                return;
            }

            if (!result.IsSuccessful)
            {
                if (result.Error != null)
                {
                    logger?.LogError(result.Error, "Crop activity failed");
                }
                else
                {
                    logger?.LogWarning("Crop activity was cancelled or failed without error details");
                }

                ImageCropper.Current?.Failure?.Invoke();
                return;
            }

            var filePath = result.GetUriFilePath(AppActivity, true);

            if (string.IsNullOrEmpty(filePath))
            {
                logger?.LogError("Crop succeeded but GetUriFilePath returned null. URI: {Uri}",
                    result.UriContent?.ToString());
                ImageCropper.Current?.Failure?.Invoke();
                return;
            }

            ImageCropper.Current?.Success?.Invoke(filePath);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Unhandled exception in crop activity result handler");
            ImageCropper.Current?.Failure?.Invoke();
        }
    }
}