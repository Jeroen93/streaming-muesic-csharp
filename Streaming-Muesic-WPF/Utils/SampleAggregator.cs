using System;

namespace Streaming_Muesic_WPF.Utils
{
    internal class SampleAggregator : IDisposable
    {
        public static event EventHandler<DataAvailableEventArgs> DataAvailable;

        public int BufferLength { get; }

        public float[] Left { get; private set; }
        public float[] Right { get; private set; }
        public float[] Mix { get; private set; }

        private int position;
        private readonly float[] tempBufferLeft;
        private readonly float[] tempBufferRight;
        private readonly float[] tempBufferMix;

        public SampleAggregator(int bufferLength = 1024)
        {
            if (!IsPowerOfTwo(bufferLength))
            {
                throw new ArgumentException("Buffer length must be a power of two");
            }

            BufferLength = bufferLength;
            tempBufferLeft = tempBufferRight = tempBufferMix = new float[bufferLength];
        }

        public void Add(float valueL, float valueR)
        {
            tempBufferLeft[position] = valueL;
            tempBufferRight[position] = valueR;
            tempBufferMix[position] = (valueL + valueR) / 2;
            position++;

            if (position >= BufferLength)
            {
                position = 0;
                Array.Copy(tempBufferLeft, 0, Left, 0, BufferLength);
                Array.Copy(tempBufferRight, 0, Right, 0, BufferLength);
                Array.Copy(tempBufferMix, 0, Mix, 0, BufferLength);

                DataAvailable?.Invoke(this, new DataAvailableEventArgs { Left = Left, Right = Right });
            }
        }

        private bool IsPowerOfTwo(int x)
        {
            return x > 0 && (x & x - 1) == 0;
        }

        public void AddDataListener(EventHandler<DataAvailableEventArgs> handler)
        {
            DataAvailable += handler;
        }

        public void RemoveDataListener(EventHandler<DataAvailableEventArgs> handler)
        {
            DataAvailable -= handler;
        }

        public void Dispose()
        {
            DataAvailable = null;
        }
    }

    internal class DataAvailableEventArgs : EventArgs
    {
        public float[] Left { get; set; }
        public float[] Right { get; set; }
    }
}
