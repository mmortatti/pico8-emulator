namespace Pico8_Emulator
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the PICO-8s AudioUnit<see cref="AudioUnit" />
    /// </summary>
    public class AudioUnit
    {
        /// <summary>
        /// How many samples per second is retrieved.
        /// </summary>
        public int sampleRate = 48000;

        /// <summary>
        /// Defines the channelCount. Used for sfx and music playing.
        /// </summary>
        public int channelCount = 4;

        /// <summary>
        /// Gets or sets the samplesPerBuffer, i.e. the size of the requested audio buffer.
        /// </summary>
        public int samplesPerBuffer
        {
            get
            {
                return audioBuffer.Count;
            }
            set
            {
                audioBuffer.Clear();
                audioBuffer.Capacity = value;
                while (audioBuffer.Count < audioBuffer.Capacity)
                    audioBuffer.Add(0);
            }
        }

        /// <summary>
        /// Defines the audioBuffer, i.e. collection of audio values between -1.0 and 1.0
        /// </summary>
        private List<float> audioBuffer;

        /// <summary>
        /// Array to return to RequestBuffer() caller.
        /// </summary>
        public float[] externalAudioBuffer;

        /// <summary>
        /// Reference to the memory unit so we can retrieve sfx information.
        /// </summary>
        private MemoryUnit _memory;

        /// <summary>
        /// sfx and music channel definition.
        /// </summary>
        public Sfx[] sfxChannels, musicChannels;

        /// <summary>
        /// Object to process music playing.
        /// </summary>
        private MusicPlayer musicPlayer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioUnit"/> class.
        /// </summary>
        /// <param name="memory">memory unit reference<see cref="MemoryUnit"/></param>
        public AudioUnit(ref MemoryUnit memory)
        {
            audioBuffer = new List<float>();
            samplesPerBuffer = 4000;
            externalAudioBuffer = new float[samplesPerBuffer];

            _memory = memory;
        }

        /// <summary>
        /// Initializes audio unit.
        /// </summary>
        public void Init()
        {
            sfxChannels = new Sfx[channelCount];
            musicChannels = new Sfx[channelCount];
            musicPlayer = new MusicPlayer(ref _memory, ref audioBuffer, sampleRate);
        }

        /// <summary>
        /// Requests next values for audio playing
        /// </summary>
        /// <param name="requestedSize">The size of the buffer to return<see cref="int"/></param>
        /// <returns>The final buffer<see cref="float[]"/></returns>
        public float[] RequestBuffer(int requestedSize = -1)
        {
            // Set the correct size that we want to extract.
            if (requestedSize != -1 && requestedSize != samplesPerBuffer)
            {
                samplesPerBuffer = requestedSize;
                externalAudioBuffer = new float[samplesPerBuffer];
            }

            ClearBuffer();
            FillBuffer();
            CompressBuffer();

            Buffer.BlockCopy(audioBuffer.ToArray(), 0, externalAudioBuffer, 0, sizeof(float) * samplesPerBuffer);
            return externalAudioBuffer;
        }

        /// <summary>
        /// play sfx n on channel (0..3) from note offset (0..31) for length notes
		/// n -1 to stop sound on that channel
        /// n -2 to release sound on that channel from looping
        /// Any music playing on the channel will be halted
        /// offset in number of notes(0..31)

        /// channel -1 (default) to automatically choose a channel that is not being used
        /// channel -2 to stop the sound from playing on any channel
        /// </summary>
        /// <param name="n">The sfx number<see cref="int"/></param>
        /// <param name="channel">The channel to play the sfx in<see cref="int?"/></param>
        /// <param name="offset">The offset note to start playing from<see cref="int?"/></param>
        /// <param name="length">The length of the sfx to play<see cref="int?"/></param>
        /// <returns>The <see cref="object"/></returns>
        public object Sfx(int n, int? channel = -1, int? offset = 0, int? length = 32)
        {
            switch (n)
            {
                case -1:
                    if (channel == -1)
                    {
                        StopAllChannels();
                        break;
                    }

                    if (channel < 0 || channel >= channelCount)
                        break;

                    sfxChannels[channel.Value] = null;
                    break;
                case -2:
                    if (channel.Value < 0 || channel.Value >= channelCount)
                        break;

                    if (sfxChannels[channel.Value] != null)
                        sfxChannels[channel.Value].loop = false;
                    break;
                default:

                    // If sound is already playing, stop it.
                    int? index = FindSoundOnAChannel(n);
                    if (index != null)
                    {
                        sfxChannels[index.Value] = null;
                    }

                    if (channel == -1)
                    {
                        channel = FindAvailableChannel();
                        if (channel == null)
                            break;
                    }

                    if (channel == -2)
                    {
                        break;
                    }

                    byte[] _sfxData = new byte[68];
                    Buffer.BlockCopy(_memory.ram, util.ADDR_SFX + 68 * n, _sfxData, 0, 68);

                    Oscillator osc = new Oscillator(sampleRate);
                    sfxChannels[channel.Value] = new Sfx(_sfxData, n, ref audioBuffer, ref osc, sampleRate);
                    sfxChannels[channel.Value].currentNote = offset.Value;
                    sfxChannels[channel.Value].lastIndex = offset.Value + length.Value - 1;
                    sfxChannels[channel.Value].Start();
                    break;
            }

            return null;
        }

        /// <summary>
        /// play music starting from pattern n (0..63)
		/// n -1 to stop music
        /// fade_len in ms(default: 0)
        /// </summary>
        /// <param name="n">The number of the music pattern to start playing<see cref="int?"/></param>
        /// <param name="fade_len">The length of the fade in effect in milliseconds<see cref="int?"/></param>
        /// <param name="channel_mask">The bitmask for reserving music channels. Not used since music and sfx are played on different channels.<see cref="int?"/></param>
        /// <returns>The <see cref="object"/></returns>
        public object Music(int? n, int? fade_len = null, int? channel_mask = null)
        {
            musicPlayer.Start(n.Value);
            return null;
        }

        /// <summary>
        /// Fills sfx and music buffer with new values.
        /// </summary>
        public void FillBuffer()
        {
            for (int i = 0; i < channelCount; i += 1)
            {
                if (sfxChannels[i] != null && !sfxChannels[i].Update())
                {
                    sfxChannels[i] = null;
                }
            }

            musicPlayer.Update();
        }

        /// <summary>
        /// Sets all buffer values to zero.
        /// </summary>
        public void ClearBuffer()
        {
            for (int i = 0; i < samplesPerBuffer; i++)
            {
                audioBuffer[i] = 0;
            }
        }

        /// <summary>
        /// Compresses buffer values so that it stays between the range [-1, 1]
        /// </summary>
        public void CompressBuffer()
        {
            for (int i = 0; i < samplesPerBuffer; i++)
            {
                audioBuffer[i] = (float)Math.Tanh(audioBuffer[i]);
            }
        }

        /// <summary>
        /// Finds the first available channel to play
        /// </summary>
        /// <returns>The index of the first available channel<see cref="int?"/></returns>
        private int? FindAvailableChannel()
        {
            for (int i = 0; i < channelCount; i += 1)
            {
                if (sfxChannels[i] == null)
                    return i;
            }

            return null;
        }

        /// <summary>
        /// Finds a specific sound index on a channel.
        /// </summary>
        /// <param name="n">The sound index<see cref="int"/></param>
        /// <returns>The channel index where the sound is playing in. Null if it is not playing on any channels<see cref="int?"/></returns>
        private int? FindSoundOnAChannel(int n)
        {
            for (int i = 0; i < channelCount; i += 1)
            {
                if (sfxChannels[i] != null && sfxChannels[i].sfxIndex == n)
                    return i;
            }

            return null;
        }

        /// <summary>
        /// Stops sound from playing in a channel
        /// </summary>
        /// <param name="index">The index of the channel<see cref="int"/></param>
        private void StopChannel(int index)
        {
            sfxChannels[index] = null;
        }

        /// <summary>
        /// Stops sound from playing in all channels.
        /// </summary>
        private void StopAllChannels()
        {
            for (int i = 0; i < sfxChannels.Length; i += 1)
            {
                StopChannel(i);
            }
        }
    }

    /// <summary>
    /// Defines all PICO-8s wave functions.<see cref="Oscillator" />
    /// </summary>
    public class Oscillator
    {
        /// <summary>
        /// Maps the waveform number in P8 to the actual waveform function.
        /// </summary>
        public Func<float, float>[] waveFuncMap;

        /// <summary>
        /// How many samples per second is retrieved.
        /// </summary>
        public float sampleRate;

        /// <summary>
        /// Tracks how much time has passed after each audio sample request.
        /// </summary>
        private float _time;

        /// <summary>
        /// Initializes a new instance of the <see cref="Oscillator"/> class.
        /// </summary>
        /// <param name="sampleRate">How many samples per second is retrieved.<see cref="float"/></param>
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

        /// <summary>
        /// Sine wave.
        /// </summary>
        /// <param name="frequency">The frequency of the wave<see cref="float"/></param>
        /// <returns>The sample value<see cref="float"/></returns>
        public float Sine(float frequency)
        {
            _time += frequency / sampleRate;
            return (float)Math.Sin(_time * 2 * Math.PI);
        }

        /// <summary>
        /// Square wave.
        /// </summary>
        /// <param name="frequency">The frequency of the wave<see cref="float"/></param>
        /// <returns>The sample value<see cref="float"/></returns>
        public float Square(float frequency)
        {
            return Sine(frequency) >= 0 ? 1.0f : -1.0f;
        }

        /// <summary>
        /// Pulse Wave 
        /// </summary>
        /// <param name="frequency">The frequency of the wave<see cref="float"/></param>
        /// <returns>The sample value<see cref="float"/></returns>
        public float Pulse(float frequency)
        {
            _time += frequency / sampleRate;
            return ((_time) % 1 < 0.3125 ? 1 : -1) * 1.0f / 3.0f;
        }

        /// <summary>
        /// TiltedSaw Wave
        /// </summary>
        /// <param name="frequency">The frequency of the wave<see cref="float"/></param>
        /// <returns>The sample value<see cref="float"/></returns>
        public float TiltedSaw(float frequency)
        {
            _time += frequency / sampleRate;
            var t = (_time) % 1;
            return (((t < 0.875) ? (t * 16 / 7) : ((1 - t) * 16)) - 1) * 0.7f;
        }

        /// <summary>
        /// Sawtooth Wave
        /// </summary>
        /// <param name="frequency">The frequency of the wave<see cref="float"/></param>
        /// <returns>The sample value<see cref="float"/></returns>
        public float Sawtooth(float frequency)
        {
            _time += frequency / sampleRate;
            return (float)(2 * (_time - Math.Floor(_time + 0.5)));
        }

        /// <summary>
        /// Triangle Wave
        /// </summary>
        /// <param name="frequency">The frequency of the wave<see cref="float"/></param>
        /// <returns>The sample value<see cref="float"/></returns>
        public float Triangle(float frequency)
        {
            _time += frequency / sampleRate;
            return (Math.Abs(((_time) % 1) * 2 - 1) * 2.0f - 1.0f) * 0.7f;
        }

        /// <summary>
        /// Organ Wave
        /// </summary>
        /// <param name="frequency">The frequency of the wave<see cref="float"/></param>
        /// <returns>The sample value<see cref="float"/></returns>
        public float Organ(float frequency)
        {
            _time += frequency / sampleRate;
            var x = _time * 4;
            return (float)((Math.Abs((x % 2) - 1) - 0.5f + (Math.Abs(((x * 0.5) % 2) - 1) - 0.5f) / 2.0f - 0.1f) * 0.7f);
        }

        /// <summary>
        /// Phaser Wave
        /// </summary>
        /// <param name="frequency">The frequency of the wave<see cref="float"/></param>
        /// <returns>The sample value<see cref="float"/></returns>
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

        /// <summary>
        /// White Noise Effect
        /// </summary>
        /// <param name="frequency">The frequency of the wave<see cref="float"/></param>
        /// <returns>The sample value<see cref="float"/></returns>
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

    /// <summary>
    /// Defines the <see cref="MusicPlayer" />
    /// </summary>
    public class MusicPlayer
    {
        /// <summary>
        /// Defines the <see cref="ChannelData" />. Used to keep channel information.
        /// </summary>
        public struct ChannelData
        {
            /// <summary>
            /// Defines if the channel is silent or not.
            /// </summary>
            public bool isSilent;

            /// <summary>
            /// Defines the sound effect index that is being played on that channel.
            /// </summary>
            public byte sfxIndex;
        }

        /// <summary>
        /// Defines the <see cref="PatternData" />. Used to keep pattern information.
        /// </summary>
        public struct PatternData
        {
            /// <summary>
            /// Defines channel data for all available channels
            /// </summary>
            public ChannelData[] channels;

            /// <summary>
            /// Defines if this pattern is the start of a loop.
            /// </summary>
            public bool loopStart;

            /// <summary>
            /// Defines if this pattern is the end of a loop.
            /// </summary>
            public bool loopEnd;

            /// <summary>
            /// Defines if this pattern is the last that should be played.
            /// </summary>
            public bool shouldStop;
        }

        /// <summary>
        /// Defines pattern data for all available patterns.
        /// </summary>
        private PatternData[] patternData;

        /// <summary>
        /// Defines the sfxs that are played in a pattern.
        /// </summary>
        private Sfx[] sfxs;

        /// <summary>
        /// RAM array to get sfx information.
        /// </summary>
        private byte[] _ram;

        /// <summary>
        /// Defines the currently playing pattern index.
        /// </summary>
        private int _patternIndex;

        /// <summary>
        /// Defines the _audioBuffer
        /// </summary>
        private List<float> _audioBuffer;

        /// <summary>
        /// Defines the _sampleRate
        /// </summary>
        private int _sampleRate;

        /// <summary>
        /// Defines the _referenceSfx
        /// </summary>
        private Sfx _referenceSfx = null;

        /// <summary>
        /// Defines the _oscillator
        /// </summary>
        private Oscillator _oscillator;

        /// <summary>
        /// Gets a value indicating whether music is being played.
        /// </summary>
        public bool isPlaying { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicPlayer"/> class.
        /// </summary>
        /// <param name="memory">The memory unit reference<see cref="MemoryUnit"/></param>
        /// <param name="audioBuffer">The audioBuffer reference<see cref="List{float}"/></param>
        /// <param name="sampleRate">The sampleRate for the music<see cref="int"/></param>
        public MusicPlayer(ref MemoryUnit memory, ref List<float> audioBuffer, int sampleRate)
        {
            sfxs = new Sfx[4] { null, null, null, null };
            isPlaying = false;
            _ram = memory.ram;

            _audioBuffer = audioBuffer;
            _sampleRate = sampleRate;

            _oscillator = new Oscillator(_sampleRate);

            patternData = new PatternData[64];
            for (int i = 0; i < patternData.Length; i += 1)
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

        /// <summary>
        /// Updates music.
        /// </summary>
        public void Update()
        {
            if (!isPlaying || _patternIndex > 63 || _patternIndex < 0)
            {
                return;
            }

            Process();

            if (isPatternDone())
            {
                if (patternData[_patternIndex].shouldStop)
                {
                    Stop();
                    return;
                }

                if (patternData[_patternIndex].loopEnd)
                {
                    _patternIndex = FindClosestLoopStart(_patternIndex);
                }
                else
                {
                    _patternIndex += 1;
                    if (_patternIndex > 63)
                    {
                        Stop();
                        return;
                    }
                }

                SetUpPattern();
                Process();
            }
        }

        /// <summary>
        /// Finds closest pattern with the loop start variable set to true.
        /// </summary>
        /// <param name="index">The index to start from<see cref="int"/></param>
        /// <returns>The index of the loop start pattern<see cref="int"/></returns>
        private int FindClosestLoopStart(int index)
        {
            for (int i = index; i >= 0; i -= 1)
            {
                if (patternData[i].loopStart)
                    return i;
            }

            return 0;
        }

        /// <summary>
        /// Process sfx data.
        /// </summary>
        private void Process()
        {
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
        }

        /// <summary>
        /// Whether or not the pattern has finished playing
        /// </summary>
        /// <returns>Whether or not the pattern has finished playing<see cref="bool"/></returns>
        private bool isPatternDone()
        {
            if (_referenceSfx != null && !_referenceSfx.isAlive)
                return true;

            return false;
        }

        /// <summary>
        /// Sets up a new pattern to be played.
        /// </summary>
        private void SetUpPattern()
        {
            bool areAllLooping = true;
            Sfx longest = null;
            Sfx longestNoLoop = null;
            int audioBufferIndex = _referenceSfx == null ? 0 : _referenceSfx.audioBufferIndex;
            for (int i = 0; i < 4; i += 1)
            {
                if (patternData[_patternIndex].channels[i].isSilent)
                {
                    sfxs[i] = null;
                    continue;
                }

                byte[] _sfxData = new byte[68];
                Buffer.BlockCopy(_ram, util.ADDR_SFX + 68 * patternData[_patternIndex].channels[i].sfxIndex, _sfxData, 0, 68);
                sfxs[i] = new Sfx(_sfxData, patternData[_patternIndex].channels[i].sfxIndex, ref _audioBuffer, ref _oscillator, _sampleRate, audioBufferIndex);
                sfxs[i].Start();

                if (!sfxs[i].HasLoop())
                {
                    areAllLooping = false;
                    if (longestNoLoop == null || longestNoLoop.duration < sfxs[i].duration)
                        longestNoLoop = sfxs[i];
                }

                if (longest == null || longest.duration < sfxs[i].duration)
                    longest = sfxs[i];
            }

            _referenceSfx = areAllLooping ? longest : longestNoLoop;
            // Remove loop from reference sfx, otherwise it'll keep looping forever.
            _referenceSfx.endLoop = _referenceSfx.startLoop;
        }

        /// <summary>
        /// Starts playing music.
        /// </summary>
        /// <param name="n">The pattern index to start playing from<see cref="int"/></param>
        public void Start(int n)
        {
            isPlaying = true;
            _patternIndex = n;

            SetUpPattern();
        }

        /// <summary>
        /// Stops playing music.
        /// </summary>
        public void Stop()
        {
            isPlaying = false;
        }
    }

    /// <summary>
    /// Defines the <see cref="Sfx" />
    /// </summary>
    public class Sfx
    {
        /// <summary>
        /// Defines the <see cref="P8Note" />. Used to track note information.
        /// </summary>
        public struct P8Note
        {
            /// <summary>
            /// Defines if the note is using custom instruments.
            /// </summary>
            public bool isCustom;

            /// <summary>
            /// Defines the effect the note uses.
            /// </summary>
            public byte effect;

            /// <summary>
            /// Defines the volume of the note.
            /// </summary>
            public byte volume;

            /// <summary>
            /// Defines the waveform of the note.
            /// </summary>
            public byte waveform;

            /// <summary>
            /// Defines the pitch of the note.
            /// </summary>
            public byte pitch;
        }

        /// <summary>
        /// Defines the notes that are present in the sfx.
        /// </summary>
        public P8Note[] notes;

        /// <summary>
        /// Defines the duration of each note in the sfx.
        /// </summary>
        public float duration;

        /// <summary>
        /// Defines the note where the loop starts.
        /// </summary>
        public byte startLoop;

        /// <summary>
        /// Defines the note where the loop ends.
        /// </summary>
        public byte endLoop;

        /// <summary>
        /// Defines if the sfx has a loop.
        /// </summary>
        public bool loop = true;

        /// <summary>
        /// Defines the _sampleRate
        /// </summary>
        private int _sampleRate;

        /// <summary>
        /// Gets a value indicating whether the sfx is alive
        /// </summary>
        public bool isAlive { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the sfx is active
        /// </summary>
        public bool isActive { get; private set; }

        /// <summary>
        /// Gets the sfx index that this sfx references to.
        /// </summary>
        public int sfxIndex { get; private set; }

        /// <summary>
        /// Defines the _audioBuffer
        /// </summary>
        private List<float> _audioBuffer;

        /// <summary>
        /// Defines the _currentNote that is being played
        /// </summary>
        private int _currentNote = 0;

        /// <summary>
        /// Gets or sets the currentNote
        /// </summary>
        public int currentNote
        {
            get { return _currentNote; }
            set
            {
                if (value < 0) _currentNote = 0;
                else if (value >= 32) _currentNote = 31;
                else _currentNote = value;
            }
        }

        /// <summary>
        /// Defines the last note index that should be played.
        /// </summary>
        private int _lastIndex = 31;

        /// <summary>
        /// Gets or sets the lastIndex
        /// </summary>
        public int lastIndex
        {
            get { return _lastIndex; }
            set
            {
                if (value < 0) _lastIndex = 0;
                else if (value >= 32) _lastIndex = 31;
                else _lastIndex = value;
            }
        }

        /// <summary>
        /// Defines the _oscillator
        /// </summary>
        private Oscillator _oscillator;

        /// <summary>
        /// Defines the notesToPlay. Should be useful for effects like arpeggio that plays 4 notes whithin a single note definition.
        /// </summary>
        private Queue<Note> notesToPlay;

        /// <summary>
        /// Defines the _fadeIn length
        /// </summary>
        private float _fadeIn;

        /// <summary>
        /// Gets the audioBufferIndex
        /// </summary>
        public int audioBufferIndex { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sfx"/> class.
        /// </summary>
        /// <param name="_sfxData">The _sfxData to use<see cref="byte[]"/></param>
        /// <param name="_sfxIndex">The _sfxIndex that this sfx references to<see cref="int"/></param>
        /// <param name="audioBuffer">The audioBuffer to fill with data<see cref="List{float}"/></param>
        /// <param name="oscillator">The oscillator reference<see cref="Oscillator"/></param>
        /// <param name="sampleRate">The sampleRate<see cref="int"/></param>
        /// <param name="audioBufferIndex">The index to start filling the audio buffer<see cref="int"/></param>
        public Sfx(byte[] _sfxData, int _sfxIndex, ref List<float> audioBuffer, ref Oscillator oscillator, int sampleRate, int audioBufferIndex = 0)
        {
            notes = new P8Note[32];
            _audioBuffer = audioBuffer;

            duration = _sfxData[65] / 120.0f;
            startLoop = _sfxData[66];
            endLoop = _sfxData[67];

            _sampleRate = sampleRate;
            sfxIndex = _sfxIndex;

            _oscillator = oscillator;

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

            this.audioBufferIndex = audioBufferIndex;

            isActive = true;

            _fadeIn = 0.05f / duration;
        }

        /// <summary>
        /// Moves forward with sfx.
        /// </summary>
        /// <returns>Whether or not the sfx has ended<see cref="bool"/></returns>
        public bool Update()
        {
            if (!isAlive)
            {
                return false;
            }

            int samplesPerBuffer = _audioBuffer.Count;
            while (audioBufferIndex < samplesPerBuffer)
            {
                // Queue next notes that need to be played. In case there are no more notes, stop everything.
                if (notesToPlay.Count == 0)
                {
                    QueueNextNotes();
                    if (notesToPlay.Count == 0)
                    {
                        isAlive = false;
                        break;
                    }
                }

                Note next = notesToPlay.Peek();
                audioBufferIndex = next.Process(audioBufferIndex, isActive);

                if (audioBufferIndex < samplesPerBuffer)
                    notesToPlay.Dequeue();
            }

            audioBufferIndex = audioBufferIndex == samplesPerBuffer ? 0 : audioBufferIndex;
            return isAlive;
        }

        /// <summary>
        /// Queue next notes for playing.
        /// </summary>
        private void QueueNextNotes()
        {
            if (_currentNote > _lastIndex)
            {
                return;
            }

            P8Note nextNote = notes[_currentNote];

            switch (nextNote.effect)
            {
                case 0:
                    ProcessNoteNoEffect(nextNote);
                    _fadeIn = 0;
                    break;
                case 1:
                    ProcessNoteSlide(nextNote);
                    _fadeIn = 0;
                    break;
                case 2:
                    ProcessNoteVibrato(nextNote);
                    _fadeIn = 0;
                    break;
                case 3:
                    ProcessNoteDrop(nextNote);
                    _fadeIn = 0;
                    break;
                case 4:
                    ProcessNoteFadeIn(nextNote);
                    _fadeIn = 0;
                    break;
                case 5:
                    ProcessNoteFadeOut(nextNote);
                    _fadeIn = 1.0f / duration;
                    break;
                case 6:
                    ProcessNoteArpeggioFast(nextNote);
                    _fadeIn = 0;
                    break;
                case 7:
                    ProcessNoteArpeggioSlow(nextNote);
                    _fadeIn = 0;
                    break;
                default:
                    _fadeIn = 0.5f / duration;
                    break;
            }

            // If sfx has loop defined, process it. Otherwise keep incrementing note index.
            if (loop && startLoop < endLoop && _currentNote == endLoop - 1)
            {
                _currentNote = startLoop;
            }
            else
            {
                _currentNote += 1;
            }
        }

        /// <summary>
        /// Whether or not this sfx has a loop defined
        /// </summary>
        /// <returns>Whether or not this sfx has a loop defined<see cref="bool"/></returns>
        public bool HasLoop()
        {
            return startLoop < endLoop;
        }

        /// <summary>
        /// Processes a new note with no effect
        /// </summary>
        /// <param name="note">The note to create<see cref="P8Note"/></param>
        private void ProcessNoteNoEffect(P8Note note)
        {
            Note noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform, note.pitch, note.pitch, _fadeIn, 0);
            notesToPlay.Enqueue(noteToPlay);
        }

        /// <summary>
        /// Processes a new note with slide effect
        /// </summary>
        /// <param name="note">The note to create<see cref="P8Note"/></param>
        private void ProcessNoteSlide(P8Note note)
        {
            int pitchFrom = _currentNote == 0 ? 32 : notes[_currentNote - 1].pitch;
            Note noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform, note.pitch, pitchFrom, _fadeIn, 0);
            notesToPlay.Enqueue(noteToPlay);
        }

        /// <summary>
        /// Processes a new note with vibrato effect
        /// </summary>
        /// <param name="note">The note to create<see cref="P8Note"/></param>
        private void ProcessNoteVibrato(P8Note note)
        {
            Note noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform, note.pitch, note.pitch, 0, 0, true);
            notesToPlay.Enqueue(noteToPlay);
        }

        /// <summary>
        /// Processes a new note with drop effect
        /// </summary>
        /// <param name="note">The note to create<see cref="P8Note"/></param>
        private void ProcessNoteDrop(P8Note note)
        {
            Note noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform, 0, note.pitch, 0, 0);
            notesToPlay.Enqueue(noteToPlay);
        }

        /// <summary>
        /// Processes a new note with fade in effect
        /// </summary>
        /// <param name="note">The note to create<see cref="P8Note"/></param>
        private void ProcessNoteFadeIn(P8Note note)
        {
            Note noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform, note.pitch, note.pitch, 95, 5);
            notesToPlay.Enqueue(noteToPlay);
        }

        /// <summary>
        /// Processes a new note with fade out effect
        /// </summary>
        /// <param name="note">The note to create<see cref="P8Note"/></param>
        private void ProcessNoteFadeOut(P8Note note)
        {
            Note noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform, note.pitch, note.pitch, 0, 95);
            notesToPlay.Enqueue(noteToPlay);
        }

        /// <summary>
        /// Processes a new note with fast arpeggio effect
        /// </summary>
        /// <param name="note">The note to create<see cref="P8Note"/></param>
        private void ProcessNoteArpeggioFast(P8Note note)
        {
            Note noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform, note.pitch, note.pitch, 0, 0);
            notesToPlay.Enqueue(noteToPlay);
        }

        /// <summary>
        /// Processes a new note with slow arpeggio effect
        /// </summary>
        /// <param name="note">The note to create<see cref="P8Note"/></param>
        private void ProcessNoteArpeggioSlow(P8Note note)
        {
            Note noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform, note.pitch, note.pitch, 0, 0);
            notesToPlay.Enqueue(noteToPlay);
        }

        /// <summary>
        /// Starts playing sfx
        /// </summary>
        public void Start()
        {
            isAlive = true;
        }

        /// <summary>
        /// Stops playing sfx.
        /// </summary>
        public void Stop()
        {
            isAlive = false;
        }
    }

    /// <summary>
    /// Defines the <see cref="Note" />
    /// </summary>
    public class Note
    {
        /// <summary>
        /// Defines the _audioBuffer
        /// </summary>
        private List<float> _audioBuffer;

        /// <summary>
        /// Defines the _duration of the note
        /// </summary>
        private float _duration;

        /// <summary>
        /// Defines the _fadeIn length
        /// </summary>
        private float _fadeIn;

        /// <summary>
        /// Defines the _fadeOut length
        /// </summary>
        private float _fadeOut;

        /// <summary>
        /// Defines the _timePassed through the notes playthrough
        /// </summary>
        private float _timePassed;

        /// <summary>
        /// Defines the _sampleRate
        /// </summary>
        private int _sampleRate;

        /// <summary>
        /// Defines the isCustom
        /// </summary>
        public bool isCustom;

        /// <summary>
        /// Defines the targetVolume
        /// </summary>
        public float targetVolume;

        /// <summary>
        /// Defines the waveform
        /// </summary>
        public byte waveform;

        /// <summary>
        /// Defines the pitch
        /// </summary>
        public byte pitch;

        /// <summary>
        /// Defines the _vibrato
        /// </summary>
        private bool _vibrato;

        /// <summary>
        /// Defines the _oscillator
        /// </summary>
        private Oscillator _oscillator;

        /// <summary>
        /// Defines the _volume
        /// </summary>
        private float _volume;

        /// <summary>
        /// Defines the _pitchFrom
        /// </summary>
        private int _pitchFrom;

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> class.
        /// </summary>
        /// <param name="audioBuffer">The audioBuffer to fill<see cref="List{float}"/></param>
        /// <param name="sampleRate">The sampleRate<see cref="int"/></param>
        /// <param name="oscillator">The oscillator reference<see cref="Oscillator"/></param>
        /// <param name="duration">The duration of the note<see cref="float"/></param>
        /// <param name="volume">The volume of the note<see cref="byte"/></param>
        /// <param name="waveform">The waveform of the note<see cref="byte"/></param>
        /// <param name="pitch">The pitch of the note<see cref="byte"/></param>
        /// <param name="pitchFrom">The pitch to start the note from<see cref="int"/></param>
        /// <param name="fadeIn">The fadeIn length<see cref="float"/></param>
        /// <param name="fadeOut">The fadeOut length<see cref="float"/></param>
        /// <param name="vibrato">If it should have a vibrato effect<see cref="bool"/></param>
        public Note(ref List<float> audioBuffer, int sampleRate, ref Oscillator oscillator, float duration, byte volume, byte waveform, byte pitch, int pitchFrom = -1, float fadeIn = 1, float fadeOut = 1, bool vibrato = false)
        {
            _audioBuffer = audioBuffer;

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

        /// <summary>
        /// Processes the note
        /// </summary>
        /// <param name="bufferOffset">The audio bufferOffset to start processing<see cref="int"/></param>
        /// <param name="writeToBuffer">If it should write to the buffer or only keep processing without sound.<see cref="bool"/></param>
        /// <returns>The last position that it filled from the audio buffer.<see cref="int"/></returns>
        public int Process(int bufferOffset = 0, bool writeToBuffer = true)
        {
            int samplesPerBuffer = _audioBuffer.Count;
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
                    else
                    {
                        _volume = targetVolume;
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
                    _audioBuffer[i] += sample * _volume;
                }

                _timePassed += 1.0f / _sampleRate;
            }

            return samplesPerBuffer;
        }
    }
}
