using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Types
{
    public class TopPlayer
    {
        public uint ID;
        public int Position;

        public string Name;
        public string Avatar;

        public int GamesCount;
        public int WonCount;
        public int Winrate;

        public int BetItemsCount;
        public int WonItemsCount;

        public double BetPrice;
        public double WonPrice;

        public string BetPrice_Str;
        public string WonPrice_Str;
    }
}
