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

        private static List<SteamItemImageQueue> QueueDownloadImage = new List<SteamItemImageQueue>();

        private static readonly object _QueueDownloadImage = new object();

        public static int LastItemPricesUpdate = 0;

        public void AddToQueueDownloadImage(SteamItemImageQueue SteamItemImageQueue)
        {
            if (!this.IsExistQueueDownloadImage(SteamItemImageQueue.ID))
            {
                lock (_QueueDownloadImage)
                {
                    SteamItemsHelper.QueueDownloadImage.Add(SteamItemImageQueue);
                }
            }
        }
        public bool IsExistQueueDownloadImage(uint id)
        {
            SteamItemImageQueue[] Queue = SteamItemsHelper.QueueDownloadImage.ToArray();
            foreach (SteamItemImageQueue SteamItemImageQueue in Queue)
            {
                if (SteamItemImageQueue != null && SteamItemImageQueue.ID == id)
                {
                    return true;
                }
            }

            return false;
        }

        public int QueueDownloadImageCount()
        {
            return SteamItemsHelper.QueueDownloadImage.Count;
        }
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
                        List<XSteamItem> SteamItems =  new List<XSteamItem>(this.Table.SelectAll());
                        for (int i = 0; i < SteamItems.Count; i++)
                        {
                            string image;
                            if (!this.GetImageFromMemory(SteamItems[i].ID, SteamItems[i].SteamGameID, out image))
                            {
                                SteamItemImageQueue SteamItemImageQueue = new SteamItemImageQueue();
                                SteamItemImageQueue.ID = SteamItems[i].ID;
                                SteamItemImageQueue.SteamGameID = SteamItems[i].SteamGameID;
                                SteamItemImageQueue.ImageURL = SteamItems[i].Image;

                                this.AddToQueueDownloadImage(SteamItemImageQueue);
                            }
                        }

                        Thread.Sleep(10);
                    }
                    catch(Exception ex)
                    {
                        Logger.ConsoleLog(ex, ConsoleColor.Red, LogLevel.Error);
                    }
                }
            }).Start();
        }

        public bool SelectByName(string name, uint SteamGameID, out USteamItem SteamItem, ushort currency)
        {
            XSteamItem XSteamItem;
            if (this.SelectByName(name, SteamGameID, out XSteamItem))
            {
                SteamItem = new USteamItem();
                SteamItem.ID = XSteamItem.ID;
                if (currency == 1)
                {
                    SteamItem.Name = XSteamItem.RusName;
                }
                else
                {
                    SteamItem.Name = XSteamItem.Name;
                }
                SteamItem.Image = XSteamItem.Image;
                SteamItem.Price = XSteamItem.Price;
                SteamItem.RarityColor = this.GetRarityColor(XSteamItem.Rarity, SteamGameID);
                SteamItem.Rarity = XSteamItem.Rarity;

                SteamItem.Color = XSteamItem.Color;
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
                SteamItem.RarityColor = this.GetRarityColor(XSteamItem.Rarity, XSteamItem.SteamGameID);
                SteamItem.Rarity = XSteamItem.Rarity;

                SteamItem.Color = XSteamItem.Color;
                SteamItem.SteamGameID = SteamGameID;

                return true;
            }

            SteamItem = null;
            return false;
        }

        public string GetRarityColor(string rarity, uint SteamGameID)
        {
            //Logger.ConsoleLog("[" + rarity + "]" + SteamGameID);

            if(SteamGameID == Configs.CSGO_STEAM_GAME_ID)
            {
                if(rarity.Equals("Common"))
                {
                    return "rgb(176, 195, 217)";
                }
                else if(rarity.Equals("Rare"))
                {
                    return "rgb(75, 105, 255)";
                }
                else if (rarity.Equals("Uncommon"))
                {
                    return "rgb(94, 152, 217)";
                }
                else if (rarity.Equals("Mythical"))
                {
                    return "rgb(136, 71, 255)";
                }
                else if (rarity.Equals("Legendary"))
                {
                    return "rgb(211, 44, 230)";
                }
                else if (rarity.Equals("Ancient"))
                {
                    return "rgb(235, 75, 75)";
                }
                else if (rarity.Equals("Contraband"))
                {
                    return "rgb(228, 174, 57)";
                }
            }

            else if (SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
            {
                if (rarity.Equals("Uncommon"))
                {
                    return "rgb(94, 152, 217)";
                }
                else if (rarity.Equals("Rare"))
                {
                    return "rgb(75, 175, 255)";
                }
                else if (rarity.Equals("Mythical"))
                {
                    return "rgb(136, 71, 255)";
                }
                else if (rarity.Equals("Immortal"))
                {
                    return "rgb(218, 174, 57)";
                }
                else if (rarity.Equals("Arcana"))
                {
                    return "rgb(173, 229, 92)";
                }
                else if (rarity.Equals("Legendary"))
                {
                    return "rgb(211, 44, 230)";
                }
            }

            return "";
        }
        public double GetMarketPrice(string ItemName, uint SteamGameID)
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    string name = ItemName.Replace(" ", "%20").Replace("|", "%7C").Replace("(", "%28").Replace(")", "%29").Replace("™", "%E2%84%A2");
                    string data = webClient.DownloadString("http://steamcommunity.com/market/priceoverview/?appid=" + SteamGameID + "&currency=1&market_hash_name=" + name);
                    if (data.Contains("\"success\":true") && (data.Contains("\"median_price\":\"") && data.Contains("\"lowest_price\":\"")))
                    {
                        string price = null;
                        if (data.Contains("\"lowest_price\":\""))
                        {
                            price = Regex.Split(data, "\"lowest_price\":\"")[1].Split('"')[0].Replace("$", "");
                        }
                        else
                        {
                            XSteamItem XSteamItem;
                            if(this.SelectByName(ItemName, SteamGameID, out XSteamItem))
                            {
                                return XSteamItem.Price;
                            }
                        }

                        return Convert.ToDouble(price);
                    }
                }

                catch {
                    string name = ItemName.Replace(" ", "%20").Replace("|", "%7C").Replace("(", "%28").Replace(")", "%29").Replace("™", "%E2%84%A2");
                    Logger.ConsoleLog("http://steamcommunity.com/market/priceoverview/?appid=" + SteamGameID + "&currency=1&market_hash_name=" + name, ConsoleColor.Red, LogLevel.Error);
                }
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

                        SteamItemsHelper.LastItemPricesUpdate = Helper.GetCurrentTime();
                    }
                    Thread.Sleep(TimeSpan.FromHours(12));
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
                            SteamItemImageQueue SteamItemImageQueue = SteamItemsHelper.QueueDownloadImage.First();
                            SteamItemsHelper.QueueDownloadImage.Remove(SteamItemImageQueue);
                            string img;
                            if (!this.GetImageFromMemory(SteamItemImageQueue.ID, SteamItemImageQueue.SteamGameID, out img))
                            {
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

                        Thread.Sleep(100);
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

                    this.AddToQueueDownloadImage(SteamItemImageQueue);
                }
            }
        }

        private void AddSteamImagesToMemory()
        {
            Logger.ConsoleLog("Images to memory...", ConsoleColor.Yellow);

            List<XSteamItem> Items = new List<XSteamItem>(this.Table.SelectAll());
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

        public string MakeTextFromRealDich(string dich)
        {
            StringBuilder res = new StringBuilder();
            byte k = 0;
            for (int i = 0; i < dich.Length; i++)
            {
                if (k == 0 && dich[i] == '\\')
                {
                    k = 1;
                }
                else if (k == 1 && dich[i] == 'u')
                {
                    k = 2;
                }
                else if (k == 2)
                {
                    string sb = dich.Substring(i + 2, 2) + dich.Substring(i, 2);

                    byte[] ns = Enumerable.Range(0, 4).Where(x => x % 2 == 0).Select(x => Convert.ToByte(sb.Substring(x, 2), 16)).ToArray();

                    res.Append(Encoding.Unicode.GetString(ns));
                    i += 3;
                    k = 0;
                }
                else
                {
                    res.Append(dich[i]);
                }
            }
            return res.ToString();
        }
    }
}
