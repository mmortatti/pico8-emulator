namespace Pico8Emulator.unit.cart {
	public class Cartridge {
		public const int RomSize = 0x8005;
		
		public byte[] Rom = new byte[RomSize];
		public string Code;
		public string Path;
	}
}