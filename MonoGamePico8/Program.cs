using System;

namespace Pico8Emulator {
	public class Program {
		[STAThread]
		public static void Main() {
			using (var game = new PicoDesktop()) {
				game.Run();
			}
		}
	}
}