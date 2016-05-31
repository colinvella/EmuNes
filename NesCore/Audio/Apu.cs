using NesCore.Audio.Filtering;
using System;
using System.Collections.Generic;
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

        public double SampleRate { get; set; }
        public WriteSampleHandler WriteSample { get; set; }

        private PulseGenerator pulse1;
        private PulseGenerator pulse2;
        private TriangleGenerator triangle;
        private NoiseGenerator noise;
        private DmcGenerator dmc;

        /// <summary>
        /// Configures the filter chain with low and high pass filters
        /// applicable to the given sampling rate
        /// </summary>
        /// <param name="sampleRate">Sample rate for which to comoute filters</param>
        public void ConfigureFilterChain(float sampleRate)
        {
            filterChain.Filters.Clear();
            filterChain.Filters.Add(FirstOrderFilter.CreateHighPassFilter(sampleRate, 90f));
            filterChain.Filters.Add(FirstOrderFilter.CreateHighPassFilter(sampleRate, 440f));
            filterChain.Filters.Add(FirstOrderFilter.CreateLowPassFilter(sampleRate, 14000f));
        }

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
    }
}
