using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 16, CharSet = CharSet.Unicode, Pack = 1)]
    public struct XLotteryUsersMemory
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public uint ID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(4)]
        public int UserID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(8)]
        public uint LotteryID;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(12)]
        public int GroupOwnerID;
    }
}
