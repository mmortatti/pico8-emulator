namespace pico8_interpreter.Pico8
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Defines the PICO-8 Cartridge.<see cref="Cartridge" />
    /// </summary>
    public class Cartridge
    {
        /// <summary>
        /// Defines some of the memory regions for the Cartridge's ROM.
        /// </summary>
        public const int ADDR_GFX = 0x0,
                         ADDR_GFX_MAP = 0x1000,
                         ADDR_MAP = 0x2000,
                         ADDR_GFX_PROPS = 0x3000,
                         ADDR_SONG = 0x3100,
                         ADDR_SFX = 0x3200;

        /// <summary>
        /// Defines the cartridges rom.
        /// </summary>
        public readonly byte[] rom;

        /// <summary>
        /// Defines the lua game code.
        /// </summary>
        public string gameCode { get; private set; } = "";

        /// <summary>
        /// Defines the path to the lua cartridge.
        /// </summary>
        private string gamePath = "";

        /// <summary>
        /// Initializes a new instance of the <see cref="Cartridge"/> class.
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        public Cartridge(string path)
        {
            rom = new byte[0x8005];

            gamePath = path;
            LoadP8(gamePath);
            Console.WriteLine(gameCode);
            gameCode = util.ProcessPico8Code(gameCode);
            Console.WriteLine(gameCode);
        }

        /// <summary>
        /// Saves cartridge ROM to a P8 file.
        /// </summary>
        /// <param name="filename">The filename to write to.<see cref="string"/></param>
        public void SaveP8(string filename = "")
        {
        }

        /// <summary>
        /// Load P8 file.
        /// </summary>
        /// <param name="path">The path to the P8 file.<see cref="string"/></param>
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

            gameCode = "";
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
                    gameCode += line + '\n';
                }
                else if (state == stateMap["__gfx__"])
                {
                    foreach (char c in line)
                    {
                        byte val = byte.Parse(c.ToString(), System.Globalization.NumberStyles.HexNumber);
                        util.SetHalf(ref rom[index / 2 + ADDR_GFX], val, index % 2 == 0);
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
                        util.SetHalf(ref rom[index / 2 + ADDR_SFX], val, index % 2 == 0);
                        index += 1;
                    }
                }
                else if (state == stateMap["__music__"])
                {
                    byte flag = byte.Parse(line.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    byte val1 = byte.Parse(line.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                    byte val2 = byte.Parse(line.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                    byte val3 = byte.Parse(line.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
                    byte val4 = byte.Parse(line.Substring(9, 2), System.Globalization.NumberStyles.HexNumber);

                    // 4th byte never has 7th bit set because it's corresponding flag value is never used.
                    switch (flag)
                    {
                        case 1:
                            val1 |= 0x80;
                            break;
                        case 2:
                            val2 |= 0x80;
                            break;
                        case 4:
                            val3 |= 0x80;
                            break;
                    }

                    rom[ADDR_SONG + index + 0] = val1;
                    rom[ADDR_SONG + index + 1] = val2;
                    rom[ADDR_SONG + index + 2] = val3;
                    rom[ADDR_SONG + index + 3] = val4;
                    index += 4;
                }
            }
        }
    }
}
