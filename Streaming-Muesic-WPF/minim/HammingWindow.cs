using System;

namespace Streaming_Muesic_WPF.minim
{
    class HammingWindow : WindowFunction
    {
        /** Constructs a Hamming window. */
        public HammingWindow()
        {
        }

        protected override float Value(int length, int index)
        {
            return 0.54f - 0.46f * (float)Math.Cos(TWO_PI * index / (length - 1));
        }

        public override string ToString()
        {
            return "Hamming Window";
        }
    }
}
