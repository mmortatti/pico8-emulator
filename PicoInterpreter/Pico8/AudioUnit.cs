using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace pico8_interpreter.Pico8
{
    public class AudioUnit<A>
    {
        public int sampleRate = 44100;
        public int channelCount = 4;
        public int samplesPerBuffer = 2000;

        public float[,] audioBuffer;

        public Action<float[,]> ConvertBufferCallback;

        private double _time = 0.0f;

        public AudioUnit()
        {
            audioBuffer = new float[channelCount, samplesPerBuffer];
        }

        public void UpdateAudio()
        {
            FillBuffer(0);

            ConvertBufferCallback(audioBuffer);
        }

        public double SineWave(double time, double frequency)
        {
            return Math.Sin(time * 2 * Math.PI * frequency);
        }

        private void FillBuffer(int channel)
        {

            for (int i = 0; i < samplesPerBuffer; i++)
            {
                // Here is where you sample your wave function
                audioBuffer[channel, i] = (float)SineWave(_time, 440);

                // Advance time passed since beginning
                // Since the amount of samples in a second equals the chosen SampleRate
                // Then each sample should advance the time by 1 / SampleRate
                _time += 1.0 / sampleRate;
            }
        }
    }
}
