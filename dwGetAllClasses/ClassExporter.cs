using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CSGO_Offset_Dumper.SDK.SourceSDK;

namespace CSGO_Offset_Dumper.dwGetAllClasses
{
    internal class ClassExporter
    {
        public class SourceClassRoot
        {
            public List<SourceClass> SourceClass { get; set; } = new();
        }

        public class SourceClass
        {
            public string ClassName { get; set; }
            public string VariableName { get; set; }
            public int Offset { get; set; }
        }







        internal static SourceClassRoot GetAllClasses(IntPtr clientClass)
        {
            SourceClassRoot root = new();
            for (IntPtr currNode = clientClass; currNode != IntPtr.Zero; currNode = currNode.Deference<ClientClass>().m_pNext)
            {
                ClientClass? node = currNode.Deference<ClientClass>();

                root.SourceClass.AddRange(LoopTable(node.m_pRecvTable, node.m_pNetworkName));

            }

            return root;
        }


        internal static IEnumerable<SourceClass> LoopTable(IntPtr ptable, string networkName)
        {

            RecvTable? table = ptable.Deference<RecvTable>();
            for (int i = 0; i < table.m_nProps; i++)
            {
                int offset = 0;
                //Size of RecvProp is 60 bytes (using sizeof(RecvTable in cpp) (Using sizeof in C# has different string sizes so it doesn't work)
                IntPtr propAddress = table.m_pProps + i * 60;

                RecvProp? prop = (propAddress).Deference<RecvProp>();
                if (prop is not null)
                {
                    offset = prop.m_Offset;

                    if (offset == 0x0)//0x0 offsets are all useless and are not meaningful
                    {
                        continue;
                    }

                    yield return new SourceClass() { ClassName = networkName, Offset = prop.m_Offset, VariableName = prop.m_pVarName };
                }

            }
        }
    }
}
