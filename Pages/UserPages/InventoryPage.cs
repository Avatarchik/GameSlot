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
        public override string URL
        {
            get { return "/inventory"; }
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
                if (client.ConnType == ConnectionType.WebSocket && client.WSData != null)
                {
                    string[] wsdata = Regex.Split(client.WSData, BaseFuncs.WSplit);
                    if (wsdata[0].Equals("GetInventory"))
                    {
                        List<SteamItem> DotaItems, CSGOItems;
                        Thread WS_thread_DOTA = new Thread(delegate() { Helper.UserHelper.GetSteamInventory(User.ProfileURL, Configs.DOTA2_STEAM_GAME_ID, out DotaItems, client, true); });
                        Thread WS_thread_CSGO = new Thread(delegate() { Helper.UserHelper.GetSteamInventory(User.ProfileURL, Configs.CSGO_STEAM_GAME_ID, out CSGOItems, client, true); });

                        WS_thread_DOTA.Start();
                        WS_thread_CSGO.Start();
                    }

                    return false;
                }
                else
                {
                    Hashtable data = new Hashtable();
                    client.HttpSend(TemplateActivator.Activate(this, client, data));
                    return true;
                }
            }

            BaseFuncs.Show403(client);
            return false;
        }
    }
}
