using static CSGO_Offset_Dumper.SDK.SourceSDK;
using Spectre.Console;
using System.IO;
namespace CSGO_Offset_Dumper.SDK
{
    internal static class Netvar
    {
        internal static int GetNetVarOffset(string tableName, string netvarName, IntPtr clientClass)
        {
            for (IntPtr currNode = clientClass; currNode != IntPtr.Zero; currNode = currNode.Deference<ClientClass>().m_pNext)
            {
                ClientClass? node = currNode.Deference<ClientClass>();

                RecvTable? table = node.m_pRecvTable.Deference<RecvTable>();

                if (tableName == table.m_pNetTableName)
                {
                    return GetOffset(node.m_pRecvTable, tableName, netvarName);
                }
            }
            return 0;
        }




        internal static int GetOffset(IntPtr ptable, string tableName, string netvarName)
        {
            RecvTable? table = ptable.Deference<RecvTable>();
            for (int i = 0; i < table.m_nProps; i++)
            {
                //Size of RecvProp is 60 bytes (using sizeof(RecvTable in cpp) (Using sizeof in C# has different string sizes so it doesn't work)
                IntPtr propAddress = table.m_pProps + i * 60;

                RecvProp? prop = (propAddress).Deference<RecvProp>();
                if (prop is not null)
                {
                    if (prop.m_pVarName == netvarName)
                    {
                        return prop.m_Offset;
                    }

                    if (prop.m_pDataTable != IntPtr.Zero)
                    {
                        int offset = GetOffset(prop.m_pDataTable, tableName, netvarName);
                        if (offset > 0)
                        {
                            return offset + prop.m_Offset;
                        }
                    }
                }

            }
            return 0;
        }

        internal static void GetNetvarOffsets(JsonClasses.Config.Netvar[] NetvarConfig, ref Dictionary<string, int> Netvars, IntPtr dwGetallClassesAddr)
        {
            foreach (var netvar in NetvarConfig)
            {
                if (Netvars.ContainsKey(netvar.name))//Remove duplicates (some configs might have duplicates)
                    continue;

                int offset = GetNetVarOffset(netvar.table, netvar.prop, dwGetallClassesAddr);

                if (netvar.offset != 0)
                {
                    offset += netvar.offset;
                }

                AnsiConsole.MarkupLine($"[grey]Found netvar [blue]{netvar.name}[/] -> [blue]0x{offset:X}[/][/]");
                Netvars.Add(netvar.name, offset);
            }
        }

    }
}
