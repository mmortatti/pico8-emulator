namespace Pico8Emulator {
	public class PicoDesktop : Game {
		public PicoDesktop() : base(new Version(0, 0, 0, 0, 0, true, true), 
			new PicoState(), "PICO-8 Emulator", 512, 512, false) {
		}
	}
}