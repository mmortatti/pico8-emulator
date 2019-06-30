using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pico8_interpreter.Pico8
{
    class MemoryUnit
    {
        public const int ADDR_GFX = 0x0,
                         ADDR_GFX_MAP = 0x1000,
                         ADDR_MAP = 0x2000,
                         ADDR_GFX_PROPS = 0x3000,
                         ADDR_SONG = 0x3100,
                         ADDR_SFX = 0x3200,
                         ADDR_USER = 0x4300,
                         ADDR_CART = 0x5e00,
                         ADDR_PALETTE_0 = 0x5f00,
                         ADDR_PALETTE_1 = 0x5f10,
                         ADDR_CURSOR_X = 0x5f26,
                         ADDR_CURSOR_Y = 0x5f27,
                         ADDR_CAMERA_X = 0x5f28,
                         ADDR_CAMERA_Y = 0x5f2a,
                         ADDR_SCREEN = 0x6000;
                         
        readonly byte[] ram;
        readonly byte[] screen;
        public byte[] FrameBuffer
        {
            get
            {
                Buffer.BlockCopy(ram, 0x6000, screen, 0, 0x2000);
                return screen;
            }
        }

        public MemoryUnit()
        {
            ram = new byte[0x8000];
            screen = new byte[0x2000];
        }
        public int cameraX
        {
            get
            {
                return ((sbyte)(ram[ADDR_CAMERA_X + 1] << 8)) | ram[ADDR_CAMERA_X];
            }
            set
            {
                ram[ADDR_CAMERA_X] = (byte)(value & 0xff);
                ram[ADDR_CAMERA_X + 1] = (byte)(value >> 8);
            }
        }
        public int cameraY
        {
            get
            {
                return ((sbyte)(ram[ADDR_CAMERA_Y + 1] << 8)) | ram[ADDR_CAMERA_Y];
            }
            set
            {
                ram[ADDR_CAMERA_Y] = (byte)(value & 0xff);
                ram[ADDR_CAMERA_Y + 1] = (byte)(value >> 8);
            }
        }

        public void WritePixel(int x, int y, int color)
        {
            int index = (y * 128 + x) / 2;

            if (index < 0 || index > 64*128 - 1)
            {
                return;
            }

            byte mask = (byte)(x % 2 == 1 ? 0b00001111 : 0b11110000);
            ram[ADDR_SCREEN + index] = (byte)((ram[ADDR_SCREEN + index] & mask) | color);
        }
    }
}
