using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Types
{
    public class USteamItem
    {
        public uint ID;

        public string Name;
        public string Color;

        public double Price;
        public string Price_Str;

        public string RarityColor;
        public string Rarity;

        public string Image;

        public ulong AssertID;
        public uint SteamGameID;

        public uint SteamBotID;
    }
}
