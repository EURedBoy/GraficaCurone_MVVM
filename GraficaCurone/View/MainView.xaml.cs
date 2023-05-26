using Camera.MAUI;
using GraficaCurone.ViewModel;

namespace GraficaCurone.View;

public partial class MainView : Shell
{
	public CameraView camera { get; set; }
    private MainViewModel mainViewModel;

    public MainView()
	{
		InitializeComponent();
        camera = cameraView;
        mainViewModel = new MainViewModel(this);
        BindingContext = mainViewModel;
	}

    private async void CameraLoaded(object sender, EventArgs e)
    {
        await mainViewModel.CameraLoadAsync();
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

}