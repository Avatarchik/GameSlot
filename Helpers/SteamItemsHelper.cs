using GameSlot.Database;
using GameSlot.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UpServer;
using XData;

namespace GameSlot.Helpers
{
    public class SteamItemsHelper
    {
        public XTable<XSteamItem> Table = new XTable<XSteamItem>();

        private static Dictionary<uint, Dictionary<uint, UFile>> SteamItemImages = new Dictionary<uint, Dictionary<uint, UFile>>();

        private static Dictionary<uint, UFile> SteamItemImages_DOTA = new Dictionary<uint, UFile>();
        private static Dictionary<uint, UFile> SteamItemImages_CSGO = new Dictionary<uint, UFile>();

        public SteamItemsHelper()
        {
            SteamItemImages.Add(Configs.DOTA2_STEAM_GAME_ID, SteamItemImages_DOTA);
            SteamItemImages.Add(Configs.CSGO_STEAM_GAME_ID, SteamItemImages_CSGO);

            this.AddSteamImagesToMemory();
        }

        public bool SelectByName(string name, uint SteamGameID, out USteamItem SteamItem)
        {
            XSteamItem XSteamItem;
            if (this.SelectByName(name, SteamGameID, out XSteamItem))
            {
                SteamItem = new USteamItem();
                SteamItem.ID = XSteamItem.ID;
                SteamItem.Name = XSteamItem.Name;
                SteamItem.Image = XSteamItem.Image;
                SteamItem.Price = XSteamItem.Price;
                SteamItem.NameColor = XSteamItem.NameColor;
                SteamItem.Type = XSteamItem.Type;
                SteamItem.SteamGameID = SteamGameID;

                return true;
            }

            SteamItem = null;
            return false;
        }

        public bool SelectByName(string name, uint SteamGameID, out XSteamItem XSteamItem)
        {
            return this.Table.SelectOne(data => data.Name.Equals(name) && data.SteamGameID == SteamGameID, out XSteamItem) ? true : false;
        }

        public bool SelectByID(uint id, uint SteamGameID, out XSteamItem XSteamItem)
        {
            return this.Table.SelectOne(data => data.ID == id && data.SteamGameID == SteamGameID, out XSteamItem) ? true : false;
        }

        public bool SelectByID(uint id, uint SteamGameID, out USteamItem SteamItem)
        {
            XSteamItem XSteamItem;
            if(this.SelectByID(id, SteamGameID, out XSteamItem))
            {
                SteamItem = new USteamItem();
                SteamItem.ID = XSteamItem.ID;
                SteamItem.Name = XSteamItem.Name;
                SteamItem.Image = XSteamItem.Image;
                SteamItem.Price = XSteamItem.Price;
                SteamItem.Price_Str = XSteamItem.Price.ToString("###,##0.00");
                SteamItem.NameColor = XSteamItem.NameColor;
                SteamItem.Type = XSteamItem.Type;
                SteamItem.SteamGameID = SteamGameID;

                return true;
            }

            SteamItem = null;
            return false;
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

                        return Convert.ToDouble(price);
                    }
                }

                catch { }
            }

            return 0d;
        }

        public void UpdatePrices(uint SteamGameID)
        {
            new Thread(delegate()
            {
                while (true)
                {
                    List<XSteamItem> Items;
                    if (this.Table.Select(data => data.SteamGameID == SteamGameID, out Items))
                    {
                        for (int i = 0; i < Items.Count; i++)
                        {
                            XSteamItem XSteamItem = Items[i];
                            XSteamItem.Price = this.GetMarketPrice(XSteamItem.Name, SteamGameID);
                            this.Table.UpdateByID(XSteamItem, XSteamItem.ID);
                            //this.DownloadItemsImage(XSteamItem.ID, XSteamItem.SteamGameID);
                        }
                    }
                    Thread.Sleep(5000);
                }
            }).Start();
        }

        public void DownloadItemsImage(uint ItemID, uint SteamGameID)
        {
            try
            {
                XSteamItem XSteamItem;
                if (this.Table.SelectByID(ItemID, out XSteamItem))
                {
                    if (!File.Exists("FileStorage\\Upload\\" + Configs.STEAM_ITEMS_STORAGE + SteamGameID + "\\" + ItemID + Configs.STEAM_ITEMS_TYPE))
                    {
                        using (WebClient WebClient = new WebClient())
                        {
                            byte[] ImageBytes = WebClient.DownloadData(XSteamItem.Image);
                            Image Image = Image.FromStream(new MemoryStream(ImageBytes));
                            if (!Directory.Exists("FileStorage\\Upload\\" + Configs.STEAM_ITEMS_STORAGE + SteamGameID))
                            {
                                Directory.CreateDirectory("FileStorage\\Upload\\" + Configs.STEAM_ITEMS_STORAGE + SteamGameID);
                            }

                            File.WriteAllBytes("FileStorage\\Upload\\" + Configs.STEAM_ITEMS_STORAGE + SteamGameID + "\\" + XSteamItem.ID + Configs.STEAM_ITEMS_TYPE, ImageBytes);
                        }
                    }
                }
            }
            catch { }
        }

        public void AddSteamImageToMemory(uint ItemID, uint SteamGameID)
        {
            string path = "FileStorage\\Upload\\" + Configs.STEAM_ITEMS_STORAGE + SteamGameID + "\\" + ItemID + Configs.STEAM_ITEMS_TYPE;
            //Console.WriteLine(path);
            if (File.Exists(path))
            {
                //Console.WriteLine(ItemID);
                UFile Image = new UFile();
                Image.Name = Configs.STEAM_ITEMS_TYPE;
                Image.Data = File.ReadAllBytes(path);

                if (!SteamItemImages[SteamGameID].ContainsKey(ItemID))
                {
                    SteamItemImages[SteamGameID].Add(ItemID, Image);
                }
            }
        }

        private void AddSteamImagesToMemory()
        {
            Console.WriteLine("Images to memory...");
            List<XSteamItem> Items = this.Table.SelectAll();
            for (int i = 0; i < Items.Count; i++)
            {
                this.AddSteamImageToMemory(Items[i].ID, Items[i].SteamGameID);
            }
            Console.WriteLine("Images to memory done!");
        }

        public bool GetImageFromMemory(uint ItemID, uint SteamGameID, out UFile image)
        {
            if (SteamItemImages.ContainsKey(SteamGameID) && SteamItemImages[SteamGameID].ContainsKey(ItemID))
            {
                image = SteamItemImages[SteamGameID][ItemID];
                return true;
            }

            image = null;
            return false;
        }
    }
}
