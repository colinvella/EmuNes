using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio.Filtering
{
    /// <summary>
    /// First order IIR filter implementation
    /// </summary>
    public class FirstOrderFilter: Filter
    {
        public FirstOrderFilter(float b0, float b1, float a1)
        {
            this.b0 = b0;
            this.b1 = b1;
            this.a1 = a1;
            previousValue = previousY = 0.0f;
        }

        /// <summary>
        /// Applies the filter to the given value
        /// </summary>
        /// <param name="value">sample value to filter</param>
        /// <returns>filtered sample value</returns>
        public float Apply(float value)
        {
            float y = b0 * value + b1 * previousValue - a1 * previousY;
            previousY = y;
            previousValue = value;
            return y;
        }

        /// <summary>
        /// Creates a first order IIR low pass filter
        /// </summary>
        /// <param name="sampleRate">Signal sampling rate</param>
        /// <param name="cutoffFrequency">Cutoff frequency</param>
        /// <returns>filtered sample value</returns>
        public static FirstOrderFilter CreateLowPassFilter(float sampleRate, float cutoffFrequency)
        {
            float c = (float)(sampleRate / (Math.PI * cutoffFrequency));
            float a0i = 1f / (1f + c);

            return new FirstOrderFilter(a0i, a0i, (1f - c) * a0i);
        }

        /// <summary>
        /// Creates a first order IIR high pass filter 
        /// </summary>
        /// <param name="sampleRate">Signal sampling rate</param>
        /// <param name="cutoffFrequency">Cutoff frequency</param>
        /// <returns>filtered sample value</returns>
        public static FirstOrderFilter CreateHighPassFilter(float sampleRate, float cutoffFrequency)
        {
            float c = (float)(sampleRate / (Math.PI * cutoffFrequency));
            float a0i = 1f / (1f + c);
            float b = c * a0i;

            return new FirstOrderFilter(b, -b, (1f - c) * a0i);
        }

        private float b0;
        private float b1;
        private float a1;
        private float previousValue;
        private float previousY;
    }
}
