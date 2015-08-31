using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 18, CharSet = CharSet.Unicode, Pack = 1)]
    public struct XOrder
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public uint ID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(4)]
        public uint UserID;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(8)]
        public double Price;

        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(16)]
        public ushort Status;
    }
}
