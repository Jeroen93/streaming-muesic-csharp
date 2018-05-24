using System;

namespace Streaming_Muesic_WPF.minim
{
    public abstract class WindowFunction
    {
        /** The float value of 2*PI. Provided as a convenience for subclasses. */
        protected const float TWO_PI = (float)(2 * Math.PI);
        protected int length;

        public WindowFunction()
        {
        }

        /** 
         * Apply the window function to a sample buffer.
         * 
         * @param samples a sample buffer
         */
        public void Apply(float[] samples)
        {
            length = samples.Length;

            for (int n = 0; n < samples.Length; n++)
            {
                samples[n] *= Value(samples.Length, n);
            }
        }

        /**
         * Apply the window to a portion of this sample buffer,
         * given an offset from the beginning of the buffer 
         * and the number of samples to be windowed.
         * 
         * @param samples
         * 		float[]: the array of samples to apply the window to
         * @param offset
         * 		int: the index in the array to begin windowing
         * @param length
         * 		int: how many samples to apply the window to
         */
        public void Apply(float[] samples, int offset, int length)
        {
            this.length = length;

            for (int n = offset; n < offset + length; ++n)
            {
                samples[n] *= Value(length, n - offset);
            }
        }

        /** 
         * Generates the curve of the window function.
         * 
         * @param length the length of the window
         * @return the shape of the window function
         */
        public float[] GenerateCurve(int length)
        {
            float[] samples = new float[length];
            for (int n = 0; n < length; n++)
            {
                samples[n] = 1f * Value(length, n);
            }
            return samples;
        }

        protected abstract float Value(int length, int index);
    }
}
