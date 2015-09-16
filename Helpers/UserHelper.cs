using GameSlot.Database;
using GameSlot.Pages.Includes;
using GameSlot.Pages.UserPages;
using GameSlot.Types;
using System;
using System.Collections.Generic;
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
    public class UserHelper
    {
        public XTable<XUser> Table = new XTable<XUser>();
        public XTable<XChipUsersInventory> Table_ChipUsersInventory = new XTable<XChipUsersInventory>();
        public XTable<XSItemUsersInventory> Table_SteamItemUsersInventory = new XTable<XSItemUsersInventory>();

        private static Dictionary<uint, Dictionary<uint, UsersInventory>> UsersInventories = new Dictionary<uint, Dictionary<uint, UsersInventory>>();
        // user.id, List
        private static Dictionary<uint, UsersInventory> UsersInventories_CSGO = new Dictionary<uint, UsersInventory>();
        private static Dictionary<uint, UsersInventory> UsersInventories_DOTA = new Dictionary<uint, UsersInventory>();

        private static Dictionary<uint, Dictionary<uint, int>> UpdatingInventories = new Dictionary<uint, Dictionary<uint, int>>();
        // user.id, started update time
        private static Dictionary<uint, int> UpdatingInventories_CSGO = new Dictionary<uint, int>();
        private static Dictionary<uint, int> UpdatingInventories_DOTA = new Dictionary<uint, int>();

        private static Dictionary<uint, Dictionary<uint, List<InventoryWaitingClients>>> InventoryClients = new Dictionary<uint, Dictionary<uint, List<InventoryWaitingClients>>>();

        private static Dictionary<uint, List<InventoryWaitingClients>> InventoryClients_DOTA = new Dictionary<uint, List<InventoryWaitingClients>>();
        private static Dictionary<uint, List<InventoryWaitingClients>> InventoryClients_CSGO = new Dictionary<uint, List<InventoryWaitingClients>>();

        private static List<uint> OnlineUsers = new List<uint>();
        private static List<uint> UpdatingInventoryThreads = new List<uint>();

        private static readonly object _OnlineUsers = new object();
        private static readonly object _UsersSteamInventories = new object();
        private static readonly object _UpdatingInventories = new object();

        public static int ExtraOnlineUsers = 0;

        public UserHelper()
        {
            UsersInventories.Add(Configs.CSGO_STEAM_GAME_ID, UsersInventories_CSGO);
            UsersInventories.Add(Configs.DOTA2_STEAM_GAME_ID, UsersInventories_DOTA);

            UpdatingInventories.Add(Configs.CSGO_STEAM_GAME_ID, UpdatingInventories_CSGO);
            UpdatingInventories.Add(Configs.DOTA2_STEAM_GAME_ID, UpdatingInventories_DOTA);

            InventoryClients.Add(Configs.CSGO_STEAM_GAME_ID, InventoryClients_CSGO);
            InventoryClients.Add(Configs.DOTA2_STEAM_GAME_ID, InventoryClients_DOTA);

            this.UpdatingOnlineUsersList();
            this.UpdatingOnlineUsersInventories();
        }

        private Dictionary<uint, Dictionary<uint, int>> GetUpdatingInventories()
        {
            lock(_UpdatingInventories)
            {
                return new Dictionary<uint, Dictionary<uint, int>>(UpdatingInventories);
            }
        }

        public void DeleteItemFromSteamInventory(uint UserID, ulong AssertID, uint SteamGameID)
        {
            if(UserHelper.UsersInventories.ContainsKey(SteamGameID) && UserHelper.UsersInventories[SteamGameID].ContainsKey(UserID))
            {
                UsersInventory UI = UserHelper.UsersInventories[SteamGameID][UserID].Clone();

                for (int i = 0; i < UI.SteamItems.Count; i++)
                {
                    if (UI.SteamItems[i].AssertID == AssertID)
                    {
                        if (UI.LastUpdate == UserHelper.UsersInventories[SteamGameID][UserID].LastUpdate)
                        {
                            lock (_UsersSteamInventories)
                            {
                                UserHelper.UsersInventories[SteamGameID][UserID].SteamItems.RemoveAt(i);
                            }
                        }

                        return;
                    }
                }             
            }
        }

        public int GetOnlineNum()
        {
            return UserHelper.ExtraOnlineUsers + UserHelper.OnlineUsers.Count;
        }

        public List<uint> GetUpdatingInventoriesThreads()
        {
            return new List<uint>(UserHelper.UpdatingInventoryThreads);
        }

        public bool IsUserOnline(uint UserID)
        {
            return OnlineUsers.Contains(UserID);
        }

        private void UpdatingOnlineUsersInventories()
        {
            new Thread(delegate()
            {
                while (true)
                {
                    try
                    {
                        List<uint> UOnline = new List<uint>(UserHelper.OnlineUsers);
                        for(int i = 0; i < UOnline.Count; i++)
                        {
                            XUser user = this.Table.SelectByID(UOnline[i]);
                            if (!UserHelper.UpdatingInventoryThreads.Contains(user.ID))
                            {
                                UserHelper.UpdatingInventoryThreads.Add(user.ID);
                                 new Thread(delegate()
                                 {
                                     while (true)
                                     {
                                         UsersInventory csgo, dota;
                                         this.GetSteamInventory(user.ID, Configs.DOTA2_STEAM_GAME_ID, out dota, true);
                                         this.GetSteamInventory(user.ID, Configs.CSGO_STEAM_GAME_ID, out csgo, true);

                                         if (!this.IsUserOnline(user.ID))
                                         {
                                             UpdatingInventoryThreads.Remove(user.ID);
                                             break;
                                         }

                                         Thread.Sleep(Configs.INVENTORY_UPDATE_TIME);
                                     }
                                 }).Start();
                            }
                        }

                        Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        Logger.ConsoleLog(ex, ConsoleColor.Red, LogLevel.Error);
                    }
                }
            }).Start();
        }
        private void UpdatingOnlineUsersList()
        {
            new Thread(delegate()
            {
                while (true)
                {
                    try
                    {
                        List<uint> UOnline = new List<uint>();

                        for(int i = 0; i < BaseFuncs.GetOnlineClients<SiteGameSlot>().Count; i++)
                        {
                            Client client = BaseFuncs.GetOnlineClients<SiteGameSlot>()[i];
                            if(client != null && !client.Closed)
                            {
                                XUser user;
                                if (this.GetCurrentUser(client, out user) && !UOnline.Contains(user.ID))
                                {
                                    UOnline.Add(user.ID);
                                }
                            }
                        }

                        int before = UserHelper.OnlineUsers.Count;
                        UserHelper.OnlineUsers = UOnline;

                        if (before != UOnline.Count)
                        {
                            WebSocketPage.UpdateOnlineUsers(this.GetOnlineNum());
                        }

                        Thread.Sleep(1);
                    }
                    catch (Exception ex){
                        Logger.ConsoleLog(ex, ConsoleColor.Red, LogLevel.Error);
                    }
                }
            }).Start();
        }

        public SteamUser GetSteamData(ulong SteamID)
        {
            try
            {
                using (WebClient WebClient = new WebClient())
                {
                    WebClient.Encoding = Encoding.UTF8;
                    string data = WebClient.DownloadString("http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key=" + Configs.STEAM_API + "&steamids=" + SteamID);
                    if (data.Contains("\"steamid\":"))
                    {
                        SteamUser SteamUser = new SteamUser();
                        SteamUser.Name = BaseFuncs.XSSReplacer(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(Regex.Split(data, "\"personaname\": \"")[1].Split('"')[0])));
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
            if (client != null)
            {
                if (client.Session.Contains("User") && client.Session["User"] != null && this.Table.SelectByID((uint)client.Session["User"], out user))
                {
                    return true;
                }
            }
            user = new XUser();
            return false;
        }

        public bool Authorized(Client client)
        {
            return client.Session.ContainsKey("User");
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
                    user.Currency = this.GetCurrency(client);
                    user = this.Table.SelectByID(this.Table.Insert(user));
                }

                client.Session["User"] = user.ID;
                client.Session["Avatar"] = user.Avatar;
                client.Session["ProfileURL"] = user.ProfileURL;
                client.Session["Name"] = user.Name;
                client.Session["SteamID"] = user.SteamID;
                client.Session["Currency"] = user.Currency;

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
                XUser user;
                return Helper.GroupHelper.SelectByID(Convert.ToUInt32(client.Session["GroupOwnerID"]), out group, out user, client);
            }

            group = null;
            return false;
        }

        /*public void UpdateOnlineUsersInventory(uint SteamGameID)
        {
            new Thread(delegate() 
            {
                while(true)
                {
                    for (int i = 0; i < BaseFuncs.GetOnlineClients<SiteGameSlot>().Count; i++ )
                    {
                        Client client = BaseFuncs.GetOnlineClients<SiteGameSlot>()[i];
                        if (client != null && !client.Closed)
                        {
                            XUser User;
                            if (this.GetCurrentUser(client, out User))
                            {
                                UsersInventory Inventory;
                                this.GetSteamInventory(User, SteamGameID, out Inventory, true);
                            }
                        }
                    }
                    Thread.Sleep(5000);
                }
            
            }).Start();
        }*/

        public void WaitingList_InventoryClient(uint UserID, Client client, uint SteamGameID, int ItemsNum)
        {
            if(client != null)
            {
                if (InventoryClients[SteamGameID].ContainsKey(UserID))
                {
                    InventoryWaitingClients InventoryWaitingClients = new InventoryWaitingClients();
                    InventoryWaitingClients.Client = client;
                    InventoryWaitingClients.ItemNum = ItemsNum;
                    InventoryClients[SteamGameID][UserID].Add(InventoryWaitingClients);
                }
                else
                {
                    List<InventoryWaitingClients> clients = new List<InventoryWaitingClients>();


                    InventoryWaitingClients InventoryWaitingClients = new InventoryWaitingClients();
                    InventoryWaitingClients.Client = client;
                    InventoryWaitingClients.ItemNum = ItemsNum;

                    clients.Add(InventoryWaitingClients);
                    InventoryClients[SteamGameID].Add(UserID, clients);
                }
            }
        }

        public bool GetSteamInventory(uint UserID, uint SteamGameID, out UsersInventory UsersInventory, bool wait = false)
        {
            XUser User;
            if (this.Table.SelectByID(UserID, out User))
            {
                if (UsersInventories.ContainsKey(SteamGameID) && UsersInventories[SteamGameID].ContainsKey(User.ID))
                {
                    UsersInventory = UsersInventories[SteamGameID][User.ID];
                    //Logger.ConsoleLog(UsersInventory.Opened + "CERCE");
                    if (!this.GetUpdatingInventories()[SteamGameID].ContainsKey(User.ID) && UsersInventory.LastUpdate + Configs.INVENTORY_UPDATE_TIME < Helper.GetCurrentTime())
                    {
                        //Logger.ConsoleLog("again update!");
                        if (wait)
                        {
                            this.UpdateSteamInventory(User, SteamGameID);
                        }
                        else
                        {
                            new Thread(delegate() { this.UpdateSteamInventory(User, SteamGameID); }).Start();
                            //return false;
                        }
                    }

                    return true;
                }

                if (!this.GetUpdatingInventories()[SteamGameID].ContainsKey(User.ID))
                {
                    //Logger.ConsoleLog("update!");
                    if (wait)
                    {
                        this.UpdateSteamInventory(User, SteamGameID);
                    }
                    else
                    {
                        new Thread(delegate() { this.UpdateSteamInventory(User, SteamGameID); }).Start();
                    }
                }
            }
            UsersInventory = null;
            return false;
        }
        public bool GetUsersSteamInventory_Json(uint UserID, uint SteamGameID, out string inventory)
        {
            XUser User;
            if (this.Table.SelectByID(UserID, out User))
            {
                try
                {
                    using (WebClient WebClient = new WebClient())
                    {
                        string data = WebClient.DownloadString(User.ProfileURL + "inventory/json/" + SteamGameID + "/2?l=english&trading=1");
                        if (data.Contains("{\"success\":true"))
                        {
                            inventory = data;
                            return true;
                        }
                    }
                }
                catch { }
            }

            inventory = null;
            return false;
        }

        private UsersInventory UpdateSteamInventory(XUser User, uint SteamGameID, int GetItemsNum = 30)
        {
            UsersInventory UsersInventory = new UsersInventory();
            UsersInventory.Opened = false;
            if (!this.GetUpdatingInventories()[SteamGameID].ContainsKey(User.ID))
            {
                double TotalPrice = 0d;
                lock (_UpdatingInventories)
                {
                    UpdatingInventories[SteamGameID][User.ID] = Helper.GetCurrentTime();
                }
                try
                {
                    using (WebClient WebClient = new WebClient())
                    {

                        string data = WebClient.DownloadString(User.ProfileURL + "inventory/json/" + SteamGameID + "/2?l=russian&trading=1");
                       // Logger.ConsoleLog(data);
                        if (User.SteamInventoryHash.Equals(BaseFuncs.MD5(data)) && UsersInventories.ContainsKey(SteamGameID) && UsersInventories[SteamGameID].ContainsKey(User.ID)
                            && SteamItemsHelper.LastItemPricesUpdate < UsersInventories[SteamGameID][User.ID].LastUpdate)
                        {
                            UsersInventory = UsersInventories[SteamGameID][User.ID];
                        }
                        else if (data.Contains("{\"success\":true"))
                        {
                            List<USteamItem> SteamItems = new List<USteamItem>();
                            string[] Item = Regex.Split(data, "{\"id\":\"");

                            for (int i = 1; i < Item.Length; i++)
                            {
                                //Logger.ConsoleLog("On " + i + " from " + (Item.Length - 1));
                                string classid = Regex.Split(Item[i], "\"classid\":\"")[1].Split('"')[0];
                                string ItemContent = Regex.Split(data, "{\"appid\":\"" + SteamGameID + "\",\"classid\":\"" + classid + "\"")[1];

                                string name = Regex.Split(ItemContent, "\"market_hash_name\":\"")[1].Split('"')[0];
                                name = BaseFuncs.XSSReplacer(Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(name)));

                                USteamItem SteamItem;
                                if (!Helper.SteamItemsHelper.SelectByName(name, SteamGameID, out SteamItem, User.Currency))
                                {
                                    XSteamItem XSteamItem = new XSteamItem();
                                    XSteamItem.Name = name;

                                    SteamItem = new USteamItem();

                                    SteamItem.Price = XSteamItem.Price = Helper.SteamItemsHelper.GetMarketPrice(XSteamItem.Name, SteamGameID);
                                    if (XSteamItem.Price < 0)
                                    {
                                        SteamItem.Price = XSteamItem.Price = 0;
                                    }

                                    SteamItem.Price_Str = SteamItem.Price.ToString("###,##0.00");

                                    XSteamItem.Rarity = Regex.Split(ItemContent, "internal_name\":\"Rarity_")[1].Split('"')[0];
                                    SteamItem.Color = XSteamItem.Color = Regex.Split(ItemContent, "\"name_color\":\"")[1].Split('"')[0]; ;

                                    string RusName = Regex.Split(ItemContent, "\"market_name\":\"")[1].Split('"')[0];
                                    XSteamItem.RusName = BaseFuncs.XSSReplacer(Helper.SteamItemsHelper.MakeTextFromRealDich(RusName));

                                    if(SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                                    {
                                        if(XSteamItem.Rarity.Contains("_"))
                                        {
                                            XSteamItem.Rarity = XSteamItem.Rarity.Split('_')[0];
                                        }
                                    }

                                    if (User.Currency == 1)
                                    {
                                        SteamItem.Name = XSteamItem.RusName;
                                    }
                                    else
                                    {
                                        SteamItem.Name = XSteamItem.Name;
                                    }

                                    SteamItem.Image = XSteamItem.Image = "http://steamcommunity-a.akamaihd.net/economy/image/" + Regex.Split(ItemContent, "\"icon_url\":\"")[1].Split('"')[0];
                                    SteamItem.Rarity = XSteamItem.Rarity;
                                    SteamItem.RarityColor = Helper.SteamItemsHelper.GetRarityColor(XSteamItem.Rarity, SteamGameID);
                                    SteamItem.SteamGameID = XSteamItem.SteamGameID = SteamGameID;
                                    SteamItem.ID = Helper.SteamItemsHelper.Table.Insert(XSteamItem);
                                }

                                string iimgg;
                                if(!Helper.SteamItemsHelper.GetImageFromMemory(SteamItem.ID, SteamItem.SteamGameID, out iimgg))
                                {
                                    SteamItemImageQueue SteamItemImageQueue = new SteamItemImageQueue();
                                    SteamItem.ID = SteamItemImageQueue.ID = SteamItem.ID;
                                    SteamItemImageQueue.SteamGameID = SteamItem.SteamGameID;
                                    SteamItemImageQueue.ImageURL = SteamItem.Image;

                                    Helper.SteamItemsHelper.AddToQueueDownloadImage(SteamItemImageQueue);
                                }

                                if (SteamItem.Price >= Configs.MIN_ITEMS_PRICE)
                                {
                                    SteamItem.AssertID = Convert.ToUInt64(Item[i].Split('"')[0]);

                                    XSteamItemsClassID XSteamItemsClassID;
                                    if (!Helper.SteamItemsHelper.Table_ClassID.SelectOne(dt => dt.AssertID == SteamItem.AssertID, out XSteamItemsClassID))
                                    {
                                        XSteamItemsClassID = new XSteamItemsClassID();
                                        XSteamItemsClassID.AssertID = SteamItem.AssertID;
                                        XSteamItemsClassID.ClassID = Convert.ToUInt64(classid);
                                        Helper.SteamItemsHelper.Table_ClassID.Insert(XSteamItemsClassID);
                                    }

                                    if (User.Currency == 1)
                                    {
                                        SteamItem.Price = SteamItem.Price * Helper.Rub_ExchangeRate;
                                        SteamItem.Price_Str = SteamItem.Price.ToString("###,###,##0");
                                    }
                                    else
                                    {
                                        SteamItem.Price_Str = SteamItem.Price.ToString("###,##0.00");
                                    }

                                    SteamItems.Add(SteamItem);
                                    TotalPrice += SteamItem.Price;
                               }                               
                            }

                            UsersInventory.SteamItems = (from it in SteamItems orderby it.Price descending select it).ToList();
                            UsersInventory.TotalPrice = TotalPrice;
                            if (User.Currency == 1)
                            {
                                UsersInventory.TotalPrice_Str = TotalPrice.ToString("###,###,##0");
                            }
                            else
                            {
                                UsersInventory.TotalPrice_Str = TotalPrice.ToString("###,##0.00");
                            }

                            UsersInventory.Opened = true;

                            XUser cur_usr = this.Table.SelectByID(User.ID);
                            cur_usr.SteamInventoryHash = BaseFuncs.MD5(data);
                            this.Table.UpdateByID(cur_usr, User.ID);
                        }
                    }
                }
                catch { }

                UsersInventory.LastUpdate = Helper.GetCurrentTime();
                lock (_UsersSteamInventories)
                {
                    UsersInventories[SteamGameID][User.ID] = UsersInventory;
                }

                lock (_UpdatingInventories)
                {
                    UpdatingInventories[SteamGameID].Remove(User.ID);
                }

                if (InventoryClients[SteamGameID].ContainsKey(User.ID))
                {
                    for (int i = 0; i < InventoryClients[SteamGameID][User.ID].Count; i++)
                    {
                        if (InventoryClients[SteamGameID][User.ID][i].Client != null && !InventoryClients[SteamGameID][User.ID][i].Client.Closed)
                        {
                            WebSocketPage.GetWebsocket_SteamInventory(UsersInventory, InventoryClients[SteamGameID][User.ID][i].Client, 0, 0, InventoryClients[SteamGameID][User.ID][i].ItemNum, SteamGameID, "");
                        }
                    }

                    InventoryClients[SteamGameID].Remove(User.ID);
                }
            }
            return UsersInventory;
        }

        public List<Chip> GetChipInventory(uint UserID, out double TotalPrice)
        {
            double price = 0;
            XUser user;
            List<Chip> chips = new List<Chip>();
            if (Helper.UserHelper.Table.SelectByID(UserID, out user))
            {
                List<XChipUsersInventory> UChips;
                if (this.Table_ChipUsersInventory.Select(data => data.UserID == UserID && !data.Deleted, out UChips))
                {
                    foreach (XChipUsersInventory xchip in UChips)
                    {
                        //Logger.ConsoleLog(xchip.AssertID + "-" + xchip.ChipID);
                        Chip ch;
                        if (Helper.ChipHelper.SelectByID(xchip.ChipID, out ch))
                        {
                            Chip chip = ch.Clone();
                            chip.AssertID = xchip.AssertID;
                            if(user.Currency == 1)
                            {
                                chip.Cost *= Helper.Rub_ExchangeRate;
                                chip.Cost_Str = chip.Cost.ToString("###,###,##0");
                            }
                            //Logger.ConsoleLog(chip.AssertID);

                            price += chip.Cost;
                            chips.Add(chip);
                        }
                    }
                }
            }

            TotalPrice = price;
            return (from it in chips orderby it.Cost descending select it).ToList();
        }

        public List<Chip> GetChipInventory(uint UserID)
        {
            double TotalPrice;
            return this.GetChipInventory(UserID, out TotalPrice);
        }

        public List<USteamItem> GetSteamLocalInventory(uint UserID, uint SteamGameID, bool tradable=true)
        {
            List<USteamItem> Items = new List<USteamItem>();
            XUser user;
            if (this.Table.SelectByID(UserID, out user))
            {
                List<XSItemUsersInventory> x_inventory;
                if (this.Table_SteamItemUsersInventory.Select(data => data.UserID == UserID && !data.Deleted && data.SteamGameID == SteamGameID, out x_inventory))
                {
                    foreach (XSItemUsersInventory inventory in x_inventory)
                    {
                        USteamItem USteamItem;
                        if (Helper.SteamItemsHelper.SelectByID(inventory.SteamItemID, inventory.SteamGameID, out USteamItem, user.Currency))
                        {
                            if (tradable)
                            {
                                if (USteamItem.Price >= Configs.MIN_ITEMS_PRICE)
                                {
                                    USteamItem.AssertID = inventory.AssertID;

                                    if (user.Currency == 1)
                                    {
                                        USteamItem.Price *= Helper.Rub_ExchangeRate;
                                        USteamItem.Price_Str = USteamItem.Price.ToString("###,###,##0");
                                    }
                                    Items.Add(USteamItem);
                                }
                            }
                            else
                            {
                                USteamItem.AssertID = inventory.AssertID;

                                if (user.Currency == 1)
                                {
                                    USteamItem.Price *= Helper.Rub_ExchangeRate;
                                    USteamItem.Price_Str = USteamItem.Price.ToString("###,###,##0");
                                }
                                Items.Add(USteamItem);
                            }
                        }

                    }
                }
            }

            Items = (from it in Items orderby it.Price descending select it).ToList();
            return Items;
        }

        public bool GetSteamLocalItem(ulong AssertID, uint ItemID, uint UserID, out XSItemUsersInventory XSItemUsersInventory)
        {
            if (this.Table_SteamItemUsersInventory.SelectOne(data => data.AssertID == AssertID && !data.Deleted && data.UserID == UserID && data.SteamItemID == ItemID, out XSItemUsersInventory))
            {
                return true;
            }

            XSItemUsersInventory = new XSItemUsersInventory();
            return false;
        }

        public bool SelectChipByAssertID(ulong AssertID, uint UserID, out Chip Chip)
        {
            if(this.UserExist(UserID))
            {
                XChipUsersInventory XChipInventory;
                if (this.Table_ChipUsersInventory.SelectOne(data => data.AssertID == AssertID && data.UserID == UserID && !data.Deleted, out XChipInventory) && Helper.ChipHelper.SelectByID(XChipInventory.ChipID, out Chip))
                {
                    Chip.AssertID = XChipInventory.AssertID;
                    //Logger.ConsoleLog(XChipInventory.AssertID + "::" + Chip.AssertID);
                    return true;
                }
            }

            Chip = new Chip();
            return false;
        }

        public bool IsUserHaveChip(ulong AssertID, uint UserID)
        {
            Chip chip;
            return this.SelectChipByAssertID(AssertID, UserID, out chip);
        }

        public bool IsUserHaveSteamItem(ulong AssertID, uint ItemID, uint UserID)
        {
            XSItemUsersInventory xitem;
            return this.GetSteamLocalItem(AssertID, ItemID, UserID, out xitem);
        }

        public bool IsUserHaveSteamItem_SteamInventory(ulong AssertID, uint ItemID, XUser User, uint SteamGameID, string UsersInventoryString)
        {
            UsersInventory UsersInventory;
            if (User.SteamInventoryHash.Equals(BaseFuncs.MD5(UsersInventoryString)) && this.GetSteamInventory(User.ID, SteamGameID, out UsersInventory))
            {
                foreach(USteamItem Item in UsersInventory.SteamItems)
                {
                    if(Item.AssertID == AssertID && Item.ID == ItemID)
                    {
                        return true;
                    }
                }
            }
            else
            {
                //Logger.ConsoleLog("Using parsging for check item!");
                if (UsersInventoryString.Contains("{\"id\":\"" + AssertID + "\""))
                {
                    string classID = Regex.Split(Regex.Split(UsersInventoryString, "{\"id\":\"" + AssertID + "\"")[1], "\"classid\":\"")[1].Split('"')[0];
                    string ItemContent = Regex.Split(UsersInventoryString, "{\"appid\":\"" + SteamGameID + "\",\"classid\":\"" + classID + "\"")[1];

                    string name = Regex.Split(ItemContent, "\"market_hash_name\":\"")[1].Split('"')[0];
                    name = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(name));

                    USteamItem it;
                    if (Helper.SteamItemsHelper.SelectByName(name, SteamGameID, out it, 0))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

       /* public bool CheckSteamTradeURL(ulong partner, string token)
        {
            try
            {
                using (WebClient WebClient = new WebClient())
                {
                    //https://steamcommunity.com/tradeoffer/new/?partner=174498889&token=%22dsfewsdwsadws
                    string data = WebClient.DownloadString("https://steamcommunity.com/tradeoffer/new/?partner=" + partner + "&token=" + token);
                    if (!data.Contains("<div class=\"error_page_content\">"))
                    {
                        return true;
                    }
                }
            }
            catch { }

            return false;
        }*/

        public double GetLocalSteamInventoryTotalPrice(uint UserID, uint SteamGameID, out string TotalPrice_Str)
        {
            double price = 0D;
            XUser User;
            if (this.Table.SelectByID(UserID, out User))
            {
                foreach (USteamItem Item in Helper.UserHelper.GetSteamLocalInventory(UserID, SteamGameID))
                {
                    price += Item.Price;
                }

                if (User.Currency == 1)
                {
                    TotalPrice_Str = price.ToString("###,###,##0");
                }
                else
                {
                    TotalPrice_Str = price.ToString("###,##0.00");
                }
            }
            else
            {
                TotalPrice_Str = "0";
            }
            return price;
        }

        public ushort GetCurrency(Client client)
        {
            XUser user;
            if (client != null && client.Session != null)
            {
                if (this.GetCurrentUser(client, out user))
                {
                    //Logger.ConsoleLog("USER" + user.Currency);
                    return user.Currency;
                }
                else if (client.Session.Contains("Currency") && ((int)client.Session["Currency"] == 1))
                {
                    //Logger.ConsoleLog("USER" + client.Session["Currency"]);
                    return 1;
                }
            }
            //Logger.ConsoleLog("STANDART 0");
            return 0;
        }
    }
}
