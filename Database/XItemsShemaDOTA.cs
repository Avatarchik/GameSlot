using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 526, CharSet = CharSet.Unicode, Pack = 1)]
    public struct XItemsShemaDOTA
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public uint ID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(4)]
        public int DefIndex;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 71)]
        [FieldOffset(8)]
        public string Name;

        //2 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 151)]
        [FieldOffset(152)]
        public string Image;

        //2bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        [FieldOffset(456)]
        public string Rarity;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(518)]
        public double Price;
    }
}
