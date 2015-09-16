using GameSlot.Database;
using GameSlot.Helpers;
using GameSlot.Types;
using SteamBotUTRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages.Includes
{
    public class WebSocketPage : SiteGameSlot
    {
        public override string URL
        {
            get { return "/websocket/do"; }
        }
        public override string TemplateAddr
        {
            get { return "Includes.WebSocketEvents.html"; }
        }
        public override bool FilterBefore
        {
            get { return false; }
        }
        public override bool FilterAfter
        {
            get { return false; }
        }
        public override bool MaintenanceAffect
        {
            get { return false; }
        }

        private static Dictionary<uint, List<Client>> ClientsGroupPage = new Dictionary<uint, List<Client>>();
        private static Dictionary<uint, List<Client>> ClientsLotteryPage = new Dictionary<uint,List<Client>>();

        public static void GetStats()
        {
            int lots = 0;
            foreach (uint key in ClientsLotteryPage.Keys)
            {
                lots += ClientsLotteryPage[key].Count;
            }

            int group_page = 0;
            foreach(uint key in ClientsGroupPage.Keys)
            {
                group_page += ClientsGroupPage[key].Count;
            }
            Logger.ConsoleLog("At now WS clients: [ClientsGroupPage]:" + group_page + " [ClientsLotteryPage]: " + lots, ConsoleColor.Cyan, LogLevel.Info);
        }

        public override bool Init(Client client)
        {
            if (client.ConnType == ConnectionType.WebSocket && client.WSData != null)
            {
                //Logger.ConsoleLog(client.WSData, ConsoleColor.Yellow);
                string[] wsdata = Regex.Split(client.WSData, BaseFuncs.WSplit);

                if (wsdata[0].Equals("GetProfile"))
                {
                    XUser user;
                    uint userID;
                    if (uint.TryParse(wsdata[1], out userID) && Helper.UserHelper.Table.SelectByID(userID, out user))
                    {
                        int Winrate;
                        int GamesCount = user.CSGO_GamesCount + user.DOTA_GamesCount;
                        int WonCount = user.DOTA_WonCount + user.CSGO_WonCount;

                        if (WonCount > 0)
                        {
                            Winrate = (int)(WonCount / ((double)GamesCount / 100d));
                        }
                        else
                        {
                            Winrate = 0;
                        }

                        string BetPrice = "";
                        string WonPrice = "";
                        ushort currency = Helper.UserHelper.GetCurrency(client);
                        if(currency == 1)
                        {
                            BetPrice = (user.CSGO_RUB_TotalBetPrice + user.DOTA_RUB_TotalBetPrice).ToString("###,###,##0");
                            WonPrice = (user.CSGO_RUB_WonTotalPrice + user.DOTA_RUB_WonTotalPrice).ToString("###,###,##0");
                        }
                        else if(currency == 0)
                        {
                            BetPrice = (user.CSGO_TotalBetPrice + user.DOTA_TotalBetPrice).ToString("###,##0.00");
                            WonPrice = (user.CSGO_WonTotalPrice + user.DOTA_WonTotalPrice).ToString("###,##0.00");
                        }

                        string GroupName = "";
                        XUser owner;
                        if(user.GroupOwnerID >= 0 && Helper.UserHelper.Table.SelectByID((uint)user.GroupOwnerID, out owner))
                        {
                            GroupName = (owner.GroupName.Length > 0) ? owner.GroupName : owner.Name;
                        }

                        string Games = "";

                        XLottery[] Lots = new XLottery[0];
                        Helper.LotteryHelper.Table.SelectArrFromEnd(data =>
                        {
                            if (data.WinnersToken > 0)
                            {
                                int wr = Convert.ToInt32(Math.Round((100 * data.WinnersBetPrice) / data.JackpotPrice));
                                string jc = "";

                                if (currency == 1)
                                {
                                    jc = (data.JackpotPrice * data.RubCurrency).ToString("###,###,##0");
                                }
                                else if (currency == 0)
                                {
                                    jc = data.JackpotPrice.ToString("###,##0.00");
                                }

                                ushort wnr = 0;
                                if(data.Winner == user.ID)
                                {
                                    wnr = 1;
                                }

                                Games += data.ID + ";" + data.ID.ToString("000-000-000") + ";" + wr + ";" + data.JackpotItemsNum + ";" + jc + ";" + wnr + "↑";
                            }
                            return false;
                        }, out Lots, 0, 50);


                        client.SendWebsocket("GetProfile" + BaseFuncs.WSplit + user.Name + BaseFuncs.WSplit + user.Avatar + BaseFuncs.WSplit + GamesCount
                            + BaseFuncs.WSplit + WonCount + BaseFuncs.WSplit + Winrate + BaseFuncs.WSplit + (user.CSGO_TotalBetItemsCount + user.DOTA_TotalBetItemsCount)
                            + BaseFuncs.WSplit + (user.CSGO_WonItemsCount + user.DOTA_WonItemsCount) + BaseFuncs.WSplit + BetPrice
                            + BaseFuncs.WSplit + WonPrice + BaseFuncs.WSplit + user.ProfileURL + BaseFuncs.WSplit + user.GroupOwnerID + BaseFuncs.WSplit + GroupName + BaseFuncs.WSplit + Games
                            + BaseFuncs.WSplit + ((Lots.Length < 50) ? 0 : 1));
                    }
                }

                else if (wsdata[0].Equals("GroupPage"))
                {
                    //ws.send("GroupPage{{=BaseFuncs.WSplit}}EntryToGroup{{=BaseFuncs.WSplit}}{{=Group.ID}}");
                    uint GroupID;
                    XUser User;

                    if (uint.TryParse(wsdata[2], out GroupID))
                    {
                        if (wsdata[1].Equals("Connect"))
                        {

                            if (WebSocketPage.ClientsGroupPage.ContainsKey(GroupID))
                            {
                                WebSocketPage.ClientsGroupPage[GroupID].Add(client);
                            }
                            else
                            {
                                List<Client> cls = new List<Client>();
                                cls.Add(client);
                                WebSocketPage.ClientsGroupPage.Add(GroupID, cls);
                            }
                        }
                        else if (wsdata[1].Equals("EntryToGroup") && Helper.UserHelper.GetCurrentUser(client, out User))
                        {
                            int OldGroup = User.GroupOwnerID;
                            if (Helper.GroupHelper.EnterToGroup(GroupID, client))
                            {
                                WebSocketPage.UpdateGroupData(GroupID);
                                if (OldGroup > -1)
                                {
                                    WebSocketPage.UpdateGroupData(Convert.ToUInt32(OldGroup));
                                }
                            }
                        }

                        else if (wsdata[1].Equals("ExitFromGroup") && Helper.UserHelper.GetCurrentUser(client, out User))
                        {
                            if (Helper.GroupHelper.ExitFromGroup(GroupID, client))
                            {
                                WebSocketPage.UpdateGroupData(GroupID);
                            }
                        }

                    }
                }
                else if(wsdata[0].Equals("MyGroupPage"))
                {
                    XUser User;
                    if (Helper.UserHelper.GetCurrentUser(client, out User))
                    {
                        if (wsdata[1].Equals("ChangeName"))
                        {
                            if (Helper.GroupHelper.UpdateNameByID(User.ID, wsdata[2]))
                            {
                                WebSocketPage.UpdateGroupData(User.ID);
                            }
                        }
                    }
                }
                else if (wsdata[0].Equals("GetInventory"))
                {
                    XUser User;
                    uint SteamGameID;
                    int ItemsNum;
                    int FromNum;
                    ushort SortByLowPrice;

                    if (Helper.UserHelper.GetCurrentUser(client, out User) && uint.TryParse(wsdata[1], out SteamGameID) && int.TryParse(wsdata[2], out FromNum) && int.TryParse(wsdata[3], out ItemsNum) && ushort.TryParse(wsdata[4], out SortByLowPrice))
                    {
                        UsersInventory Inventory = null;
                        bool InList = Helper.UserHelper.GetSteamInventory(User.ID, SteamGameID, out Inventory);
                        if (InList)
                        {
                            WebSocketPage.GetWebsocket_SteamInventory(Inventory, client, SortByLowPrice, FromNum, ItemsNum, SteamGameID, wsdata[5]);
                        }
                        else
                        {
                            Helper.UserHelper.WaitingList_InventoryClient(User.ID, client, SteamGameID, ItemsNum);
                        }
                    }
                }
                else if (wsdata[0].Equals("GetLocalInventory"))
                {
                    XUser User;
                    uint SteamGameID;
                    int ItemsNum;
                    int FromNum;
                    ushort SortByLowPrice;
                    if (Helper.UserHelper.GetCurrentUser(client, out User) && uint.TryParse(wsdata[1], out SteamGameID) && int.TryParse(wsdata[2], out FromNum) && int.TryParse(wsdata[3], out ItemsNum) && ushort.TryParse(wsdata[4], out SortByLowPrice))
                    {
                        Logger.ConsoleLog("Local:" + ItemsNum);
                        string StrItems = "";
                        List<USteamItem> SteamItems = Helper.SteamItemsHelper.SearchByString(Helper.UserHelper.GetSteamLocalInventory(User.ID, SteamGameID), wsdata[5]);

                        if (SortByLowPrice == 1)
                        {
                            SteamItems = (from it in SteamItems orderby it.Price ascending select it).ToList();
                        }

                        if (FromNum < 0)
                        {
                            FromNum = 0;
                        }
                        else if (FromNum > ItemsNum && FromNum == SteamItems.Count)
                        {
                            FromNum -= ItemsNum;
                        }

                        for (int i = FromNum; i < Math.Min(FromNum + ItemsNum, SteamItems.Count); i++)
                        {
                            StrItems += WebSocketPage.InventoryItemToString(SteamItems[i]);
                        }
                        string total_price_str;
                        Helper.UserHelper.GetLocalSteamInventoryTotalPrice(User.ID, SteamGameID, out total_price_str);
                        WebSocketPage.UpdateLocalInventory(StrItems, total_price_str, SteamItems.Count, client);
                    }
                }
                else if (wsdata[0].Equals("LotteryPage"))
                {
                    XLottery x_lot;
                    uint LotteryID;
                    if (wsdata[1].Equals("Connect") && uint.TryParse(wsdata[2], out LotteryID) && Helper.LotteryHelper.Table.SelectByID(LotteryID, out x_lot))
                    {
                        if (WebSocketPage.ClientsLotteryPage.ContainsKey(x_lot.ID))
                        {
                            WebSocketPage.ClientsLotteryPage[x_lot.ID].Add(client);
                        }
                        else
                        {
                            List<Client> cls = new List<Client>();
                            cls.Add(client);
                            WebSocketPage.ClientsLotteryPage.Add(x_lot.ID, cls);
                        }
                    }
                    else if (wsdata[1].Equals("SetBet"))
                    {
                        /*assert_id : item_type (0: chip, 1: steam: 2: local) ; */

                        XUser user;
                        if (Helper.UserHelper.GetCurrentUser(client, out user))
                        {
                            if (uint.TryParse(wsdata[2], out LotteryID) && Helper.LotteryHelper.Table.SelectByID(LotteryID, out x_lot))
                            {
                                string[] bet = wsdata[3].Split(';');
                                if (bet.Length > 1)
                                {
                                    List<USteamItem> SteamItems = new List<USteamItem>();
                                    List<Chip> Chips = new List<Chip>();

                                    for (int i = 0; i < Math.Min(25, bet.Length); i++)
                                    {
                                        string[] item = bet[i].Split(':');
                                        int item_type;
                                        ulong assert_id;
                                        uint item_id;

                                        if (item.Length >= 2 && ulong.TryParse(item[0], out assert_id) && int.TryParse(item[1], out item_type) && uint.TryParse(item[2], out item_id))
                                        {
                                            if (item_type == 1 || item_type == 2)
                                            {
                                                USteamItem SteamItem = new USteamItem();
                                                SteamItem.ID = item_id;
                                                SteamItem.AssertID = assert_id;
                                                SteamItems.Add(SteamItem);
                                            }
                                            else if (item_type == 3)
                                            {
                                                Chip chip = new Chip();
                                                chip.ID = item_id;
                                                chip.AssertID = assert_id;
                                                Chips.Add(chip);
                                            }

                                        }
                                    }

                                    ushort result = Helper.LotteryHelper.SetBet(x_lot.ID, user.ID, SteamItems, Chips, client);
                                    Logger.ConsoleLog(result + "::test from: " + SteamItems.Count + "::" + Chips.Count);

                                    if (result != 2)
                                    {
                                        client.SendWebsocket("BetDone" + BaseFuncs.WSplit + result);
                                    }
                                }
                            }
                        }
                    }                   
                }
                else if (wsdata[0].Equals("UpdateTradeURL"))
                {
                    XUser user;
                    if (Helper.UserHelper.GetCurrentUser(client, out user))
                    {
                        string token;
                        ulong partner;
                        if (wsdata[1].Contains("partner=") && wsdata[1].Contains("token=") && ulong.TryParse(Regex.Split(wsdata[1], "partner=")[1].Split('&')[0], out partner))
                        {
                            token = Regex.Split(wsdata[1], "token=")[1];
                            if (token.Length > 0 && partner > 0)
                            {
                                if (token != user.TradeToken || partner != user.TradePartner)
                                {
                                    user.TradeToken = token;
                                    user.TradePartner = partner;
                                    Helper.UserHelper.Table.UpdateByID(user, user.ID);
                                    Logger.ConsoleLog("Updated trade url (" + user.ID + "):" + user.TradeToken);
                                }

                                client.SendWebsocket("UpdateTradeURL" + BaseFuncs.WSplit + "1" + BaseFuncs.WSplit + partner + BaseFuncs.WSplit + token);
                            }
                        }
                        else
                        {
                            client.SendWebsocket("UpdateTradeURL" + BaseFuncs.WSplit + "0");
                        }
                    }
                }

                else if(wsdata[0].Equals("SendLocalItemsToSteam"))
                {
                    XUser user;
                    uint SteamGameID;
                    if (Helper.UserHelper.GetCurrentUser(client, out user) && uint.TryParse(wsdata[1], out SteamGameID))
                    {
                        List<USteamItem> SteamItems = Helper.UserHelper.GetSteamLocalInventory(user.ID, SteamGameID);
                        if(SteamItems.Count > 0)
                        {
                            Dictionary<uint, List<UTRequestSteamItem>> BotsRequests = new Dictionary<uint, List<UTRequestSteamItem>>();
                            Dictionary<long, uint> AssertItemsIDs = new Dictionary<long, uint>();

                            foreach(USteamItem SteamItem in SteamItems)
                            {
                                XSItemUsersInventory XSItemUsersInventory;
                                if(Helper.UserHelper.Table_SteamItemUsersInventory.SelectOne(data => data.AssertID == SteamItem.AssertID && !data.Deleted, out XSItemUsersInventory))
                                {
                                    XSItemUsersInventory.Deleted = true;
                                    Helper.UserHelper.Table_SteamItemUsersInventory.UpdateByID(XSItemUsersInventory, XSItemUsersInventory.ID);

                                    UTRequestSteamItem UTRequestSteamItem = new UTRequestSteamItem();
                                    UTRequestSteamItem.appid = (int)SteamItem.SteamGameID;
                                    UTRequestSteamItem.contextid = 2;

                                    UTRequestSteamItem.assertid = (long)SteamItem.AssertID;

                                    AssertItemsIDs.Add((long)SteamItem.AssertID, SteamItem.ID);

                                    if (BotsRequests.ContainsKey(SteamItem.SteamBotID))
                                    {
                                        BotsRequests[SteamItem.SteamBotID].Add(UTRequestSteamItem);
                                    }
                                    else
                                    {
                                        List<UTRequestSteamItem> item_rq = new List<UTRequestSteamItem>();
                                        item_rq.Add(UTRequestSteamItem);
                                        BotsRequests.Add(SteamItem.SteamBotID, item_rq);
                                    }
                                }
                            }

                            foreach (uint key in BotsRequests.Keys)
                            {
                                if (!UTSteam.SendClientOffer.ContainsKey(user.SteamID))
                                {
                                    UTSteam.SendClientOffer.Add(user.SteamID, client);
                                }
                                else if (UTSteam.SendClientOffer[user.SteamID] != client)
                                {
                                    UTSteam.SendClientOffer[user.SteamID] = client;
                                }

                                XBotsOffer XBotsOffer;
                                while (!Helper.SteamBotHelper.Table_BotsOffer.SelectOne(bt => bt.SteamUserID == user.SteamID && bt.Status == 0, out XBotsOffer))
                                {
                                    XBotsOffer = new XBotsOffer();
                                    XBotsOffer.BotID = key;
                                    XBotsOffer.SteamUserID = user.SteamID;
                                    XBotsOffer.SentTime = Helper.GetCurrentTime();
                                    uint bots_offerid = Helper.SteamBotHelper.Table_BotsOffer.Insert(XBotsOffer);

                                    UTRequestSteamMain UTRequestSteamMain = new UTRequestSteamMain();

                                    foreach (UTRequestSteamItem URequest in BotsRequests[key])
                                    {
                                        XBotOffersItem XBotOffersItem = new XBotOffersItem();
                                        XBotOffersItem.BotsOfferID = bots_offerid;
                                        XBotOffersItem.AssertID = (ulong)URequest.assertid;

                                        Logger.ConsoleLog("Sending item ID: ");
                                        Logger.ConsoleLog(AssertItemsIDs[URequest.assertid]);

                                        XBotOffersItem.SteamItemID = AssertItemsIDs[URequest.assertid];

                                        Helper.SteamBotHelper.Table_BotOffersItem.Insert(XBotOffersItem);

                                        XSteamItemsClassID XSteamItemsClassID;
                                        Helper.SteamItemsHelper.Table_ClassID.SelectOne(dt => dt.AssertID == XBotOffersItem.AssertID, out XSteamItemsClassID);
                                        URequest.assertid = (long)XSteamItemsClassID.ClassID;

                                        UTRequestSteamMain.Items.Add(URequest);

                                        Logger.ConsoleLog("-----------------------------");
                                        Logger.ConsoleLog("Item class id: " + URequest.assertid);
                                        Logger.ConsoleLog("Item contex id: " + URequest.contextid);
                                        Logger.ConsoleLog("Item app id: " + URequest.appid);
                                        Logger.ConsoleLog("Steam item id: " + XBotOffersItem.SteamItemID);
                                    }

                                    UTRequestSteamMain.message = "Peredacha veshhej iz vremennogo inventarja GAMESLOT. Dannoe predlozhenie budet avtomaticheski udaleno spustja chas.";
                                    UTRequestSteamMain.BotID = (int)key;
                                    UTRequestSteamMain.steamid = user.SteamID.ToString();
                                    UTRequestSteamMain.trade_acc_id = user.TradeToken;
                                    UTRequestSteamMain.SendItems = true;

                                    Logger.ConsoleLog("Sending offer from local inventory... [" + UTRequestSteamMain.Items.Count + "] Bot ID:" + UTRequestSteamMain.BotID);
                                    UpTunnel.Sender.Send(UTSteam.sk, UTRequestSteamMain);
                                    Thread.Sleep(300);
                                }
                            }
                        }
                    }
                }

                else if (wsdata[0].Equals("BuyChip"))
                {
                    XUser user;
                    uint chipID;
                    if (Helper.UserHelper.GetCurrentUser(client, out user) && uint.TryParse(wsdata[1], out chipID))
                    {
                        Chip chip;
                        if(Helper.ChipHelper.SelectByID(chipID, out chip))
                        {
                            double ChipPrice = chip.Cost;

                            if(user.Currency == 1)
                            {
                                ChipPrice *= Helper.Rub_ExchangeRate;
                                if(user.Wallet >= ChipPrice)
                                {
                                    user.Wallet -= ChipPrice;
                                    Helper.UserHelper.Table.UpdateByID(user, user.ID);
                                    Helper.ChipHelper.AddChipToUser(chip.ID, user.ID);

                                    client.SendWebsocket("BuyChip" + BaseFuncs.WSplit + "1" + BaseFuncs.WSplit + chip.ID + BaseFuncs.WSplit + user.Wallet);
                                }
                                else
                                {
                                    client.SendWebsocket("BuyChip" + BaseFuncs.WSplit + "0");
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static void UpdateGroupData(uint GroupID)
        {
            UGroup group;
            XUser user;
            if (Helper.GroupHelper.SelectByID(GroupID, out group, out user, null) && WebSocketPage.ClientsGroupPage.ContainsKey(group.ID))
            {
                for (int i = 0; i < WebSocketPage.ClientsGroupPage[group.ID].Count; i++)
                {
                    if (WebSocketPage.ClientsGroupPage[group.ID] == null || WebSocketPage.ClientsGroupPage[group.ID][i].Closed)
                    {
                        WebSocketPage.ClientsGroupPage[group.ID].Remove(WebSocketPage.ClientsGroupPage[group.ID][i--]);
                        continue;
                    }

                    int UsersGroupID = -1;
                    if (Helper.UserHelper.GetCurrentUser(WebSocketPage.ClientsGroupPage[group.ID][i], out user))
                    {
                        UsersGroupID = user.GroupOwnerID;
                    }
                    // 0: action, 1:name, 2: usercouent, 3: group_id, 4: my_group_id
                    WebSocketPage.ClientsGroupPage[group.ID][i].SendWebsocket("UpdateGroupData" + BaseFuncs.WSplit + group.Name + BaseFuncs.WSplit + group.UserCount + BaseFuncs.WSplit + group.ID + BaseFuncs.WSplit + UsersGroupID);
                }
            }
        }

        public static void UpdateInventory(string StrItems, string TotalPrice_Str, int ItemsNum, Client client, bool InventoryOpened)
        {
            if (InventoryOpened)
            {
                client.SendWebsocket("SteamInventory" + BaseFuncs.WSplit + TotalPrice_Str + BaseFuncs.WSplit + StrItems + BaseFuncs.WSplit + ItemsNum);
                //Logger.ConsoleLog(TotalPrice, ConsoleColor.Red);
                return;
            }

            client.SendWebsocket("SteamInventoryClosed" + BaseFuncs.WSplit);
        }

        public static void UpdateLocalInventory(string StrItems, string TotalPrice_Str, int ItemsNum, Client client)
        {
            client.SendWebsocket("LocalInventory" + BaseFuncs.WSplit + TotalPrice_Str + BaseFuncs.WSplit + StrItems + BaseFuncs.WSplit + ItemsNum);
        }

        public static string InventoryItemToString(USteamItem SteamItem)
        {
            string image;
            Helper.SteamItemsHelper.GetImageFromMemory(SteamItem.ID, SteamItem.SteamGameID, out image);
            //Logger.ConsoleLog(image + "::" + SteamItem.ID + "::" + SteamItem.SteamGameID);
            // 4- last
            //return SteamItem.Name + "↓" + SteamItem.Price_Str + "↓" + SteamItem.SteamGameID + "↓" + SteamItem.ID + "↓" + SteamItem.AssertID + "↓" + SteamItem.Image + ";";
            //Logger.ConsoleLog("[" + SteamItem.Color);
            return SteamItem.Name + "↓" + SteamItem.Price_Str + "↓" + SteamItem.SteamGameID + "↓" + SteamItem.ID + "↓" + SteamItem.AssertID + "↓" + SteamItem.Color + "↓" + SteamItem.Rarity + "↓" + SteamItem.RarityColor + "→";
        }

        /*public static void ChangeBetProcessStatus(ulong UserSteamID, ushort status)
        {
            Logger.ConsoleLog("Send WS TO USER!! status: " + status, ConsoleColor.Yellow);
        }*/

        public static void AddNewLotteryBet(XLotteryBet XBet, int ExtraTime, int LotteryLeftTime)
        {
            XLottery lottery = Helper.LotteryHelper.Table.SelectByID(XBet.LotteryID);

            if (WebSocketPage.ClientsLotteryPage.ContainsKey(XBet.LotteryID))
            {
                XUser user;
                Helper.UserHelper.Table.SelectByID(XBet.UserID, out user);

                XLotteryUsersBetsPrice XLotteryUsersBetsPrice;
                Helper.LotteryHelper.TableUsersBetsPrice.SelectOne(data => data.UserID == XBet.UserID && data.LotteryID == XBet.LotteryID, out XLotteryUsersBetsPrice);

                double TotalPrice = XLotteryUsersBetsPrice.TotalBetsPrice;
                int TotalItemsNum = XLotteryUsersBetsPrice.TotalBetsItemsNum;
                int BetsCount = XLotteryUsersBetsPrice.BetsCount;

                // first_token : last_token : bet_items_num : bet_price ↓
                string OtherBets = "";

                int Winrate = (int)Math.Round(TotalPrice / (lottery.JackpotPrice / 100));
                if (Winrate > 100)
                {
                    Winrate = 100;
                }
                else if(Winrate <= 0)
                {
                    Winrate = 1;
                }

                List<USteamItem> BankSteamItems;
                List<Chip> BankChips;
                Helper.LotteryHelper.GetBetItems(XBet.LotteryID, null, out BankSteamItems, out BankChips);

                List<Bet> AllBets = Helper.LotteryHelper.GetBets(XBet.LotteryID, null, true);

                // Logger.ConsoleLog(Winrate + ":" + TotalPrice + "::" + Bank);

                string UsersGroupName = "";
                if(user.GroupOwnerID >= 0)
                {
                    UsersGroupName = Helper.GroupHelper.SelectByID((uint)user.GroupOwnerID).Name;
                }

                for (int i = 0; i < WebSocketPage.ClientsLotteryPage[XBet.LotteryID].Count; i++)
                {
                    if (WebSocketPage.ClientsLotteryPage[XBet.LotteryID][i] == null || WebSocketPage.ClientsLotteryPage[XBet.LotteryID][i].Closed)
                    {
                        WebSocketPage.ClientsLotteryPage[XBet.LotteryID].RemoveAt(i--);
                        continue;
                    }

                    ushort currency = Helper.UserHelper.GetCurrency(ClientsLotteryPage[XBet.LotteryID][i]);

                    List<TopPriceItem> TopPriceItems = new List<TopPriceItem>();

                    for (int g = 0; g < Math.Min(7, BankSteamItems.Count); g++)
                    {
                        TopPriceItem TopPriceItem = new TopPriceItem();
                        TopPriceItem.Type = 0;
                        TopPriceItem.Price = BankSteamItems[g].Price;
                        TopPriceItem.Position = g;

                        TopPriceItems.Add(TopPriceItem);
                    }

                    for (int g = 0; g < Math.Min(7, BankChips.Count); g++)
                    {
                        TopPriceItem TopPriceItem = new TopPriceItem();
                        TopPriceItem.Type = 1;
                        TopPriceItem.Price = BankChips[g].Cost;
                        TopPriceItem.Position = g;

                        TopPriceItems.Add(TopPriceItem);
                    }

                    TopPriceItems = (from it in TopPriceItems orderby it.Price descending select it).ToList();

                    string top_items = "";
                    for (int g = 0; g < Math.Min(7, TopPriceItems.Count); g++)
                    {
                        TopPriceItem CurTopPriceItem = TopPriceItems[g];

                        // name↓image↓price; 
                        if (TopPriceItems[g].Type == 0)
                        {
                            string image = "/steam-image/" + BankSteamItems[CurTopPriceItem.Position].SteamGameID + "/" + BankSteamItems[CurTopPriceItem.Position].ID + "↓";

                            if (currency == 1)
                            {
                                top_items += Helper.SteamItemsHelper.Table.SelectByID(BankSteamItems[CurTopPriceItem.Position].ID).RusName + "↓";
                                top_items += image;
                                double price = BankSteamItems[CurTopPriceItem.Position].Price * lottery.RubCurrency;
                                top_items += price.ToString("###,###,##0") + "↓";
                            }
                            else
                            {
                                top_items += BankSteamItems[CurTopPriceItem.Position].Name + "↓";
                                top_items += image;
                                top_items += BankSteamItems[CurTopPriceItem.Position].Price_Str + "↓";
                            }

                            top_items += BankSteamItems[CurTopPriceItem.Position].Color + "↓";
                            top_items += BankSteamItems[CurTopPriceItem.Position].Rarity + "↓";
                            top_items += BankSteamItems[CurTopPriceItem.Position].RarityColor + "→";
                        }
                        else
                        {
                            top_items += "Фишка на сумму " + BankChips[CurTopPriceItem.Position].Cost + "$↓";
                            top_items += "/chip-image/" + BankChips[CurTopPriceItem.Position].ID + "↓";
                            if (currency == 1)
                            {
                                double price = BankChips[CurTopPriceItem.Position].Cost * lottery.RubCurrency;
                                top_items += price.ToString("###,###,##0") + "→";
                            }
                            else
                            {
                                top_items += BankChips[CurTopPriceItem.Position].Cost + "→";
                            }
                        }
                    }

                    string ws = "";
                    string TotalPrice_Str;
                    string Current_Bet_TotalPrice;
                    // 1: bank
                    if (currency == 1)
                    {
                        ws = (lottery.JackpotPrice * lottery.RubCurrency).ToString("###,###,##0");
                        TotalPrice_Str = (TotalPrice * lottery.RubCurrency).ToString("###,###,##0");

                        Current_Bet_TotalPrice = (XBet.TotalPrice * lottery.RubCurrency).ToString("###,###,##0");
                    }
                    else
                    {
                        ws = lottery.JackpotPrice.ToString("###,##0.00");
                        TotalPrice_Str = TotalPrice.ToString("###,##0.00");
                        Current_Bet_TotalPrice = (XBet.TotalPrice).ToString("###,##0.00");
                    }

                    // 4: ExtraTime
                    ws += BaseFuncs.WSplit + XBet.TotalPrice + BaseFuncs.WSplit + lottery.JackpotItemsNum + BaseFuncs.WSplit + ExtraTime + BaseFuncs.WSplit;
                    // 5: items_num; 6: totalprice; 7: first_token; 8: last_token
                    ws += XBet.SteamItemsNum + XBet.ChipsNum + BaseFuncs.WSplit + TotalPrice_Str + BaseFuncs.WSplit + XBet.FisrtToken.ToString("D8") + BaseFuncs.WSplit + XBet.LastToken.ToString("D8") + BaseFuncs.WSplit;
                    // 9: bets_num; 10: TotalPrice (bets); 11: TotalItems (bets); 12: WINRATE 
                    ws += BetsCount + BaseFuncs.WSplit + Current_Bet_TotalPrice + BaseFuncs.WSplit + TotalItemsNum + BaseFuncs.WSplit + Winrate + BaseFuncs.WSplit;
                    // 13: OtherBets; 14: UserID; 15: UserName; 16: UsersAvatar 17: BetID 18: LotteryLeftTime
                    ws += OtherBets + BaseFuncs.WSplit + user.ID + BaseFuncs.WSplit + user.Name + BaseFuncs.WSplit + user.Avatar + BaseFuncs.WSplit + XBet.ID + BaseFuncs.WSplit + LotteryLeftTime + BaseFuncs.WSplit;
                    // 19 (steam_items): ITEM_ID :: GAME_ID, ITEM_NAME, ITEM_PRICE ↓ (REPEAT); 20 (chips): ID :: Price ↓ 

                    for (uint g = 0; g < XBet.SteamItemsNum; g++)
                    {
                        XSteamItem SteamItem;
                        Helper.SteamItemsHelper.SelectByID(XBet.SteamItemIDs[g], lottery.SteamGameID, out SteamItem);

                        string ItemPrice;
                        double Price = 0d;
                        if(user.Currency == 1)
                        {
                            SteamItem.Name = Helper.SteamItemsHelper.Table.SelectByID(SteamItem.ID).RusName;
                            Price = XBet.SteamItemsPrice[g] * lottery.RubCurrency;
                            ItemPrice = Price.ToString("###,###,##0");
                        }
                        else
                        {
                            Price = XBet.SteamItemsPrice[g];
                            ItemPrice = Price.ToString("###,##0.00");
                        }
                        ws += SteamItem.ID + "::" + lottery.SteamGameID + "::" + SteamItem.Name + "::" + ItemPrice + "::" + SteamItem.Rarity + "::" + SteamItem.Color + "::" 
                            + Helper.SteamItemsHelper.GetRarityColor(SteamItem.Rarity, SteamItem.SteamGameID) + "↓";
                    }

                    ws += BaseFuncs.WSplit;
                    for (uint g = 0; g < XBet.ChipsNum; g++)
                    {
                        Chip chip;
                        Helper.ChipHelper.SelectByID(XBet.ChipIDs[g], out chip);

                        string chip_name = chip.Cost.ToString() + "$";
                        if(user.Currency == 1)
                        {
                            chip_name = "Фишка на сумму " + chip.Cost.ToString() + "$";
                            chip.Cost *= lottery.RubCurrency;
                            chip.Cost_Str = chip.Cost.ToString("###,###,##0");
                        }

                        ws += chip.ID + "::" + chip.Cost_Str + "::" + chip_name + "$↓";
                    }

                    // 21: top_items
                    ws += BaseFuncs.WSplit + top_items;

                    string GamersStats = "";
                    foreach (Bet UsersBet in AllBets)
                    {
                        GamersStats += UsersBet.XUser.ID + "↓";
                        GamersStats += UsersBet.Winrate + "↓";
                        GamersStats += UsersBet.BetsNum + "↓";
                        GamersStats += UsersBet.TotalItemsNum + "↓";

                        if(currency == 1)
                            GamersStats += (UsersBet.TotalPrice * lottery.RubCurrency).ToString("###,###,##0") + ";";
                        else
                            GamersStats += UsersBet.TotalPrice_Str + ";";
                    }

                    //22: all bets
                    ws += BaseFuncs.WSplit + GamersStats;
                    // 23 group
                    ws += BaseFuncs.WSplit + user.GroupOwnerID;
                    // 24 group name
                    ws += BaseFuncs.WSplit + UsersGroupName;
                    // 25
                    ws += BaseFuncs.WSplit + lottery.GamersCount;

                    WebSocketPage.ClientsLotteryPage[XBet.LotteryID][i].SendWebsocket("AddedNewLotteryBet" + BaseFuncs.WSplit + ws);
                }
            }

            WebSocketPage.UpdateMainPageLotteries(lottery);
        }

        public static void UpdateMainPageLotteries(XLottery lottery)
        {
            int LotteryLeftTime = (lottery.EndTime > 0) ? Helper.LotteryHelper.CalcLeftTime(lottery.ID) : -1;
            int ItemsNumRecord;
            double Today_JackpotPriceRecord;
            int TodayGamesNum = Helper.LotteryHelper.TodaysGames(lottery.SteamGameID, out Today_JackpotPriceRecord, out ItemsNumRecord, 0).Length;

            double MaxJackpot_Rub = LotteryHelper.RUB_MaxJackpotPrice[lottery.SteamGameID];
            double MaxJackpot = LotteryHelper.MaxJackpotPrice[lottery.SteamGameID];

            if(lottery.JackpotPrice > MaxJackpot)
            {
                MaxJackpot = lottery.JackpotPrice;
            }
            if((lottery.JackpotPrice * lottery.RubCurrency) > MaxJackpot_Rub)
            {
                MaxJackpot_Rub = (lottery.JackpotPrice * lottery.RubCurrency);
            }

            string MaxJackpot_Rub_Str = MaxJackpot_Rub.ToString("###,###,##0");
            string MaxJackpot_Str = MaxJackpot.ToString("###,##0.00");

            Client[] wclients = BaseFuncs.GetWebsocketClients<SiteGameSlot>().ToArray();
            for (int i = 0; i < wclients.Length; i++)
            {
                string max_jackpot = "";
                Client client = wclients[i];
                if (client != null)
                {
                    ushort currency = Helper.UserHelper.GetCurrency(client);
                    string jackpot = "";
                    if (currency == 1)
                    {
                        jackpot = (lottery.JackpotPrice * lottery.RubCurrency).ToString("###,###,##0");
                        max_jackpot = MaxJackpot_Rub_Str;
                    }

                    else if (currency == 0)
                    {
                        jackpot = lottery.JackpotPrice.ToString("###,##0.00");
                        max_jackpot = MaxJackpot_Str;
                    }

                    client.SendWebsocket("MainPageLotteries" + BaseFuncs.WSplit + lottery.SteamGameID + BaseFuncs.WSplit + lottery.GamersCount + BaseFuncs.WSplit + jackpot + BaseFuncs.WSplit + LotteryLeftTime + BaseFuncs.WSplit + lottery.JackpotItemsNum
                        + BaseFuncs.WSplit + TodayGamesNum + BaseFuncs.WSplit + max_jackpot);
                }
            }
        }
        public static void SendLotteryRoulette(XLottery XLottery, List<LotteryRouletteData> RouletteData, XUser Winner)
        {
            if (ClientsLotteryPage.ContainsKey(XLottery.ID))
            {                    
                // from 1: JackpotItems 2: JackpotPrice 3:Winner.ID 4: Winner.Name 5:RaundNumber 6: WinnerToken 7:(JackpotPrice * 100) 8: Winner.Avatar
                string ws = "";
                // 12: data
                foreach (LotteryRouletteData data in RouletteData)
                {
                    ws += data.UsersAvatar + "↑" + data.Token.ToString("D8") + "↑" + data.Winner + "↓";
                }

                for (int i = 0; i < WebSocketPage.ClientsLotteryPage[XLottery.ID].Count; i++)
                {
                    if (WebSocketPage.ClientsLotteryPage[XLottery.ID][i] == null || WebSocketPage.ClientsLotteryPage[XLottery.ID][i].Closed)
                    {
                        WebSocketPage.ClientsLotteryPage[XLottery.ID].RemoveAt(i--);
                        continue;
                    }

                    string extra = XLottery.JackpotItemsNum + BaseFuncs.WSplit;

                    string WinnersBetPrice;
                    if (Helper.UserHelper.GetCurrency(WebSocketPage.ClientsLotteryPage[XLottery.ID][i]) == 1)
                    {
                        extra += (XLottery.JackpotPrice * XLottery.RubCurrency).ToString("###,###,##0");
                        WinnersBetPrice = (XLottery.WinnersBetPrice * XLottery.RubCurrency).ToString("###,###,##0");
                    }
                    else
                    {
                        extra += XLottery.JackpotPrice.ToString("###,##0.00");
                        WinnersBetPrice = XLottery.WinnersBetPrice.ToString("###.##0.00");
                    }

                    extra += BaseFuncs.WSplit + Winner.ID + BaseFuncs.WSplit + Winner.Name + BaseFuncs.WSplit;
                    extra += XLottery.RaundNumber + BaseFuncs.WSplit + XLottery.WinnersToken + BaseFuncs.WSplit + (XLottery.JackpotPrice * 100) + BaseFuncs.WSplit + Winner.Avatar + BaseFuncs.WSplit;
                    // 9: wonrate; 10: winners_items_num; 11: winners_price
                    extra += XLottery.Wonrate + BaseFuncs.WSplit + XLottery.WinnersBetItemsNum + BaseFuncs.WSplit + WinnersBetPrice + BaseFuncs.WSplit;
                    extra += ws + BaseFuncs.WSplit;

                    XUser CurrentUser;
                    if (Helper.UserHelper.GetCurrentUser(WebSocketPage.ClientsLotteryPage[XLottery.ID][i], out CurrentUser) && CurrentUser.ID == Winner.ID)
                    {
                        extra += 1.ToString();
                        Logger.ConsoleLog("Winner detected!", ConsoleColor.Yellow);
                    }
                    else
                    {
                        extra += 0.ToString();
                    }
                    //14
                    extra += BaseFuncs.WSplit + BaseFuncs.MD5(XLottery.RaundNumber.ToString());

                    Logger.ConsoleLog("User:" + CurrentUser.ID, ConsoleColor.Yellow);
                    WebSocketPage.ClientsLotteryPage[XLottery.ID][i].SendWebsocket("LotteryRouletteStarted" + BaseFuncs.WSplit + extra);
                }

                WebSocketPage.ClientsLotteryPage.Remove(XLottery.ID);
            }


            WebSocketPage.UpdateMainPageLotteries(XLottery);
        }

        public static void SendItemsOffer(ulong UserSteamID, ushort status, ulong offerID = 0)
        {
            if(UTSteam.SendClientOffer.ContainsKey(UserSteamID))
            {
                Client client = UTSteam.SendClientOffer[UserSteamID];
                if (status == 1)
                {
                    client.SendWebsocket("SendLocalItemsToSteam" + BaseFuncs.WSplit + "1" + BaseFuncs.WSplit + offerID);
                }
                else if(status==0)
                {
                    client.SendWebsocket("SendLocalItemsToSteam" + BaseFuncs.WSplit + "0");
                }
                else if (status == 2)
                {
                    client.SendWebsocket("SendLocalItemsToSteam" + BaseFuncs.WSplit + "2" + BaseFuncs.WSplit + offerID);
                }
            }
        }

        public static void UpdateOnlineUsers(int num)
        {
            Client[] wclients = BaseFuncs.GetWebsocketClients<SiteGameSlot>().ToArray();
            for (int i = 0; i < wclients.Length; i++)
            {
                Client client = wclients[i];
                if (client != null)
                {
                    client.SendWebsocket("UpdateOnlineUsers" + BaseFuncs.WSplit + num);
                }
            }         
        }

        public static void GetWebsocket_SteamInventory(UsersInventory Inventory, Client client, ushort SortByLowPrice, int FromNum, int ItemsNum, uint SteamGameID, string SearchString)
        {

            if (!Inventory.Opened)
            {
                WebSocketPage.UpdateInventory("", 0.ToString(), 0, client, false);
            }
            else
            {
                string StrItems = "";
                List<USteamItem> SteamItems = Helper.SteamItemsHelper.SearchByString(Inventory.SteamItems, SearchString);

                if (SortByLowPrice == 1)
                {
                    SteamItems = (from it in SteamItems orderby it.Price ascending select it).ToList();
                }

                if (FromNum < 0)
                {
                    FromNum = 0;
                }
                else if (FromNum > ItemsNum && FromNum == SteamItems.Count)
                {
                    FromNum -= ItemsNum;
                }
                Logger.ConsoleLog("From num: " + FromNum + " ItemsNum:" + ItemsNum);
                for (int i = FromNum; i < Math.Min(FromNum + ItemsNum, SteamItems.Count); i++)
                {
                    StrItems += WebSocketPage.InventoryItemToString(SteamItems[i]);
                }

                Logger.ConsoleLog("Total price of steam inventory: " + Inventory.TotalPrice + "|" + Inventory.TotalPrice_Str);
                WebSocketPage.UpdateInventory(StrItems, Inventory.TotalPrice_Str, SteamItems.Count, client, true);
            }
        }
    }
}
