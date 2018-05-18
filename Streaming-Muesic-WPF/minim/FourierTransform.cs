using System;

namespace Streaming_Muesic_WPF.minim
{
    public abstract class FourierTransform
    {
        public static readonly WindowFunction NONE = new RectangularWindow();
        public static readonly WindowFunction HAMMING = new HammingWindow();

        protected const int LINAVG = 1;
        protected const int LOGAVG = 2;
        protected const int NOAVG = 3;

        protected const float TWO_PI = (float)(2 * Math.PI);
        protected int timeSize;
        protected int sampleRate;
        protected float bandWidth;
        protected WindowFunction currentWindow;
        protected float[] real;
        protected float[] imag;
        protected float[] spectrum;
        protected float[] averages;
        protected int whichAverage;
        protected int octaves;
        protected int avgPerOctave;

        /**
         * Construct a FourierTransform that will analyze sample buffers that are
         * <code>ts</code> samples long and contain samples with a <code>sr</code>
         * sample rate.
         * 
         * @param ts
         *          the length of the buffers that will be analyzed
         * @param sr
         *          the sample rate of the samples that will be analyzed
         */
        public FourierTransform(int ts, float sr)
        {
            timeSize = ts;
            sampleRate = (int)sr;
            bandWidth = (2f / timeSize) * ((float)sampleRate / 2f);
            NoAverages();
            AllocateArrays();
            currentWindow = new RectangularWindow(); // a Rectangular window is analogous to using no window. 
        }

        // allocating real, imag, and spectrum are the responsibility of derived
        // classes
        // because the size of the arrays will depend on the implementation being used
        // this enforces that responsibility
        protected abstract void AllocateArrays();

        protected void SetComplex(float[] r, float[] i)
        {
            if (real.Length != r.Length && imag.Length != i.Length)
            {
                Console.WriteLine("FourierTransform.setComplex: the two arrays must be the same length as their member counterparts.");
            }
            else
            {
                Array.Copy(r, 0, real, 0, r.Length);
                Array.Copy(i, 0, imag, 0, i.Length);
            }
        }

        // fill the spectrum array with the amps of the data in real and imag
        // used so that this class can handle creating the average array
        // and also do spectrum shaping if necessary
        protected void FillSpectrum()
        {
            for (int i = 0; i < spectrum.Length; i++)
            {
                spectrum[i] = (float)Math.Sqrt(real[i] * real[i] + imag[i] * imag[i]);
            }

            if (whichAverage == LINAVG)
            {
                int avgWidth = (int)spectrum.Length / averages.Length;
                for (int i = 0; i < averages.Length; i++)
                {
                    float avg = 0;
                    int j;
                    for (j = 0; j < avgWidth; j++)
                    {
                        int offset = j + i * avgWidth;
                        if (offset < spectrum.Length)
                        {
                            avg += spectrum[offset];
                        }
                        else
                        {
                            break;
                        }
                    }
                    avg /= j + 1;
                    averages[i] = avg;
                }
            }
            else if (whichAverage == LOGAVG)
            {
                for (int i = 0; i < octaves; i++)
                {
                    float lowFreq, hiFreq, freqStep;
                    if (i == 0)
                    {
                        lowFreq = 0;
                    }
                    else
                    {
                        lowFreq = (sampleRate / 2) / (float)Math.Pow(2, octaves - i);
                    }
                    hiFreq = (sampleRate / 2) / (float)Math.Pow(2, octaves - i - 1);
                    freqStep = (hiFreq - lowFreq) / avgPerOctave;
                    float f = lowFreq;
                    for (int j = 0; j < avgPerOctave; j++)
                    {
                        int offset = j + i * avgPerOctave;
                        averages[offset] = CalcAvg(f, f + freqStep);
                        f += freqStep;
                    }
                }
            }
        }

        /**
           * Sets the object to not compute averages.
           * 
           * @related FFT
           */
        public void NoAverages()
        {
            averages = new float[0];
            whichAverage = NOAVG;
        }

        /**
         * Sets the number of averages used when computing the spectrum and spaces the
         * averages in a linear manner. In other words, each average band will be
         * <code>specSize() / numAvg</code> bands wide.
         * 
         * @param numAvg
         *          int: how many averages to compute
         *          
         * @example Analysis/SoundSpectrum
         * 
         * @related FFT
         */
        public void LinAverages(int numAvg)
        {
            if (numAvg > spectrum.Length / 2)
            {
                Console.WriteLine($"The number of averages for this transform can be at most {spectrum.Length / 2}.");
                return;
            }
            else
            {
                averages = new float[numAvg];
            }
            whichAverage = LINAVG;
        }

        /**
           * Sets the number of averages used when computing the spectrum based on the
           * minimum bandwidth for an octave and the number of bands per octave. For
           * example, with audio that has a sample rate of 44100 Hz,
           * <code>logAverages(11, 1)</code> will result in 12 averages, each
           * corresponding to an octave, the first spanning 0 to 11 Hz. To ensure that
           * each octave band is a full octave, the number of octaves is computed by
           * dividing the Nyquist frequency by two, and then the result of that by two,
           * and so on. This means that the actual bandwidth of the lowest octave may
           * not be exactly the value specified.
           * 
           * @param minBandwidth
           *          int: the minimum bandwidth used for an octave, in Hertz.
           * @param bandsPerOctave
           *          int: how many bands to split each octave into
           *
           * @example Analysis/SoundSpectrum
           * 
           * @related FFT
           */
        public void LogAverages(int minBandwidth, int bandsPerOctave)
        {
            float nyq = sampleRate / 2f;
            octaves = 1;
            while ((nyq /= 2) > minBandwidth)
            {
                octaves++;
            }
            Console.WriteLine($"Number of octaves = {octaves}");
            avgPerOctave = bandsPerOctave;
            averages = new float[octaves * bandsPerOctave];
            whichAverage = LOGAVG;
        }

        /**
   * Sets the window to use on the samples before taking the forward transform.
   * If an invalid window is asked for, an error will be reported and the
   * current window will not be changed.
   * 
   * @param windowFunction 
   * 			the new WindowFunction to use, typically one of the statically defined 
   * 			windows like HAMMING or BLACKMAN
   * 
   * @related FFT
   * @related WindowFunction
   * 
   * @example Analysis/FFT/Windows
   */
        public void Window(WindowFunction windowFunction)
        {
            this.currentWindow = windowFunction;
        }

        protected void DoWindow(float[] samples)
        {
            currentWindow.Apply(samples);
        }

        /**
         * Returns the length of the time domain signal expected by this transform.
         * 
         * @return int: the length of the time domain signal expected by this transform
         * 
         * @related FFT
         */
        public int TimeSize()
        {
            return timeSize;
        }

        /**
         * Returns the size of the spectrum created by this transform. In other words,
         * the number of frequency bands produced by this transform. This is typically
         * equal to <code>timeSize()/2 + 1</code>, see above for an explanation.
         * 
         * @return int: the size of the spectrum
         * 
         * @example Basics/AnalyzeSound
         * 
         * @related FFT
         */
        public int SpecSize()
        {
            return spectrum.Length;
        }

        /**
         * Returns the amplitude of the requested frequency band.
         * 
         * @param i
         *          int: the index of a frequency band
         *          
         * @return float: the amplitude of the requested frequency band
         * 
         * @example Basics/AnalyzeSound
         * 
         * @related FFT
         */
        public float GetBand(int i)
        {
            if (i < 0) i = 0;
            if (i > spectrum.Length - 1) i = spectrum.Length - 1;
            return spectrum[i];
        }

        /**
         * Returns the width of each frequency band in the spectrum (in Hz). It should
         * be noted that the bandwidth of the first and last frequency bands is half
         * as large as the value returned by this function.
         * 
         * @return float: the width of each frequency band in Hz.
         * 
         * @related FFT
         */
        public float GetBandWidth()
        {
            return bandWidth;
        }

        /**
         * Returns the bandwidth of the requested average band. Using this information 
         * and the return value of getAverageCenterFrequency you can determine the 
         * lower and upper frequency of any average band.
         * 
         * @param averageIndex
         * 			int: the index of the average you want the bandwidth of
         * 
         * @return float: the bandwidth of the request average band, in Hertz.
         * 
         * @example Analysis/SoundSpectrum
         * 
         * @see #getAverageCenterFrequency(int)
         * 
         * @related getAverageCenterFrequency ( )
         * @related FFT
         *
         */
        public float GetAverageBandWidth(int averageIndex)
        {
            if (whichAverage == LINAVG)
            {
                // an average represents a certain number of bands in the spectrum
                int avgWidth = (int)spectrum.Length / averages.Length;
                return avgWidth * GetBandWidth();

            }
            else if (whichAverage == LOGAVG)
            {
                // which "octave" is this index in?
                int octave = averageIndex / avgPerOctave;
                float lowFreq, hiFreq, freqStep;
                // figure out the low frequency for this octave
                if (octave == 0)
                {
                    lowFreq = 0;
                }
                else
                {
                    lowFreq = (sampleRate / 2) / (float)Math.Pow(2, octaves - octave);
                }
                // and the high frequency for this octave
                hiFreq = (sampleRate / 2) / (float)Math.Pow(2, octaves - octave - 1);
                // each average band within the octave will be this big
                freqStep = (hiFreq - lowFreq) / avgPerOctave;

                return freqStep;
            }

            return 0;
        }

        /**
         * Sets the amplitude of the <code>i<sup>th</sup></code> frequency band to
         * <code>a</code>. You can use this to shape the spectrum before using
         * <code>inverse()</code>.
         * 
         * @param i
         *          int: the frequency band to modify
         * @param a
         *          float: the new amplitude
         *          
         * @example Analysis/FFT/SetBand
         * 
         * @related FFT
         */
        public abstract void SetBand(int i, float a);

        /**
         * Scales the amplitude of the <code>i<sup>th</sup></code> frequency band
         * by <code>s</code>. You can use this to shape the spectrum before using
         * <code>inverse()</code>.
         * 
         * @param i
         *          int: the frequency band to modify
         * @param s
         *          float: the scaling factor
         *          
         * @example Analysis/FFT/ScaleBand
         * 
         * @related FFT
         */
        public abstract void ScaleBand(int i, float s);

        /**
         * Returns the index of the frequency band that contains the requested
         * frequency.
         * 
         * @param freq
         *          float: the frequency you want the index for (in Hz)
         *          
         * @return int: the index of the frequency band that contains freq
         * 
         * @related FFT
         * 
         * @example Analysis/SoundSpectrum
         */
        public int FreqToIndex(float freq)
        {
            // special case: freq is lower than the bandwidth of spectrum[0]
            if (freq < GetBandWidth() / 2) return 0;
            // special case: freq is within the bandwidth of spectrum[spectrum.length - 1]
            if (freq > sampleRate / 2 - GetBandWidth() / 2) return spectrum.Length - 1;
            // all other cases
            float fraction = freq / sampleRate;
            int i = (int)Math.Round(timeSize * fraction);
            return i;
        }

        /**
         * Returns the middle frequency of the i<sup>th</sup> band.
         * 
         * @param i
         *        int: the index of the band you want to middle frequency of
         * 
         * @return float: the middle frequency, in Hertz, of the requested band of the spectrum
         * 
         * @related FFT
         */
        public float IndexToFreq(int i)
        {
            float bw = GetBandWidth();
            // special case: the width of the first bin is half that of the others.
            //               so the center frequency is a quarter of the way.
            if (i == 0) return bw * 0.25f;
            // special case: the width of the last bin is half that of the others.
            if (i == spectrum.Length - 1)
            {
                float lastBinBeginFreq = (sampleRate / 2) - (bw / 2);
                float binHalfWidth = bw * 0.25f;
                return lastBinBeginFreq + binHalfWidth;
            }
            // the center frequency of the ith band is simply i*bw
            // because the first band is half the width of all others.
            // treating it as if it wasn't offsets us to the middle 
            // of the band.
            return i * bw;
        }

        /**
         * Returns the center frequency of the i<sup>th</sup> average band.
         * 
         * @param i
         *     int: which average band you want the center frequency of.
         *     
         * @return float: the center frequency of the i<sup>th</sup> average band.
         * 
         * @related FFT
         * 
         * @example Analysis/SoundSpectrum
         */
        public float GetAverageCenterFrequency(int i)
        {
            if (whichAverage == LINAVG)
            {
                // an average represents a certain number of bands in the spectrum
                int avgWidth = (int)spectrum.Length / averages.Length;
                // the "center" bin of the average, this is fudgy.
                int centerBinIndex = i * avgWidth + avgWidth / 2;
                return IndexToFreq(centerBinIndex);

            }
            else if (whichAverage == LOGAVG)
            {
                // which "octave" is this index in?
                int octave = i / avgPerOctave;
                // which band within that octave is this?
                int offset = i % avgPerOctave;
                float lowFreq, hiFreq, freqStep;
                // figure out the low frequency for this octave
                if (octave == 0)
                {
                    lowFreq = 0;
                }
                else
                {
                    lowFreq = (sampleRate / 2) / (float)Math.Pow(2, octaves - octave);
                }
                // and the high frequency for this octave
                hiFreq = (sampleRate / 2) / (float)Math.Pow(2, octaves - octave - 1);
                // each average band within the octave will be this big
                freqStep = (hiFreq - lowFreq) / avgPerOctave;
                // figure out the low frequency of the band we care about
                float f = lowFreq + offset * freqStep;
                // the center of the band will be the low plus half the width
                return f + freqStep / 2;
            }

            return 0;
        }


        /**
         * Gets the amplitude of the requested frequency in the spectrum.
         * 
         * @param freq
         *          float: the frequency in Hz
         *          
         * @return float: the amplitude of the frequency in the spectrum
         * 
         * @related FFT
         */
        public float GetFreq(float freq)
        {
            return GetBand(FreqToIndex(freq));
        }

        /**
         * Sets the amplitude of the requested frequency in the spectrum to
         * <code>a</code>.
         * 
         * @param freq
         *          float: the frequency in Hz
         * @param a
         *          float: the new amplitude
         *          
         * @example Analysis/FFT/SetFreq
         * 
         * @related FFT
         */
        public void SetFreq(float freq, float a)
        {
            SetBand(FreqToIndex(freq), a);
        }

        /**
         * Scales the amplitude of the requested frequency by <code>a</code>.
         * 
         * @param freq
         *          float: the frequency in Hz
         * @param s
         *          float: the scaling factor
         *          
         * @example Analysis/FFT/ScaleFreq
         * 
         * @related FFT
         */
        public void ScaleFreq(float freq, float s)
        {
            ScaleBand(FreqToIndex(freq), s);
        }

        /**
         * Returns the number of averages currently being calculated.
         * 
         * @return int: the length of the averages array
         * 
         * @related FFT
         */
        public int AvgSize()
        {
            return averages.Length;
        }

        /**
         * Gets the value of the <code>i<sup>th</sup></code> average.
         * 
         * @param i
         *          int: the average you want the value of
         * @return float: the value of the requested average band
         * 
         * @related FFT
         */
        public float GetAvg(int i)
        {
            float ret;
            if (averages.Length > 0)
                ret = averages[i];
            else
                ret = 0;
            return ret;
        }

        /**
         * Calculate the average amplitude of the frequency band bounded by
         * <code>lowFreq</code> and <code>hiFreq</code>, inclusive.
         * 
         * @param lowFreq
         *          float: the lower bound of the band, in Hertz
         * @param hiFreq
         *          float: the upper bound of the band, in Hertz
         *          
         * @return float: the average of all spectrum values within the bounds
         * 
         * @related FFT
         */
        public float CalcAvg(float lowFreq, float hiFreq)
        {
            int lowBound = FreqToIndex(lowFreq);
            int hiBound = FreqToIndex(hiFreq);
            float avg = 0;
            for (int i = lowBound; i <= hiBound; i++)
            {
                avg += spectrum[i];
            }
            avg /= (hiBound - lowBound + 1);
            return avg;
        }

        /**
         * Get the Real part of the Complex representation of the spectrum.
         * 
         * @return float[]: an array containing the values for the Real part of the spectrum.
         * 
         * @related FFT
         */
        public float[] GetSpectrumReal()
        {
            return real;
        }

        /**
         * Get the Imaginary part of the Complex representation of the spectrum.
         * 
         * @return float[]: an array containing the values for the Imaginary part of the spectrum.
         * 
         * @related FFT
         */
        public float[] GetSpectrumImaginary()
        {
            return imag;
        }

        /**
         * Performs a forward transform on <code>buffer</code>.
         * 
         * @param buffer
         *          float[]: the buffer to analyze, must be the same length as timeSize()
         *    
         * @example Basics/AnalyzeSound
         * 
         * @related FFT
         */
        public abstract void Forward(float[] buffer);

        /**
         * Performs a forward transform on values in <code>buffer</code>.
         * 
         * @param buffer
         *          float[]: the buffer to analyze, must be the same length as timeSize()
         * @param startAt
         *          int: the index to start at in the buffer. there must be at least timeSize() samples
         *          between the starting index and the end of the buffer. If there aren't, an
         *          error will be issued and the operation will not be performed.
         *          
         */
        public void Forward(float[] buffer, int startAt)
        {
            if (buffer.Length - startAt < timeSize)
            {
                Console.WriteLine("FourierTransform.forward: not enough samples in the buffer between " +
                             startAt + " and " + buffer.Length + " to perform a transform."
                           );
                return;
            }

            // copy the section of samples we want to analyze
            float[] section = new float[timeSize];
            Array.Copy(buffer, startAt, section, 0, section.Length);
            Forward(section);
        }

        /**
         * Performs an inverse transform of the frequency spectrum and places the
         * result in <code>buffer</code>.
         * 
         * @param buffer
         *          float[]: the buffer to place the result of the inverse transform in
         *          
         *          
         * @related FFT
         */
        public abstract void Inverse(float[] buffer);

        /**
         * Performs an inverse transform of the frequency spectrum represented by
         * freqReal and freqImag and places the result in buffer.
         * 
         * @param freqReal
         *          float[]: the real part of the frequency spectrum
         * @param freqImag
         *          float[]: the imaginary part the frequency spectrum
         * @param buffer
         *          float[]: the buffer to place the inverse transform in
         */
        public void Inverse(float[] freqReal, float[] freqImag, float[] buffer)
        {
            SetComplex(freqReal, freqImag);
            Inverse(buffer);
        }
    }
}
