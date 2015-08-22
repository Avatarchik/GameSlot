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
        public double Price;
        public string Price_Str;

        public string NameColor;
        public string Type;

        public string Image;

        public ulong AssertID;
        public uint SteamGameID;

        public uint SteamBotID;
    }
}
