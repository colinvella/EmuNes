using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Audio.Filtering
{
    /// <summary>
    /// Signal filtering interfaced
    /// </summary>
    public interface Filter
    {
        /// <summary>
        /// Applies the filter to the given value
        /// </summary>
        /// <param name="value">sample value to filter</param>
        /// <returns>filtered sample value</returns>
        float Apply(float value);
    }
}
