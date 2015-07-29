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
        public List<SteamItem> Items;

        public double Price;
        public double TotalPrice;

        public uint StartToken;
        public uint LastToken;

        public List<Bet> OtherBets;
    }
}
