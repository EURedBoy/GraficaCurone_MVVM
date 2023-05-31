using Camera.MAUI;
using Camera.MAUI.ZXingHelper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraficaCurone.Manager;
using GraficaCurone.View;
using Plugin.Maui.Audio;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.NFC;
using PdfiumViewer;
using System.Threading;
using GraficaCurone.Utils;

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
        [ObservableProperty] private Microsoft.Maui.Controls.View currentPage;
        [ObservableProperty]
        public LocalizationResourceManager localizationManager = LocalizationResourceManager.Instance;

        private CameraView cameraView;
        private MainView mainView;
        private bool isBusy;

        public NFCManager nfcManager { get; set; }
        public TrackManager trackManager { get; set; }
        public IFileSaver fileSaver;
        public CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        #endregion

        //public void ControlloNFC()
        //{
        //    bool c = true;
        //    while (c)
        //    {
        //        if (nfcManager.NfcIsEnabled)
        //        {
        //            c = false;
        //        }
        //    }
        //}

        public MainViewModel(MainView mainView) 
        {
            //Thread thread = new Thread(ControlloNFC);
            //thread.Start();
            //MapVisible = true;
            this.mainView = mainView;
            cameraView = mainView.camera;
            trackManager = new TrackManager(AudioManager.Current);
            nfcManager = new NFCManager(trackManager, this);
            fileSaver = FileSaver.Default;

            ShowMap();
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
                LocalizationResourceManager.Instance.SettaCultura(new CultureInfo("it-IT"));
            }
            else if(linguaScelta == "english")
            {
                trackManager.InEnglish = true;
                LocalizationResourceManager.Instance.SettaCultura(new CultureInfo("en-gb"));
            }
            else
            {
                return;
            }
            
            if (trackManager.player != null && trackManager.player.IsPlaying)
            {
                await trackManager.LoadTracksAsync();
                trackManager.secondi = trackManager.player.CurrentPosition;
                await trackManager.PlayTheTrack(trackManager.LastTrack);
            }
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

            CurrentPage = null;
            CurrentPage = new MapPage(this).Content;
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
                CurrentPage = null;
                CurrentPage = new CompassPage(this).Content;
            }
        }
        private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            Rotation = -(e.Reading.HeadingMagneticNorth);
            TextCompass = $"{Math.Round(e.Reading.HeadingMagneticNorth, 0)}°";
        }
        #endregion

        #region Camera

        public async void ShowCamera()
        {
            MapVisible = false;
            CompassVisible = false;
            CameraVisible = true;

            CurrentPage = null;
            CurrentPage = new QrCodePage(this).Content;
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
                await trackManager.PlayTheTrack(int.Parse(args.Result[0].Text) - 1);
                mainView.camera.BarCodeDetectionEnabled = false;
                MapVisible = true;
                CompassVisible = false;
                CameraVisible = false;
            });
        }
        #endregion

        #endregion

        //public void PdfViewerPage(string pdfFilePath)
        //{
        //    if (File.Exists(pdfFilePath))
        //    {
        //        using (var stream = new FileStream(pdfFilePath, FileMode.Open))
        //        {
        //            var document = PdfDocument.Load(stream);

        //            var imageSource = ImageSource.FromStream(() =>
        //            {
        //                var memoryStream = new MemoryStream();
        //                document.
        //                document.Render(0, 300, 300, true).Save(memoryStream, Microsoft.Maui.Graphics.ImageFormat.Png);
        //                memoryStream.Seek(0, SeekOrigin.Begin);
        //                return memoryStream;
        //            });

        //            var image = new Image { Source = imageSource };
        //            Content = new ScrollView { Content = image };
        //        }
        //    }
        //}

        [RelayCommand]
        private async void SaveFile()
        {
            if (isBusy) return;
            //isBusy = true;

            CrossNFC.Current.StopListening();
            //var externalDir = FileSystem.AppDataDirectory;
            //var externalDownloadDir = Path.Combine(externalDir, "Download");
            var fileStream = await FileSystem.Current.OpenAppPackageFileAsync("documentazione_applicazione_curone.pdf");

            bool status = await PermissionUtils.CheckForStoragePermission();
            if (!status) return;

            var result = await FolderPicker.Default.PickAsync(CancellationToken.None);
            if (!result.IsSuccessful) return;

            var endStream = File.Create(Path.Combine(result.Folder.Path, "documentazione_applicazione_curone.pdf"));
            await fileStream.CopyToAsync(endStream);

            isBusy = false;


            //var path = await FileSaver.Default.SaveAsync(Path.Combine(result.Folder.Path, "documentazione_applicazione_curone.pdf"), fileStream, CancellationTokenSource.Token);
            //if (path.IsSuccessful)
            //{
            //    await Toast.Make($"The file was saved successfully to location: {path.FilePath}").Show(default);
            //}
            //else
            //{
            //    await Toast.Make($"The file was not saved successfully with error: {path.Exception.Message}").Show(default);
            //}
        }
    }
}