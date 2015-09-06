using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 28, CharSet = CharSet.Unicode, Pack = 1)]
    public struct XLotteryUsersBetsPrice
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public uint ID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(4)]
        public uint UserID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(8)]
        public uint LotteryID;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(12)]
        public double TotalBetsPrice;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(20)]
        public int TotalBetsItemsNum;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(24)]
        public int BetsCount;
    }
}
