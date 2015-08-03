using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 1050, CharSet = CharSet.Unicode, Pack = 1)]
    public struct XSteamItem
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public uint ID;

        //4 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 71)]
        [FieldOffset(8)]
        public string Name;

        //2 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 401)]
        [FieldOffset(152)]
        public string Image;

        // 6 bts
        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(960)]
        public double Price;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
        [FieldOffset(968)]
        public string NameColor;

        //2 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 31)]
        [FieldOffset(984)]
        public string Type;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(1046)]
        public uint SteamGameID;
    }
}
