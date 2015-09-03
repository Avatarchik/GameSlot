using GameSlot.Database;
using GameSlot.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages.UserPages
{
    public class InventoryPage : SiteGameSlot
    {
        public override PageType PageType
        {
            get { return PageType.Multi; }
        }
        public override string URL
        {
            get { return "/inventory/"; }
        }
        public override string TemplateAddr
        {
            get { return "UserPages.Inventory.html"; }
        }
        public override bool Init(Client client)
        {
            XUser User;
            if (Helper.UserHelper.GetCurrentUser(client, out User))
            {
                uint SteamGameID = 0;
                string Action = BaseFuncs.GetAdditionalURLArray(client.URL, this.URL)[0];

                if (Action.Equals("dota2"))
                {
                    SteamGameID = Configs.DOTA2_STEAM_GAME_ID;
                }
                else if (Action.Equals("csgo"))
                {
                    SteamGameID = Configs.CSGO_STEAM_GAME_ID;
                }
                else
                {
                    BaseFuncs.Show404(client);
                    return false;
                }

                string total_local_price;
                //List<USteamItem> LocalItems = Helper.UserHelper.GetSteamLocalInventory(User.ID, SteamGameID);
                Helper.UserHelper.GetLocalSteamInventoryTotalPrice(User.ID, SteamGameID, out total_local_price);
                UsersInventory UsersInventory;

                Hashtable data = new Hashtable();
                if(Helper.UserHelper.GetSteamInventory(User, SteamGameID, out UsersInventory))
                {
                    data.Add("SteamInventoryLoaded", true);
                    data.Add("UsersInventory", UsersInventory);
                }
                else
                {
                    data.Add("SteamInventoryLoaded", false);
                    data.Add("UsersInventory", null);
                }

                data.Add("SteamGameID", SteamGameID);
                data.Add("Chips", Helper.UserHelper.GetChipInventory(User.ID));
                data.Add("LocalSteamInventory", Helper.UserHelper.GetSteamLocalInventory(User.ID, SteamGameID));
                data.Add("LocalTotalPrice", total_local_price);
                data.Add("Title", "Мой инвентарь");
                data.Add("User", User);
                client.HttpSend(TemplateActivator.Activate(this, client, data));
                return true;
            }

            BaseFuncs.Show403(client);
            return false;
        }
    }
}
