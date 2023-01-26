using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace RSD.Monitor.Native
{
    public class SharedMemoryConsts
    {
        public const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        public const UInt32 SECTION_QUERY = 0x0001;
        public const UInt32 SECTION_MAP_WRITE = 0x0002;
        public const UInt32 SECTION_MAP_READ = 0x0004;
        public const UInt32 SECTION_MAP_EXECUTE = 0x0008;
        public const UInt32 SECTION_EXTEND_SIZE = 0x0010;

        public static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public const int PAGE_READWRITE = 0x04;
        public const int PAGE_READONLY = 0x02;
        public const int FILE_MAP_READ = 0x0004;
        public const int FILE_MAP_WRITE = 0x0002;
    }

    public class SharedMemory
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr OpenFileMapping(uint dwDesiredAccess, bool bInheritHandle, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr MapViewOfFile(
            IntPtr hFileMappingObject,
            uint dwDesiredAccess,
            uint dwFileOffsetHigh,
            uint dwFileOffsetLow,
            uint dwNumberOfBytesToMap);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool UnmapViewOfFile(IntPtr lpBaseAddress);
    }
}
