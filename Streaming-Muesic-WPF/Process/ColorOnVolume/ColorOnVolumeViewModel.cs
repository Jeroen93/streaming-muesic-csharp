using NAudio.Wave;
using System;

namespace Streaming_Muesic_WPF.Process.ColorOnVolume
{
    internal class ColorOnVolumeViewModel : ProcessViewModelBase
    {
        private float maxLeft;
        private float maxRight;
        private int sampleCount;
        private float vuMix;
        private double sliderValue = 0.5;
        private readonly int sampleRate;

        public ColorOnVolumeViewModel()
        {
            using (var capture = new WasapiLoopbackCapture())
            {
                sampleRate = capture.WaveFormat.SampleRate;
            }
        }

        public override void Calculate(float left, float right)
        {
            maxLeft = Math.Max(maxLeft, Math.Abs(left));
            maxRight = Math.Max(maxRight, Math.Abs(right));
            sampleCount++;

            if (sampleCount >= sampleRate / 10)
            {
                float mix = (maxLeft + maxRight) / 2f;
                VuMix = mix;

                if (mix >= sliderValue)
                {
                    Console.WriteLine("Send to hue! " + mix);
                }

                sampleCount = 0;
                maxLeft = maxRight = 0;
            }
        }

        public float VuMix
        {
            get => vuMix;
            set
            {
                vuMix = value;
                OnPropertyChanged(nameof(VuMix));
            }
        }

        public double SliderValue
        {
            get => sliderValue;
            set
            {
                sliderValue = value;
                OnPropertyChanged(nameof(SliderValue));
            }
        }
    }
}
