using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 216, CharSet = CharSet.Unicode, Pack = 1)]
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

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R8, SizeConst = 24)]
        [FieldOffset(112)]
        public double[] Prices;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(208)]
        public uint FisrtToken;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(212)]
        public uint LastToken;
    }
}
