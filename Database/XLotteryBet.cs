using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 612, CharSet = CharSet.Unicode, Pack = 1)]
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

        // 4 bts
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

        [MarshalAs(UnmanagedType.Bool)]
        [FieldOffset(312)]
        public bool Transmitted;

        // 4 bts
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U4, SizeConst = 24)]
        [FieldOffset(320)]
        public uint[] ChipIDs;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U8, SizeConst = 24)]
        [FieldOffset(416)]
        public ulong[] ChipAssertIDs;

        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(608)]
        public ushort SteamItemsNum;

        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(610)]
        public ushort ChipsNum;
    }
}
