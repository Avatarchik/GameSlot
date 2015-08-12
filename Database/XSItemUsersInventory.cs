using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 32, CharSet = CharSet.Unicode, Pack = 1)]
    public struct XSItemUsersInventory
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public uint ID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(4)]
        public uint UserID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(8)]
        public uint SteamItemID;

        [MarshalAs(UnmanagedType.U8)]
        [FieldOffset(12)]
        public ulong AssertID;

        [MarshalAs(UnmanagedType.Bool)]
        [FieldOffset(20)]
        public bool Deleted;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(24)]
        public uint SteamGameID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(28)]
        public uint SteamBotID;
    }
}
