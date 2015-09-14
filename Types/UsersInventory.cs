﻿using System;
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
        public string TotalPrice_Str;
        public int LastUpdate;
        public bool Opened;

        public UsersInventory Clone()
        {
            return (UsersInventory)this.MemberwiseClone();
        } 
    }
}
