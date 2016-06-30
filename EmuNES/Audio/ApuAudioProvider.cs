using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNes.Audio
{
    public class ApuAudioProvider : WaveProvider32
    {
        public ApuAudioProvider()
        {
            cyclicBuffer = new float[4096];
            readIndex = writeIndex = 0;
            Enabled = true;
        }

        public bool Enabled { get; set; }

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            lock (queueLock)
            {
                if (!Enabled || size == 0)
                {
                    buffer[offset] = 0;
                    return 1;
                }

                sampleCount = Math.Min(sampleCount, size);
                
                for (int n = 0; n < sampleCount; n++)
                {
                    buffer[n + offset] = cyclicBuffer[readIndex++];
                    readIndex %= cyclicBuffer.Length;
                    --size;
                }
                return sampleCount;
            }
        }

        public void Queue(float[] sampleValues)
        {
            lock (queueLock)
            {
                for (int index = 0; index < sampleValues.Length; index++)
                {
                    if (size >= cyclicBuffer.Length)
                        return;

                    cyclicBuffer[writeIndex] = sampleValues[index];
                    ++writeIndex;
                    writeIndex %= cyclicBuffer.Length;
                    ++size;
                }
            }
        }

        private float[] cyclicBuffer = new float[8192];
        private int readIndex;
        private int writeIndex;
        private int size;
        private object queueLock = new object();
    }

}
