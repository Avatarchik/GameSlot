using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 770, CharSet = CharSet.Unicode, Pack = 1)]
    public struct XSteamBot
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public uint ID;

        //4 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 71)]
        [FieldOffset(8)]
        public string Login;

        //2 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 101)]
        [FieldOffset(152)]
        public string Password;

        // 6 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 101)]
        [FieldOffset(360)]
        public string Email;

        // 6 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 101)]
        [FieldOffset(568)]
        public string EmailPassword;
    }
}
