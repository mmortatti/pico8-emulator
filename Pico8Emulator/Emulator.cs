using System;
using System.Collections.Generic;
using System.IO;
using Pico8Emulator.lua;
using Pico8Emulator.unit;
using Pico8Emulator.unit.audio;
using Pico8Emulator.unit.cart;
using Pico8Emulator.unit.graphics;
using Pico8Emulator.unit.mem;

namespace Pico8Emulator {
	public class Emulator {
		private List<Unit> units = new List<Unit>();

		public MemoryUnit Memory;
		public GraphicsUnit Graphics;
		public AudioUnit Audio;
		public CartridgeUnit CartridgeLoader;
		
		public Emulator() {
			units.Add(Memory = new MemoryUnit(this));
			units.Add(Graphics = new GraphicsUnit(this));
			units.Add(Audio = new AudioUnit(this));
			units.Add(CartridgeLoader = new CartridgeUnit(this));

			foreach (var unit in units) {
				unit.Init();
			}
		}

		public void Shutdown() {
			foreach (var unit in units) {
				unit.Destroy();
			}
		}
		
		private DateTime timeStart;


		public G[] screenColorData {
			get { return graphics.screenColorData; }
			set { graphics.screenColorData = value; }
		}

		public Func<int, int, int, G> rgbToColor {
			get { return graphics.rgbToColor; }
			set { graphics.rgbToColor = value; }
		}

		/// <summary>
		/// Defines the <see cref="Game" />
		/// Struct to hold all information about a running pico8 game.
		/// </summary>
		public struct Game {
			/// <summary>
			/// Defines the cartridge
			/// </summary>
			public Cartridge cartridge;

			/// <summary>
			/// Defines the interpreter
			/// </summary>
			public LuaInterpreter interpreter;

			/// <summary>
			/// Defines the path
			/// </summary>
			public string path;

			/// <summary>
			/// Defines the cartdata_id
			/// </summary>
			public string cartdata_id;

			/// <summary>
			/// Defines the cartdata
			/// </summary>
			public Int32[] cartdata;
		}

		/// <summary>
		/// Defines the currently loaded game.
		/// </summary>
		public Game loadedGame;

		/// <summary>
		/// Defines the cartdata folder path.
		/// </summary>
		public string cartdataPath = "cartdata/";
		
		private void InitApi(LuaInterpreter script) {
			foreach (var unit in units) {
				unit.DefineApi(script);
			}
		}

		public object Import(string filename, bool onlyHalf = false) {
			Bitmap sheet = new Bitmap(filename);

			if (sheet.Height != 128 || sheet.Width != 128) {
				throw new ArgumentException($"{filename} must be a 128x128 image, but is {sheet.Width}x{sheet.Width}.");
			}

			int div = onlyHalf ? 2 : 1;

			for (int i = 0; i < sheet.Height / div; i += 1) {
				for (int j = 0; j < sheet.Width; j += 1) {
					byte val = graphics.ColorToPalette(sheet.GetPixel(j, i));
					graphics.Sset(j, i, val);
				}
			}

			sheet.Dispose();

			return null;
		}

		public object Export(string filename) {
			Bitmap sheet = new Bitmap(128, 128);

			for (int i = 0; i < sheet.Height; i += 1) {
				for (int j = 0; j < sheet.Width; j += 1) {
					byte val = graphics.Sget(j, i);

					Color col = System.Drawing.Color.FromArgb(graphics.pico8Palette[val, 0],
						graphics.pico8Palette[val, 1],
						graphics.pico8Palette[val, 2]);

					sheet.SetPixel(j, i, col);
				}
			}

			if (File.Exists(filename))
				File.Delete(filename);

			sheet.Save(filename, ImageFormat.Png);

			sheet.Dispose();

			return null;
		}

		/// <summary>
		/// Print a string to the console,
		/// </summary>
		/// <param name="s">The string to print</param>
		public object Printh(object s) {
			Console.WriteLine(String.Format("{0:####.####}", s));
			return null;
		}

		public object Cartdata(string id) {
			Trace.Assert(loadedGame.cartdata_id.Length == 0, "cartdata() can only be called once");
			Trace.Assert(id.Length <= 64, "cart data id too long");
			Trace.Assert(id.Length != 0, "empty cart data id");

			Trace.Assert(Regex.IsMatch(id, "^[a-zA-Z0-9_]*$"), "cart data id: bad char");

			var fileName = cartdataPath + id;

			if (File.Exists(fileName)) {
				using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open))) {
					for (int i = 0; i < loadedGame.cartdata.Length; i++) {
						loadedGame.cartdata[i] = reader.ReadInt32();
					}
				}
			} else {
				using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create))) {
					for (int i = 0; i < loadedGame.cartdata.Length; i++) {
						writer.Write(0);
						loadedGame.cartdata[i] = 0;
					}
				}
			}

			loadedGame.cartdata_id = id;

			return null;
		}

		public object Dget(int index) {
			Trace.Assert(index < loadedGame.cartdata.Length, "bad index");
			return Util.FixedToFloat(loadedGame.cartdata[index]);
		}

		public object Dset(int index, double value) {
			Trace.Assert(index < loadedGame.cartdata.Length, "bad index");
			loadedGame.cartdata[index] = Util.FloatToFixed(value);
			SaveCartdata(cartdataPath + loadedGame.cartdata_id);

			return null;
		}

		private void SaveCartdata(string fileName) {
			using (BinaryWriter writer = new BinaryWriter(File.Open(fileName, FileMode.Create))) {
				for (int i = 0; i < loadedGame.cartdata.Length; i++) {
					writer.Write(loadedGame.cartdata[i]);
				}
			}
		}

		public double Time() {
			return (DateTime.Now - timeStart).TotalSeconds;
		}

		public object Reload(int dest_addr, int source_addr, int len, string filename = "") {
			Trace.Assert(dest_addr < 0x4300);
			Cartridge cart = filename.Length == 0 ? loadedGame.cartridge : new Cartridge(filename);

			memory.Memcpy(dest_addr, source_addr, len, cart.rom);
			return null;
		}
		
		public object Cstore(int dest_addr, int source_addr, int len, string filename = null) {
			Trace.Assert(dest_addr < 0x4300);
			Cartridge cart = filename == null ? loadedGame.cartridge : new Cartridge(filename, true);

			Buffer.BlockCopy(memory.ram, source_addr, cart.rom, dest_addr, len);
			cart.SaveP8();
			return null;
		}

		public void LoadGame(string path, LuaInterpreter interpreter) {
			// Verify if all conditions to run a game are met.
			if (graphics.screenColorData == null)
				throw new ArgumentNullException(
					"Pico8 must have a reference to the screenColorData array to fill it with pico8's screen data.");

			if (graphics.screenColorData.Length != 128 * 128)
				throw new ArgumentException(
					$"screenColorData array must be of length 16.384 (128 * 128), but is of length {graphics.screenColorData.Length}.");

			loadedGame = new Game();

			loadedGame.path = path;
			loadedGame.cartridge = new Cartridge(path);
			loadedGame.interpreter = interpreter;
			loadedGame.cartdata_id = "";
			loadedGame.cartdata = new Int32[64];

			InitAPI(ref loadedGame.interpreter);

			memory.LoadCartridgeData(loadedGame.cartridge.rom);

			audio.Init();

			loadedGame.interpreter.RunScript(loadedGame.cartridge.gameCode);

			timeStart = DateTime.Now;

			loadedGame.interpreter.CallIfDefined("_init");
		}

		public void Update() {
			foreach (var unit in units) {
				unit.Update();
			}
		}

		public void Draw() {
			Graphics.Draw();
		}
	}
}