using NAudio.Wave;
using Streaming_Muesic_WPF.Utils;
using Streaming_Muesic_WPF.ViewModel;
using System;

namespace Streaming_Muesic_WPF.Input.Wasapi
{
    internal class WasapiViewModel : BaseInputVM, IDisposable
    {
        private IWaveIn capture;
        private SampleAggregator aggregator;
        private float maxLeft;
        private float maxRight;
        private float vuLeft;
        private float vuRight;
        private int sampleCount;
        private bool btnStartEnabled = true;
        private bool btnStopEnabled;

        public RelayCommand StartCommand { get; }
        public RelayCommand StopCommand { get; set; }

        public float VuLeft
        {
            get => vuLeft;
            set
            {
                vuLeft = value;
                OnPropertyChanged(nameof(VuLeft));
            }
        }

        public float VuRight
        {
            get => vuRight;
            set
            {
                vuRight = value;
                OnPropertyChanged(nameof(VuRight));
            }
        }

        public bool BtnStartEnabled
        {
            get => btnStartEnabled;
            set
            {
                btnStartEnabled = value;
                OnPropertyChanged(nameof(BtnStartEnabled));
            }
        }

        public bool BtnStopEnabled
        {
            get => btnStopEnabled;
            set
            {
                btnStopEnabled = value;
                OnPropertyChanged(nameof(BtnStopEnabled));
            }
        }

        public WasapiViewModel()
        {
            Aggregator = new SampleAggregator();
            StartCommand = new RelayCommand(Start);
            StopCommand = new RelayCommand(Stop);
        }

        private void Capture_RecordingStopped(object sender, StoppedEventArgs e)
        {
            capture.Dispose();
            capture = null;
            BtnStartEnabled = true;
        }

        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            for (int i = 0; i < e.BytesRecorded; i += 8)
            {
                float leftSample = BitConverter.ToSingle(e.Buffer, i);
                float rightSample = BitConverter.ToSingle(e.Buffer, i + 4);

                //aggregator.Add(leftSample, rightSample);

                maxLeft = Math.Max(maxLeft, Math.Abs(leftSample));
                maxRight = Math.Max(maxRight, Math.Abs(rightSample));
                sampleCount++;

                if (sampleCount >= capture.WaveFormat.SampleRate / 25)
                {
                    VuLeft = maxLeft;
                    VuRight = maxRight;
                    sampleCount = 0;
                    maxLeft = maxRight = 0;
                }
            }
        }

        private void Start()
        {
            capture = new WasapiLoopbackCapture();
            capture.DataAvailable += Capture_DataAvailable;
            capture.RecordingStopped += Capture_RecordingStopped;
            capture.StartRecording();
            BtnStartEnabled = false;
            BtnStopEnabled = true;
        }

        private void Stop()
        {
            capture?.StopRecording();
            BtnStopEnabled = false;
        }

        public void Dispose()
        {
            capture?.Dispose();
            capture = null;
        }

    }
}
