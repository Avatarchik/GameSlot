using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 34, CharSet = CharSet.Unicode, Pack = 1)]
    public struct XBotsOffer
    {
        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(0)]
        public uint ID;

        [MarshalAs(UnmanagedType.U8)]
        [FieldOffset(4)]
        public ulong SteamUserID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(12)]
        public uint BotID;

        // 0: trade created. 1: send 2: canceled by user 3: canceled_by_system; 4: accepted; 5: error 6: proceed
        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(16)]
        public ushort Status;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(18)]
        public int SentTime;

        [MarshalAs(UnmanagedType.U8)]
        [FieldOffset(22)]
        public ulong OfferID;

        [MarshalAs(UnmanagedType.U4)]
        [FieldOffset(30)]
        public uint UserID;
    }
}
