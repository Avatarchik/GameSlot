using GameSlot.Database;
using GameSlot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        private static Dictionary<uint, List<Client>> ClientsGroupPage = new Dictionary<uint, List<Client>>();
        private static Dictionary<uint, List<Client>> ClientsLotteryPage = new Dictionary<uint,List<Client>>();

        public override bool Init(Client client)
        {
            if (client.ConnType == ConnectionType.WebSocket && client.WSData != null)
            {
                //Logger.ConsoleLog(client.WSData, ConsoleColor.Yellow);
                string[] wsdata = Regex.Split(client.WSData, BaseFuncs.WSplit);

                if (wsdata[0].Equals("GroupPage"))
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
                        bool InList = Helper.UserHelper.GetSteamInventory(User, SteamGameID, out Inventory);
                        if (InList)
                        {
                            if (!Inventory.Opened)
                            {
                                WebSocketPage.UpdateInventory("", 0d, 0, client, false);
                            }
                            else
                            {
                                string StrItems = "";
                                List<USteamItem> SteamItems = Helper.SteamItemsHelper.SearchByString(Inventory.SteamItems, wsdata[5]);

                                if(SortByLowPrice == 1)
                                {
                                    SteamItems = (from it in SteamItems orderby it.Price ascending select it).ToList();
                                }

                                if(FromNum < 0)
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

                                WebSocketPage.UpdateInventory(StrItems, Inventory.TotalPrice, SteamItems.Count, client, true);
                            }
                        }
                        else
                        {
                            Helper.UserHelper.WaitingList_InventoryClient(User.ID, client, SteamGameID);
                        }
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
                        //Logger.ConsoleLog((Regex.Split(wsdata[1], "partner=")[1].Split('&')[0] + "||"));
                        if (wsdata[1].Contains("partner=") && wsdata[1].Contains("token=") && ulong.TryParse(Regex.Split(wsdata[1], "partner=")[1].Split('&')[0], out partner))
                        {
                            token = Regex.Split(wsdata[1], "token=")[1];
                            if (token.Length > 0)
                            {
                                if (token != user.TradeToken || partner != user.TradePartner)
                                {
                                    user.TradeToken = token;
                                    user.TradePartner = partner;
                                    Helper.UserHelper.Table.UpdateByID(user, user.ID);
                                }

                                client.SendWebsocket("UpdateTradeURL" + BaseFuncs.WSplit + "1" + BaseFuncs.WSplit + partner + BaseFuncs.WSplit + token);
                               // Logger.ConsoleLog("upd!");
                            }
                        }
                        else
                        {
                            client.SendWebsocket("UpdateTradeURL" + BaseFuncs.WSplit + "0");
                            //Logger.ConsoleLog("no upd!");
                        }
                    }
                    //Logger.ConsoleLog("!!!!");
                }
            }

            return false;
        }

        public static void UpdateGroupData(uint GroupID)
        {
            UGroup group;
            XUser user;
            if (Helper.GroupHelper.SelectByID(GroupID, out group, out user) && WebSocketPage.ClientsGroupPage.ContainsKey(group.ID))
            {
                for (int i = 0; i < WebSocketPage.ClientsGroupPage[group.ID].Count; i++)
                {
                    if (WebSocketPage.ClientsGroupPage[group.ID][i].Closed)
                    {
                        WebSocketPage.ClientsGroupPage[group.ID].Remove(WebSocketPage.ClientsGroupPage[group.ID][i]);
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

        public static void UpdateInventory(string StrItems, double TotalPrice, int ItemsNum, Client client, bool InventoryOpened)
        {
            if (InventoryOpened)
            {
                client.SendWebsocket("SteamInventory" + BaseFuncs.WSplit + TotalPrice.ToString("###,##0.00") + BaseFuncs.WSplit + StrItems + BaseFuncs.WSplit + ItemsNum);
                //Logger.ConsoleLog(TotalPrice, ConsoleColor.Red);
                return;
            }
            client.SendWebsocket("SteamInventoryClosed" + BaseFuncs.WSplit);
        }

        public static string InventoryItemToString(USteamItem SteamItem)
        {
            string image;
            Helper.SteamItemsHelper.GetImageFromMemory(SteamItem.ID, SteamItem.SteamGameID, out image);
            //Logger.ConsoleLog(image + "::" + SteamItem.ID + "::" + SteamItem.SteamGameID);
            // 4- last
            return SteamItem.Name + "↓" + SteamItem.Price.ToString("###,##0.00") + "↓" + SteamItem.SteamGameID + "↓" + SteamItem.ID + "↓" + SteamItem.AssertID + "↓" + SteamItem.Image + ";";
           // return SteamItem.Name + "↓" + SteamItem.Price.ToString("###,##0.00") + "↓" + SteamItem.SteamGameID + "↓" + SteamItem.ID + "↓" + SteamItem.AssertID + ";";
        }

        /*public static void ChangeBetProcessStatus(ulong UserSteamID, ushort status)
        {
            Logger.ConsoleLog("Send WS TO USER!! status: " + status, ConsoleColor.Yellow);
        }*/

        public static void AddNewLotteryBet(XLotteryBet XBet, double Bank, int BankItemsNum, int ExtraTime, int LotteryLeftTime)
        {
            if (WebSocketPage.ClientsLotteryPage.ContainsKey(XBet.LotteryID))
            {
                double TotalPrice = 0d;
                int TotalItemsNum = 0;

                XUser user;
                Helper.UserHelper.Table.SelectByID(XBet.UserID, out user);
                // first_token : last_token : bet_items_num : bet_price ↓
                string OtherBets = "";

                uint count = 0;
                XLotteryBet[] XUsersBets;
                if (Helper.LotteryHelper.TableBet.SelectArrFromEnd(data => data.LotteryID == XBet.LotteryID && data.UserID == XBet.UserID, out XUsersBets))
                {
                    int ItemsNum;
                    double price;
                    for (int i = 0; i < XUsersBets.Length; i++)
                    {
                        TotalItemsNum += ItemsNum = XUsersBets[i].SteamItemsNum + XUsersBets[i].ChipsNum;
                        TotalPrice += price = XUsersBets[i].TotalPrice;

                        if (count < 15 && XUsersBets[i].ID != XBet.ID)
                        {
                            OtherBets += XUsersBets[i].FisrtToken.ToString("D8") + "::" + XUsersBets[i].LastToken.ToString("D8") + "::" + ItemsNum + "::" + price.ToString("###,##0.00") + "↓";
                            count++;
                        }
                    }
                }

                int Winrate = (int)Math.Round(TotalPrice / (Bank / 100));
                if (Winrate > 100)
                {
                    Winrate = 100;
                }
                // Logger.ConsoleLog(Winrate + ":" + TotalPrice + "::" + Bank);
                string ws = "";
                // 4: ExtraTime
                ws += Bank.ToString("###,##0.00") + BaseFuncs.WSplit + XBet.TotalPrice + BaseFuncs.WSplit + BankItemsNum + BaseFuncs.WSplit + ExtraTime + BaseFuncs.WSplit;
                // 5: items_num; 6: totalprice; 7: first_token; 8: last_token
                ws += XBet.SteamItemsNum + XBet.ChipsNum + BaseFuncs.WSplit + XBet.TotalPrice.ToString("###,##0.00") + BaseFuncs.WSplit + XBet.FisrtToken.ToString("D8") + BaseFuncs.WSplit + XBet.LastToken.ToString("D8") + BaseFuncs.WSplit;
                // 9: bets_num; 10: TotalPrice (bets); 11: TotalItems (bets); 12: WINRATE 
                ws += XUsersBets.Length + BaseFuncs.WSplit + TotalPrice.ToString("###,##0.00") + BaseFuncs.WSplit + TotalItemsNum + BaseFuncs.WSplit + Winrate + BaseFuncs.WSplit;
                // 13: OtherBets; 14: UserID; 15: UserName; 16: UsersAvatar 17: BetID 18: LotteryLeftTime
                ws += OtherBets + BaseFuncs.WSplit + user.ID + BaseFuncs.WSplit + user.Name + BaseFuncs.WSplit + user.Avatar + BaseFuncs.WSplit + XBet.ID + BaseFuncs.WSplit + LotteryLeftTime + BaseFuncs.WSplit;
                // 19 (steam_items): ITEM_ID :: GAME_ID, ITEM_NAME, ITEM_PRICE ↓ (REPEAT); 20 (chips): ID :: Price ↓ 21:

                XLottery lottery = Helper.LotteryHelper.Table.SelectByID(XBet.LotteryID);
                for (uint i = 0; i < XBet.SteamItemsNum; i++)
                {
                    XSteamItem SteamItem;
                    Helper.SteamItemsHelper.SelectByID(XBet.SteamItemIDs[i], lottery.SteamGameID, out SteamItem);
                    ws += SteamItem.ID + "::" + lottery.SteamGameID + "::" + SteamItem.Name + "::" + XBet.SteamItemsPrice[i].ToString("###,##0.00") + "↓";
                }

                ws += BaseFuncs.WSplit;
                for (uint i = 0; i < XBet.ChipsNum; i++)
                {
                    Chip chip;
                    Helper.ChipHelper.SelectByID(XBet.ChipIDs[i], out chip);
                    ws += chip.ID + "::" + chip.Cost.ToString("###,##0.00") + "↓";
                }

                for (int i = 0; i < WebSocketPage.ClientsLotteryPage[XBet.LotteryID].Count; i++)
                {
                    /*if (WebSocketPage.ClientsLotteryPage[XBet.LotteryID][i].Closed)
                    {
                        WebSocketPage.ClientsLotteryPage[XBet.LotteryID].Remove(WebSocketPage.ClientsLotteryPage[XBet.LotteryID][i]);
                        continue;
                    }*/

                    WebSocketPage.ClientsLotteryPage[XBet.LotteryID][i].SendWebsocket("AddedNewLotteryBet" + BaseFuncs.WSplit + ws);
                }
            }
        }

        public static void SendLotteryRoulette(XLottery XLottery, List<LotteryRouletteData> RouletteData, XUser Winner)
        {
            if (ClientsLotteryPage.ContainsKey(XLottery.ID))
            {
                // from 1: JackpotItems 2: JackpotPrice 3:Winner.ID 4: Winner.Name 5:RaundNumber 6: WinnerToken 7:(JackpotPrice * 100) 8: Winner.Avatar
                string ws = XLottery.JackpotItemsNum + BaseFuncs.WSplit + XLottery.JackpotPrice + BaseFuncs.WSplit + Winner.ID + BaseFuncs.WSplit + Winner.Name + BaseFuncs.WSplit;
                ws += XLottery.RaundNumber + BaseFuncs.WSplit + XLottery.WinnersToken + BaseFuncs.WSplit + (XLottery.JackpotPrice * 100) + BaseFuncs.WSplit + Winner.Avatar + BaseFuncs.WSplit;
                // 9: wonrate; 10: winners_items_num; 11: winners_price
                ws += XLottery.Wonrate + BaseFuncs.WSplit + XLottery.WinnersBetItemsNum + BaseFuncs.WSplit + XLottery.WinnersBetPrice + BaseFuncs.WSplit;

                // 12: data
                foreach (LotteryRouletteData data in RouletteData)
                {
                    ws += data.UsersAvatar + "↑" + data.Token.ToString("D8") + "↑" + data.Winner + "↓";
                }

                ws += BaseFuncs.WSplit;
                for (int i = 0; i < WebSocketPage.ClientsLotteryPage[XLottery.ID].Count; i++)
                {
                    XUser CurrentUser;
                    if (Helper.UserHelper.GetCurrentUser(WebSocketPage.ClientsLotteryPage[XLottery.ID][i], out CurrentUser))
                    {
                        ws += 1.ToString();
                    }
                    else
                    {
                        ws += 0.ToString();
                    }

                    WebSocketPage.ClientsLotteryPage[XLottery.ID][i].SendWebsocket("LotteryRouletteStarted" + BaseFuncs.WSplit + ws);
                }
            }
        }
    }
}
