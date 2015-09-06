using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 20, CharSet = CharSet.Unicode, Pack = 1)]
    public struct XSteamItemsClassID
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public uint ID;

        [MarshalAs(UnmanagedType.U8)]
        [FieldOffset(4)]
        public ulong AssertID;

        [MarshalAs(UnmanagedType.U8)]
        [FieldOffset(12)]
        public ulong ClassID;
    }
}
