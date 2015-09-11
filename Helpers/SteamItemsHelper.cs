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
        public XTable<XSteamItemsClassID> Table_ClassID = new XTable<XSteamItemsClassID>();

        private static Dictionary<uint, Dictionary<uint, string>> SteamItemImages = new Dictionary<uint, Dictionary<uint, string>>();

        private static Dictionary<uint, string> SteamItemImages_DOTA = new Dictionary<uint, string>();
        private static Dictionary<uint, string> SteamItemImages_CSGO = new Dictionary<uint, string>();

        public static string NoneImage = "";

        public static Queue<SteamItemImageQueue> QueueDownloadImage = new Queue<SteamItemImageQueue>();

        public SteamItemsHelper()
        {
            SteamItemImages.Add(Configs.DOTA2_STEAM_GAME_ID, SteamItemImages_DOTA);
            SteamItemImages.Add(Configs.CSGO_STEAM_GAME_ID, SteamItemImages_CSGO);

            this.AddSteamImagesToMemory();
            this.DownloadImagesFromQueue();

            string path = "FileStorage\\Upload\\322image.jpg";
            if (File.Exists(path))
            {
                NoneImage = FilesProcessor.CacheFile(File.ReadAllBytes(path), ".jpg");
            }

            new Thread(delegate()
            {
                while(true)
                {
                    try
                    {
                        List<XSteamItem> SteamItems = this.Table.SelectAll();
                        for (int i = 0; i < SteamItems.Count; i++)
                        {
                            string image;
                            if (this.GetImageFromMemory(SteamItems[i].ID, SteamItems[i].SteamGameID, out image))
                            {
                                SteamItemImageQueue SteamItemImageQueue = new SteamItemImageQueue();
                                SteamItemImageQueue.ID = SteamItems[i].ID;
                                SteamItemImageQueue.SteamGameID = SteamItems[i].SteamGameID;
                                SteamItemImageQueue.ImageURL = SteamItems[i].Image;

                                if (!SteamItemsHelper.QueueDownloadImage.Contains(SteamItemImageQueue))
                                {
                                    SteamItemsHelper.QueueDownloadImage.Enqueue(SteamItemImageQueue);
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Logger.ConsoleLog(ex, ConsoleColor.Red, LogLevel.Error);
                    }
                }
            }).Start();
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
                    if (data.Contains("\"success\":true") && (data.Contains("\"median_price\":\"") && data.Contains("\"lowest_price\":\"")))
                    {
                        string price = null;
                        if (data.Contains("\"lowest_price\":\""))
                        {
                            price = Regex.Split(data, "\"lowest_price\":\"")[1].Split('"')[0].Replace("$", "");
                        }
                        /*else
                        {
                            price = Regex.Split(data, "\"median_price\":\"")[1].Split('"')[0].Replace("$", "");
                        }*/

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
                    Thread.Sleep(60*60*12);
                }
            }).Start();
        }

        private void DownloadImagesFromQueue()
        {
            new Thread(delegate()
            {
                while (true)
                {
                    try
                    {
                        if (SteamItemsHelper.QueueDownloadImage.Count > 0)
                        {
                            SteamItemImageQueue SteamItemImageQueue = SteamItemsHelper.QueueDownloadImage.Dequeue();

                            using (WebClient WebClient = new WebClient())
                            {
                                byte[] ImageBytes = WebClient.DownloadData(SteamItemImageQueue.ImageURL);
                                Image Image = Image.FromStream(new MemoryStream(ImageBytes));
                                if (!Directory.Exists("FileStorage\\Upload\\" + Configs.STEAM_ITEMS_STORAGE + SteamItemImageQueue.SteamGameID))
                                {
                                    Directory.CreateDirectory("FileStorage\\Upload\\" + Configs.STEAM_ITEMS_STORAGE + SteamItemImageQueue.SteamGameID);
                                }

                                File.WriteAllBytes("FileStorage\\Upload\\" + Configs.STEAM_ITEMS_STORAGE + SteamItemImageQueue.SteamGameID + "\\" + SteamItemImageQueue.ID + Configs.STEAM_IMAGE_TYPE, ImageBytes);
                            }

                            this.AddSteamImageToCache(SteamItemImageQueue.ID, SteamItemImageQueue.SteamGameID);
                        }
                    }
                    catch { }
                }
               
            }).Start();
        }

        public void AddSteamImageToCache(uint ItemID, uint SteamGameID)
        {
            string path = "FileStorage\\Upload\\" + Configs.STEAM_ITEMS_STORAGE + SteamGameID + "\\" + ItemID + Configs.STEAM_IMAGE_TYPE;
            if (File.Exists(path))
            {
                if (SteamItemImages.ContainsKey(SteamGameID) && !SteamItemImages[SteamGameID].ContainsKey(ItemID))
                {
                    string image = FilesProcessor.CacheFile(File.ReadAllBytes(path), Configs.STEAM_IMAGE_TYPE);
                    SteamItemImages[SteamGameID].Add(ItemID, image);
                }
            }
            else
            {
                XSteamItem XSteamItem;
                if (this.Table.SelectByID(ItemID, out XSteamItem))
                {
                    SteamItemImageQueue SteamItemImageQueue = new SteamItemImageQueue();
                    SteamItemImageQueue.ID = ItemID;
                    SteamItemImageQueue.SteamGameID = SteamGameID;
                    SteamItemImageQueue.ImageURL = XSteamItem.Image;
                    SteamItemsHelper.QueueDownloadImage.Enqueue(SteamItemImageQueue);
                }
            }
        }

        private void AddSteamImagesToMemory()
        {
            Logger.ConsoleLog("Images to memory...", ConsoleColor.Yellow);

            List<XSteamItem> Items = this.Table.SelectAll();
            foreach(XSteamItem Item in Items)
            {
                this.AddSteamImageToCache(Item.ID, Item.SteamGameID);
            }

            Logger.ConsoleLog("Images to memory done!", ConsoleColor.Yellow);
        }

        public bool GetImageFromMemory(uint ItemID, uint SteamGameID, out string image)
        {
            if (SteamItemImages.ContainsKey(SteamGameID) && SteamItemImages[SteamGameID].ContainsKey(ItemID))
            {
                image = SteamItemImages[SteamGameID][ItemID];
                return true;
            }

            image = null;
            return false;
        }

        public List<USteamItem> SearchByString(List<USteamItem> SteamItems, string str)
        {
            if (str.Length > 0)
            {
                List<USteamItem> Items = new List<USteamItem>();
                foreach (USteamItem item in SteamItems)
                {
                    if (item.Name.ToUpper().Contains(str.ToUpper()))
                    {
                        Items.Add(item);
                    }
                }
                return Items;
            } 

            return SteamItems;
        }
    }
}
