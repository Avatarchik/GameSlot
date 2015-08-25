using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 904, CharSet = CharSet.Unicode, Pack = 1)]
    public struct XLotteryBet
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public uint ID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(4)]
        public uint LotteryID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(8)]
        public uint UserID;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(12)]
        public int GroupOwnerID;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U4, SizeConst = 24)]
        [FieldOffset(16)]
        public uint[] SteamItemIDs;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U8, SizeConst = 24)]
        [FieldOffset(112)]
        public ulong[] ItemAssertIDs;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(304)]
        public uint FisrtToken;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(308)]
        public uint LastToken;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U4, SizeConst = 24)]
        [FieldOffset(312)]
        public uint[] ChipIDs;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U8, SizeConst = 24)]
        [FieldOffset(408)]
        public ulong[] ChipAssertIDs;

        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(600)]
        public ushort SteamItemsNum;

        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(602)]
        public ushort ChipsNum;

        // 4 bts
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U4, SizeConst = 24)]
        [FieldOffset(608)]
        public uint[] SteamBotIDs;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R8, SizeConst = 24)]
        [FieldOffset(704)]
        public double[] SteamItemsPrice;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(896)]
        public double TotalPrice;

    }
}
