using Pico8Emulator.backend;
using Pico8Emulator.lua;
using Pico8Emulator.unit;
using Pico8Emulator.unit.audio;
using Pico8Emulator.unit.cart;
using Pico8Emulator.unit.input;
using Pico8Emulator.unit.math;
using Pico8Emulator.unit.mem;
using System;
using System.Threading;
using System.Collections.Generic;
using GraphicsUnit = Pico8Emulator.unit.graphics.GraphicsUnit;
using System.Diagnostics;

namespace Pico8Emulator {
	public class Emulator {
		public readonly List<Unit> units = new List<Unit>();

		public MemoryUnit Memory;
		public GraphicsUnit Graphics;
		public AudioUnit Audio;
		public MathUnit Math;
		public InputUnit Input;
		public CartridgeUnit CartridgeLoader;

		public readonly GraphicsBackend GraphicsBackend;
		public readonly AudioBackend AudioBackend;
		public readonly InputBackend InputBackend;

		public bool HasCodeLoaded;

		public Thread gameThread;

		public EventWaitHandle flipWait = new ManualResetEvent(true);
		public EventWaitHandle engineWait = new ManualResetEvent(true);
		public object waitLock = new object();

		public Stopwatch cartLoopTimer = new Stopwatch();

		public int GameLoopCalls { get; private set; } = 0;

		public Emulator(GraphicsBackend graphics, AudioBackend audio, InputBackend input) {
			GraphicsBackend = graphics;
			AudioBackend = audio;
			InputBackend = input;

			graphics.Emulator = this;
			audio.Emulator = this;
			input.Emulator = this;

			units.Add(Memory = new MemoryUnit(this));
			units.Add(Graphics = new GraphicsUnit(this));
			units.Add(Audio = new AudioUnit(this));
			units.Add(Math = new MathUnit(this));
			units.Add(Input = new InputUnit(this));
			units.Add(CartridgeLoader = new CartridgeUnit(this));

			foreach (var unit in units) {
				unit.Init();
			}
		}

		public void GameLoop()
		{
			while(true)
			{
				GameLoopCalls++;
				Update60();
				Update30();
				Update60();
				Draw();
				Graphics.Flip();
			}
		}

		public void Shutdown() {
			foreach (var unit in units) {
				unit.Destroy();
			}
		}

		public void Update30() {
			CartridgeLoader.Update30();
		}

		public void Update60() {
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

			script.AddFunction("printh", (Action<object>)Printh);
			script.AddFunction("stat", (Func<object>)Stat);
			script.AddFunction("menuitem", (Action<int, string, object>)Menuitem);

			script.RunScript(LuaPatcher.PatchCode(Api.All));
		}

		public void Menuitem(int index, string label = null, object callback = null) {
			// TODO: implement
		}

		public void Printh(object s) {
			Console.WriteLine($"{s:####.####}");
		}

		public object Stat() {
			return 0; // TODO: implement
		} 
	}
}