using NesCore.Audio.Filtering;
using NesCore.Audio.Generators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio
{
    public class Apu
    {
        public delegate void WriteSampleHandler(float sampleValue);

        public Apu()
        {
            pulse1 = new PulseGenerator(1);
            pulse2 = new PulseGenerator(2);
            triangle = new TriangleGenerator();
            noise = new NoiseGenerator();
            dmc = new DmcGenerator();

            filterChain = new FilterChain();
        }

        public byte Status
        {
            get
            {
                byte result = 0;
                if (pulse1.LengthValue > 0)
                    result |= 1;
                if (pulse2.LengthValue > 0)
                    result |= 2;
                if (triangle.LengthValue > 0)
                    result |= 4;
                if (noise.LengthValue > 0)
                    result |= 8;
                if (dmc.CurrentLength > 0)
                    result |= 16;
                return result;
            }
        }

        /// <summary>
        /// Sets the supported sample rate and configures the pass filters accordingly
        /// </summary>
        public float SampleRate
        {
            get { return sampleRate; }
            set
            {
                sampleRate = value;

                filterChain.Filters.Clear();
                filterChain.Filters.Add(FirstOrderFilter.CreateHighPassFilter(sampleRate, 90f));
                filterChain.Filters.Add(FirstOrderFilter.CreateHighPassFilter(sampleRate, 440f));
                filterChain.Filters.Add(FirstOrderFilter.CreateLowPassFilter(sampleRate, 14000f));
            }
        }

        /// <summary>
        /// Handler for writing output samples
        /// </summary>
        public WriteSampleHandler WriteSample { get; set; }

        /// <summary>
        /// Handler for triggering interrupt requests
        /// </summary>
        public Action TriggerIrq { get; set; }

        public float Output
        {
            get
            {
                byte pulseOutput1 = pulse1.Output;
                byte pulseOutput2 = pulse2.Output;
                float pulseOutput = pulseTable[pulseOutput1 + pulseOutput2];

                byte triangleOutput = triangle.Output;
                byte noiseOutput = noise.Output;
                byte dmcOutput = dmc.Output;
                float tndOutput = tndTable[3 * triangleOutput + 2 * noiseOutput + dmcOutput];

                return pulseOutput + tndOutput;
            }
        }

        public void Step()
        {
            ulong lastCycle = cycle;
            ++cycle;
            ulong nextCycle = cycle;

            StepTimer();

            int lastCycleFrame = (int)((double)lastCycle / FrameCounterRate);
            int nextCycleFrame = (int)((double)nextCycle / FrameCounterRate);

            if (lastCycleFrame != nextCycleFrame)
                StepFrameCounter();

            int lastCycleSample = (int)((double)lastCycle / SampleRate);
            int nextCycleSample = (int)((double)nextCycle / SampleRate);

            if (lastCycleSample != nextCycleSample)
            {
                float filteredOutput = filterChain.Apply(Output);
                WriteSample?.Invoke(filteredOutput);
            }
        }

        public void SaveState(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(cycle);
            binaryWriter.Write(framePeriod);
            binaryWriter.Write(frameValue);
            binaryWriter.Write(frameIrq);

            pulse1.SaveState(binaryWriter);
            pulse2.SaveState(binaryWriter);
            triangle.SaveState(binaryWriter);
            noise.SaveState(binaryWriter);
            dmc.SaveState(binaryWriter);
        }

        public void LoadState(BinaryReader binaryReader)
        {
            cycle = binaryReader.ReadUInt64();
            framePeriod = binaryReader.ReadByte();
            frameValue = binaryReader.ReadByte();
            frameIrq = binaryReader.ReadBoolean();

            pulse1.LoadState(binaryReader);
            pulse2.LoadState(binaryReader);
            triangle.LoadState(binaryReader);
            noise.LoadState(binaryReader);
            dmc.LoadState(binaryReader);
        }

        private void StepTimer()
        {
            if (cycle % 2 == 0)
            {
                pulse1.StepTimer();
                pulse2.StepTimer();
                noise.StepTimer();
                dmc.StepTimer();
            }
            triangle.StepTimer();
        }

        private void StepFrameCounter()
        {
            if (framePeriod == 4)
            {
                ++frameValue;
                frameValue %= 4;

                StepEnvelope();

                if (frameValue == 1)
                {
                    StepSweep();
                    StepLength();
                }
                else if (frameValue == 3)
                {
                    StepSweep();
                    StepLength();
                    TriggerIrq?.Invoke();
                }
            } else if (framePeriod == 5)
            {
                ++frameValue;
                frameValue %= 5;

                StepEnvelope();

                if (frameValue == 0 ||frameValue == 2)
                {
                    StepSweep();
                    StepLength();
                }
            }
        }

        private void StepEnvelope()
        {
            pulse1.StepEnvelope();
            pulse2.StepEnvelope();
            triangle.StepCounter();
            noise.StepEnvelope();
        }

        private void StepSweep()
        {
            pulse1.StepSweep();
            pulse2.StepSweep();
        }

        private void StepLength()
        {
            pulse1.StepLength();
            pulse2.StepLength();
            triangle.StepLength();
            noise.StepLength();
        }

        private PulseGenerator pulse1;
        private PulseGenerator pulse2;
        private TriangleGenerator triangle;
        private NoiseGenerator noise;
        private DmcGenerator dmc;

        private float sampleRate;
        private ulong cycle;
        private byte framePeriod;
        private byte frameValue;
        private bool frameIrq;
        private FilterChain filterChain;

        /// <summary>
        /// Builds the pulse and triangle/noise/dmc (tnd) tables
        /// </summary>
        static Apu()
        {
            for (int i = 0; i < 31; i++)
                pulseTable[i] = 95.52f / (8128.0f / (float)i + 100.0f);

            for (int i = 0; i < 203; i++)
                tndTable[i] = 163.67f / (24329.0f / (float)i + 100.0f);
        }

        private static readonly float[] pulseTable = new float[31];
        private static readonly float[] tndTable = new float[203];

        private const uint CpuFrequency = 1789773;
        private const double FrameCounterRate = CpuFrequency / 240.0;
    }
}
