﻿using Camera.MAUI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraficaCurone.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace GraficaCurone.ViewModel
{
    public partial class ViewModel : ObservableObject
    {
        #region Variabili
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
        [ObservableProperty]
        private CameraInfo camera;
        [ObservableProperty]
        private bool autoStartPreview;
        public ObservableCollection<CameraInfo> Cameras { get => Cameras; set { Camera = value.First(); Cameras = value; } }

        #endregion


        public ViewModel() 
        {
            MapVisible = true;
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
                    Compass.Default.Start(SensorSpeed.UI);
                }
            }
        }
        private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            Rotation = -(e.Reading.HeadingMagneticNorth);
            TextCompass = $"{Math.Round(e.Reading.HeadingMagneticNorth, 0)}°";
        }
        #endregion

        private void ShowCamera() 
        {
            MapVisible = false;
            CompassVisible = false;
            CameraVisible = true;
            AutoStartPreview = true;
        }

        //private void cameraView_CamerasLoaded(object sender, EventArgs e)
        //{
        //    if (cameraView.Cameras.Count > 0)
        //    {
        //        cameraView.Camera = cameraView.Cameras.First();
        //        MainThread.BeginInvokeOnMainThread(async () =>
        //        {
        //            await cameraView.StopCameraAsync();
        //            await cameraView.StartCameraAsync();
        //        });
        //    }
        //}

        //private void cameraView_BarcodeDetected(object sender, Camera.MAUI.ZXingHelper.BarcodeEventArgs args)
        //{
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        Qr.Text = $"{args.Result[0].BarcodeFormat}: {args.Result[0].Text}";
        //    });
        //}
    }
}