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
            audioStreamCurrent = new AudioStream(sampleRate, bufferSize);
            audioStreamBackup = new AudioStream(sampleRate, bufferSize);


            soundPlayerCurrent = new SoundPlayer(audioStreamCurrent);
            soundPlayerCurrent.Load();
            soundPlayerBackup = new SoundPlayer(audioStreamBackup);
            soundPlayerBackup.Load();
        }

        public void Start()
        {
            running = true;
            RunAudioThread();
        }

        public void Stop()
        {
            running = false;
        }

        public void WriteSample(short sampleValue)
        {
            audioStreamCurrent.WriteSample(sampleValue);
            audioStreamBackup.WriteSample(sampleValue);
        }

        public void WriteSample(float sampleValue)
        {
            audioStreamCurrent.WriteSample(sampleValue);
            audioStreamBackup.WriteSample(sampleValue);
        }

        private void RunAudioThread()
        {
            Task.Factory.StartNew(() =>
            {
                while (running)
                {
                    SoundPlayer soundPlayerTemp = soundPlayerCurrent;
                    soundPlayerCurrent = soundPlayerBackup;
                    soundPlayerBackup = soundPlayerTemp;

                    AudioStream audioStreamTemp = audioStreamCurrent;
                    audioStreamCurrent = audioStreamBackup;
                    audioStreamBackup = audioStreamTemp;

                    audioStreamBackup.Position = 0;
                    soundPlayerBackup.Stream = audioStreamBackup;
                    soundPlayerBackup.LoadAsync();

                    //audioStreamCurrent.Position = 0;
                    soundPlayerCurrent.PlaySync();
                }
            });
        }

        private SoundPlayer soundPlayerCurrent;
        private SoundPlayer soundPlayerBackup;
        private AudioStream audioStreamCurrent;
        private AudioStream audioStreamBackup;
        private bool running;
    }
}
