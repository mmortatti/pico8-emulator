using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;
using Pico8Emulator.lua;
using Pico8Emulator.unit;
using Pico8Emulator.unit.audio;
using Pico8Emulator.unit.cart;
using Pico8Emulator.unit.input;
using Pico8Emulator.unit.math;
using Pico8Emulator.unit.mem;
using GraphicsUnit = Pico8Emulator.unit.graphics.GraphicsUnit;

namespace Pico8Emulator {
	public class Emulator {
		private List<Unit> units = new List<Unit>();

		public MemoryUnit Memory;
		public GraphicsUnit Graphics;
		public AudioUnit Audio;
		public MathUnit Math;
		public InputUnit Input;
		public CartridgeUnit CartridgeLoader;

		public GraphicsDevice GraphicsDevice;
		
		public Emulator(GraphicsDevice graphics) {
			GraphicsDevice = graphics;
			
			units.Add(Memory = new MemoryUnit(this));
			units.Add(Graphics = new GraphicsUnit(this, graphics));
			units.Add(Audio = new AudioUnit(this));
			units.Add(Math = new MathUnit(this));
			units.Add(Input = new InputUnit(this));
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

		public void Update() {
			foreach (var unit in units) {
				unit.Update();
			}
		}

		public void Draw() {
			CartridgeLoader.Draw();
		}

		public void InitApi(LuaInterpreter script) {
			foreach (var unit in units) {
				unit.DefineApi(script);
			}
			
			script.AddFunction("printh", (Func<object, object>) Printh);
			script.RunScript(LuaPatcher.PatchCode(Api.All));
		}

		public object Printh(object s) {
			Console.WriteLine($"{s:####.####}");
			return null;
		}
	}
}