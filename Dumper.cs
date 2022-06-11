using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;
using CSGO_Offset_Dumper.dwGetAllClasses;
using Spectre.Console;

namespace CSGO_Offset_Dumper
{
    internal static class Dumper
    {
        //Netvar and Signature Dumpers
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
                writer.WriteLine($"namespace {AppConfig.CurrentConfig.ExportNamespace} {{");
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

                writer.Write($"}} // namespace {AppConfig.CurrentConfig.ExportNamespace}");

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
                writer.WriteLine($"namespace {AppConfig.CurrentConfig.ExportNamespace}");
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
            File.WriteAllText(filename + ".min.json", JsonSerializer.Serialize(jsonNode));


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


        public static void DumpCheatTable(Dictionary<string, int> netvars, Dictionary<string, int> signatures, string filename, dwGetAllClasses.ClassExporter.SourceClassRoot allClasses)
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

                //End signatures


                //LocalPlayer
                writer.WriteLine("<CheatEntry>");
                writer.WriteLine("<ID>6</ID>");
                writer.WriteLine("<Description>\"Local Player\"</Description>");
                writer.WriteLine("<Options moManualExpandCollapse=\"1\"/>");
                writer.WriteLine("<LastState Value=\"\" RealAddress=\"00000000\"/>");
                writer.WriteLine("<Color>C22925</Color>");
                writer.WriteLine("<GroupHeader>1</GroupHeader>");


                //Entries in LocalPlayer
                writer.WriteLine("<CheatEntries>");

                var AllClasses = allClasses.SourceClass.OrderBy(x => x.Offset).ToList();
                foreach (var offset in AllClasses)
                {
                    if (AppConfig.CurrentConfig.LocalPlayerClasses.Any(offset.ClassName.Contains))
                    {
                        GenerateDynamicCheatEntry(AllClasses, offset, writer);
                    }

                }
                writer.WriteLine($"</CheatEntries>");
                writer.WriteLine($"</CheatEntry>");

                //End LocalPlayer

                writer.WriteLine($"</CheatEntries>");
                writer.WriteLine($"<UserdefinedSymbols>");

                List<string> AddedOffsets = new List<string>();

                foreach (var item in netvars)
                {
                    if (!AddedOffsets.Contains(item.Key))
                    {
                        writer.WriteLine($"<SymbolEntry>");
                        writer.WriteLine($"<Name>{item.Key}</Name>");
                        writer.WriteLine($"<Address>0x{item.Value.ToString("X")}</Address>");
                        writer.WriteLine($"</SymbolEntry>");
                        AddedOffsets.Add(item.Key);
                    }
                }

                foreach (var item in signatures)
                {
                    if (!AddedOffsets.Contains(item.Key))
                    {
                        writer.WriteLine($"<SymbolEntry>");
                        writer.WriteLine($"<Name>{item.Key}</Name>");
                        writer.WriteLine($"<Address>0x{item.Value.ToString("X")}</Address>");
                        writer.WriteLine($"</SymbolEntry>");
                        AddedOffsets.Add(item.Key);
                    }
                }

                foreach (var item in allClasses.SourceClass)
                {
                    if (!AddedOffsets.Contains(item.VariableName))
                    {
                        writer.WriteLine($"<SymbolEntry>");
                        writer.WriteLine($"<Name>{item.VariableName}</Name>");
                        writer.WriteLine($"<Address>0x{item.Offset.ToString("X")}</Address>");
                        writer.WriteLine($"</SymbolEntry>");
                    }
                }


                writer.WriteLine($"</UserdefinedSymbols>");


                writer.Write($"</CheatTable>");

                writer.Flush();
            }

            AnsiConsole.MarkupLine($"[blue]Dumped to [green]{filename}[/][/]");
        }


        //All netvar dumpers
        public static void DumpJson(dwGetAllClasses.ClassExporter.SourceClassRoot allClasses)
        {
            string filename = "Classes\\netvardump";
            Directory.CreateDirectory("Classes");

            File.WriteAllText(filename + ".json", JsonSerializer.Serialize(allClasses.SourceClass, new JsonSerializerOptions() { WriteIndented = true }));
            File.WriteAllText(filename + ".min.json", JsonSerializer.Serialize(allClasses.SourceClass));

            AnsiConsole.MarkupLine($"[blue]Dumped to [green]{filename}.json[/][/]");
            AnsiConsole.MarkupLine($"[blue]Dumped to [green]{filename}.min.json[/][/]");

        }

        public static void DumpCPP(ClassExporter.SourceClassRoot allClasses)
        {
            string filename = "Classes\\netvardump.hpp";
            Directory.CreateDirectory("Classes");

            using (StreamWriter writer = File.CreateText(filename))
            {
                writer.WriteLine("#pragma once");
                writer.WriteLine("#include <cstdint>");
                writer.WriteLine("#include <string>");
                writer.WriteLine("");
                writer.WriteLine($"// {DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:tt")} UTC");
                writer.WriteLine("");

                writer.WriteLine("#define STR_MERGE_IMPL(a, b) a##b");
                writer.WriteLine("#define STR_MERGE(a, b) STR_MERGE_IMPL(a, b)");
                writer.WriteLine("#define MAKE_PAD(size) STR_MERGE(_pad, __COUNTER__)[size]");
                writer.WriteLine("#define DEFINE_MEMBER_N(type, name, offset) struct {unsigned char MAKE_PAD(offset); type name;}");
                writer.WriteLine("struct Vector3 { float x, y, z; };");

                writer.WriteLine("");
                writer.WriteLine($"namespace {AppConfig.CurrentConfig.ExportNamespace}");
                writer.WriteLine("{");
                writer.WriteLine($"	constexpr int64_t timestamp = {DateTimeOffset.UtcNow.ToUnixTimeSeconds()};");

                string previousClass = "";

                List<dwGetAllClasses.ClassExporter.SourceClass> AllClasses = allClasses.SourceClass.OrderBy(x => x.ClassName).ToList();
                for (int i = 0; i < AllClasses.Count(); i++)
                {
                    string currentClassName = AllClasses[i].ClassName;
                    string offset = $"0x{AllClasses[i].Offset.ToString("X")}";
                    if (previousClass != currentClassName)
                    {
                        writer.WriteLine($"	class {currentClassName}");
                        writer.WriteLine("	{");
                        writer.WriteLine("	public:");
                        writer.WriteLine("		union");
                        writer.WriteLine("		{");
                    }

                    string VarToType = VariableNameToDataType(AllClasses[i].VariableName);


                    writer.WriteLine($"			DEFINE_MEMBER_N({VarToType}, {AllClasses[i].VariableName.Replace(".", "")},{offset} );");


                    if (i + 1 < AllClasses.Count && currentClassName != AllClasses[i + 1].ClassName)
                    {
                        writer.WriteLine("		};");
                        writer.WriteLine("	};");
                    }
                    else if (i == AllClasses.Count - 1)//Add the last item properly
                    {
                        writer.WriteLine("		};");
                        writer.WriteLine("	};");
                    }
                    previousClass = currentClassName;
                }


                writer.Write($"}} // namespace {AppConfig.CurrentConfig.ExportNamespace}");

                writer.Flush();
            }
            AnsiConsole.MarkupLine($"[blue]Dumped to [green]{filename}[/][/]");
        }



        internal static string VariableNameToDataType(string varName)
        {
            if (varName.StartsWith("m_b"))
                return "char";
            if (varName.StartsWith("m_i") || varName.StartsWith("m_n"))
                return "int";

            if (varName.StartsWith("m_sz"))
                return "std::string";
            if (varName.StartsWith("m_fl"))
                return "float";

            if (varName.StartsWith("m_vec"))
                return "Vector3";




            return "int";
        }

        internal static string VariableNameToCEDataType(string varName)
        {
            if (varName.StartsWith("m_b"))
                return "Byte";
            if (varName.StartsWith("m_i") || varName.StartsWith("m_n") || varName.StartsWith("int"))
                return "4 Bytes";

            if (varName.StartsWith("m_sz"))
                return "String";
            if (varName.StartsWith("m_fl"))
                return "Float";

            if (varName.StartsWith("m_vec"))
                return "Vector3";




            return "4 Bytes";
        }


        private static void GenerateDynamicCheatEntry(List<ClassExporter.SourceClass> AllClasses, ClassExporter.SourceClass offset, StreamWriter writer)
        {
            int index = AllClasses.ToList().IndexOf(offset) * 1000;
            string VariableType = VariableNameToCEDataType(offset.VariableName);

            if (VariableType != "Vector3")
            {
                writer.WriteLine("<CheatEntry>");
                writer.WriteLine($"<ID>{index}</ID>");
                writer.WriteLine($"<Description>\"{offset.VariableName}\"</Description>");
                writer.WriteLine($"<ShowAsSigned>0</ShowAsSigned>");
                writer.WriteLine($"<VariableType>{VariableType}</VariableType>");
                writer.WriteLine($"<Address>[client.dll + dwLocalPlayer] + {offset.VariableName}</Address>");
                writer.WriteLine($"</CheatEntry>");
            }
            else
            {
                //X
                writer.WriteLine("<CheatEntry>");
                writer.WriteLine($"<ID>{index}</ID>");
                writer.WriteLine($"<Description>\"{offset.VariableName} (X)\"</Description>");
                writer.WriteLine($"<ShowAsSigned>0</ShowAsSigned>");
                writer.WriteLine($"<VariableType>Float</VariableType>");
                writer.WriteLine($"<Address>[client.dll + dwLocalPlayer] + {offset.VariableName}</Address>");
                writer.WriteLine($"</CheatEntry>");

                //Y
                writer.WriteLine("<CheatEntry>");
                writer.WriteLine($"<ID>{index}</ID>");
                writer.WriteLine($"<Description>\"{offset.VariableName} (Y)\"</Description>");
                writer.WriteLine($"<ShowAsSigned>0</ShowAsSigned>");
                writer.WriteLine($"<VariableType>Float</VariableType>");
                writer.WriteLine($"<Address>[client.dll + dwLocalPlayer] + {offset.VariableName} + 0x4</Address>");
                writer.WriteLine($"</CheatEntry>");

                //Z
                writer.WriteLine("<CheatEntry>");
                writer.WriteLine($"<ID>{index}</ID>");
                writer.WriteLine($"<Description>\"{offset.VariableName} (Z)\"</Description>");
                writer.WriteLine($"<ShowAsSigned>0</ShowAsSigned>");
                writer.WriteLine($"<VariableType>4 Bytes</VariableType>");
                writer.WriteLine($"<Address>[client.dll + dwLocalPlayer] + {offset.VariableName} + 0x8</Address>");
                writer.WriteLine($"</CheatEntry>");
            }


        }
    }
}
