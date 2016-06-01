using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace EmuNES
{
    public class AudioManager
    {
        public AudioManager(uint sampleRate, uint bufferSize)
        {
            soundPlayer = new SoundPlayer(new AudioStream(sampleRate, bufferSize));
        }

        public void Start()
        {
            soundPlayer.PlayLooping();
        }

        public void Stop()
        {
            soundPlayer.Stop();
        }

        private SoundPlayer soundPlayer;
    }
}
