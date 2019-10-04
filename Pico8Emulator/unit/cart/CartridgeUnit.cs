using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Pico8Emulator.unit.cart {
	public class CartridgeUnit : Unit {
		public Cartridge Loaded;

		public CartridgeUnit(Emulator emulator) : base(emulator) {
			
		}

		public void Load(string name) {
			var possibleNames = new[] {
				name,
				$"{name}.p8.png",
				$"{name}.png",
				$"{name}.p8"
			};

			/*
			 * TODO: this is super basic file searching,
			 * should probably introduce filesystem unit or smth,
			 * so that the user could set the search/root path
			 */
			foreach (var possibleName in possibleNames) {
				if (File.Exists(possibleName)) {
					ReadCart(possibleName, possibleName.EndsWith(".png"));
				}
			}

			/*
			 * TODO: report the issue, that the cart was not found
			 */
		}

		private void ReadCart(string name, bool image) {
			if (image) {
				/*
				 * TODO: implement
				 */
			} else {
				LoadTextCart(name);
			}
		}

		private void LoadTextCart(string path) {
			var streamReader = new StreamReader(path);
			var stateMap = new Dictionary<string, int> {
				{ "__lua__", 0 },
				{ "__gfx__", 1 },
				{ "__gff__", 2 },
				{ "__map__", 3 },
				{ "__sfx__", 4 },
				{ "__music__", 5 },
				{ "__label__", 6 }
			};
			
			Loaded = new Cartridge();
			Loaded.Path = path;

			var state = -1;
			var index = 0;
			var codeBuilder = new StringBuilder();
			string line;

			while (!streamReader.EndOfStream) {
				line = streamReader.ReadLine();

				if (line == null) {
					/*
					 * FIXME: report error??
					 */
					break;
				}

				if (stateMap.ContainsKey(line)) {
					state = stateMap[line];
					index = 0;
					continue;
				}

				if (state == -1) {
					if (Regex.IsMatch(line, @"[vV]ersion\ *")) {
						Loaded.Rom[RomAddress.Meta] = byte.Parse(Regex.Replace(line, @"[vV]ersion\ *", ""), NumberStyles.Integer);
					}
				} else if (state == stateMap["__lua__"]) {
					codeBuilder.AppendLine(line);
				} else if (state == stateMap["__gfx__"]) {
					foreach (var c in line) {
						var val = byte.Parse(c.ToString(), NumberStyles.HexNumber);
						Util.SetHalf(ref Loaded.Rom[index / 2 + RomAddress.Gfx], val, index % 2 == 0);
						index += 1;
					}
				} else if (state == stateMap["__gff__"]) {
					for (var i = 0; i < line.Length; i += 2) {
						Loaded.Rom[RomAddress.GfxProps + index] = byte.Parse(line.Substring(i, 2), NumberStyles.HexNumber);
						index += 1;
					}
				} else if (state == stateMap["__map__"]) {
					for (var i = 0; i < line.Length; i += 2) {
						Loaded.Rom[RomAddress.Map + index] = byte.Parse(line.Substring(i, 2), NumberStyles.HexNumber);
						index += 1;
					}
				} else if (state == stateMap["__sfx__"]) {
					if (Regex.IsMatch(line, @"^\s*$")) {
						continue;
					}

					var editor = byte.Parse(line.Substring(0, 2), NumberStyles.HexNumber);
					var speed = byte.Parse(line.Substring(2, 2), NumberStyles.HexNumber);
					var startLoop = byte.Parse(line.Substring(4, 2), NumberStyles.HexNumber);
					var endLoop = byte.Parse(line.Substring(6, 2), NumberStyles.HexNumber);

					Loaded.Rom[RomAddress.Sfx + index * 68 + 64] = editor;
					Loaded.Rom[RomAddress.Sfx + index * 68 + 65] = speed;
					Loaded.Rom[RomAddress.Sfx + index * 68 + 66] = startLoop;
					Loaded.Rom[RomAddress.Sfx + index * 68 + 67] = endLoop;

					var off = 0;

					for (var i = 0; i < line.Length - 8; i += 5) {
						var pitch = byte.Parse(line.Substring(i + 8, 2), NumberStyles.HexNumber);
						var waveform = byte.Parse(line.Substring(i + 8 + 2, 1), NumberStyles.HexNumber);
						var volume = byte.Parse(line.Substring(i + 8 + 3, 1), NumberStyles.HexNumber);
						var effect = byte.Parse(line.Substring(i + 8 + 4, 1), NumberStyles.HexNumber);

						var lo = (byte) (pitch | (waveform << 6));
						var hi = (byte) ((waveform >> 2) | (volume << 1) | (effect << 4));

						Loaded.Rom[RomAddress.Sfx + index * 68 + off] = lo;
						Loaded.Rom[RomAddress.Sfx + index * 68 + off + 1] = hi;
						off += 2;
					}

					index += 1;
				} else if (state == stateMap["__music__"]) {
					if (Regex.IsMatch(line, @"^\s*$")) {
						continue;
					}

					var flag = byte.Parse(line.Substring(0, 2), NumberStyles.HexNumber);
					var val1 = byte.Parse(line.Substring(3, 2), NumberStyles.HexNumber);
					var val2 = byte.Parse(line.Substring(5, 2), NumberStyles.HexNumber);
					var val3 = byte.Parse(line.Substring(7, 2), NumberStyles.HexNumber);
					var val4 = byte.Parse(line.Substring(9, 2), NumberStyles.HexNumber);

					// 4th byte never has 7th bit set because it's corresponding flag value is never used.
					if ((flag & 0x1) != 0) {
						val1 |= 0x80;
					}

					if ((flag & 0x2) != 0) {
						val2 |= 0x80;
					}

					if ((flag & 0x4) != 0) {
						val3 |= 0x80;
					}

					Loaded.Rom[RomAddress.Song + index + 0] = val1;
					Loaded.Rom[RomAddress.Song + index + 1] = val2;
					Loaded.Rom[RomAddress.Song + index + 2] = val3;
					Loaded.Rom[RomAddress.Song + index + 3] = val4;
					
					index += 4;
				}
			}

			streamReader.Close();
		}

		public void SaveP8(string filename = null) {
			if (Loaded == null) {
				return;
			}
			
			if (filename == null) {
				filename = Loaded.Path;
			}

			var fs = new FileStream(filename, FileMode.OpenOrCreate);

			using (var file = new StreamWriter(fs)) {
				file.WriteLine("pico-8 cartridge // http://www.pico-8.com");
				file.WriteLine($"version {Loaded.Rom[RomAddress.Meta]}");

				file.WriteLine("__lua__");
				file.WriteLine(Loaded.Code);

				file.WriteLine("__gfx__");

				for (var j = 0; j < 128; j += 1) {
					for (var i = 0; i < 64; i += 1) {
						var left = Util.GetHalf(Loaded.Rom[j * 64 + i + RomAddress.Gfx], false);
						var right = Util.GetHalf(Loaded.Rom[j * 64 + i + RomAddress.Gfx]);
						file.Write($"{right:x}{left:x}");
					}

					file.Write("\n");
				}

				file.WriteLine("__gff__");

				for (var j = 0; j < 2; j += 1) {
					for (var i = 0; i < 128; i += 1) {
						var left = Util.GetHalf(Loaded.Rom[j * 128 + i + RomAddress.GfxProps], false);
						var right = Util.GetHalf(Loaded.Rom[j * 128 + i + RomAddress.GfxProps]);
						file.Write($"{left:x}{right:x}");
					}

					file.Write("\n");
				}

				file.WriteLine("__map__");

				for (var j = 0; j < 64; j += 1) {
					for (var i = 0; i < 64; i += 1) {
						var left = Util.GetHalf(Loaded.Rom[j * 64 + i + RomAddress.Map], false);
						var right = Util.GetHalf(Loaded.Rom[j * 64 + i + RomAddress.Map]);
						file.Write($"{left:x}{right:x}");
					}

					file.Write("\n");
				}

				file.WriteLine("__sfx__");

				for (var j = 0; j < 64; j += 1) {
					var editor = Loaded.Rom[RomAddress.Sfx + j * 68 + 64];
					var speed = Loaded.Rom[RomAddress.Sfx + j * 68 + 65];
					var startLoop = Loaded.Rom[RomAddress.Sfx + j * 68 + 66];
					var endLoop = Loaded.Rom[RomAddress.Sfx + j * 68 + 67];

					file.Write(
						$"{editor.ToString("x2")}{speed.ToString("x2")}{startLoop.ToString("x2")}{endLoop.ToString("x2")}");

					for (var i = 0; i < 64; i += 2) {
						var lo = Loaded.Rom[RomAddress.Sfx + j * 68 + i];
						var high = Loaded.Rom[RomAddress.Sfx + j * 68 + i + 1];

						var pitch = (byte) (lo & 0b00111111);
						var waveform = (byte) (((lo & 0b11000000) >> 6) | ((high & 0b1) << 2));
						var volume = (byte) ((high & 0b00001110) >> 1);
						var effect = (byte) ((high & 0b01110000) >> 4);

						file.Write($"{pitch:x2}{waveform:x}{volume:x}{effect:x}");
					}

					file.Write("\n");
				}

				file.WriteLine("__music__");

				for (var j = 0; j < 64; j += 1) {
					byte flag = 0;
					var val0 = Loaded.Rom[j * 4 + 0 + RomAddress.Song];
					var val1 = Loaded.Rom[j * 4 + 1 + RomAddress.Song];
					var val2 = Loaded.Rom[j * 4 + 2 + RomAddress.Song];
					var val3 = Loaded.Rom[j * 4 + 3 + RomAddress.Song];

					if ((val0 & 0x80) == 0x80) {
						flag |= 1;
						val0 &= 0x7F;
					}

					if ((val1 & 0x80) == 0x80) {
						flag |= 2;
						val1 &= 0x7F;
					}

					if ((val2 & 0x80) == 0x80) {
						flag |= 4;
						val2 &= 0x7F;
					}

					file.Write($"{flag:D2} {val0:x2}{val1:x2}{val2:x2}{val3:x2}\n");
				}

				file.Close();
			}
		}
	}
}