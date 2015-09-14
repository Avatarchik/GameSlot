using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 1166, CharSet = CharSet.Unicode, Pack = 1)]
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

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(954)]
        public double Price;
        
        // 6 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        [FieldOffset(968)]
        public string Rarity;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(1000)]
        public uint SteamGameID;

        // 4 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 7)]
        [FieldOffset(1008)]
        public string Color;

        // 2 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 71)]
        [FieldOffset(1024)]
        public string RusName;
    }
}
