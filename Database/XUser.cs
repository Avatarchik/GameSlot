using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 824, CharSet = CharSet.Unicode, Pack = 1)]
    public struct XUser
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public uint ID;

        [MarshalAs(UnmanagedType.U8)]
        [FieldOffset(4)]
        public ulong SteamID;

        // 4 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        [FieldOffset(16)]
        public string Cookie;

        // 6 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        [FieldOffset(88)]
        public string Name;

        // 6 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 151)]
        [FieldOffset(136)]
        public string Avatar;

        // 2 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 151)]
        [FieldOffset(440)]
        public string ProfileURL;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(742)]
        public int GroupOwnerID;

        //6 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 36)]
        [FieldOffset(752)]
        public string GroupName;
    }
}
