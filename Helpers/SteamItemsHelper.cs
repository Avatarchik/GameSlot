using GameSlot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
                            string res = Regex.Split(data, "\"" + i + "\"")[1];
                            if (res.Contains("\"name\""))
                            {
                                string name = Regex.Split(res, "\"name\"")[1].Split('"')[1];
                                string img = Regex.Split(res, "\"image_inventory\"")[1].Split('"')[1];
                                string rarity = Regex.Split(res, "\"item_rarity\"")[1].Split('"')[1];

                                if (!name.StartsWith("#") && img.Length > 0)
                                {
                                    XItemsShemaDOTA item = new XItemsShemaDOTA();
                                    item.Name = name;
                                    item.Rarity = rarity;
                                    item.DefIndex = i;
                                    item.Image = img;
                                    this.TableShemaDOTA.Insert(item);
                                    //Logger.ConsoleLog("Added [" + i + "]: name: " + name + ", rarity: " + rarity);
                                }
                            }
                        }
                        catch { }
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

                    if (data.Contains("\"success\":true"))
                    {
                        return double.Parse(data.Split(';')[1].Split('"')[0]);
                    }
                }

                catch { }
            }

            return -1;
        }
    }
}
