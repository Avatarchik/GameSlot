using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Types
{
    public class UsersInventory
    {
        public List<USteamItem> SteamItems;
        public double TotalPrice;
        public int LastUpdate;
        public bool Opened;
    }
}
