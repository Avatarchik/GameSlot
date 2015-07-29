using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 24, CharSet = CharSet.Unicode, Pack = 1)]
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
        public uint Winner;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(12)]
        public int EndTime;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(16)]
        public double RaundNumber;
    }
}
