using System.Diagnostics;
using System.Net;
using System.Text.Json;
using CSGO_Offset_Dumper.JsonClasses;
using Spectre.Console;
namespace CSGO_Offset_Dumper
{
    internal class Program
    {
        const string configFilePath = "config.json";
        public static int ProcessID { get; private set; }
        private static int dwGetAllClassesOffset => Signatures.FirstOrDefault(x => x.Key.Equals("dwGetAllClasses")).Value;

        private static Dictionary<string, int> Signatures = new();
        private static Dictionary<string, int> Netvars = new();

        private const string clientdll = "client.dll";


        static void Main(string[] args)
        {
            Console.Title = "CSGO Offset Dumper C# | Made by KyeOnDiscord";
            AppConfig.InitConfig();
            AnsiConsole.Write(new FigletText("CSGO Offset Dumper").LeftAligned().Color(Color.Blue));

            if (!File.Exists(configFilePath))
            {
                AnsiConsole.MarkupLine($"[red][[Error]] Could not find {configFilePath}! Downloading config from {AppConfig.CurrentConfig.FallbackConfigURL}![/]");
                new WebClient().DownloadFile(AppConfig.CurrentConfig.FallbackConfigURL, configFilePath);
            }


            Config.Rootobject? config = JsonSerializer.Deserialize<Config.Rootobject>(File.ReadAllText(configFilePath));

            AnsiConsole.MarkupLine($"[purple]Loading [green]{config.netvars.Length}[/] netvars and [green]{config.signatures.Length}[/] signatures[/]");

            AnsiConsole.MarkupLine($"[grey]Trying to find {config.executable}[/]");

            Process[] csgoproc = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(config.executable));

            if (csgoproc.Length > 0)
            {
                ProcessID = csgoproc[0].Id;
                AnsiConsole.MarkupLine($"[green]Found {csgoproc[0].ProcessName}.exe -> {ProcessID}[/]");

                AnsiConsole.MarkupLine("[grey]Trying to get process handle[/]");

                Win32.ProcessHandle = Win32.OpenProcess(0x0008 | 0x0010 | 0x0020, false, ProcessID);

                if (Win32.ProcessHandle != IntPtr.Zero)
                {
                    AnsiConsole.MarkupLine($"[green]Found Proccess Handle -> 0x{Win32.ProcessHandle.ToString("X")}[/]");

                    AnsiConsole.MarkupLine($"[grey]Searching for {clientdll}[/]");
                    var clientModule = Win32.GetModule((IntPtr)ProcessID, clientdll);
                    if (clientModule != null)
                    {
                        string path = clientModule.Value.szExePath;

                        AnsiConsole.MarkupLine($"[green]Found {clientdll} -> {path}[/]");

                        AnsiConsole.MarkupLine($"[grey]Trying to load {clientdll}[/]");
                        IntPtr internalClientDll = Win32.LoadLibrary(path);
                        if (internalClientDll != IntPtr.Zero)
                        {
                            AnsiConsole.MarkupLine($"[green]Loaded {clientdll} -> 0x{internalClientDll}[/]");

                            PatternScan.GetSignatureOffsets(config.signatures, ref Signatures);

                            AnsiConsole.MarkupLine($"[green]Finished getting signatures![/]");

                            IntPtr dwGetallClassesAddr = internalClientDll + dwGetAllClassesOffset;

                            var allClasses = dwGetAllClasses.ClassExporter.GetAllClasses(dwGetallClassesAddr);

                            SDK.Netvar.GetNetvarOffsets(config.netvars, ref Netvars, dwGetallClassesAddr);


                            AnsiConsole.MarkupLine($"[green]Found [blue]{Netvars.Count}/{config.netvars.Length}[/] netvars and [blue]{Signatures.Count}/{config.signatures.Length}[/] signatures[/]");

                            //Dumps netvars and signatures
                            Dumper.DumpCPP(Netvars, Signatures, config.filename);
                            Dumper.DumpCSharp(Netvars, Signatures, config.filename);
                            Dumper.DumpJson(Netvars, Signatures, config.filename);
                            Dumper.DumpTOML(Netvars, Signatures, config.filename);
                            Dumper.DumpCheatTable(Netvars, Signatures, config.filename, allClasses);


                            //Dumps all netvar classes
                            Dumper.DumpJson(allClasses);
                            Dumper.DumpCPP(allClasses);


                            AnsiConsole.MarkupLine("");
                            AnsiConsole.MarkupLine("");
                            AnsiConsole.MarkupLine($"[bold underline green]Finished![/]");
                        }
                    }
                }
            }
            else
            {
                AnsiConsole.MarkupLine($"[red][[Error]] Could not find {config.executable}! Maybe try opening the game?[/]");
            }

            Console.ReadLine();

        }
    }
}