using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pico8_interpreter.Pico8
{
    public class AudioUnit<A>
    {
        public int sampleRate = 48000;
        public int channelCount = 4;
        public int samplesPerBuffer = 3000;
        public const int Polyphony = 4;

        public float[,] audioBuffer;

        public Action<float[,]> ConvertBufferCallback;

        private int ADDR_SFX = 0x3200;

        private MemoryUnit _memory;

        public Sfx[] sfxChannels, musicChannels;

        private byte _reservedChannelsMask = 0;

        private MusicPlayer musicPlayer;

        public AudioUnit(ref MemoryUnit memory)
        {
            audioBuffer = new float[channelCount, samplesPerBuffer];

            _memory = memory;
        }

        public void Init()
        {
            sfxChannels = new Sfx[channelCount];
            musicChannels = new Sfx[channelCount];
            musicPlayer = new MusicPlayer(ref _memory, ref audioBuffer, sampleRate);
        }

        public void UpdateAudio()
        {
            ClearBuffer();
            FillBuffer();
            musicPlayer.Update();

            ConvertBufferCallback(audioBuffer);
        }

        public object Sfx(int n, int? channel = -1, int? offset = null, int? length = null)
        {
            switch(n)
            {
                case -1:
                    sfxChannels[n] = null;
                    break;
                case -2:
                    if (sfxChannels[n] != null)
                        sfxChannels[n].loop = false;
                    break;
                default:

                    if (channel == -1)
                    {
                        channel = FindAvailableChannel();
                        if (channel == null)
                            return null;
                    }
                    else if(isChannelReserved(channel.Value))
                    {
                        return null;
                    }

                    byte[] _sfxData = new byte[68];
                    Buffer.BlockCopy(_memory.ram, ADDR_SFX + 68 * n, _sfxData, 0, 68);

                    sfxChannels[channel.Value] = new Sfx(_sfxData, n, ref audioBuffer, n, sampleRate);
                    sfxChannels[channel.Value].Start();
                    break;
            }

            return null;
        }

        public object Music(int? n, int? fade_len, int? channel_mask)
        {
            musicPlayer.Start(n.Value);
            return null;
        }

        public void FillBuffer()
        {
            for (int i = 0; i < channelCount; i += 1)
            {
                if (sfxChannels[i] != null && !sfxChannels[i].Update())
                {
                    sfxChannels[i] = null;
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

        private int? FindAvailableChannel()
        {
            for (int i = 0; i < channelCount; i += 1)
            {
                if (sfxChannels[i] == null && !isChannelReserved(i))
                    return i;
            }

            return null;
        }

        private bool isChannelReserved(int channel)
        {
            return ((1 << channel) & _reservedChannelsMask) != 0;
        }
    }

    public class Oscillator
    {
        public Func<float, float>[] waveFuncMap;

        public float sampleRate;
        private float _time;

        public Oscillator(float sampleRate)
        {
            waveFuncMap = new Func<float, float>[]{
                Triangle,
                TiltedSaw,
                Sawtooth, 
                Square,
                Pulse,
                Organ, // Organ
                Noise, // Noise
                Phaser // Phaser
            };

            this.sampleRate = sampleRate;
            tscale = util.NoteToFrequency(63) / sampleRate;
            _time = 0.0f;
        }

        public float Sine(float frequency)
        {
            _time += frequency / sampleRate;
            return (float)Math.Sin(_time * 2 * Math.PI);
        }

        public float Square(float frequency)
        {
            return Sine(frequency) >= 0 ? 1.0f : -1.0f;
        }
        public float Pulse(float frequency)
        {
            _time += frequency / sampleRate;
            return ((_time) % 1 < 0.3125 ? 1 : - 1) * 1.0f / 3.0f;
        }
        public float TiltedSaw(float frequency)
        {
            _time += frequency / sampleRate;
            var t = (_time) % 1;
            return (((t < 0.875) ? (t * 16 / 7)  : ((1 - t) * 16)) -1) * 0.7f;
        }
        public float Sawtooth(float frequency)
        {
            _time += frequency / sampleRate;
            return (float)(2 * (_time - Math.Floor(_time + 0.5)));
        }

        public float Triangle(float frequency)
        {
            _time += frequency / sampleRate;
            return (Math.Abs(((_time) % 1) * 2 - 1) * 2.0f -   1.0f) * 0.7f;
        }

        public float Organ(float frequency)
        {
            _time += frequency / sampleRate;
            var x = _time * 4;
            return (float)((Math.Abs((x % 2) - 1) - 0.5f + (Math.Abs(((x * 0.5) % 2) - 1) - 0.5f) / 2.0f - 0.1f) * 0.7f);
        }

        public float Phaser(float frequency)
        {
            _time += frequency / sampleRate;
            var x = _time * 2;
            return (Math.Abs((x % 2) - 1) - 0.5f + (Math.Abs(((x * 127 / 128) % 2) - 1) - 0.5f) / 2) - 1.0f / 4.0f;
        }

        private float lastx = 0;
        private float sample = 0;
        private float tscale;
        private Random random = new Random();
        public float Noise(float frequency)
        {
            _time += frequency / sampleRate;
            float scale = (_time - lastx) / tscale;
            float lsample = sample;
            sample = (lsample + scale * ((float)random.NextDouble() * 2 - 1)) / (1.0f + scale);
            lastx = _time;
            return Math.Min(Math.Max((lsample + sample) * 4.0f / 3.0f * (1.75f - scale), -1), 1) * 0.7f;
        }
    }

    public class MusicPlayer
    {
        public struct ChannelData
        {
            public bool isSilent;
            public byte sfxIndex;
        }
        public struct PatternData
        {
            public ChannelData[] channels;
            public bool loopStart;
            public bool loopEnd;
            public bool shouldStop;
        }
       
        private PatternData[] patternData;

        private Sfx[] sfxs;
        private byte[] _ram;
        private int _patternIndex;

        private float[,] _audioBuffer;
        private int _sampleRate;

        public bool isPlaying { get; private set; }
        public MusicPlayer(ref MemoryUnit memory, ref float[,] audioBuffer, int sampleRate)
        {
            sfxs = new Sfx[4] { null, null, null, null };
            isPlaying = false;
            _ram = memory.ram;

            _audioBuffer = audioBuffer;
            _sampleRate = sampleRate;

            patternData = new PatternData[64];
            for(int i = 0; i < patternData.Length; i += 1)
            {
                byte[] vals = { _ram[i * 4 + 0 + util.ADDR_SONG],
                                _ram[i * 4 + 1 + util.ADDR_SONG],
                                _ram[i * 4 + 2 + util.ADDR_SONG],
                                _ram[i * 4 + 3 + util.ADDR_SONG] };

                if ((vals[0] & 0x80) == 0x80)
                {
                    patternData[i].loopStart = true;
                }
                if ((vals[1] & 0x80) == 0x80)
                {
                    patternData[i].loopEnd = true;
                }
                if ((vals[2] & 0x80) == 0x80)
                {
                    patternData[i].shouldStop = true;
                }

                patternData[i].channels = new ChannelData[4];
                for (int j = 0; j < 4; j += 1)
                {
                    patternData[i].channels[j] = new ChannelData();
                    if ((vals[j] & 0b01000000) != 0)
                    {
                        patternData[i].channels[j].isSilent = true;
                    }
                    patternData[i].channels[j].sfxIndex = (byte)(vals[j] & 0b00111111);
                }
            }
        }

        public void Update()
        {
            if (!isPlaying || _patternIndex > 63 || _patternIndex < 0)
            {
                return;
            }

            for (int i = 0; i < 4; i += 1)
            {
                if (sfxs[i] == null)
                    continue;

                if (!sfxs[i].Update())
                {
                    sfxs[i] = null;
                    continue;
                }
            }

            if (isPatternDone())
            {
                _patternIndex += 1;
                SetUpPattern();
            }

        }

        private bool isPatternDone()
        {
            for (int i = 0; i < 4; i += 1)
            {
                if (sfxs[i] == null)
                    continue;

                return false;
            }

            return true;
        }

        private void SetUpPattern()
        {
            for (int i = 0; i < 4; i += 1)
            {
                if (patternData[_patternIndex].channels[i].isSilent)
                {
                    sfxs[i] = null;
                    continue;
                }

                byte[] _sfxData = new byte[68];
                Buffer.BlockCopy(_ram, util.ADDR_SFX + 68 * patternData[_patternIndex].channels[i].sfxIndex, _sfxData, 0, 68);
                sfxs[i] = new Sfx(_sfxData, patternData[_patternIndex].channels[i].sfxIndex, ref _audioBuffer, i, _sampleRate);
                sfxs[i].Start();
            }
        }
        public void Start(int n) {
            isPlaying = true;
            _patternIndex = n;

            SetUpPattern();
        }
        public void Stop() { isPlaying = false; }
    }

    public class Sfx
    {
        public struct P8Note
        {
            public bool isCustom;
            public byte effect;
            public byte volume;
            public byte waveform;
            public byte pitch;
        }

        public P8Note[] notes;
        public float speed;
        public byte startLoop;
        public byte endLoop;

        public bool loop = true;

        private int _sampleRate;

        public bool isAlive { get; private set; }
        public bool isActive { get; private set; }
        public int sfxIndex { get; private set; }

        private float[,] _audioBuffer;
        private int _channel;

        private int _currentNote = 0;

        private Oscillator oscillator;

        private Queue<Note> notesToPlay;

        public int audioBufferIndex { get; private set; }

        public Sfx(byte[] _sfxData, int _sfxIndex, ref float[,] audioBuffer, int channel, int sampleRate)
        {
            notes = new P8Note[32];
            _audioBuffer = audioBuffer;
            _channel = channel;

            speed = _sfxData[65] / 127.0f;
            startLoop = _sfxData[66];
            endLoop = _sfxData[67];

            _sampleRate = sampleRate;
            sfxIndex = _sfxIndex;

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

            oscillator = new Oscillator(sampleRate);
            notesToPlay = new Queue<Note>();

            audioBufferIndex = 0;

            isActive = true;
        }

        public bool Update()
        {
            if (!isAlive)
            {
                return false;
            }

            audioBufferIndex = 0;
            int samplesPerBuffer = _audioBuffer.GetLength(1);
            while (audioBufferIndex < samplesPerBuffer)
            {
                // Queue next notes that need to be played. In case there are no more notes, stop everything.
                if (notesToPlay.Count == 0)
                {
                    QueueNextNotes();
                    if (notesToPlay.Count == 0)
                    {
                        isAlive = false;
                        return false;
                    }
                }

                Note next = notesToPlay.Peek();
                audioBufferIndex = next.Process(audioBufferIndex, isActive);

                if (audioBufferIndex < samplesPerBuffer)
                    notesToPlay.Dequeue();
            }

            return true;
        }

        private void QueueNextNotes()
        {
            if (_currentNote >= notes.Length)
            {
                return;
            }

            P8Note nextNote = notes[_currentNote];

            switch(nextNote.effect)
            {
                case 0:
                    ProcessNoteNoEffect(nextNote);
                    break;
                case 1:
                    ProcessNoteSlide(nextNote);
                    break;
                case 2:
                    ProcessNoteVibrato(nextNote);
                    break;
                case 3:
                    ProcessNoteDrop(nextNote);
                    break;
                case 4:
                    ProcessNoteFadeIn(nextNote);
                    break;
                case 5:
                    ProcessNoteFadeOut(nextNote);
                    break;
                case 6:
                    ProcessNoteArpeggioFast(nextNote);
                    break;
                case 7:
                    ProcessNoteArpeggioSlow(nextNote);
                    break;
            }

            // If sfx has loop defined, process it. Otherwise keep incrementing note index.
            if (loop && startLoop < endLoop && _currentNote == endLoop-1)
            {
                _currentNote = startLoop;
            }
            else
            {
                _currentNote += 1;
            }
        }

        private void ProcessNoteNoEffect(P8Note note)
        {
            Note noteToPlay = new Note(ref _audioBuffer, _channel, _sampleRate, ref oscillator, speed, note.volume, note.waveform, note.pitch, note.pitch, 0, 0);
            notesToPlay.Enqueue(noteToPlay);
        }

        private void ProcessNoteSlide(P8Note note)
        {
            int pitchFrom = _currentNote == 0 ? 32 : notes[_currentNote - 1].pitch;
            Note noteToPlay = new Note(ref _audioBuffer, _channel, _sampleRate, ref oscillator, speed, note.volume, note.waveform, note.pitch, pitchFrom, 0, 0);
            notesToPlay.Enqueue(noteToPlay);
        }

        private void ProcessNoteVibrato(P8Note note)
        {
            Note noteToPlay = new Note(ref _audioBuffer, _channel, _sampleRate, ref oscillator, speed, note.volume, note.waveform, note.pitch, note.pitch, 0, 0, true);
            notesToPlay.Enqueue(noteToPlay);
        }

        private void ProcessNoteDrop(P8Note note)
        {
            Note noteToPlay = new Note(ref _audioBuffer, _channel, _sampleRate, ref oscillator, speed, note.volume, note.waveform, 0, note.pitch, 0, 0);
            notesToPlay.Enqueue(noteToPlay);
        }

        private void ProcessNoteFadeIn(P8Note note)
        {
            Note noteToPlay = new Note(ref _audioBuffer, _channel, _sampleRate, ref oscillator, speed, note.volume, note.waveform, note.pitch, note.pitch, 95, 5);
            notesToPlay.Enqueue(noteToPlay);
        }

        private void ProcessNoteFadeOut(P8Note note)
        {
            Note noteToPlay = new Note(ref _audioBuffer, _channel, _sampleRate, ref oscillator, speed, note.volume, note.waveform, note.pitch, note.pitch, 5, 95);
            notesToPlay.Enqueue(noteToPlay);
        }

        private void ProcessNoteArpeggioFast(P8Note note)
        {
            Note noteToPlay = new Note(ref _audioBuffer, _channel, _sampleRate, ref oscillator, speed, note.volume, note.waveform, note.pitch, note.pitch, 0, 0);
            notesToPlay.Enqueue(noteToPlay);
        }

        private void ProcessNoteArpeggioSlow(P8Note note)
        {
            Note noteToPlay = new Note(ref _audioBuffer, _channel, _sampleRate, ref oscillator, speed, note.volume, note.waveform, note.pitch, note.pitch, 0, 0);
            notesToPlay.Enqueue(noteToPlay);
        }

        public void Start() { isAlive = true; }
        public void Stop() { isAlive = false; }
    }

    public class Note
    {
        private float[,] _audioBuffer;
        private int _channel;
        private float _duration;
        private float _fadeIn;
        private float _fadeOut;

        private float _timePassed;
        private int _sampleRate;

        public bool isCustom;
        public float targetVolume;
        public byte waveform;
        public byte pitch;

        private bool _vibrato;

        private Oscillator _oscillator;

        private float _volume;

        private int _pitchFrom;

        public Note(ref float[,] audioBuffer, int channel, int sampleRate, ref Oscillator oscillator, float duration, byte volume, byte waveform, byte pitch, int pitchFrom = -1, float fadeIn = 1, float fadeOut = 1, bool vibrato = false)
        {
            _audioBuffer = audioBuffer;
            _channel = channel;

            _duration = duration;
            _sampleRate = sampleRate;

            _fadeIn = fadeIn * duration / 100.0f;
            _fadeOut = fadeOut * duration / 100.0f;

            _timePassed = 0.0f;
            _volume = 1;

            isCustom = waveform > 7;
            targetVolume = volume / 7.0f;
            this.waveform = waveform;
            this.pitch = pitch;

            if (pitchFrom == -1)
                _pitchFrom = pitch;
            else
                _pitchFrom = pitchFrom;

            _oscillator = oscillator;

            _vibrato = vibrato;
        }

        public int Process(int bufferOffset = 0, bool writeToBuffer = true)
        {
            int samplesPerBuffer = _audioBuffer.GetLength(1);
            for (int i = bufferOffset; i < samplesPerBuffer; i++)
            {
                if (writeToBuffer)
                {
                    if (_timePassed < _fadeIn)
                    {
                        _volume = util.Lerp(0, targetVolume, _timePassed / _fadeIn);
                    }
                    else if (_timePassed > _duration - _fadeOut)
                    {
                        _volume = util.Lerp(targetVolume, 0, (_timePassed - (_duration - _fadeOut)) / _fadeOut);
                    }

                    if (_timePassed >= _duration)
                    {
                        return i;
                    }

                    float freq;
                    if (_vibrato)
                    {
                        freq = util.Lerp(util.NoteToFrequency(pitch), util.NoteToFrequency(pitch + 0.5f), (float)Math.Sin(_timePassed * 2 * Math.PI * 8));
                    }
                    else
                    {
                        freq = util.Lerp(util.NoteToFrequency(_pitchFrom), util.NoteToFrequency(pitch), _timePassed / _duration);
                    }

                    float sample = _oscillator.waveFuncMap[waveform](freq);
                    _audioBuffer[_channel, i] += sample * _volume;
                }

                _timePassed += 1.0f / _sampleRate;
            }

            return samplesPerBuffer;
        }
    }
}
