using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using Spectre.Console;

namespace CSGO_Offset_Dumper
{
    internal static class Dumper
    {
        public static void DumpCPP(Dictionary<string, int> netvars, Dictionary<string, int> signatures, string filename)
        {
            filename += ".hpp";//Add file extension
            using (StreamWriter writer = File.CreateText(filename))
            {
                writer.WriteLine("#pragma once");
                writer.WriteLine("#include <cstdint>");
                writer.WriteLine("");
                writer.WriteLine($"// {DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:tt")} UTC");
                writer.WriteLine("");
                writer.WriteLine("namespace kyedumper {");
                writer.WriteLine($"constexpr int64_t timestamp = {DateTimeOffset.UtcNow.ToUnixTimeSeconds()};");
                writer.WriteLine($"namespace netvars {{");


                foreach (var netvar in netvars.OrderBy(x => x.Key))
                {
                    writer.WriteLine($"constexpr uintptr_t {netvar.Key} = 0x{netvar.Value.ToString("X")};");
                }

                writer.WriteLine($"}} // namespace netvars");

                writer.WriteLine($"namespace signatures {{");

                foreach (var sig in signatures.OrderBy(x => x.Key))
                {
                    writer.WriteLine($"constexpr uintptr_t {sig.Key} = 0x{sig.Value.ToString("X")};");
                }

                writer.WriteLine($"}} // namespace signatures");

                writer.Write($"}} // namespace kyedumper");

                writer.Flush();
            }
            AnsiConsole.MarkupLine($"[blue]Dumped to [green]{filename}[/][/]");
        }

        public static void DumpCSharp(Dictionary<string, int> netvars, Dictionary<string, int> signatures, string filename)
        {
            filename += ".cs";//Add file extension
            using (StreamWriter writer = File.CreateText(filename))
            {
                writer.WriteLine("using System;");
                writer.WriteLine("");
                writer.WriteLine($"// {DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:tt")} UTC");
                writer.WriteLine("");
                writer.WriteLine("namespace kyedumper");
                writer.WriteLine("{");
                writer.WriteLine("    public static class info");
                writer.WriteLine("    {");
                writer.WriteLine($"        public static DateTime timestamp = DateTimeOffset.FromUnixTimeSeconds({DateTimeOffset.Now.ToUnixTimeSeconds()}).DateTime;");
                writer.WriteLine("    }");
                writer.WriteLine("    public static class netvars");
                writer.WriteLine("    {");


                foreach (var netvar in netvars.OrderBy(x => x.Key))
                {
                    writer.WriteLine($"        public const int {netvar.Key} = 0x{netvar.Value.ToString("X")};");
                }

                writer.WriteLine("    }");

                writer.WriteLine("    public static class signatures");
                writer.WriteLine("    {");

                foreach (var sig in signatures.OrderBy(x => x.Key))
                {
                    writer.WriteLine($"        public const int {sig.Key} = 0x{sig.Value.ToString("X")};");
                }

                writer.Write("    }\n}");

                writer.Flush();
            }

            AnsiConsole.MarkupLine($"[blue]Dumped to [green]{filename}[/][/]");
        }

        public static void DumpJson(Dictionary<string, int> netvars, Dictionary<string, int> signatures, string filename)
        {
            JsonNode jsonNode = JsonNode.Parse("{}");
            jsonNode["timestamp"] = DateTimeOffset.Now.ToUnixTimeSeconds();
            jsonNode["netvars"] = JsonNode.Parse(JsonSerializer.Serialize(netvars));
            jsonNode["signatures"] = JsonNode.Parse(JsonSerializer.Serialize(signatures));

            File.WriteAllText(filename + ".json", JsonSerializer.Serialize(jsonNode, new JsonSerializerOptions() { WriteIndented = true }));
            File.WriteAllText(filename + "min.json", JsonSerializer.Serialize(jsonNode));


            AnsiConsole.MarkupLine($"[blue]Dumped to [green]{filename}.json[/][/]");
            AnsiConsole.MarkupLine($"[blue]Dumped to [green]{filename}.min.json[/][/]");
        }

        public static void DumpTOML(Dictionary<string, int> netvars, Dictionary<string, int> signatures, string filename)
        {
            filename += ".toml";//Add file extension
            using (StreamWriter writer = File.CreateText(filename))
            {
                writer.WriteLine($"timestamp = {DateTimeOffset.Now.ToUnixTimeSeconds()}");
                writer.WriteLine("");
                writer.WriteLine("[netvars]");
                foreach (var netvar in netvars.OrderBy(x => x.Key))
                {
                    writer.WriteLine($"{netvar.Key} = {netvar.Value}");
                }

                writer.WriteLine("[signatures]");
                foreach (var sig in signatures.OrderBy(x => x.Key))
                {
                    writer.WriteLine($"{sig.Key} = {sig.Value}");
                }
                writer.Flush();
            }

            AnsiConsole.MarkupLine($"[blue]Dumped to [green]{filename}[/][/]");
        }


        public static void DumpCheatTable(Dictionary<string, int> netvars, Dictionary<string, int> signatures, string filename)
        {
            filename += ".ct";//Add file extension



            using (StreamWriter writer = File.CreateText(filename))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                writer.WriteLine("<CheatTable CheatEngineTableVersion=\"31\">");
                writer.WriteLine("<CheatEntries>");

                //Blank Row
                writer.WriteLine("<CheatEntry>");
                writer.WriteLine("<ID>0</ID>");
                writer.WriteLine("<Description>\"\"</Description>");
                writer.WriteLine("<LastState Value=\"\" RealAddress=\"00000000\"/>");
                writer.WriteLine("<GroupHeader>1</GroupHeader>");
                writer.WriteLine("</CheatEntry>");


                //Dumped by KyeDumper with date
                writer.WriteLine("<CheatEntry>");
                writer.WriteLine("<ID>1</ID>");
                writer.WriteLine($"<Description>\"Dumped by KyeDumper on {DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:tt")} UTC\"</Description>");
                writer.WriteLine("<LastState Value=\"\" RealAddress=\"00000000\"/>");
                writer.WriteLine("<Color>C8450D</Color>");
                writer.WriteLine("<GroupHeader>1</GroupHeader>");
                writer.WriteLine("</CheatEntry>");

                //Blank Row
                writer.WriteLine("<CheatEntry>");
                writer.WriteLine("<ID>3</ID>");
                writer.WriteLine("<Description>\"\"</Description>");
                writer.WriteLine("<LastState Value=\"\" RealAddress=\"00000000\"/>");
                writer.WriteLine("<GroupHeader>1</GroupHeader>");
                writer.WriteLine("</CheatEntry>");

                //Netvars
                writer.WriteLine("<CheatEntry>");
                writer.WriteLine("<ID>4</ID>");
                writer.WriteLine("<Description>\"Netvars\"</Description>");
                writer.WriteLine("<Options moManualExpandCollapse=\"1\"/>");
                writer.WriteLine("<LastState Value=\"\" RealAddress=\"00000000\"/>");
                writer.WriteLine("<Color>0000FF</Color>");
                writer.WriteLine("<GroupHeader>1</GroupHeader>");

                //Entries in netvars
                writer.WriteLine("<CheatEntries>");

                var SortedNetVars = netvars.OrderBy(x => x.Key);
                foreach (var netvar in SortedNetVars)
                {
                    int index = SortedNetVars.ToList().IndexOf(netvar) * 10;//Just make the netvars end with `0`
                    writer.WriteLine("<CheatEntry>");
                    writer.WriteLine($"<ID>{index}</ID>");
                    writer.WriteLine($"<Description>\"{netvar.Key}\"</Description>");
                    writer.WriteLine($"<ShowAsSigned>0</ShowAsSigned>");
                    writer.WriteLine($"<VariableType>4 Bytes</VariableType>");
                    writer.WriteLine($"<Address>{netvar.Key}</Address>");
                    writer.WriteLine($"</CheatEntry>");
                }
                writer.WriteLine($"</CheatEntries>");
                writer.WriteLine($"</CheatEntry>");

                //End netvars

                //Signatures
                writer.WriteLine("<CheatEntry>");
                writer.WriteLine("<ID>5</ID>");
                writer.WriteLine("<Description>\"Signatures\"</Description>");
                writer.WriteLine("<Options moManualExpandCollapse=\"1\"/>");
                writer.WriteLine("<LastState Value=\"\" RealAddress=\"00000000\"/>");
                writer.WriteLine("<Color>0000FF</Color>");
                writer.WriteLine("<GroupHeader>1</GroupHeader>");

                //Entries in signatures
                writer.WriteLine("<CheatEntries>");

                var SortedSigs = signatures.OrderBy(x => x.Key);
                foreach (var sig in SortedSigs)
                {
                    int index = SortedNetVars.ToList().IndexOf(sig) * 100;//Just make the netvars end with `0`
                    writer.WriteLine("<CheatEntry>");
                    writer.WriteLine($"<ID>{index}</ID>");
                    writer.WriteLine($"<Description>\"{sig.Key}\"</Description>");
                    writer.WriteLine($"<ShowAsSigned>0</ShowAsSigned>");
                    writer.WriteLine($"<VariableType>4 Bytes</VariableType>");
                    writer.WriteLine($"<Address>{sig.Key}</Address>");
                    writer.WriteLine($"</CheatEntry>");
                }
                writer.WriteLine($"</CheatEntries>");
                writer.WriteLine($"</CheatEntry>");

                //End netvars

                writer.WriteLine($"</CheatEntries>");
                writer.WriteLine($"<UserdefinedSymbols>");

                foreach (var item in netvars)
                {
                    writer.WriteLine($"<SymbolEntry>");
                    writer.WriteLine($"<Name>{item.Key}</Name>");
                    writer.WriteLine($"<Address>0x{item.Value.ToString("X")}</Address>");
                    writer.WriteLine($"</SymbolEntry>");
                }

                foreach (var item in signatures)
                {
                    writer.WriteLine($"<SymbolEntry>");
                    writer.WriteLine($"<Name>{item.Key}</Name>");
                    writer.WriteLine($"<Address>0x{item.Value.ToString("X")}</Address>");
                    writer.WriteLine($"</SymbolEntry>");
                }


                writer.WriteLine($"</UserdefinedSymbols>");


                writer.Write($"</CheatTable>");

                writer.Flush();
            }

            AnsiConsole.MarkupLine($"[blue]Dumped to [green]{filename}[/][/]");
        }
    }
}
