using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 56, CharSet = CharSet.Unicode, Pack = 1)]
    public struct XLottery
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public uint ID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(4)]
        public uint SteamGameID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(8)]
        public uint WinnersToken;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(12)]
        public int EndTime;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(16)]
        public double RaundNumber;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(24)]
        public uint Winner;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(28)]
        public int StartTime;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(32)]
        public uint BetsItemsNum;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(36)]
        public double JackpotPrice;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(44)]
        public int JackpotItemsNum;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(48)]
        public int Wonrate;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(52)]
        public int WinnerGroupID;
    }
}
