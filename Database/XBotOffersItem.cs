using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 20, CharSet = CharSet.Unicode, Pack = 1)]
    public struct XBotOffersItem
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public uint ID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(4)]
        public uint BotsOfferID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(8)]
        public uint SteamItemID;

        [MarshalAs(UnmanagedType.U8)]
        [FieldOffset(12)]
        public ulong AssertID;
    }
}
