using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CSGO_Offset_Dumper
{
    //Platform Invoked
    public static class Win32
    {
        [Flags]
        enum LoadLibraryFlags : uint
        {
            None = 0,
            DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
            LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
            LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
            LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
            LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
            LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200,
            LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000,
            LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100,
            LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800,
            LOAD_LIBRARY_SEARCH_USER_DIRS = 0x00000400,
            LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008
        }



        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, LoadLibraryFlags dwFlags);

        [DllImport("kernel32.dll")]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, string buffer, int size, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] buffer, int size, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] buffer, int size, out int lpNumberOfBytesWritten);

        public static IntPtr ProcessHandle = IntPtr.Zero;
        public static int m_iBytesRead = 0;
        public static int m_iBytesWrite = 0;

        public static T ReadMemory<T>(int Adress) where T : struct
        {
            int ByteSize = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[ByteSize];
            ReadProcessMemory((int)ProcessHandle, Adress, buffer, buffer.Length, ref m_iBytesRead);

            return ByteArrayToStructure<T>(buffer);
        }

        public const uint PAGE_EXECUTE_READWRITE = 0x40;
        public static byte[] ReadMemoryBytes(int Adress, int bytesToRead, ref int bytesRead)
        {
            byte[] buffer = new byte[bytesToRead];

            if (VirtualProtectEx(ProcessHandle, (IntPtr)Adress, (UIntPtr)bytesToRead, PAGE_EXECUTE_READWRITE, out uint oldProtect))
            {
                if (ReadProcessMemory((int)ProcessHandle, Adress, buffer, buffer.Length, ref bytesRead))
                {
                    VirtualProtectEx(ProcessHandle, (IntPtr)Adress, (UIntPtr)bytesToRead, oldProtect, out uint oldProtectagain);
                }
            }







            return buffer;
        }



        public const uint SizeOfMemoryBasicInfo = 28;
        public const uint MEM_COMMIT = 0x00001000;
        public const uint PAGE_NOACCESS = 0x01;
        [StructLayout(LayoutKind.Sequential)]
        public struct MEMORY_BASIC_INFORMATION
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public IntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [DllImport("kernel32.dll")]
        public static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);


        private const int MemoryPageSize = 0x1000;
        private static Dictionary<string, byte[]> ModuleBytes = new();

        //Reads a specified module memory page at a time.
        public static byte[] ReadModule(MODULEENTRY32 pMod)
        {
            if (ModuleBytes.TryGetValue(pMod.szModule, out byte[] module))
            {
                return module;
            }
            else
            {
                //Create a byte array the size of our module
                byte[] bytesRead = new byte[(int)pMod.modBaseSize];


                for (int i = 0; i < pMod.modBaseSize; i += MemoryPageSize)
                {
                    int currentAddress = (int)pMod.modBaseAddr + i;
                    if (VirtualQueryEx(ProcessHandle, (IntPtr)currentAddress, out MEMORY_BASIC_INFORMATION mbi, SizeOfMemoryBasicInfo) != 0)//Return 0 means fail
                    {
                        if (mbi.State != MEM_COMMIT || mbi.State == PAGE_NOACCESS)
                            continue;
                        int i_BytesRead = 0;
                        byte[] thisRegion = ReadMemoryBytes(currentAddress, MemoryPageSize, ref i_BytesRead);
                        if (i_BytesRead > 0)
                            Array.Copy(thisRegion, 0, bytesRead, i, MemoryPageSize);
                    }
                }

                //Add our module to our dictionary so we can fetch it later.
                ModuleBytes.Add(pMod.szModule, bytesRead);
                return bytesRead;
            }
        }


        public static void WriteMemory<T>(int Adress, object Value)
        {
            byte[] buffer = StructureToByteArray(Value);

            WriteProcessMemory((int)ProcessHandle, Adress, buffer, buffer.Length, out m_iBytesWrite);
        }

        public static string ReadString(int baseAddress, int size)
        {
            //create buffer for string
            byte[] buffer = new byte[size];


            ReadProcessMemory((int)ProcessHandle, baseAddress, buffer, size, ref m_iBytesWrite);

            //encode bytes to ASCII
            return Encoding.ASCII.GetString(buffer);
        }
        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        public static byte[] StructureToByteArray(object obj, int size = -1)
        {
            int len = Marshal.SizeOf(obj);

            byte[] arr;
            if (size == -1)
                arr = new byte[len];
            else
                arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(len);

            Marshal.StructureToPtr(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, len);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        public static unsafe byte[] ConvertStruct<T>(ref T str) where T : struct
        {
            int size = Marshal.SizeOf(str);
            var arr = new byte[size];

            fixed (byte* arrPtr = arr)
            {
                Marshal.StructureToPtr(str, (IntPtr)arrPtr, true);
            }

            return arr;
        }


        public static T? Deference<T>(this IntPtr address) => Marshal.PtrToStructure<T>(address);



        const long INVALID_HANDLE_VALUE = -1;
        [Flags]

        private enum SnapshotFlags : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            Inherit = 0x80000000,
            All = 0x0000001F,
            NoHeaps = 0x40000000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        public struct MODULEENTRY32
        {
            public uint dwSize;
            public uint th32ModuleID;
            public uint th32ProcessID;
            public uint GlblcntUsage;
            public uint ProccntUsage;
            public IntPtr modBaseAddr;
            public uint modBaseSize;
            public IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExePath;
        }

        [DllImport("kernel32.dll")]
        static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll")]
        static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle([In] IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateToolhelp32Snapshot(SnapshotFlags dwFlags, IntPtr th32ProcessID);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int AddDllDirectory(string NewDirectory);


        //Just store modules here so we don't have to call them everytime we need a basemoduleaddress
        private static List<MODULEENTRY32> ModuleList = new();
        public static MODULEENTRY32? GetModule(IntPtr procId, string modName)
        {
            var modList = ModuleList.Where(x => x.szModule.Equals(modName));
            if (modList.Count() > 0)
            {
                return modList.First();
            }
            else
            {
                IntPtr hSnap = CreateToolhelp32Snapshot(SnapshotFlags.Module | SnapshotFlags.Module32, procId);

                if (hSnap.ToInt64() != INVALID_HANDLE_VALUE)
                {
                    MODULEENTRY32 modEntry = new MODULEENTRY32();
                    modEntry.dwSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32));

                    if (Module32First(hSnap, ref modEntry))
                    {
                        do
                        {
                            if (modEntry.szModule.Equals(modName))
                            {
                                ModuleList.Add(modEntry);//Add module for future use
                                return modEntry;
                            }
                        } while (Module32Next(hSnap, ref modEntry));
                    }
                }
                CloseHandle(hSnap);
            }




            return null;
        }


        public static IntPtr LoadLibrary(string dllPath)
        {

            string? Directory = Path.GetFullPath(dllPath);
            Directory = Path.GetFullPath(Path.Combine(Directory, @"..\..\..\bin"));


            if (Directory != null && AddDllDirectory(Directory) != 0)
            {
                IntPtr result = LoadLibraryEx(dllPath, IntPtr.Zero, LoadLibraryFlags.LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);

                if (result != IntPtr.Zero)
                    return result;
            }


            return IntPtr.Zero;
        }

    }
}
