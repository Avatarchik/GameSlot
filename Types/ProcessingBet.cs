using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Types
{
    public class ProcessingBet
    {
        public List<USteamItem> SteamItems = new List<USteamItem>();
        public List<Chip> Chips = new List<Chip>();

        public uint UserID;
        public uint SteamGameID;

        public int ProcessCreatedTime;
        public uint LotteryID;

        public Client client;
    }
}
