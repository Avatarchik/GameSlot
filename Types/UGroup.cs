using GameSlot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Types
{
    public class UGroup
    {
        public uint ID;
        public string Name;
        public List<XUser> Users;
        public int UserCount;

        public int Winrate;
        public int BetItemsCount;
        public double BetPrice;

        public string BetItemsPrice_Str;

        public double GotPriceFromGroup;
        public int GotItemsFromGroup;

        public string GotPriceFromGroup_Str;
    }
}
