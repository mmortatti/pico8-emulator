namespace pico8_interpreter.Pico8
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

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
                         ADDR_SFX = 0x3200,
                         ADDR_META = 0x8000;

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
        public Cartridge(string path, bool createNew = false)
        {
            rom = new byte[0x8005];

            gamePath = "Pico8/Games/" + path;
            if (createNew && !File.Exists(gamePath))
            {
                File.Create(gamePath).Close();
            }
            LoadP8(gamePath);
            gameCode = util.ProcessPico8Code(gameCode);
        }

        /// <summary>
        /// Saves cartridge ROM to a P8 file.
        /// </summary>
        /// <param name="filename">The filename to write to.<see cref="string"/></param>
        public void SaveP8(string filename = null)
        {
            if (filename == null)
            {
                filename = gamePath;
            }

            var fs = new FileStream(filename, FileMode.OpenOrCreate);
            using (StreamWriter file = new StreamWriter(fs))
            {
                file.WriteLine("pico-8 cartridge // http://www.pico-8.com");
                file.WriteLine($"version {rom[ADDR_META]}");

                file.WriteLine("__lua__");
                file.WriteLine(gameCode);

                file.WriteLine("__gfx__");
                for (int j = 0; j < 128; j += 1)
                {
                    for (int i = 0; i < 64; i += 1)
                    {
                        byte left = util.GetHalf(rom[j * 64 + i + ADDR_GFX], false);
                        byte right = util.GetHalf(rom[j * 64 + i + ADDR_GFX], true);
                        file.Write($"{right.ToString("x")}{left.ToString("x")}");
                    }
                    file.Write("\n");
                }

                file.WriteLine("__gff__");
                for (int j = 0; j < 2; j += 1)
                {
                    for (int i = 0; i < 128; i += 1)
                    {
                        byte left = util.GetHalf(rom[j * 128 + i + ADDR_GFX_PROPS], false);
                        byte right = util.GetHalf(rom[j * 128 + i + ADDR_GFX_PROPS], true);
                        file.Write($"{left.ToString("x")}{right.ToString("x")}");
                    }
                    file.Write("\n");
                }

                file.WriteLine("__map__");
                for (int j = 0; j < 64; j += 1)
                {
                    for (int i = 0; i < 64; i += 1)
                    {
                        byte left = util.GetHalf(rom[j * 64 + i + ADDR_MAP], false);
                        byte right = util.GetHalf(rom[j * 64 + i + ADDR_MAP], true);
                        file.Write($"{left.ToString("x")}{right.ToString("x")}");
                    }
                    file.Write("\n");
                }

                file.WriteLine("__sfx__");
                for (int j = 0; j < 64; j += 1)
                {
                    for (int i = 0; i < 84; i += 1)
                    {
                        byte left = util.GetHalf(rom[j * 84 + i + ADDR_SFX], false);
                        byte right = util.GetHalf(rom[j * 84 + i + ADDR_SFX], true);
                        file.Write($"{right.ToString("x")}{left.ToString("x")}");
                    }
                    file.Write("\n");
                }

                file.WriteLine("__music__");
                for (int j = 0; j < 64; j += 1)
                {
                    byte flag = 0;
                    byte val0 = rom[j * 4 + 0 + ADDR_SONG];
                    byte val1 = rom[j * 4 + 1 + ADDR_SONG];
                    byte val2 = rom[j * 4 + 2 + ADDR_SONG];
                    byte val3 = rom[j * 4 + 3 + ADDR_SONG];

                    if ((val0 & 0x80) == 0x80)
                    {
                        flag = 1;
                        val0 &= 0x7F;
                    }
                    else if ((val1 & 0x80) == 0x80)
                    {
                        flag = 2;
                        val1 &= 0x7F;
                    }
                    else if ((val2 & 0x80) == 0x80)
                    {
                        flag = 4;
                        val2 &= 0x7F;
                    }

                    file.Write($"{flag.ToString("D2")} {val0.ToString("x2")}{val1.ToString("x2")}{val2.ToString("x2")}{val3.ToString("x2")}\n");
                }

                file.Close();
            }
        }

        /// <summary>
        /// Load P8 file.
        /// </summary>
        /// <param name="path">The path to the P8 file.<see cref="string"/></param>
        private void LoadP8(string path)
        {
            var streamReader = new StreamReader(path);

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

                if (state == -1)
                {
                    if (Regex.IsMatch(line, @"[vV]ersion\ *"))
                    {
                        line = Regex.Replace(line, @"[vV]ersion\ *", "");
                        rom[ADDR_META] = byte.Parse(line, System.Globalization.NumberStyles.Integer);
                    }
                }
                else if (state == stateMap["__lua__"])
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
                    if (Regex.IsMatch(line, @"^\s*$"))
                    {
                        continue;
                    }

                    byte editor = byte.Parse(line.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    byte speed = byte.Parse(line.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    byte startLoop = byte.Parse(line.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    byte endLoop = byte.Parse(line.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);

                    rom[ADDR_SFX + index * 68 + 64] = editor;
                    rom[ADDR_SFX + index * 68 + 65] = speed;
                    rom[ADDR_SFX + index * 68 + 66] = startLoop;
                    rom[ADDR_SFX + index * 68 + 67] = editor;

                    int off = 0;
                    for (int i = 0; i < line.Length - 8; i += 5)
                    {
                        byte pitch = byte.Parse(line.Substring(i + 8, 2), System.Globalization.NumberStyles.HexNumber);
                        byte waveform = byte.Parse(line.Substring(i + 8 + 2, 1), System.Globalization.NumberStyles.HexNumber);
                        byte volume = byte.Parse(line.Substring(i + 8 + 3, 1), System.Globalization.NumberStyles.HexNumber);
                        byte effect = byte.Parse(line.Substring(i + 8 + 4, 1), System.Globalization.NumberStyles.HexNumber);

                        byte lo = (byte)(pitch | (waveform << 6));
                        byte hi = (byte)((waveform >> 2) | (volume << 1) | (effect << 4));

                        rom[ADDR_SFX + index * 68 + off] = lo;
                        rom[ADDR_SFX + index * 68 + off + 1] = hi;
                        off += 2;
                    }

                    index += 1;
                }
                else if (state == stateMap["__music__"])
                {
                    if (Regex.IsMatch(line, @"^\s*$"))
                    {
                        continue;
                    }

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

            streamReader.Close();
        }
    }
}
