using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Types
{
    public class Lottery
    {
        public uint ID;
        public string IDStr;

        public double JackpotPrice;
        public int JackpotItems;

        public double RaundNumber;
        // secs
        public int LeftTime;

        public List<Bet> Bets;
        public List<USteamItem> SteamItems;
        public List<Chip> Chips;
    }
}
