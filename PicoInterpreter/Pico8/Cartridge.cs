using System.Collections.Generic;
using System.IO;

namespace pico8_interpreter.Pico8
{
    public class Cartridge
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
            var streamReader = new StreamReader(completePath);

            Dictionary<string, int> stateMap = new Dictionary<string, int>
            {
                {"__lua__", 0},
                {"__gfx__", 1},
                {"__gff__", 2},
                {"__map__", 3},
                {"__sfx__", 4},
                {"__music__", 5},
                {"__label__", 6}
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
                        util.SetHalf(rom, index / 2 + ADDR_GFX, val, index % 2 == 0);
                        index += 1;
                    }
                }
                else if (state == stateMap["__gff__"])
                {
                    for (int i = 0; i < line.Length; i += 2)
                    {
                        rom[ADDR_GFX_PROPS + index] = byte.Parse(line.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
                        index += 1;
                    }
                }
                else if (state == stateMap["__map__"])
                {
                    for (int i = 0; i < line.Length; i += 2)
                    {
                        rom[ADDR_MAP + index] = byte.Parse(line.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
                        index += 1;
                    }
                }
                else if (state == stateMap["__sfx__"])
                {
                    foreach (char c in line)
                    {
                        byte val = byte.Parse(c.ToString(), System.Globalization.NumberStyles.HexNumber);
                        util.SetHalf(rom, index / 2 + ADDR_SFX, val, index % 2 == 0);
                        index += 1;
                    }
                }
                else if (state == stateMap["__music__"])
                {

                }
            }

            //_gameCode = Depicofier.Depicofy.Clean(_gameCode, false);

            //for (int y = 0; y < 32; y++) 
            //{
            //    for (int x = 0; x < 256; x++)
            //    {
            //        Console.Write("{0:x}", GetHalf(x, y, 256, 32, ADDR_MAP));
            //    }
            //    Console.Write("\n");
            //}
            //Console.Write("\n");
            //for (int y = 32; y < 64; y++)
            //{
            //    for (int x = 0; x < 256; x++)
            //    {
            //        Console.Write("{0:x}", GetHalf(x, y % 32, 256, 32, ADDR_GFX_MAP));
            //    }
            //    Console.Write("\n");
            //}
        }
    }
}
