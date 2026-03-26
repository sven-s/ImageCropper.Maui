using Microsoft.Extensions.Logging;

namespace ImageCropperDemo
{
    public partial class MainPage : ContentPage
    {
        private readonly ILogger<MainPage> _logger;

        public MainPage(ILogger<MainPage> logger)
        {
            InitializeComponent();
            _logger = logger;
        }

        protected async void OnClickedRectangle(object sender, EventArgs e)
        {
            imageView.Source = null;
            await new ImageCropper.Maui.ImageCropper
            {
                //                PageTitle = "Test Title",
                //                AspectRatioX = 1,
                //                AspectRatioY = 1,
                Success = (imageFile) =>
                {
                    Dispatcher.Dispatch(() =>
                    {
                        imageView.Source = ImageSource.FromFile(imageFile);
                    });
                },
                Failure = () =>
                {
                    _logger.LogWarning("Failed to capture or crop image");
                }
            }.Show(this);
        }

        private async void OnClickedCircle(object sender, EventArgs e)
        {
            imageView.Source = null;
            await new ImageCropper.Maui.ImageCropper()
            {
                CropShape = ImageCropper.Maui.ImageCropper.CropShapeType.Oval,
                Success = (imageFile) =>
                {
                    Dispatcher.Dispatch(() =>
                    {
                        imageView.Source = ImageSource.FromFile(imageFile);
                    });
                },
                Failure = () =>
                {
                    _logger.LogWarning("Failed to capture or crop image");
                }
            }.Show(this);
        }
    }
}