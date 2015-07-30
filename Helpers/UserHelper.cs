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
                        SteamUser.ProfileUrl = Regex.Split(data, "\"profileurl\": \"")[1].Split('"')[0];
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
                user.ProfileUrl = SteamUser.ProfileUrl;
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
                client.Session["ProfileUrl"] = user.ProfileUrl;
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

        public bool GetSteamInventory(ulong UserSteamID, uint SteamGameID, out List<SteamItem> SteamItems)
        {
            //http://api.steampowered.com/IEconItems_570/GetPlayerItems/v0001/?key=D2D57807EDF7C09134C7F1BA077A9658&steamid=76561198208049985
            //http://steamcommunity.com/id/unilogx/inventory/json/570/2
            try
            {


                using (WebClient WebClient = new WebClient())
                {
                    string data = WebClient.DownloadString("http://api.steampowered.com/IEconItems_" + SteamGameID + "/GetPlayerItems/v0001/?key=" + Configs.STEAM_API + "&steamid=" + UserSteamID);
                    if (data.Contains("\"status\": 1"))
                    {
                        SteamItems = new List<SteamItem>();
                        string[] slots = data.Split('{');

                        for (int i = 1; i < slots.Length; i++)
                        {
                            //Logger.ConsoleLog(i + "_" + slots.Length);
                            string item = slots[i];
                            if (item.Contains("\"id\":") && item.Contains("\"defindex\":"))
                            {
                                uint DefIndex = Convert.ToUInt32(Regex.Split(item, "\"defindex\": ")[1].Split(',')[0]);

                                SteamItem Item;
                                if (Helper.ItemsSchemaHelper.SelectSteamItemByDefIndex(DefIndex, Configs.DOTA2_STEAM_GAME_ID, out Item))
                                {
                                    if (Item.Price > -1) SteamItems.Add(Item);
                                }
                            }
                        }
                        return true;
                    }
                }
            }
            catch { }

            SteamItems = null;
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
