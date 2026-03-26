using Android.Content.Res;
using Com.Canhub.Cropper;
using Android.Graphics;
using Microsoft.Extensions.Logging;

namespace ImageCropper.Maui;

// All the code in this file is only included on Android.
public class PlatformImageCropper : IImageCropperWrapper
{
    private static bool IsDarkMode =>
        (Android.App.Application.Context.Resources.Configuration.UiMode & UiMode.NightMask) == UiMode.NightYes;

    public void ShowFromFile(ImageCropper imageCropper, string imageFile)
    {
        try
        {
            imageCropper.Logger?.LogInformation("Launching crop activity for {ImageFile}", imageFile);

            var cropImageOptions = new CropImageOptions();

            cropImageOptions.CropMenuCropButtonTitle = new Java.Lang.String(imageCropper.CropButtonTitle);
            cropImageOptions.OutputCompressFormat = Bitmap.CompressFormat.Png;

            cropImageOptions.CropShape = imageCropper.CropShape == ImageCropper.CropShapeType.Oval ? CropImageView.CropShape.Oval : CropImageView.CropShape.Rectangle;

            if (imageCropper.CropShape == ImageCropper.CropShapeType.Oval)
            {
                // Force 1:1 aspect ratio for oval to ensure a circle, not an ellipse
                cropImageOptions.FixAspectRatio = true;
                cropImageOptions.AspectRatioX = 1;
                cropImageOptions.AspectRatioY = 1;
            }
            else if (imageCropper.AspectRatioX > 0 && imageCropper.AspectRatioY > 0)
            {
                cropImageOptions.FixAspectRatio = true;
                cropImageOptions.AspectRatioX = imageCropper.AspectRatioX;
                cropImageOptions.AspectRatioY = imageCropper.AspectRatioY;
            }
            else
            {
                cropImageOptions.FixAspectRatio = false;
            }

            cropImageOptions.InitialCropWindowPaddingRatio = imageCropper.InitialCropWindowPaddingRatio;

            // Theme-aware toolbar colors for readable status bar in both light and dark mode
            if (IsDarkMode)
            {
                cropImageOptions.ToolbarColor = new Java.Lang.Integer(Android.Graphics.Color.Argb(255, 33, 33, 33));
                cropImageOptions.ToolbarTitleColor = new Java.Lang.Integer(Android.Graphics.Color.Argb(255, 255, 255, 255));
                cropImageOptions.ActivityMenuIconColor = Android.Graphics.Color.Argb(255, 255, 255, 255);
                cropImageOptions.ActivityMenuTextColor = new Java.Lang.Integer(Android.Graphics.Color.Argb(255, 255, 255, 255));
                cropImageOptions.ActivityBackgroundColor = Android.Graphics.Color.Argb(255, 0, 0, 0);
            }
            else
            {
                cropImageOptions.ToolbarColor = new Java.Lang.Integer(Android.Graphics.Color.Argb(255, 245, 245, 245));
                cropImageOptions.ToolbarTitleColor = new Java.Lang.Integer(Android.Graphics.Color.Argb(255, 33, 33, 33));
                cropImageOptions.ActivityMenuIconColor = Android.Graphics.Color.Argb(255, 33, 33, 33);
                cropImageOptions.ActivityMenuTextColor = new Java.Lang.Integer(Android.Graphics.Color.Argb(255, 33, 33, 33));
                cropImageOptions.ActivityBackgroundColor = Android.Graphics.Color.Argb(255, 255, 255, 255);
            }

            if (!string.IsNullOrWhiteSpace(imageCropper.PageTitle))
            {
                cropImageOptions.ActivityTitle = new Java.Lang.String(imageCropper.PageTitle);
            }

            Platform.ImageCropperActivityResultLauncher.Launch(
                new CropImageContractOptions(Android.Net.Uri.FromFile(new Java.IO.File(imageFile)), cropImageOptions));
        }
        catch (Exception ex)
        {
            imageCropper.Logger?.LogError(ex, "Failed to launch crop activity");
        }
    }
}