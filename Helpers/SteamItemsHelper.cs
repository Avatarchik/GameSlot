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
    public class SteamItemsHelper
    {
        public XTable<XSteamItemDOTA> TableDOTA = new XTable<XSteamItemDOTA>();
        //public XTable<XItemsShemaCSGO> TableShemaCSGO = new XTable<XItemsShemaCSGO>();

        public SteamItemsHelper()
        {

        }

        public void Insert(SteamItem SteamItem, uint SteamGameID)
        {
            if (SteamItem.Name.Length > 0 && SteamItem.Image.Length > 10 && SteamItem.NameColor.Length == 6)
            {
                if (SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                {
                    XSteamItemDOTA Item = new XSteamItemDOTA();
                    Item.Name = SteamItem.Name;
                    Item.Image = SteamItem.Image;
                    Item.Price = SteamItem.Price;
                    Item.NameColor = SteamItem.NameColor;
                    Item.Type = SteamItem.Type;

                    //Logger.ConsoleLog(Item.Name);
                    this.TableDOTA.Insert(Item);
                }
                else if (SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                {

                }
            }
        }

        public bool SelectByName(string name, uint SteamGameID, out SteamItem SteamItem)
        {
            if (SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
            {
                XSteamItemDOTA XSteamItemDOTA;
                if (this.SelectByName_DOTA(name, out XSteamItemDOTA))
                {
                    SteamItem = new SteamItem();
                    SteamItem.ID = XSteamItemDOTA.ID;
                    SteamItem.Name = XSteamItemDOTA.Name;
                    SteamItem.Image = XSteamItemDOTA.Image;
                    SteamItem.Price = XSteamItemDOTA.Price;
                    SteamItem.NameColor = XSteamItemDOTA.NameColor;
                    SteamItem.Type = XSteamItemDOTA.Type;
                    return true;
                }
            }
            else if (SteamGameID == Configs.CSGO_STEAM_GAME_ID)
            {

            }

            SteamItem = null;
            return false;
        }

        public bool SelectByName_DOTA(string name, out XSteamItemDOTA SteamItemDOTA)
        {
            return this.TableDOTA.SelectOne(data => data.Name.Equals(name), out SteamItemDOTA) ? true : false;
        }

        public double GetMarketPrice(string ItemName, uint SteamGameID)
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    string name = ItemName.Replace(" ", "%20").Replace("|", "%7C").Replace("(", "%28").Replace(")", "%29");
                    string data = webClient.DownloadString("http://steamcommunity.com/market/priceoverview/?appid=" + SteamGameID + "&currency=1&market_hash_name=" + name);
                    if (data.Contains("\"success\":true") && (data.Contains("\"median_price\":\"") || data.Contains("\"lowest_price\":\"")))
                    {
                        string price = null;
                        if (data.Contains("\"median_price\":\""))
                        {
                            price = Regex.Split(data, "\"median_price\":\"")[1].Split('"')[0].Replace("$", "");
                        }
                        else
                        {
                            price = Regex.Split(data, "\"lowest_price\":\"")[1].Split('"')[0].Replace("$", "");
                        }

                        return double.Parse(price);
                    }

                }

                catch { }
            }

            return 0;
        }
    }
}
