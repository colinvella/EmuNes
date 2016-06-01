using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmuNES
{
    public class AudioStream: Stream
    {
        public AudioStream(uint sampleRate, uint bufferSize)
        {
            this.sampleRate = sampleRate;
            this.numSamples = bufferSize;

            ushort numChannels = 1;
            ushort bitsPerSample = 16;
            uint subChunk2Size = numSamples * numChannels * bitsPerSample / 8;
            uint chunkSize = 36 + subChunk2Size;
            uint subChunk1Size = 16;
            ushort audioFormat = 1;
            uint byteRate = sampleRate * numChannels * bitsPerSample / 8;
            this.blockAlign = (ushort)(numChannels * bitsPerSample / 8);

            this.data = new byte[44 + subChunk2Size];
            this.position = 0;

            // wave header
            WriteAsciiBytes("RIFF"); // chuck id
            WriteUInt(chunkSize);
            WriteAsciiBytes("WAVE"); // audio format
            WriteAsciiBytes("fmt "); // fmt subchunk id
            WriteUInt(subChunk1Size); // 16 for PCM
            WriteUShort(audioFormat); // pcm
            WriteUShort(numChannels); // mono
            WriteUInt(sampleRate); // 44100Hz
            WriteUInt(byteRate);
            WriteUShort(blockAlign);
            WriteUShort(bitsPerSample);
            WriteAsciiBytes("data"); // subchunk 2 id
            WriteUInt(subChunk2Size);

            // data part used for audio mixing
            bufferStart = Position;
            Write(new byte[subChunk2Size], 0, (int)subChunk2Size);

            // reset position for sound player
            position = 0;
        }

        public void WriteSample(short sampleValue)
        {
            // sample index within wave
            long index = bufferStart + samplePosition * blockAlign;

            // get sample
            //            short currentValue = BitConverter.ToInt16(data, (int)index);

            // mix samples
            //            int mixedValue = currentValue + sampleValue;

            // clamp to range
            //            mixedValue = Math.Min(mixedValue, short.MaxValue);
            //            mixedValue = Math.Max(mixedValue, short.MinValue);

            //           short clampedValue = (short)mixedValue;
            //          byte[] bytes = BitConverter.GetBytes(clampedValue);

            byte[] bytes = BitConverter.GetBytes(sampleValue);
            Array.Copy(bytes, 0, data, index, 2);

            ++samplePosition;
            samplePosition %= numSamples;
        }

        public void WriteSample(float sampleValue)
        {
            WriteSample((short)(sampleValue * short.MaxValue));
        }
            
        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            lock(positionLock)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin: position = offset; break;
                    case SeekOrigin.Current: position += offset; break;
                    case SeekOrigin.End: position = data.Length + offset; break;
                }
                return position;
            }
        }

        public override void SetLength(long value)
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (positionLock)
            {
                if (position + count > data.Length)
                    count = (int)(data.Length - position);
                Array.Copy(data, position, buffer, offset, count);
                position += count;
                return count;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (positionLock)
            {
                Array.Copy(buffer, offset, data, position, count);
                position += count;
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return data.Length; }
        }

        public override long Position
        {
            get
            {
                lock (positionLock)
                {
                    return position;
                }
            }
            set
            {
                lock (positionLock)
                {
                    position = value;
                }
            }
        }

        private void WriteAsciiBytes(string value)
        {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        private void WriteUInt(uint value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        private void WriteUShort(ushort value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Write(bytes, 0, bytes.Length);
        }

        // base stream fields
        private byte[] data;
        private long position;
        private object positionLock = new object();

        // audio stream fields
        private uint sampleRate;
        private uint numSamples;
        private long bufferStart;
        private long samplePosition;
        private ushort blockAlign;
    }
}
