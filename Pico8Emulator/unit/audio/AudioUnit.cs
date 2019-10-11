using System;
using Pico8Emulator.lua;
using Pico8Emulator.unit.mem;

namespace Pico8Emulator.unit.audio {
	public class AudioUnit : Unit {
		public const int SampleRate = 48000;
		public const int BufferSize = 2048;
		public const int ChannelCount = 4;

		public Sfx[] SfxChannels = new Sfx[ChannelCount];
		public float[] ExternalAudioBuffer = new float[BufferSize];
		public float[] AudioBuffer = new float[BufferSize];

		private MusicPlayer musicPlayer;

		public AudioUnit(Emulator emulator) : base(emulator) {
			musicPlayer = new MusicPlayer(Emulator);
		}

		public override void OnCartridgeLoad() {
			musicPlayer.LoadMusic();
		}

		public override void DefineApi(LuaInterpreter script) {
			base.DefineApi(script);

			script.AddFunction("music", (Action<int, int?, int?>) Music);
			script.AddFunction("sfx", (Action<int, int?, int?, int?>) Sfx);
		}

		public override void Update() {
			base.Update();
			Emulator.AudioBackend.Update();
		}

		public float[] RequestBuffer() {
			ClearBuffer();
			FillBuffer();
			CompressBuffer();

			Buffer.BlockCopy(AudioBuffer, 0, ExternalAudioBuffer, 0, sizeof(float) * BufferSize);
			return ExternalAudioBuffer;
		}
		
		public void Sfx(int n, int? channel = -1, int? offset = 0, int? length = 32) {
			switch (n) {
				case -1:
					if (channel == -1) {
						StopAllChannelCount();
						break;
					}

					if (channel < 0 || channel >= ChannelCount)
						break;

					SfxChannels[channel.Value] = null;
					break;
				case -2:
					if (channel.Value < 0 || channel.Value >= ChannelCount)
						break;

					if (SfxChannels[channel.Value] != null) {
						SfxChannels[channel.Value].loop = false;
					}

					break;
				default:
					// If sound is already playing, stop it.
					int? index = FindSoundOnAChannel(n);

					if (index != null) {
						SfxChannels[index.Value] = null;
					}

					if (channel == -1) {
						channel = FindAvailableChannel();

						if (channel == null)
							break;
					}

					if (channel == -2) {
						break;
					}

					byte[] _sfxData = new byte[68];
					Buffer.BlockCopy(Emulator.Memory.Ram, RamAddress.Sfx + 68 * n, _sfxData, 0, 68);

					var osc = new Oscillator(SampleRate);
					SfxChannels[channel.Value] = new Sfx(_sfxData, n, ref AudioBuffer, ref osc, SampleRate);
					SfxChannels[channel.Value].currentNote = offset.Value;
					SfxChannels[channel.Value].lastIndex = offset.Value + length.Value - 1;
					SfxChannels[channel.Value].Start();
					break;
			}
		}

		public void Music(int n, int? fade_len = null, int? channel_mask = null) {
			musicPlayer.Start(n);
		}

		public void FillBuffer() {
			for (int i = 0; i < ChannelCount; i += 1) {
				var s = SfxChannels[i];
				
				if (s != null && !s.Update()) {
					SfxChannels[i] = null;
				}
			}

			musicPlayer.Update();
		}
		
		public void ClearBuffer() {
			for (int i = 0; i < BufferSize; i++) {
				AudioBuffer[i] = 0;
			}
		}

		public void CompressBuffer() {
			for (int i = 0; i < BufferSize; i++) {
				AudioBuffer[i] = (float) Math.Tanh(AudioBuffer[i]);
			}
		}

		private int? FindAvailableChannel() {
			for (int i = 0; i < ChannelCount; i += 1) {
				if (SfxChannels[i] == null) {
					return i;
				}
			}

			return null;
		}

		private int? FindSoundOnAChannel(int n) {
			for (int i = 0; i < ChannelCount; i += 1) {
				var s = SfxChannels[i];

				if (s != null && s.sfxIndex == n) {
					return i;
				}
			}

			return null;
		}

		private void StopChannel(int index) {
			SfxChannels[index] = null;
		}
		
		private void StopAllChannelCount() {
			for (int i = 0; i < ChannelCount; i += 1) {
				StopChannel(i);
			}
		}
	}
}