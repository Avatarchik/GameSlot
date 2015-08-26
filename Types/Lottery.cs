using GameSlot.Database;
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
        public string JackpotPrice_Str;
        public int JackpotItems;

        public double RaundNumber;
        public string RaundNumber_MD5;

        // secs
        public int LeftTime;

        public uint SteamGameID;

        public int WinnersToken;
        public XUser Winner;
    }
}
