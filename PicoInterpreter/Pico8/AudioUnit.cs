using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pico8_interpreter.Pico8
{
    public class AudioUnit<A>
    {
        public int sampleRate = 44100;
        public int channelCount = 4;
        public int samplesPerBuffer = 3000;
        public const int Polyphony = 4;

        public float[,] audioBuffer;

        public Action<float[,]> ConvertBufferCallback;

        private int ADDR_SFX = 0x3200;

        private MemoryUnit _memory;

        public Sfx[,] sfxs;

        public AudioUnit(ref MemoryUnit memory)
        {
            audioBuffer = new float[channelCount, samplesPerBuffer];

            _memory = memory;

            sfxs = new Sfx[channelCount, Polyphony];
        }

        public void UpdateAudio()
        {
            ClearBuffer();
            FillBuffer();

            ConvertBufferCallback(audioBuffer);
        }

        public object Sfx(int? n, int? channel = null, int? offset = null, int? length = null)
        {
            byte[] _sfx = new byte[68];
            Buffer.BlockCopy(_memory.ram, ADDR_SFX + 68 * n.Value, _sfx, 0, 68);

            sfxs[0, 0] = new Sfx(_sfx, ref audioBuffer, 0, sampleRate);
            sfxs[0, 0].Start();

            return null;
        }

        public object Music(int? n, int? fade_len, int? channel_mask)
        {
            return null;
        }

        public void FillBuffer()
        {
            foreach (var s in sfxs)
            {
                if (s != null)
                {
                    s.Update();
                }
            }
        }

        public void ClearBuffer()
        {
            for (int j = 0; j < channelCount; j += 1)
            {
                for (int i = 0; i < samplesPerBuffer; i++)
                {
                    audioBuffer[j, i] = 0;
                }
            }
        }
    }

    public class Oscillator
    {
        public Func<float, float, float>[] waveFuncMap;

        public Oscillator()
        {
            waveFuncMap = new Func<float, float, float>[]{
                Triangle,
                TiltedSaw,
                Sawtooth, 
                Square,
                Pulse,
                Organ, // Organ
                Noise, // Noise
                Phaser // Phaser
            };
        }

        public static float Sine(float frequency, float time)
        {
            return (float)Math.Sin(frequency * time * 2 * Math.PI);
        }

        public static float Square(float frequency, float time)
        {
            return Sine(frequency, time) >= 0 ? 1.0f : -1.0f;
        }
        public static float Pulse(float frequency, float time)
        {
            return ((time * frequency) % 1 < 0.3125 ? 1 : - 1) * 1.0f / 3.0f;
        }
        public static float TiltedSaw(float frequency, float time)
        {
            var t = (time * frequency) % 1;
            return (((t < 0.875) ? (t * 16 / 7)  : ((1 - t) * 16)) -1) * 0.7f;
        }
        public static float Sawtooth(float frequency, float time)
        {
            return (float)(2 * (time * frequency - Math.Floor(time * frequency + 0.5)));
        }

        public static float Triangle(float frequency, float time)
        {
            return (Math.Abs(((time*frequency) % 1) * 2 - 1) * 2.0f -   1.0f) * 0.7f;
        }

        public static float Organ(float frequency, float time)
        {
            var x = frequency * time * 4;
            return (float)((Math.Abs((x % 2) - 1) - 0.5f + (Math.Abs(((x * 0.5) % 2) - 1) - 0.5f) / 2.0f - 0.1f) * 0.7f);
        }

        public static float Phaser(float frequency, float time)
        {
            var x = frequency * time * 2;
            return (Math.Abs((x % 2) - 1) - 0.5f + (Math.Abs(((x * 127 / 128) % 2) - 1) - 0.5f) / 2) - 1.0f / 4.0f;
        }

        private float lastx = 0;
        private float sample = 0;
        private float tscale = util.NoteToFrequency(63) / 44100.0f;
        private Random random = new Random();
        public float Noise(float frequency, float time)
        {
            float scale = (frequency * time - lastx) / tscale;
            float lsample = sample;
            sample = (lsample + scale * ((float)random.NextDouble() * 2 - 1)) / (1.0f + scale);
            lastx = frequency * time;
            return Math.Min(Math.Max((lsample + sample) * 4.0f / 3.0f * (1.75f - scale), -1), 1) * 0.7f;
        }
    }

    public delegate float OscillatorDelegate(float frequency, float time);

    public class Sfx
    {
        public struct Note
        {
            public bool isCustom;
            public byte effect;
            public byte volume;
            public byte waveform;
            public byte pitch;
        }

        public Note[] notes;
        public float speed;
        public byte startLoop;
        public byte endLoop;

        private int _sampleRate;

        public bool isAlive { get; private set; }

        private float _time = 0.0f;
        private float _noteDuration = 0.0f;
        private float[,] _audioBuffer;
        private int _channel;

        private int _currentNote = 0;

        private Oscillator oscillator;

        public Sfx(byte[] _sfxData, ref float[,] audioBuffer, int channel, int sampleRate)
        {
            notes = new Note[32];
            _audioBuffer = audioBuffer;
            _channel = channel;

            speed = (float)_sfxData[65] / 128.0f;
            startLoop = _sfxData[66];
            endLoop = _sfxData[67];

            _sampleRate = sampleRate;

            Console.WriteLine($"header {_sfxData[64]} {_sfxData[65]} {_sfxData[66]} {_sfxData[67]}");

            for (int i = 0; i < _sfxData.Length - 4; i += 2)
            {
                byte lo = _sfxData[i];
                byte hi = _sfxData[i + 1];

                notes[i / 2].pitch = (byte)(lo & 0b00111111);
                notes[i / 2].waveform = (byte)(((lo & 0b11000000) >> 6) | ((hi & 0b1) << 2));
                notes[i / 2].volume = (byte)((hi & 0b00001110) >> 1);
                notes[i / 2].effect = (byte)((hi & 0b01110000) >> 4);
                notes[i / 2].isCustom = (byte)((hi & 0b10000000) >> 7) == 1;

                Console.WriteLine($"{i} {notes[i / 2].pitch} {notes[i / 2].waveform} {notes[i / 2].volume} {notes[i / 2].effect} {notes[i / 2].isCustom}");
            }

            oscillator = new Oscillator();
        }

        public void Update()
        {
            if (isAlive)
            {
                int samplesPerBuffer = _audioBuffer.GetLength(1);
                for (int i = 0; i < samplesPerBuffer; i++)
                {
                    if (_noteDuration >= speed)
                    {
                        _noteDuration = 0;
                        _currentNote += 1;
                        if (_currentNote >= notes.Length)
                        {
                            isAlive = false;
                            break;
                        }
                    }

                    float sample = oscillator.waveFuncMap[notes[_currentNote].waveform](util.NoteToFrequency(notes[_currentNote].pitch), _time);
                    _audioBuffer[_channel, i] += sample * notes[_currentNote].volume / 7;

                    _time += 1.0f / (float)_sampleRate;
                    _noteDuration += 1.0f / (float)_sampleRate;
                }
            }
        }

        public void Start() { isAlive = true; }
        public void Stop() { isAlive = false; }
    }
}
