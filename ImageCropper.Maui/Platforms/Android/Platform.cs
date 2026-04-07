using Android.Graphics;
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
            if (cropImageResult is not CropImageView.CropResult result)
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

            // Android's CanHub cropper always outputs a rectangle, even for oval crops.
            // Apply a circular mask when oval/circle crop was requested.
            if (ImageCropper.Current?.CropShape == ImageCropper.CropShapeType.Oval)
            {
                filePath = ApplyCircularMask(filePath, logger);
                if (filePath == null)
                {
                    ImageCropper.Current?.Failure?.Invoke();
                    return;
                }
            }

            logger?.LogInformation("Crop completed, output at {Path}", filePath);
            ImageCropper.Current?.Success?.Invoke(filePath);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Unhandled exception in crop activity result handler");
            ImageCropper.Current?.Failure?.Invoke();
        }
    }

    private static string ApplyCircularMask(string filePath, ILogger logger)
    {
        try
        {
            using var source = BitmapFactory.DecodeFile(filePath);
            if (source == null)
            {
                logger?.LogError("Failed to decode bitmap for circular mask: {Path}", filePath);
                return null;
            }

            int size = Math.Min(source.Width, source.Height);
            var output = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888);
            var canvas = new Canvas(output);

            var paint = new Android.Graphics.Paint { AntiAlias = true };
            canvas.DrawCircle(size / 2f, size / 2f, size / 2f, paint);

            paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
            var srcRect = new Android.Graphics.Rect(
                (source.Width - size) / 2,
                (source.Height - size) / 2,
                (source.Width + size) / 2,
                (source.Height + size) / 2);
            var destRect = new Android.Graphics.Rect(0, 0, size, size);
            canvas.DrawBitmap(source, srcRect, destRect, paint);

            // Save as PNG to preserve transparency
            var outputPath = System.IO.Path.Combine(
                FileSystem.CacheDirectory,
                $"cropped_circle_{Guid.NewGuid()}.png");
            using var stream = new System.IO.FileStream(outputPath, System.IO.FileMode.Create);
            output.Compress(Bitmap.CompressFormat.Png, 100, stream);

            source.Recycle();
            output.Recycle();

            return outputPath;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to apply circular mask");
            return null;
        }
    }
}