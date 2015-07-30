using GameSlot.Database;
using GameSlot.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages.UserPages
{
    public class InventoryDOTA : SiteGameSlot
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
                string Inventory = BaseFuncs.GetAdditionalURLArray(client.URL, this.URL)[0];

                if (Inventory.Equals("dota"))
                {
                    SteamGameID = Configs.DOTA2_STEAM_GAME_ID;
                }
                else if (Inventory.Equals("csgo"))
                {
                    SteamGameID = Configs.CSGO_STEAM_GAME_ID;
                }
                else
                {
                    BaseFuncs.Show404(client);
                    return false;
                }

                if (client.ConnType == ConnectionType.WebSocket && client.WSData != null)
                {
                    string[] wsdata = Regex.Split(client.WSData, BaseFuncs.WSplit);
                    if (wsdata[0].Equals("GetInventory"))
                    {
                        List<SteamItem> Items;
                        if (Helper.UserHelper.GetSteamInventory(User.SteamID, SteamGameID, out Items))
                        {
                            string items = "";
                            double TotalPrice = 0d;

                            foreach(SteamItem Item in Items)
                            {
                                items += Item.Name + ":" + Item.Price + ";";
                                TotalPrice += Item.Price;
                            }

                            client.SendWebsocket("Inventory" + BaseFuncs.WSplit + TotalPrice + BaseFuncs.WSplit + items);
                        }
                        else
                        {
                            client.SendWebsocket("inventory_closed");
                        }
                    }

                    return false;
                }
                else
                {
                    Hashtable data = new Hashtable();
                    data.Add("SteamGameID", Configs.DOTA2_STEAM_GAME_ID);
                    client.HttpSend(TemplateActivator.Activate(this, client, data));
                    return true;
                }
            }

            BaseFuncs.Show403(client);
            return false;
        }
    }
}
