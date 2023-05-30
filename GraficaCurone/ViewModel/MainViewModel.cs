using Camera.MAUI;
using Camera.MAUI.ZXingHelper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraficaCurone.Manager;
using GraficaCurone.View;
using Plugin.Maui.Audio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GraficaCurone.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        #region Variabili_Parte_Grafica
        [ObservableProperty]
        private bool selected;
        [ObservableProperty]
        private bool mapVisible;
        [ObservableProperty]
        private bool compassVisible;
        [ObservableProperty]
        private bool cameraVisible;
        [ObservableProperty]
        private double rotation;
        [ObservableProperty]
        private string textCompass;


        private CameraView cameraView;
        private MainView mainView;
        public NFCManager nfcManager { get; set; }
        public TrackManager trackManager { get; set; }
        #endregion


        public MainViewModel(MainView mainView) 
        {
            MapVisible = true;
            this.mainView = mainView;
            cameraView = mainView.camera;
            trackManager = new TrackManager(AudioManager.Current);
            nfcManager = new NFCManager(trackManager);
        }

        #region MetodiPagine

        [RelayCommand]
        public async void BottoneLinguaClicked(EventArgs e)
        {
            string linguaScelta;
            linguaScelta = await App.Current.MainPage.DisplayActionSheet("Select language:", "Cancel", null, "italiano", "english");

            if (linguaScelta == "italiano")
            {
                trackManager.InEnglish = false;
            }
            else
            {
                trackManager.InEnglish = true;
            }
            await trackManager.LoadTracksAsync();
            trackManager.secondi = trackManager.player.CurrentPosition;
            await trackManager.PlayTheTrack(trackManager.LastTrack);
        }

        #region ChooseThePage
        [RelayCommand]
        public void SwitchPage(string pageType)
        {
            //utilizziamo una scringa come valore di riferimento in MVVM, ogni volta che un bottone verrà premuto, chiamerà il seguente metodo 
            //utilizzando come stringa "pageType", e il metodo non farà altro di verificare quale delle pagine è stata chiamata. 
            switch (pageType)
            {
                case "showmap":
                    ShowMap();
                    break;

                case "showcompass":
                    ShowCompass();
                    break;

                case "showcamera":
                    ShowCamera();
                    break;
            }
        }
        #endregion

        #region Mappa
        private void ShowMap()
        {
            MapVisible = true;
            CompassVisible= false;
            CameraVisible= false;
        }
        #endregion

        #region Bussola
        private void ShowCompass()
        {
            MapVisible = false;
            CompassVisible = true;
            CameraVisible = false;
            if (Compass.Default.IsSupported)
            {
                if (!Compass.Default.IsMonitoring)
                {
                    // Turn on compass
                    Compass.Default.ReadingChanged += Compass_ReadingChanged;
                    Compass.Default.Start(SensorSpeed.Game);
                }
            }
        }
        private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            Rotation = -(e.Reading.HeadingMagneticNorth);
            TextCompass = $"{Math.Round(e.Reading.HeadingMagneticNorth, 0)}°";
        }
        #endregion

        #region Camera

        private async void ShowCamera()
        {
            MapVisible = false;
            CompassVisible = false;
            CameraVisible = true;

            mainView.camera.BarCodeDetectionEnabled = true;
        }

        public async Task CameraLoadAsync()
        {
            if (mainView.camera.Cameras.Count > 0)
            {
                mainView.camera.Camera = mainView.camera.Cameras.First();

                if (mainView.camera ==  null) return;

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await mainView.camera.StopCameraAsync();
                    await mainView.camera.StartCameraAsync();
                });
            }
        }
        public async Task BarCodeResultAsync(BarcodeEventArgs args)
        {
            if (args == null) { }
            MainThread.BeginInvokeOnMainThread(async() =>
            {
                await trackManager.PlayTheTrack(int.Parse(args.Result[0].Text));
                mainView.camera.BarCodeDetectionEnabled = false;
                MapVisible = true;
                CompassVisible = false;
                CameraVisible = false;
            });
        }
        #endregion

        #endregion

    }
}