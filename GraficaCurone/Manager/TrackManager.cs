﻿using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.Maui.Audio;
using Plugin.NFC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraficaCurone.Manager
{
    public partial class TrackManager : ObservableObject
    {
        #region Variabili
        [ObservableProperty]
        private string currentText;
        public List<string> tracceAudio = new List<string>(5);
        public List<string> tracceTesto = new List<string>(5);
        public IAudioPlayer player;
        private bool InEnglish = false;
        private IAudioManager audioManager;
        #endregion

        public TrackManager(IAudioManager audioManager)
        {
            this.audioManager = audioManager;
        }

        public async Task Init()
        {
            var load = LoadTracksAsync();
            StartAccelerometer();
            await load;
        }

        public async Task LoadTracksAsync()
        {
            var path = "";
            if (InEnglish)
            {
                for (int i = 1; i < tracceTesto.Capacity + 1; i++)
                {
                    tracceAudio.Add($"audioTrack_{i}.mp3");
                    path = $"textTrack_{i}.txt";
                    var result = await FileSystem.OpenAppPackageFileAsync(path);
                    StreamReader stream = new StreamReader(result);
                    tracceTesto.Add(stream.ReadToEnd());
                }
            }
            else
            {
                for (int i = 1; i < tracceTesto.Capacity + 1; i++)
                {
                    tracceAudio.Add($"tracceAudio_{i}.mp3");
                    path = $"tracceTesto_{i}.txt";
                    var result = await FileSystem.OpenAppPackageFileAsync(path);
                    StreamReader stream = new StreamReader(result);
                    tracceTesto.Add(stream.ReadToEnd());
                }
            }
        }

        public async Task PlayTheTrack(int i)
        {
            CurrentText = tracceTesto[i];
            player = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(tracceAudio[i]));
            if (player == null) { return; }
            player.Play();
        }

        public void Accelerometer_ShakeDetected(object sender, EventArgs e)
        {
            if (player == null || player == default)
            {
                return;
            }

            if (!player.IsPlaying)
            {
                player.Play();
            }
            else
            {
                player.Pause();
            }
        }

        public void StartAccelerometer()
        {
            if (Accelerometer.Default.IsSupported)
            {
                if (!Accelerometer.Default.IsMonitoring)
                {
                    // Turn on accelerometer
                    Accelerometer.Default.ShakeDetected += Accelerometer_ShakeDetected;
                    Accelerometer.Default.Start(SensorSpeed.UI);
                }
                else
                {
                    // Turn off accelerometer
                    Accelerometer.Default.Stop();
                    Accelerometer.Default.ShakeDetected -= Accelerometer_ShakeDetected;
                }
            }
        }
    }
}
