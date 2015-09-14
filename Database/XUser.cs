using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Database
{
    [StructLayout(LayoutKind.Explicit, Size = 1162, CharSet = CharSet.Unicode, Pack = 1)]
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
        public int DOTA_TotalBetItemsCount;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(932)]
        public int CSGO_TotalBetItemsCount;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(936)]
        public double DOTA_TotalBetPrice;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(944)]
        public double CSGO_TotalBetPrice;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(952)]
        public int DOTA_GamesCount;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(956)]
        public int CSGO_GamesCount;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(960)]
        public int DOTA_WonCount;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(964)]
        public int CSGO_WonCount;

        // groups data
        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(968)]
        public int DOTA_GroupTotalBetItemsCount;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(972)]
        public int CSGO_GroupTotalBetItemsCount;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(976)]
        public double DOTA_GroupTotalBetPrice;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(984)]
        public double CSGO_GroupTotalBetPrice;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(992)]
        public int DOTA_GroupGamesCount;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(996)]
        public int CSGO_GroupGamesCount;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(1000)]
        public int DOTA_GroupWonCount;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(1004)]
        public int CSGO_GroupWonCount;

        // won info
        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(1008)]
        public int DOTA_WonItemsCount;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(1012)]
        public int CSGO_WonItemsCount;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1016)]
        public double DOTA_WonTotalPrice;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1024)]
        public double CSGO_WonTotalPrice;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(1032)]
        public int DOTA_GroupWonItemsCount;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(1036)]
        public int CSGO_GroupWonItemsCount;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1040)]
        public double DOTA_GroupWonTotalPrice;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1048)]
        public double CSGO_GroupWonTotalPrice;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(1056)]
        public int DOTA_GotItemsFromGroup;

        [MarshalAs(UnmanagedType.I4)]
        [FieldOffset(1060)]
        public int CSGO_GotItemsFromGroup;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1064)]
        public double DOTA_GotPriceFromGroup;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1072)]
        public double CSGO_GotPriceFromGroup;

        // 0: usd; 1: rub
        [MarshalAs(UnmanagedType.U2)]
        [FieldOffset(1080)]
        public ushort Currency;

        // rubs
        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1082)]
        public double DOTA_RUB_GotPriceFromGroup;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1090)]
        public double CSGO_RUB_GotPriceFromGroup;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1098)]
        public double DOTA_RUB_GroupTotalBetPrice;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1106)]
        public double CSGO_RUB_GroupTotalBetPrice;



        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1114)]
        public double DOTA_RUB_WonTotalPrice;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1122)]
        public double CSGO_RUB_WonTotalPrice;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1130)]
        public double DOTA_RUB_GroupWonTotalPrice;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1138)]
        public double CSGO_RUB_GroupWonTotalPrice;


        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1146)]
        public double DOTA_RUB_TotalBetPrice;

        [MarshalAs(UnmanagedType.R8)]
        [FieldOffset(1154)]
        public double CSGO_RUB_TotalBetPrice;
    }
}
