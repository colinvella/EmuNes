using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesCore.Storage.Hacks
{
    class PpuStatusSpinHack : Hack
    {
        public override byte Read(ushort address, byte originalValue)
        {
            if (originalValue == spinSequence[loopState])
            {
                ++loopState;
                loopState %= spinSequence.Length;
                ++loopCounter;
            }
            else
                loopCounter = 0;

            if (loopCounter > 10000 && originalValue == 0xF0)
                return 0xD0; // return BNE instead of BEQ

            return originalValue;
        }

        private int loopState;
        private int loopCounter;

        // Loop:
        // $862E $2C $02 $20  BIT $2002  
        // $8631 $F0 $FB      BEQ $FB     ; branch to Loop
        private readonly byte[] spinSequence = { 0x2C, 0x02, 0x20, 0xF0, 0xFB };
    }
}
