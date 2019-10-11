using System;
using System.Collections.Generic;
using Pico8Emulator.unit.mem;

namespace Pico8Emulator.unit.audio {
	public class MusicPlayer {
		private PatternData[] patternData;
		private Sfx[] channels;
		private int _patternIndex;
    private Emulator _emulator;

		private Sfx referenceSfx;

		private Oscillator oscillator;

		public bool isPlaying { get; private set; }

		public MusicPlayer(Emulator emulator) {
      _emulator = emulator;        
    }

    public void LoadMusic() {
      channels = new Sfx[4] { null, null, null, null };
      isPlaying = false;

      oscillator = new Oscillator(AudioUnit.SampleRate);
      patternData = new PatternData[64];

      for (int i = 0; i < patternData.Length; i += 1)
      {
        byte[] vals = {
            _emulator.Memory.Ram[i * 4 + 0 + RamAddress.Song],
            _emulator.Memory.Ram[i * 4 + 1 + RamAddress.Song],
            _emulator.Memory.Ram[i * 4 + 2 + RamAddress.Song],
            _emulator.Memory.Ram[i * 4 + 3 + RamAddress.Song]
        };

        if ((vals[0] & 0x80) == 0x80)
        {
          patternData[i].LoopStart = true;
        }

        if ((vals[1] & 0x80) == 0x80)
        {
          patternData[i].LoopEnd = true;
        }

        if ((vals[2] & 0x80) == 0x80)
        {
          patternData[i].ShouldStop = true;
        }

        patternData[i].ChannelCount = new ChannelData[4];

        for (int j = 0; j < 4; j += 1)
        {
          patternData[i].ChannelCount[j] = new ChannelData();

          if ((vals[j] & 0b01000000) != 0)
          {
            patternData[i].ChannelCount[j].IsSilent = true;
          }

          patternData[i].ChannelCount[j].SfxIndex = (byte)(vals[j] & 0b00111111);
        }
      }
    }

		public void Update() {
			if (!isPlaying || _patternIndex > 63 || _patternIndex < 0) {
				return;
			}

			Process();

			if (isPatternDone()) {
				if (patternData[_patternIndex].ShouldStop) {
					Stop();
					return;
				}

				if (patternData[_patternIndex].LoopEnd) {
					_patternIndex = FindClosestLoopStart(_patternIndex);
				} else {
					_patternIndex += 1;

					if (_patternIndex > 63) {
						Stop();
						return;
					}
				}

				SetUpPattern();
				Process();
			}
		}

		private int FindClosestLoopStart(int index) {
			for (int i = index; i >= 0; i -= 1) {
				if (patternData[i].LoopStart)
					return i;
			}

			return 0;
		}

		private void Process() {
			for (int i = 0; i < 4; i += 1) {
				if (channels[i] == null)
					continue;

				if (!channels[i].Update()) {
					channels[i] = null;
					continue;
				}
			}
		}
		
		private bool isPatternDone() {
			if (referenceSfx != null && !referenceSfx.isAlive)
				return true;

			return false;
		}

		private void SetUpPattern() {
			bool areAllLooping = true;
			Sfx longest = null;
			Sfx longestNoLoop = null;
			int audioBufferIndex = referenceSfx?.audioBufferIndex ?? 0;

			for (int i = 0; i < 4; i += 1) {
				if (patternData[_patternIndex].ChannelCount[i].IsSilent) {
					channels[i] = null;
					continue;
				}

				byte[] _channelsData = new byte[68];
				Buffer.BlockCopy(_emulator.Memory.Ram, RamAddress.Sfx + 68 * patternData[_patternIndex].ChannelCount[i].SfxIndex, _channelsData, 0, 68);

				channels[i] = new Sfx(_channelsData, patternData[_patternIndex].ChannelCount[i].SfxIndex, ref _emulator.Audio.AudioBuffer, ref oscillator,
					AudioUnit.SampleRate, audioBufferIndex);

				channels[i].Start();

				if (!channels[i].HasLoop()) {
					areAllLooping = false;

					if (longestNoLoop == null || longestNoLoop.duration < channels[i].duration)
						longestNoLoop = channels[i];
				}

				if (longest == null || longest.duration < channels[i].duration)
					longest = channels[i];
			}

			referenceSfx = areAllLooping ? longest : longestNoLoop;
			referenceSfx.endLoop = referenceSfx.startLoop;
		}

		public void Start(int n) {
			isPlaying = true;
			_patternIndex = n;

			SetUpPattern();
		}

		public void Stop() {
			isPlaying = false;
		}
	}
}