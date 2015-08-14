using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 346, CharSet = CharSet.Unicode, Pack = 1)]
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
        [FieldOffset(112)]
        public ulong[] ItemAssertIDs;

        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(304)]
        public ushort SteamItemsNum;

        // 0: standart, 1: sent offer, 2: accepted, 4: user declined, 5: sent_error, 6: system declined
        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(306)]
        public ushort Status;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(308)]
        public int SentTime;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(312)]
        public int StatusChangedTime;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(316)]
        public uint SteamGameID;

        [MarshalAs(UnmanagedType.U8)]
        [FieldOffset(320)]
        public ulong OfferID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(328)]
        public uint UserID;

        // 4 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        [FieldOffset(336)]
        public string ProtectionCode;
    }
}
