using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;

namespace pico8_interpreter.Pico8
{
    class Cartridge
    {
        public const int ADDR_GFX = 0x0,
                         ADDR_GFX_MAP = 0x1000,
                         ADDR_MAP = 0x2000,
                         ADDR_GFX_PROPS = 0x3000,
                         ADDR_SONG = 0x3100,
                         ADDR_SFX = 0x3200;

        public readonly byte[] rom;

        public string gameCode
        {
            get
            {
                return _gameCode;
            }
        }

        private string _gameCode = "";

        public Cartridge(string path)
        {
            rom = new byte[0x8005];

            LoadP8(path);
        }

        private void LoadP8(string path)
        {
            string completePath = "Pico8/Games/" + path;
            var streamReader = new StreamReader(TitleContainer.OpenStream(completePath));

            Dictionary<string, int> stateMap = new Dictionary<string, int>
            {
                {"__lua__", 0},
                {"__gfx__", 1},
                {"__gff__", 2},
                {"__map__", 3},
                {"__sfx__", 4},
                {"__music__", 5},
            };

            string line;
            int state = -1;
            int index = 0;

            _gameCode = "";
            while (!streamReader.EndOfStream)
            {
                line = streamReader.ReadLine();
                if (stateMap.ContainsKey(line))
                {
                    state = stateMap[line];
                    index = 0;
                    continue;
                }

                if (state == stateMap["__lua__"])
                {
                    _gameCode += line + '\n';
                }
                else if (state == stateMap["__gfx__"])
                {
                    foreach (char c in line)
                    {
                        byte val = byte.Parse(c.ToString(), System.Globalization.NumberStyles.HexNumber);
                        WriteHalf(index, val, ADDR_GFX);
                        index += 1;
                    }
                }
                else if (state == stateMap["__gff__"])
                {

                }
                else if (state == stateMap["__map__"])
                {

                }
                else if (state == stateMap["__sfx__"])
                {

                }
                else if (state == stateMap["__music__"])
                {

                }
            }
        }

        public void WriteHalf(int index, int value, int offset = ADDR_GFX)
        {

            if (index < 0 || index > 64 * 128 - 1)
            {
                return;
            }

            byte mask = (byte)(index % 2 == 1 ? 0x0f : 0xf0);
            value = index % 2 == 1 ? value << 4 : value;
            rom[offset + index / 2] = (byte)((byte)(rom[offset + index / 2] & mask) | value);
        }
    }
}
