using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGO_Offset_Dumper.JsonClasses
{
    internal class Config
    {

        public class Rootobject
        {
            public string executable { get; set; }
            public string filename { get; set; }
            public Signature[] signatures { get; set; }
            public Netvar[] netvars { get; set; }
        }

        public class Signature
        {
            public string name { get; set; }
            public int extra { get; set; }
            public bool relative { get; set; }
            public string module { get; set; }
            public int[] offsets { get; set; } = new int[0];
            public string pattern { get; set; }
        }

        public class Netvar
        {
            public string name { get; set; }
            public string prop { get; set; }
            public string table { get; set; }
            public int offset { get; set; } = 0;
        }

    }
}
