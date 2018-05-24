using System;

namespace Streaming_Muesic_WPF.minim
{
    class BeatDetect
    {
        /** Constant used to request frequency energy tracking mode.
	     * 
	     *  @example Analysis/FrequencyEnergyBeatDetection
	     */
        public const int FREQ_ENERGY = 0;

        /** Constant used to request sound energy tracking mode.
         * 
         *  @example Analysis/SoundEnergyBeatDetection
         */
        public const int SOUND_ENERGY = 1;

        private int algorithm;
        private readonly int sampleRate;
        private readonly int timeSize;
        private readonly int sampleTime;
        private int valCnt;
        private float[] valGraph;
        private readonly int sensitivity;
        // for circular buffer support
        private int insertAt;
        // vars for sEnergy
        private bool isOnset;
        private float[] eBuffer;
        private float[] dBuffer;
        private long timer;
        // vars for fEnergy
        private bool[] fIsOnset;
        private FFT spect;
        private float[,] feBuffer;
        private float[,] fdBuffer;
        private long[] fTimer;
        private float[] varGraph;
        private int varCnt;

        /**
         * Create a BeatDetect object that is in SOUND_ENERGY mode.
         * <code>timeSize</code> and <code>sampleRate</code> will be set to 1024
         * and 44100, respectively, so that it is possible to switch into FREQ_ENERGY
         * mode with meaningful values.
         * 
         */

        public BeatDetect()
        {
            sampleRate = 44100;
            timeSize = 1024;
            InitSEResources();
            InitGraphs();
            algorithm = SOUND_ENERGY;
            sensitivity = 10;
        }

        /**
	     * Create a BeatDetect object that is in FREQ_ENERGY mode and expects a
	     * sample buffer with the requested attributes.
	     * 
	     * @param timeSize
	     *           int: the size of the buffer
	     * @param sampleRate
	     *           float: the sample rate of the samples in the buffer
	     *           
	     * @related BeatDetect
	     */

        public BeatDetect(int timeSize, float sampleRate)
        {
            this.sampleRate = (int)sampleRate;
            this.timeSize = timeSize;
            sampleTime = this.sampleRate / timeSize;
            InitFEResources();
            InitGraphs();
            algorithm = FREQ_ENERGY;
            sensitivity = 10;
        }

        /**
	     * Set the object to use the requested algorithm. If an invalid value is
	     * passed, the function will report and error and default to
	     * BeatDetect.SOUND_ENERGY
	     * 
	     * @param algo
	     *           int: either BeatDetect.SOUND_ENERGY or BeatDetect.FREQ_ENERGY
	     *           
	     * @related BeatDetect
	     */
        public void DetectMode(int algo)
        {
            if (algo < 0 || algo > 1)
            {
                algo = SOUND_ENERGY;
            }
            if (algo == SOUND_ENERGY)
            {
                if (algorithm == FREQ_ENERGY)
                {
                    ReleaseFEResources();
                    InitSEResources();
                    InitGraphs();
                    algorithm = algo;
                }
            }
            else
            {
                if (algorithm == SOUND_ENERGY)
                {
                    ReleaseSEResources();
                    InitFEResources();
                    InitGraphs();
                    algorithm = FREQ_ENERGY;
                }
            }
        }

        private void InitGraphs()
        {
            valCnt = varCnt = 0;
            valGraph = new float[512];
            varGraph = new float[512];
        }

        private void InitSEResources()
        {
            isOnset = false;
            eBuffer = new float[sampleTime];
            dBuffer = new float[sampleTime];
            timer = DateTime.Now.Ticks;
            insertAt = 0;
        }

        private void InitFEResources()
        {
            spect = new FFT(timeSize, sampleRate);
            spect.LogAverages(60, 3);
            int numAvg = spect.AvgSize();
            fIsOnset = new bool[numAvg];
            feBuffer = new float[numAvg, sampleTime];
            fdBuffer = new float[numAvg, sampleTime];
            fTimer = new long[numAvg];
            long start = DateTime.Now.Ticks;
            for (int i = 0; i < fTimer.Length; i++)
            {
                fTimer[i] = start;
            }
            insertAt = 0;
        }

        private void ReleaseSEResources()
        {
            isOnset = false;
            eBuffer = null;
            dBuffer = null;
            timer = 0;
        }

        private void ReleaseFEResources()
        {
            spect = null;
            fIsOnset = null;
            feBuffer = null;
            fdBuffer = null;
            fTimer = null;
        }

        /**
	     * Analyze the samples in <code>buffer</code>. This is a cumulative
	     * process, so you must call this function every frame.
	     * 
	     * @param buffer
	     *           float[]: the buffer to analyze
	     *           
	     * @related BeatDetect
	     */
        public void Detect(float[] buffer)
        {
            switch (algorithm)
            {
                case SOUND_ENERGY:
                    SEnergy(buffer);
                    break;
                case FREQ_ENERGY:
                    FEnergy(buffer);
                    break;
            }
        }

        /**
	 * In sound energy mode this returns true when a beat has been detected. In
	 * frequency energy mode this always returns false.
	 * 
	 * @return boolean: true if a beat has been detected.
	 * 
	 * @example Analysis/SoundEnergyBeatDetection
	 * 
	 * @related BeatDetect
	 */
        public bool IsOnset()
        {
            return isOnset;
        }

        /**
         * In frequency energy mode this returns true when a beat has been detect in
         * the <code>i<sup>th</sup></code> frequency band. In sound energy mode
         * this always returns false.
         * 
         * @param i
         *           int: the frequency band to query
         * @return boolean: true if a beat has been detected in the requested band
         * 
         * @example Analysis/SoundEnergyBeatDetection
         * 
         * @related BeatDetect
         */
        public bool IsOnset(int i)
        {
            if (algorithm == SOUND_ENERGY)
            {
                return false;
            }
            return fIsOnset[i];
        }

        /**
         * In frequency energy mode this returns true if a beat corresponding to the
         * frequency range of a kick drum has been detected. This has been tuned to
         * work well with dance / techno music and may not perform well with other
         * styles of music. In sound energy mode this always returns false.
         * 
         * @return boolean: true if a kick drum beat has been detected
         * 
         * @example Analysis/FrequencyEnergyBeatDetection
         * 
         * @related BeatDetect
         */
        public bool IsKick()
        {
            if (algorithm == SOUND_ENERGY)
            {
                return false;
            }
            int upper = 6 >= spect.AvgSize() ? spect.AvgSize() : 6;
            return IsRange(1, upper, 2);
        }

        /**
         * In frequency energy mode this returns true if a beat corresponding to the
         * frequency range of a snare drum has been detected. This has been tuned to
         * work well with dance / techno music and may not perform well with other
         * styles of music. In sound energy mode this always returns false.
         * 
         * @return boolean: true if a snare drum beat has been detected
         * 
         * @example Analysis/FrequencyEnergyBeatDetection
         * 
         * @related BeatDetect
         */
        public bool IsSnare()
        {
            if (algorithm == SOUND_ENERGY)
            {
                return false;
            }
            int lower = 8 >= spect.AvgSize() ? spect.AvgSize() : 8;
            int upper = spect.AvgSize() - 1;
            int thresh = (upper - lower) / 3 + 1;
            return IsRange(lower, upper, thresh);
        }

        /**
         * In frequency energy mode this returns true if a beat corresponding to the
         * frequency range of a hi hat has been detected. This has been tuned to work
         * well with dance / techno music and may not perform well with other styles
         * of music. In sound energy mode this always returns false.
         * 
         * @return boolean: true if a hi hat beat has been detected
         * 
         * @example Analysis/FrequencyEnergyBeatDetection
         * 
         * @related BeatDetect
         */
        public bool IsHat()
        {
            if (algorithm == SOUND_ENERGY)
            {
                return false;
            }
            int lower = spect.AvgSize() - 7 < 0 ? 0 : spect.AvgSize() - 7;
            int upper = spect.AvgSize() - 1;
            return IsRange(lower, upper, 1);
        }

        /**
         * In frequency energy mode this returns true if at least
         * <code>threshold</code> bands of the bands included in the range
         * <code>[low, high]</code> have registered a beat. In sound energy mode
         * this always returns false.
         * 
         * @param low
         *           int: the index of the lower band
         * @param high
         *           int: the index of the higher band
         * @param threshold
         *           int: the smallest number of bands in the range
         *           <code>[low, high]</code> that need to have registered a beat
         *           for this to return true
         * @return boolean: true if at least <code>threshold</code> bands of the bands
         *         included in the range <code>[low, high]</code> have registered a
         *         beat
         *         
         * @related BeatDetect
         */
        public bool IsRange(int low, int high, int threshold)
        {
            if (algorithm == SOUND_ENERGY)
            {
                return false;
            }
            int num = 0;
            for (int i = low; i < high + 1; i++)
            {
                if (IsOnset(i))
                {
                    num++;
                }
            }
            return num >= threshold;
        }

        private void SEnergy(float[] samples)
        {
            // compute the energy level
            float level = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                level += (samples[i] * samples[i]);
            }
            level /= samples.Length;
            level = (float)Math.Sqrt(level);
            float instant = level * 100;
            // compute the average local energy
            float E = Average(eBuffer);
            // compute the variance of the energies in eBuffer
            float V = Variance(eBuffer, E);
            // compute C using a linear digression of C with V
            float C = (-0.0025714f * V) + 1.5142857f;
            // filter negaive values
            float diff = Math.Max(instant - C * E, 0);
            PushVal(diff);
            // find the average of only the positive values in dBuffer
            float dAvg = SpecAverage(dBuffer);
            // filter negative values
            float diff2 = Math.Max(diff - dAvg, 0);
            PushVar(diff2);
            // report false if it's been less than 'sensitivity'
            // milliseconds since the last true value
            if (DateTime.Now.Ticks - timer < sensitivity)
            {
                isOnset = false;
            }
            // if we've made it this far then we're allowed to set a new
            // value, so set it true if it deserves to be, restart the timer
            else if (diff2 > 0 && instant > 2)
            {
                isOnset = true;
                timer = DateTime.Now.Ticks;
            }
            // OMG it wasn't true!
            else
            {
                isOnset = false;
            }
            eBuffer[insertAt] = instant;
            dBuffer[insertAt] = diff;
            insertAt++;
            if (insertAt == eBuffer.Length)
                insertAt = 0;
        }

        //sizeof(float)
        private const int floatSize = 4;

        private void FEnergy(float[] samples)
        {
            spect.Forward(samples);
            float instant, E, V, C, diff, dAvg, diff2;
            for (int i = 0; i < feBuffer.Length; i++)
            {

                float[] feBufferRow = new float[sampleTime];
                Buffer.BlockCopy(feBuffer, floatSize * sampleTime * i, feBufferRow, 0, floatSize * sampleTime);
                float[] fdBufferRow = new float[sampleTime];
                Buffer.BlockCopy(fdBuffer, floatSize * sampleTime * i, fdBufferRow, 0, floatSize * sampleTime);

                instant = spect.GetAvg(i);
                E = Average(feBufferRow);
                V = Variance(feBufferRow, E);
                C = (-0.0025714f * V) + 1.5142857f;
                diff = Math.Max(instant - C * E, 0);
                dAvg = SpecAverage(fdBufferRow);
                diff2 = Math.Max(diff - dAvg, 0);
                if (DateTime.Now.Ticks - fTimer[i] < sensitivity)
                {
                    fIsOnset[i] = false;
                }
                else if (diff2 > 0)
                {
                    fIsOnset[i] = true;
                    fTimer[i] = DateTime.Now.Ticks;
                }
                else
                {
                    fIsOnset[i] = false;
                }
                feBuffer[i, insertAt] = instant;
                fdBuffer[i, insertAt] = diff;
            }
            insertAt++;
            if (insertAt == feBuffer.GetLength(0))
            {
                insertAt = 0;
            }
        }

        private void PushVal(float v)
        {
            if (valCnt == valGraph.Length)
            {
                valCnt = 0;
                valGraph = new float[valGraph.Length];
            }
            valGraph[valCnt] = v;
            valCnt++;
        }

        private void PushVar(float v)
        {
            if (varCnt == varGraph.Length)
            {
                varCnt = 0;
                varGraph = new float[varGraph.Length];
            }
            varGraph[varCnt] = v;
            varCnt++;
        }

        private float Average(float[] arr)
        {
            float avg = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                avg += arr[i];
            }
            avg /= arr.Length;
            return avg;
        }

        private float SpecAverage(float[] arr)
        {
            float avg = 0;
            float num = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] > 0)
                {
                    avg += arr[i];
                    num++;
                }
            }
            if (num > 0)
            {
                avg /= num;
            }
            return avg;
        }

        private float Variance(float[] arr, float val)
        {
            float V = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                V += (float)Math.Pow(arr[i] - val, 2);
            }
            V /= arr.Length;
            return V;
        }
    }
}
