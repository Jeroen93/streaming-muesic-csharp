using NAudio.Wave;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Streaming_Muesic_WPF.Input.Wasapi
{
    /// <summary>
    /// Interaction logic for WasapiView.xaml
    /// </summary>
    public partial class WasapiView : UserControl
    {
        private IWaveIn capture;

        private float maxLeft;
        private float maxRight;
        private float vuLeft, vuRight;
        private int sampleCount;
        private readonly Action action;

        public WasapiView()
        {
            InitializeComponent();

            action = new Action(() =>
            {
                pbLeft.Value = vuLeft;
                pbRight.Value = vuRight;
            });
        }

        private void Capture_RecordingStopped(object sender, StoppedEventArgs e)
        {
            capture.Dispose();
            capture = null;
            btnStart.IsEnabled = true;
        }

        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            for (int i = 0; i < e.BytesRecorded; i += 8)
            {
                float leftSample = BitConverter.ToSingle(e.Buffer, i);
                float rightSample = BitConverter.ToSingle(e.Buffer, i + 4);

                maxLeft = Math.Max(maxLeft, Math.Abs(leftSample));
                maxRight = Math.Max(maxRight, Math.Abs(rightSample));
                sampleCount++;

                if (sampleCount >= capture.WaveFormat.SampleRate / 25)
                {
                    vuLeft = maxLeft;
                    vuRight = maxRight;

                    RunOnUIThread(action);

                    sampleCount = 0;
                    maxLeft = maxRight = 0;
                }
            }
        }

        private void Btn_Start_Click(object sender, RoutedEventArgs e)
        {
            capture = new WasapiLoopbackCapture();
            capture.DataAvailable += Capture_DataAvailable;
            capture.RecordingStopped += Capture_RecordingStopped;
            capture.StartRecording();
            btnStart.IsEnabled = false;
            btnStop.IsEnabled = true;
        }

        private void Btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            capture?.StopRecording();
            btnStop.IsEnabled = false;
        }

        private void RunOnUIThread(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }
    }
}
