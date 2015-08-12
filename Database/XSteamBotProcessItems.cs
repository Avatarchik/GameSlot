using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 324, CharSet = CharSet.Unicode, Pack = 1)]
    public struct XSteamBotProcessItem
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public uint ID;

        [MarshalAs(UnmanagedType.U8)]
        [FieldOffset(4)]
        public ulong UserSteamID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(12)]
        public uint SteamBotID;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U4, SizeConst = 24)]
        [FieldOffset(16)]
        public uint[] SteamItemIDs;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U8, SizeConst = 24)]
        [FieldOffset(104)]
        public ulong[] ItemAssertIDs;

        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(296)]
        public ushort SteamItemsNum;

        // 0: standart, 1: sent offer, 2: accepted, 4: user declined, 5: sent_error, 6: system declined
        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(298)]
        public ushort Status;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(300)]
        public int SentTime;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(304)]
        public int StatusChangedTime;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(308)]
        public uint SteamGameID;

        [MarshalAs(UnmanagedType.U8)]
        [FieldOffset(312)]
        public ulong OfferID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(320)]
        public uint UserID;
    }
}
