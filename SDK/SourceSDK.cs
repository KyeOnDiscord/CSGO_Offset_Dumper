using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSGO_Offset_Dumper.SDK
{
    internal class SourceSDK
    {
        [StructLayout(LayoutKind.Sequential)]
        internal class RecvTable
        {
            public IntPtr m_pProps;//RecvProp*
            public int m_nProps;
            public IntPtr m_pDecoder;
            public string m_pNetTableName;
            public bool m_bInitialized;
            public bool m_bInMainList;
        };


        //unnecessary class pointers have been converted to void* for simplicity
        [StructLayout(LayoutKind.Sequential)]
        internal class RecvProp
        {
            public string m_pVarName;
            public IntPtr m_RecvType;
            public int m_Flags;
            public int m_StringBufferSize;
            public int m_bInsideArray;
            public IntPtr m_pExtraData;
            public IntPtr m_pArrayProp;//RecvProp*
            public IntPtr m_ArrayLengthProxy;
            public IntPtr m_ProxyFn;
            public IntPtr m_DataTableProxyFn;
            public IntPtr m_pDataTable;//RecvTable*
            public int m_Offset;
            public int m_ElementStride;
            public int m_nElements;
            string m_pParentArrayPropName;
        };

        [StructLayout(LayoutKind.Sequential)]
        internal class ClientClass
        {
            public IntPtr m_pCreateFn;
            public IntPtr m_pCreateEventFn;
            public string m_pNetworkName;
            public IntPtr m_pRecvTable;//RecvTable*
            public IntPtr m_pNext;//ClientClass*
            //public int m_ClassID; This only works inside csgo not when we load a dll so yeah.
        };

    }
}
