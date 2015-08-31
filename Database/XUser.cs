using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 1004, CharSet = CharSet.Unicode, Pack = 1)]
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

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
        [FieldOffset(824)]
        public string TradeToken;

        [MarshalAs(UnmanagedType.U8)]
        [FieldOffset(846)]
        public ulong TradePartner;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(854)]
        public double Wallet;

        // 2 bts
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        [FieldOffset(864)]
        public string SteamInventoryHash;

        //users data
        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(928)]
        public int TotalBetItemsCount;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(932)]
        public double TotalBetPrice;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(940)]
        public int GamesCount;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(944)]
        public int WonCount;

        // groups data
        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(948)]
        public int GroupTotalBetItemsCount;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(952)]
        public double GroupTotalBetPrice;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(960)]
        public int GroupGamesCount;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(964)]
        public int GroupWonCount;



        // won info
        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(968)]
        public int WonItemsCount;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(972)]
        public double WonTotalPrice;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(980)]
        public int GroupWonItemsCount;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(984)]
        public double GroupWonTotalPrice;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(992)]
        public int GotItemsFromGroup;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(996)]
        public double GotPriceFromGroup;
    }
}
