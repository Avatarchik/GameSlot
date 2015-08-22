using GameSlot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Types
{
    public class Bet
    {
        public uint ID;
        public List<USteamItem> SteamItems;
        public List<Chip> Chips;
        public int ItemsNum;
        
        public double Price = 0d;
        public double TotalPrice = 0d;

        public string Price_Str;
        public string TotalPrice_Str;

        public uint FirstToken;
        public uint LastToken;

        public List<Bet> UserBets;

        public XUser XUser;

        public int Winrate;

        public int BetsNum;
        public int TotalItemsNum;
    }
}
