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
        public XTable<XItemsShemaDOTA> TableShemaDOTA = new XTable<XItemsShemaDOTA>();
        //public XTable<XItemsShemaCSGO> TableShemaCSGO = new XTable<XItemsShemaCSGO>();

        public SteamItemsHelper()
        {

        }
        //http://api.steampowered.com/IEconItems_730/GetSchemaURL/v2/?key=D2D57807EDF7C09134C7F1BA077A9658
        public void InsertItemsDOTA()
        {
            if (this.TableShemaDOTA.SelectAll().Count >= 1)
            {
                Console.WriteLine("Dota2 items are exists!");
                return;
            }

            using (WebClient webClient = new WebClient())
            {
                try
                {
                    Console.WriteLine("Downloading DOTA 2 items schema...");
                    string data = webClient.DownloadString("http://api.steampowered.com/IEconItems_570/GetSchemaURL/v0001/?key=" + Configs.STEAM_API);
                    string url = Regex.Split(data, "\"items_game_url\": \"")[1].Split('"')[0];
                    data = webClient.DownloadString(url);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Downloaded DOTA 2 items schema!");
                    Console.ResetColor();
                    Console.WriteLine("Inserting...");

                    for (int i = 0; i < 50000; i++)
                    {
                        try
                        {
                            string[] result_item = Regex.Split(data, "\"" + i + "\"");

                            for (int item_i = 1; item_i < result_item.Length; item_i++)
                            {
                                //Logger.ConsoleLog(item_i, ConsoleColor.Yellow);
                                if (result_item[item_i].Contains("\"name\""))
                                {
                                    string name = Regex.Split(result_item[item_i], "\"name\"")[1].Split('"')[1];
                                    string img = Regex.Split(result_item[item_i], "\"image_inventory\"")[1].Split('"')[1];
                                    string rarity = Regex.Split(result_item[item_i], "\"item_rarity\"")[1].Split('"')[1];

                                    if (!name.StartsWith("#") && img.Length > 0)
                                    {
                                        XItemsShemaDOTA item = new XItemsShemaDOTA();
                                        item.Name = name;
                                        item.Rarity = rarity;
                                        item.DefIndex = i;
                                        item.Image = img;
                                        item.Price = this.GetMarketPrice(name, Configs.DOTA2_STEAM_GAME_ID);
                                        this.TableShemaDOTA.Insert(item);
                                        Logger.ConsoleLog("Added [" + i + "]: name: " + name + ", price: " + item.Price + ", DefIndex: " + i);
                                    }
                                }
                            }
                        }
                        catch{ }
                    }
                }
                catch (Exception Exception) { Console.WriteLine(Exception); Console.ReadKey(); }
            }
        }

        public double GetMarketPrice(string ItemName, uint SteamGameID)
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    string name = ItemName.Replace(" ", "%20").Replace("|", "%7C").Replace("(", "%28").Replace(")", "%29");
                    string data = webClient.DownloadString("http://steamcommunity.com/market/priceoverview/?appid=" + SteamGameID + "&currency=1&market_hash_name=" + name);
                    //Logger.ConsoleLog(data, ConsoleColor.Yellow);
                    if (data.Contains("\"success\":true") && data.Contains("\"median_price\":\""))
                    {
                        //Logger.ConsoleLog(data, ConsoleColor.Yellow);
                        string price = Regex.Split(data, "\"median_price\":\"")[1].Split('"')[0].Replace("$", "");
                        //Logger.ConsoleLog(price, ConsoleColor.Yellow);
                        return double.Parse(price);
                    }
                    
                }

                catch { }
            }

            return -1;
        }

        public bool SelectSteamItemByDefIndex(uint DefIndex, uint SteamGameID, out SteamItem SteamItem)
        {
            SteamItem = new SteamItem();

            if(SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
            {
                XItemsShemaDOTA item;
                if (this.TableShemaDOTA.SelectOne(data => data.DefIndex == DefIndex, out item))
                {
                    SteamItem.ID = item.ID;
                    SteamItem.DefIndex = DefIndex;
                    SteamItem.Name = item.Name;
                    SteamItem.Price = item.Price;
                    return true;
                }
            }

            return false;
        }
    }
}
