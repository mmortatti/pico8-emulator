using Pico8Emulator.lua;

namespace Pico8Emulator.unit.cart {
	public class Cartridge {
		public const string CartDataPath = "cartdata/";
		public const int CartDataSize = 64;
		public const int RomSize = 0x8005;
		
		public byte[] Rom = new byte[RomSize];
		public string Code;
		public string Path;
		
		public int[] CartData = new int[CartDataSize];
		public string CartDataId;

		public LuaInterpreter Interpreter;
	}
}