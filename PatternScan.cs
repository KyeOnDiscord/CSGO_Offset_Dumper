using System.Globalization;
using static CSGO_Offset_Dumper.Win32;
using Spectre.Console;
namespace CSGO_Offset_Dumper
{
    //https://guidedhacking.com/threads/simple-c-pattern-scan.13981/
    internal class PatternScan
    {
        internal static void GetSignatureOffsets(JsonClasses.Config.Signature[] SignatureConfig, ref Dictionary<string, int> Signatures)
        {
            foreach (var sig in SignatureConfig)
            {
                if (Signatures.ContainsKey(sig.name))//Remove duplicates (some configs might have duplicates)
                    continue;

                var mod = (MODULEENTRY32)Win32.GetModule((IntPtr)Program.ProcessID, sig.module);

                int offset = (int)PatternScanMod(mod, sig.pattern);


                if (offset == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Could not find {sig.name}");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    continue;
                }


                //Offsets
                foreach (int sigoffset in sig.offsets)
                {
                    offset += sigoffset;

                    offset = Win32.ReadMemory<int>(offset);
                }

                //Extra
                if (sig.extra > 0)
                {
                    offset += sig.extra;
                }

                //Relative
                if (sig.relative)
                {
                    offset -= (int)mod.modBaseAddr;
                }
                AnsiConsole.MarkupLine($"[grey]Found signature [blue]{sig.name}[/] -> [blue]0x{offset:X}[/][/]");
                Signatures.Add(sig.name, offset);
            }
        }



        public static bool CheckPattern(string pattern, byte[] array2check)
        {
            string[] strBytes = pattern.Split(' ');
            int x = 0;
            foreach (byte b in array2check)
            {
                if (strBytes[x] == "?" || strBytes[x] == "??")
                {
                    x++;
                }
                else if (byte.Parse(strBytes[x], NumberStyles.HexNumber) == b)
                {
                    x++;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }


        public static IntPtr PatternScanMod(Win32.MODULEENTRY32 pMod, string pattern)
        {
            try
            {
                byte[] module = ReadModule(pMod);

                int offset = ScanBasic(pattern, module);

                if (offset == -1)
                {
                    return IntPtr.Zero;
                    // throw new Exception("Pattern could not be found in module " + pMod.szModule);
                }

                //Return it with the full address to resolve offsets, remove it later if its relative
                return (IntPtr)(offset + (int)pMod.modBaseAddr);

            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }


        public static int ScanBasic(string pattern, byte[] buffer)
        {
            string[] pBytes = pattern.Split(' ');

            for (int y = 0; y < buffer.Length; y++)
            {
                if (buffer[y] == byte.Parse(pBytes[0], NumberStyles.HexNumber))
                {
                    byte[] checkArray = new byte[pBytes.Length];
                    for (int x = 0; x < pBytes.Length; x++)
                    {
                        checkArray[x] = buffer[y + x];
                    }
                    if (CheckPattern(pattern, checkArray))
                    {
                        return y;
                    }
                    //else
                    //{
                    //    //Sometimes the pattern might be inside the wrong checkArray but the start of the right pattern is inside checkArray
                    //    //y += pBytes.Length - (pBytes.Length / 2);
                    //}
                }
            }

            return -1;
        }
    }
}
