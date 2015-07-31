using GameSlot.Database;
using GameSlot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UpServer;
using XData;

namespace GameSlot.Helpers
{
    public class UserHelper
    {
        public XTable<XUser> Table = new XTable<XUser>();

        public UserHelper()
        {
        }

        public SteamUser GetSteamData(ulong SteamID)
        {
            try
            {
                using (WebClient WebClient = new WebClient())
                {
                    string data = WebClient.DownloadString("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + Configs.STEAM_API + "&steamids=" + SteamID);
                    if (data.Contains("\"steamid\":"))
                    {
                        SteamUser SteamUser = new SteamUser();
                        SteamUser.Name = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Regex.Split(data, "\"personaname\": \"")[1].Split('"')[0]));
                        SteamUser.Avatar = Regex.Split(data, "\"avatarfull\": \"")[1].Split('"')[0];
                        SteamUser.ProfileURL = Regex.Split(data, "\"profileurl\": \"")[1].Split('"')[0];
                        SteamUser.SteamID = SteamID;
                        return SteamUser;
                    }
                }
            }
            catch { Logger.ConsoleLog("Failed get steam data!", ConsoleColor.Red); }

            return null;
        }

        public bool GetCurrentUser(Client client, out XUser user)
        {
            if (client.Session["User"] != null && this.Table.SelectByID((uint)client.Session["User"], out user))
            {
                return true;
            }

            user = new XUser();
            return false;
        }

        public bool Authorized(Client client)
        {
            XUser user;
            return this.GetCurrentUser(client, out user) ? true : false;
        }

        public bool SelectBySteamID(ulong SteamID, out XUser user)
        {
            if (this.Table.SelectOne(data => data.SteamID == SteamID, out user))
            {
                return true;
            }

            user = new XUser();
            return false;
        }

        public bool SelectBySteamID(ulong SteamID)
        {
            XUser user;
            return this.SelectBySteamID(SteamID, out user);
        }

        public bool Auth(ulong SteamID, out XUser user, Client client)
        {
            SteamUser SteamUser = this.GetSteamData(SteamID);

            if (SteamUser != null)
            {
                bool Exist = this.SelectBySteamID(SteamID, out user);
                user.Name = SteamUser.Name;
                user.ProfileURL = SteamUser.ProfileURL;
                user.Avatar = SteamUser.Avatar;

                if (Exist)
                {
                    //update user
                    this.Table.UpdateByID(user, user.ID);
                }
                else
                {
                    //insert new user
                    user.SteamID = SteamID;
                    // -1 (default, no group).
                    user.GroupOwnerID = -1;
                    user = this.Table.SelectByID(this.Table.Insert(user));
                }

                client.Session["User"] = user.ID;
                client.Session["Avatar"] = user.Avatar;
                client.Session["ProfileUrl"] = user.ProfileURL;
                client.Session["Name"] = user.Name;
                client.Session["SteamID"] = user.SteamID;

                //localhost/group/79384474774 -- URL
                client.Session["GroupOwnerID"] = user.GroupOwnerID;
                return true;
            }
            user = new XUser();
            return false;
        }

        public bool UserExist(uint id)
        {
            XUser user;
            return this.Table.SelectByID(id, out user);
        }

        public bool UsersGroup(Client client, out UGroup group)
        {
            if(Convert.ToInt32(client.Session["GroupOwnerID"]) >= 0)
            {
                return Helper.GroupHelper.SelectByID(Convert.ToUInt32(client.Session["GroupOwnerID"]), out group);
            }

            group = null;
            return false;
        }

        public bool GetSteamInventory(string profileurl, uint SteamGameID, out List<SteamItem> SteamItems, Client client, bool WS_Send = false)
        {
            //http://api.steampowered.com/IEconItems_570/GetPlayerItems/v0001/?key=D2D57807EDF7C09134C7F1BA077A9658&steamid=76561198208049985
            //http://steamcommunity.com/id/unilogx/inventory/json/570/2
            try
            {
                using (WebClient WebClient = new WebClient())
                {
                    string data = WebClient.DownloadString(profileurl + "inventory/json/" + SteamGameID + "/2");
                    if (data.Contains("{\"success\":true"))
                    {
                        SteamItems = new List<SteamItem>();
                        string[] Item = Regex.Split(data, "{\"id\":\"");

                        string WSItems = "";
                        double TotalPrice = 0;

                        for (int i = 1; i < Item.Length; i++)
                        {
                            //Logger.ConsoleLog("On " + i + " from " + (Item.Length - 1));
                            string classid = Regex.Split(Item[i], "\"classid\":\"")[1].Split('"')[0];
                            string ItemContent = Regex.Split(data, "{\"appid\":\"" + SteamGameID + "\",\"classid\":\"" + classid + "\"")[1];

                            if (Regex.Split(ItemContent, "\"tradable\":")[1].Split(',')[0].Equals("1") && Regex.Split(ItemContent, "\"marketable\":")[1].Split(',')[0].Equals("1"))
                            {
                                // TODO: расшифровка странной херни, как в пирке! для названия
                                string name = Regex.Split(ItemContent, "\"market_name\":\"")[1].Split('"')[0];

                                SteamItem SteamItem;
                                if (!Helper.SteamItemsHelper.SelectByName(name, SteamGameID, out SteamItem))
                                {
                                    SteamItem = new SteamItem();
                                    SteamItem.Name = name;
                                    SteamItem.Price = Helper.SteamItemsHelper.GetMarketPrice(SteamItem.Name, SteamGameID);
                                    SteamItem.NameColor = Regex.Split(ItemContent, "\"name_color\":\"")[1].Split('"')[0];

                                    if (SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                                    {
                                        SteamItem.Type = Regex.Split(Regex.Split(ItemContent, "\"background_color\":\"")[1], ",\"type\":\"")[1].Split(',')[1].Split('"')[0];
                                    }
                                    SteamItem.Image = "http://steamcommunity-a.akamaihd.net/economy/image/" + Regex.Split(ItemContent, "\"icon_url_large\":\"")[1].Split('"')[0];
                                    Helper.SteamItemsHelper.Insert(SteamItem, SteamGameID);
                                }

                                if (SteamItem.Price >= Configs.MIN_ITEMS_PRICE)
                                {
                                    TotalPrice+= SteamItem.Price;

                                    SteamItem.AssertID = Convert.ToUInt64(Item[i].Split('"')[0]);
                                    SteamItems.Add(SteamItem);

                                    if (WS_Send)
                                    {
                                        WSItems += SteamItem.Name + ":" + SteamItem.Price + ";";
                                    }
                                }
                            }
                        }

                        if (WS_Send)
                        {
                            client.SendWebsocket("Inventory" + BaseFuncs.WSplit + SteamGameID + BaseFuncs.WSplit + TotalPrice + BaseFuncs.WSplit + WSItems);
                        }

                        return true;
                    }
                }
            }
            catch { }

            SteamItems = null;

            if (!WS_Send)
            {
                client.SendWebsocket("InventoryClosed" + BaseFuncs.WSplit + SteamGameID);
            }

            return false;
        }

        public string GetSteamInventoryString(ulong UserSteamID, uint SteamGameID)
        {
            using(WebClient WebClient = new WebClient())
            {
                return WebClient.DownloadString("http://api.steampowered.com/IEconItems_" + SteamGameID + "/GetPlayerItems/v0001/?key=" + Configs.STEAM_API + "&steamid=" + UserSteamID);
            }
        }
    }
}
