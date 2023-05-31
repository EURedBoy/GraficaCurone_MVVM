using Camera.MAUI.ZXingHelper;
using Camera.MAUI;
using GraficaCurone.ViewModel;

namespace GraficaCurone.View;

public partial class QrCodePage : ContentPage
{
    private MainViewModel viewModel;

    public QrCodePage(MainViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
    }

    private async void CameraLoaded(object sender, EventArgs e)
    {
        if (cameraView.Cameras.Count > 0)
        {
            cameraView.Camera = cameraView.Cameras.First();

            if (cameraView == null) return;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await cameraView.StopCameraAsync();
                await cameraView.StartCameraAsync();
            });
        }
    }

    protected override void OnAppearing()
    {
        if (cameraView.Cameras.Count > 0)
        {
            cameraView.Camera = cameraView.Cameras.First();

            if (cameraView == null) return;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await cameraView.StopCameraAsync();
                await cameraView.StartCameraAsync();
            });
        }
    }

    private async void BarcodeDetected(object sender, BarcodeEventArgs args)
    {
        await viewModel.BarCodeResultAsync(args);
    }
}