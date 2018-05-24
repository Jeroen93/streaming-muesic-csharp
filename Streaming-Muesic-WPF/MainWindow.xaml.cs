using System;
using System.Windows;
using System.Windows.Threading;
using NAudio.Wave;

namespace Streaming_Muesic_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IWaveIn capture;

        private float maxLeft;
        private float maxRight;
        private float vuLeft, vuRight;
        private int sampleCount;
        private Action action;

        public MainWindow()
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

                    Application.Current.Dispatcher.BeginInvoke(
                        DispatcherPriority.Background, action);
                    
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new HuePlugin();
        }

        private void Btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            capture?.StopRecording();
            btnStop.IsEnabled = false;
        }
    }
}
