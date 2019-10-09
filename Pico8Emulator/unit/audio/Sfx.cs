using System;
using System.Collections.Generic;

namespace Pico8Emulator.unit.audio {
	public class Sfx {
		public PicoNote[] notes;
		public float duration;
		public byte startLoop;
		public byte endLoop;
		public bool loop = true;
		private int _sampleRate;

		public bool isAlive { get; private set; }
		public bool isActive { get; private set; }
		public int sfxIndex { get; private set; }

		private float[] _audioBuffer;

		private int _currentNote = 0;

		public int currentNote {
			get { return _currentNote; }
			set {
				if (value < 0) _currentNote = 0;
				else if (value >= 32) _currentNote = 31;
				else _currentNote = value;
			}
		}

		private int _lastIndex = 31;

		public int lastIndex {
			get { return _lastIndex; }
			set {
				if (value < 0) _lastIndex = 0;
				else if (value >= 32) _lastIndex = 31;
				else _lastIndex = value;
			}
		}

		private Oscillator _oscillator;
		private Queue<Note> notesToPlay;

		private float _fadeIn;

		public int audioBufferIndex { get; private set; }

		public Sfx(byte[] _sfxData, int _sfxIndex, ref float[] audioBuffer, ref Oscillator oscillator, int sampleRate,
			int audioBufferIndex = 0) {
			notes = new PicoNote[32];
			_audioBuffer = audioBuffer;

			duration = _sfxData[65] / 120.0f;
			startLoop = _sfxData[66];
			endLoop = _sfxData[67];

			_sampleRate = sampleRate;
			sfxIndex = _sfxIndex;

			_oscillator = oscillator;

			// Console.WriteLine($"header {_sfxData[64]} {_sfxData[65]} {_sfxData[66]} {_sfxData[67]}");

			for (int i = 0; i < _sfxData.Length - 4; i += 2) {
				byte lo = _sfxData[i];
				byte hi = _sfxData[i + 1];

				notes[i / 2].pitch = (byte) (lo & 0b00111111);
				notes[i / 2].waveform = (byte) (((lo & 0b11000000) >> 6) | ((hi & 0b1) << 2));
				notes[i / 2].volume = (byte) ((hi & 0b00001110) >> 1);
				notes[i / 2].effect = (byte) ((hi & 0b01110000) >> 4);
				notes[i / 2].isCustom = (byte) ((hi & 0b10000000) >> 7) == 1;

				// Console.WriteLine($"{i} {notes[i / 2].pitch} {notes[i / 2].waveform} {notes[i / 2].volume} {notes[i / 2].effect} {notes[i / 2].isCustom}");
			}

			oscillator = new Oscillator(sampleRate);
			notesToPlay = new Queue<Note>();

			this.audioBufferIndex = audioBufferIndex;

			isActive = true;

			_fadeIn = 0.05f / duration;
		}

		public bool Update() {
			if (!isAlive) {
				return false;
			}

			while (audioBufferIndex < AudioUnit.BufferSize) {
				// Queue next notes that need to be played. In case there are no more notes, stop everything.
				if (notesToPlay.Count == 0) {
					QueueNextNotes();

					if (notesToPlay.Count == 0) {
						isAlive = false;
						break;
					}
				}

				Note next = notesToPlay.Peek();
				audioBufferIndex = next.Process(audioBufferIndex, isActive);

				if (audioBufferIndex < AudioUnit.BufferSize)
					notesToPlay.Dequeue();
			}

			audioBufferIndex = audioBufferIndex == AudioUnit.BufferSize ? 0 : audioBufferIndex;
			return isAlive;
		}

		private void QueueNextNotes() {
			if (_currentNote > _lastIndex) {
				return;
			}

			var nextNote = notes[_currentNote];

			switch (nextNote.effect) {
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
			if (loop && startLoop < endLoop && _currentNote == endLoop - 1) {
				_currentNote = startLoop;
			} else {
				_currentNote += 1;
			}
		}

		public bool HasLoop() {
			return startLoop < endLoop;
		}

		private void ProcessNoteNoEffect(PicoNote note) {
			Note noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform,
				note.pitch, note.pitch, _fadeIn, 0);

			notesToPlay.Enqueue(noteToPlay);
		}

		private void ProcessNoteSlide(PicoNote note) {
			int pitchFrom = _currentNote == 0 ? 32 : notes[_currentNote - 1].pitch;

			Note noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform,
				note.pitch, pitchFrom, _fadeIn, 0);

			notesToPlay.Enqueue(noteToPlay);
		}

		private void ProcessNoteVibrato(PicoNote note) {
			Note noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform,
				note.pitch, note.pitch, 0, 0, true);

			notesToPlay.Enqueue(noteToPlay);
		}

		private void ProcessNoteDrop(PicoNote note) {
			var noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform,
				0, note.pitch, 0, 0);

			notesToPlay.Enqueue(noteToPlay);
		}

		private void ProcessNoteFadeIn(PicoNote note) {
			var noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform,
				note.pitch, note.pitch, 95, 5);

			notesToPlay.Enqueue(noteToPlay);
		}

		private void ProcessNoteFadeOut(PicoNote note) {
			var noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform,
				note.pitch, note.pitch, 0, 95);

			notesToPlay.Enqueue(noteToPlay);
		}

		private void ProcessNoteArpeggioFast(PicoNote note) {
			var noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform,
				note.pitch, note.pitch, 0, 0);

			notesToPlay.Enqueue(noteToPlay);
		}

		private void ProcessNoteArpeggioSlow(PicoNote note) {
			var noteToPlay = new Note(ref _audioBuffer, _sampleRate, ref _oscillator, duration, note.volume, note.waveform,
				note.pitch, note.pitch, 0, 0);

			notesToPlay.Enqueue(noteToPlay);
		}

		public void Start() {
			isAlive = true;
		}

		public void Stop() {
			isAlive = false;
		}
	}
}