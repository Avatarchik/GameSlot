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

                Hashtable data = new Hashtable();
                data.Add("SteamGameID", SteamGameID);
                data.Add("Chips", Helper.UserHelper.GetChipInventory(User.ID));
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
