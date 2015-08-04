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

                if (Action.Equals("dota"))
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

                if (client.ConnType == ConnectionType.WebSocket && client.WSData != null)
                {
                    string[] wsdata = Regex.Split(client.WSData, BaseFuncs.WSplit);
                    if (wsdata[0].Equals("GetInventory"))
                    {
                        UsersInventory Inventory = null;
                        bool InList = Helper.UserHelper.GetSteamInventory(User, SteamGameID, out Inventory);
                        if (InList)
                        {
                            if (!Inventory.Opened)
                            {
                                WS_UpdateInventory("", 0d, client, false);
                            }
                            else
                            {
                                string StrItems = "";
                                foreach (SteamItem Item in Inventory.SteamItems)
                                {
                                    StrItems += ItemToString(Item);
                                }

                                WS_UpdateInventory(StrItems, Inventory.TotalPrice, client, true);
                            }
                        }
                        else
                        {
                            Helper.UserHelper.WaitingList_InventoryClient(User.ID, client, SteamGameID);
                        }
                    }

                    return false;
                }

                Hashtable data = new Hashtable();
                client.HttpSend(TemplateActivator.Activate(this, client, data));
                return true;
            }

            BaseFuncs.Show403(client);
            return false;
        }

        public static void WS_UpdateInventory(string StrItems, double TotalPrice, Client client, bool InventoryOpened)
        {
            if (InventoryOpened)
            {
                client.SendWebsocket("Inventory" + BaseFuncs.WSplit + TotalPrice + BaseFuncs.WSplit + StrItems);
                //Logger.ConsoleLog("[" + StrItems + "]", ConsoleColor.Red);
                return;
            }
            //Logger.ConsoleLog("CLOSE!");
            client.SendWebsocket("InventoryClosed" + BaseFuncs.WSplit);
        }

        public static string ItemToString(SteamItem SteamItem)
        {
            return SteamItem.Name + "↓" + SteamItem.Price + "↓" + SteamItem.Image + ";";
        }
    }
}
