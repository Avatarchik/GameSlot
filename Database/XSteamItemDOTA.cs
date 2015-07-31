using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 844, CharSet = CharSet.Unicode, Pack = 1)]
    public struct XSteamItemDOTA
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public uint ID;

        //4 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 71)]
        [FieldOffset(8)]
        public string Name;

        //2 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 301)]
        [FieldOffset(152)]
        public string Image;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(754)]
        public double Price;

        //4 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
        [FieldOffset(768)]
        public string NameColor;

        //2 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        [FieldOffset(784)]
        public string Type;
    }
}
