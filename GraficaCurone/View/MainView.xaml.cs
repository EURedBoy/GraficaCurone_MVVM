using Camera.MAUI;
using GraficaCurone.ViewModel;

namespace GraficaCurone.View;

public partial class MainView : Shell
{
	public CameraView camera { get; set; }
    private MainViewModel mainViewModel;

    bool i = true;

    public MainView()
	{
		InitializeComponent();
        mainViewModel = new MainViewModel(this);
        BindingContext = mainViewModel;
	}

    private void CameraLoaded(object sender, EventArgs e)
    {
        //await mainViewModel.CameraLoadAsync();
        if (cameraView.Cameras.Count > 0)
        {
            cameraView.Camera = cameraView.Cameras.First();

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await cameraView.StopCameraAsync();
                await cameraView.StartCameraAsync();
            });
        }
    }

    private async void BarcodeDetected(object sender, Camera.MAUI.ZXingHelper.BarcodeEventArgs args)
    {
        await mainViewModel.BarCodeResultAsync(args);
    }

    protected override async void OnAppearing()
    {
        await mainViewModel.trackManager.Init();
        await mainViewModel.nfcManager.Init();
    }

    private async void OnAppearing(object sender, EventArgs e)
    {
        if (i)
        {
            i = false;
            return;
        }
        camera = cameraView;
        CameraLoaded(sender, e);
    }
}