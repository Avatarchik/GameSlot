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
        public List<USteamItem> Items = new List<USteamItem>();
        public List<Chip> Chips = new List<Chip>();

        public double Price;
        public double TotalPrice;

        public uint StartToken;
        public uint LastToken;

        public List<Bet> OtherBets = new List<Bet>();

        public XUser XUser;
    }
}
