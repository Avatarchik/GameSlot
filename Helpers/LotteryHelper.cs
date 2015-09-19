using GameSlot.Database;
using GameSlot.Pages.Includes;
using GameSlot.Types;
using SteamBotUTRequest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpServer;
using XData;

namespace GameSlot.Helpers
{
    public class LotteryHelper
    {
        public XTable<XLottery> Table = new XTable<XLottery>();
        public XTable<XLotteryBet> TableBet = new XTable<XLotteryBet>();
        public XTable<XLotteryUsersMemory> TableUsersMemory = new XTable<XLotteryUsersMemory>();
        public XTable<XLotteryPercentage> TablePercentage = new XTable<XLotteryPercentage>();
        public XTable<XLotteryUsersBetsPrice>TableUsersBetsPrice = new XTable<XLotteryUsersBetsPrice>();

        private static List<ProcessingBet> ProcessingBets = new List<ProcessingBet>();
        //private static Dictionary<uint, List<ProcessingBet>> ProcessingBets = new Dictionary<uint,List<ProcessingBet>>();
        private static Dictionary<uint, List<Bet>> LotteryBets = new Dictionary<uint, List<Bet>>();

        public static Dictionary<uint, double> MaxJackpotPrice = new Dictionary<uint, double>();
        public static Dictionary<uint, double> RUB_MaxJackpotPrice = new Dictionary<uint, double>();
        public static Dictionary<uint, int> ItemsRecord = new Dictionary<uint, int>();

        private static readonly object _MaxJackpotPrice = new object();
        private static readonly object _RUB_MaxJackpotPrice = new object();
        private static readonly object _ItemsRecord = new object();

        private static Dictionary<uint, List<LotteryRouletteData>> LotteryRouletteData = new Dictionary<uint, List<LotteryRouletteData>>();

        public LotteryHelper()
        {
            new Thread(delegate()
            {
                while (true)
                {
                    try
                    {
                        for (int i = 0; i < LotteryHelper.ProcessingBets.Count; i++)
                        {
                            if (LotteryHelper.ProcessingBets[i].ProcessCreatedTime + 360 < Helper.GetCurrentTime())
                            {
                                LotteryHelper.ProcessingBets.Remove(LotteryHelper.ProcessingBets[i]);
                                Thread.Sleep(1);
                            }
                        }
                    }
                    catch { }
                }
                
            }).Start();
        }

        public List<LotteryRouletteData> GetLotteryRouletteData(uint SteamGameID)
        {
            if(LotteryHelper.LotteryRouletteData.ContainsKey(SteamGameID))
            {
                return LotteryHelper.LotteryRouletteData[SteamGameID];
            }

            return new List<LotteryRouletteData>();
        }
        public void UpdateRecords(uint SteamGameID)
        {
            int ItemsRecord;
            double MaxJackpotPrice = this.MaxJackpot(SteamGameID, out ItemsRecord, 0);

            lock (_MaxJackpotPrice)
            {
                if (LotteryHelper.MaxJackpotPrice.ContainsKey(SteamGameID))
                {
                    LotteryHelper.MaxJackpotPrice[SteamGameID] = MaxJackpotPrice;
                }
                else
                {
                    LotteryHelper.MaxJackpotPrice.Add(SteamGameID, MaxJackpotPrice);
                }
            }

            double RUB_MaxJackpotPrice = this.MaxJackpot(SteamGameID, out ItemsRecord, 1);

            lock (_RUB_MaxJackpotPrice)
            {
                if (LotteryHelper.RUB_MaxJackpotPrice.ContainsKey(SteamGameID))
                {
                    LotteryHelper.RUB_MaxJackpotPrice[SteamGameID] = RUB_MaxJackpotPrice;
                }
                else
                {
                    LotteryHelper.RUB_MaxJackpotPrice.Add(SteamGameID, RUB_MaxJackpotPrice);
                }
            }

            lock (_ItemsRecord)
            {
                if (LotteryHelper.ItemsRecord.ContainsKey(SteamGameID))
                {
                    LotteryHelper.ItemsRecord[SteamGameID] = ItemsRecord;
                }
                else
                {
                    LotteryHelper.ItemsRecord.Add(SteamGameID, ItemsRecord);
                }
            }
        }

        public XLottery CreateNew(uint SteamGameID)
        {
            XLottery lottery;
            if (!this.Table.SelectOne(data => data.WinnersToken == 0 && data.SteamGameID == SteamGameID, out lottery))
            {
                XLottery new_lot = new XLottery();
                new_lot.SteamGameID = SteamGameID;
                new_lot.WinnersToken = 0;
                new_lot.RaundNumber = new Random().NextDouble();
                //new_lot.EndTime = Helper.GetCurrentTime() + Configs.LOTTERY_GAME_TIME;
                new_lot.EndTime = -1;
                XLottery last_lot;
                new_lot.StartTime = (this.Table.SelectOne(data => data.SteamGameID == SteamGameID, out last_lot)) ? Helper.GetCurrentTime() + Configs.LOTTERY_BREAK_TIME : 0;
                new_lot.RubCurrency = Helper.Rub_ExchangeRate;

                lottery = this.Table.SelectByID(this.Table.Insert(new_lot));

               /* Logger.ConsoleLog("Added new lottery!", ConsoleColor.Yellow);
                Logger.ConsoleLog("Lottery ID: [" + lottery.ID + "]");
                Logger.ConsoleLog("Steam game ID: [" + lottery.SteamGameID + "]");
                Logger.ConsoleLog("Raund number: [" + lottery.RaundNumber + "]");
                Logger.ConsoleLog("StartTime: [" + lottery.StartTime + "]");
                Logger.ConsoleLog("--------------------------------------------------\n");
                */
                return lottery;
            }

            this.Table.SelectOne(data => data.SteamGameID == SteamGameID && data.WinnersToken == 0, out lottery);
            return lottery;
        }

        public void StartLottery(uint SteamGameID)
        {
            new Thread(delegate()
            {
                XLottery XLottery = this.CreateNew(SteamGameID);
                Random rnd = new Random();

                while (true)
                {
                    XLottery = this.Table.SelectByID(XLottery.ID);
                    if (XLottery.EndTime > 0 && (XLottery.EndTime) < Helper.GetCurrentTime())
                    {
                        Dictionary<uint, double> UserBetsTotalPrice = new Dictionary<uint, double>();
                        Dictionary<uint, List<Tokens>> UserBetsTokens = new Dictionary<uint, List<Tokens>>();

                        List<LotteryRouletteData> RouletteData = new List<LotteryRouletteData>();

                        List<USteamItem> SteamItems;
                        List<Chip> Chips;

                        XLottery.WinnersToken = (int)(XLottery.JackpotPrice * 100 * XLottery.RaundNumber);
                        Logger.ConsoleLog(XLottery.JackpotPrice + ":" + XLottery.WinnersToken);
                        if (XLottery.WinnersToken <= 0)
                        {
                            XLottery.WinnersToken = 1;
                        }
                        XUser WinnerUser = this.GetUserByToken(XLottery.WinnersToken, XLottery.ID);
                        uint WinnerUserID = XLottery.Winner = WinnerUser.ID;
                        //WINRATE = (int)Math.Round(bet.TotalPrice / (this.GetBank(XLottery.ID, out bank_items) / 100));

                        double TotalPrice_WinnersBets = 0d;

                        XLotteryBet[] UsersBets;
                        Helper.LotteryHelper.TableBet.SelectArr(data => 
                        {
                            if(data.LotteryID == XLottery.ID)
                            {
                                if (!UserBetsTotalPrice.ContainsKey(data.UserID))
                                {
                                    List<Tokens> Tokens = new List<Tokens>();
                                    Tokens Token = new Tokens();
                                    Token.FirstToken = data.FisrtToken;
                                    Token.LastToken = data.LastToken;
                                    Tokens.Add(Token);

                                    UserBetsTokens.Add(data.UserID, Tokens);

                                    UserBetsTotalPrice.Add(data.UserID, data.TotalPrice);
                                }
                                else
                                {
                                    Tokens Token = new Tokens();
                                    Token.FirstToken = data.FisrtToken;
                                    Token.LastToken = data.LastToken;

                                    UserBetsTokens[data.UserID].Add(Token);
                                    Logger.ConsoleLog(UserBetsTokens[data.UserID].Last());
                                    UserBetsTotalPrice[data.UserID] += data.TotalPrice;
                                }

                                if (data.UserID == XLottery.Winner)
                                {
                                    TotalPrice_WinnersBets += data.TotalPrice;
                                    XLottery.WinnersBetItemsNum += data.SteamItemsNum + data.ChipsNum;
                                }
                            }
                            return false;
                        }, out UsersBets);

                        XLottery.WinnersBetPrice = TotalPrice_WinnersBets;

                        LotteryRouletteData winners_data = new LotteryRouletteData();
                        foreach(uint user_id in UserBetsTotalPrice.Keys)
                        {
                            int UsersWinrate = (int)Math.Round(UserBetsTotalPrice[user_id] / (XLottery.JackpotPrice / 100));
                            int AvatarNums = (UsersWinrate <= 1) ? 1 : (int) (UsersWinrate / 2);
                            AvatarNums *= 2;
                            XUser xuser = Helper.UserHelper.Table.SelectByID(user_id);

                            if (user_id == WinnerUserID)
                            {
                                AvatarNums -= 1;
                                winners_data.UsersAvatar = xuser.Avatar;
                            }

                            for(int i = 0; i < AvatarNums; i++)
                            {
                                LotteryRouletteData data = new LotteryRouletteData();
                                data.UsersAvatar = xuser.Avatar;
                             
                                int TokensListID = rnd.Next(0, UserBetsTokens[user_id].Count);
                                data.Token = rnd.Next(UserBetsTokens[user_id][TokensListID].FirstToken, UserBetsTokens[user_id][TokensListID].LastToken + 1);

                                RouletteData.Add(data);
                            }
                        }

                        // shuffle list
                        for (int i = RouletteData.Count; i > 1; i--)
                        {
                            int pos = rnd.Next(i);
                            var x = RouletteData[i - 1];
                            RouletteData[i - 1] = RouletteData[pos];
                            RouletteData[pos] = x;
                        }

                        winners_data.Token = XLottery.WinnersToken;
                        winners_data.Winner = 1;
                        RouletteData.Insert(RouletteData.Count - 20, winners_data);

                        if (LotteryHelper.LotteryRouletteData.ContainsKey(SteamGameID))
                        {
                            LotteryHelper.LotteryRouletteData[SteamGameID] = RouletteData;
                        }
                        else
                        {
                            LotteryHelper.LotteryRouletteData.Add(SteamGameID, RouletteData);
                        }

                        XLottery.Wonrate = (int)Math.Round(TotalPrice_WinnersBets / (XLottery.JackpotPrice / 100));
                        XLottery.WinnerGroupID = WinnerUser.GroupOwnerID;

                        this.Table.UpdateByID(XLottery, XLottery.ID);
                        WebSocketPage.SendLotteryRoulette(XLottery, RouletteData, WinnerUser);

                        this.GetBetItems(XLottery.ID, new Client(), out SteamItems, out Chips);

                        double WinnersBetsPrice = 0d;
                        int WinnersBetsItemsCount = 0;

                        XLotteryBet[] WinnersBets;
                        List<ulong> WinnersSteamItems = new List<ulong>();
                        List<ulong> WinnersChipItems = new List<ulong>();

                        this.TableBet.SelectArr(data => 
                        { 
                            if(data.LotteryID == XLottery.ID && data.UserID == WinnerUser.ID)
                            {
                                WinnersBetsItemsCount += data.SteamItemsNum + data.ChipsNum;
                                WinnersBetsPrice += data.TotalPrice;

                                for(int i = 0; i < data.SteamItemsNum; i++)
                                {
                                    WinnersSteamItems.Add(data.ItemAssertIDs[i]);
                                }

                                for (int i = 0; i < data.ChipsNum; i++)
                                {
                                    WinnersChipItems.Add(data.ChipAssertIDs[i]);
                                }
                            }
                            return false;
                        }, out WinnersBets);

                        // ???
                        XLottery LastLottery = XLottery;
                        XLottery = this.CreateNew(SteamGameID);

                        // update winners info (group and user)
                        if(LastLottery.SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                        {
                            WinnerUser.DOTA_WonCount++;
                            WinnerUser.DOTA_WonTotalPrice += LastLottery.JackpotPrice - WinnersBetsPrice;
                            WinnerUser.DOTA_RUB_WonTotalPrice += ((LastLottery.JackpotPrice - WinnersBetsPrice) * LastLottery.RubCurrency);
                            WinnerUser.DOTA_WonItemsCount += SteamItems.Count + Chips.Count - WinnersBetsItemsCount;
                        }
                        else if (LastLottery.SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                        {
                            WinnerUser.CSGO_WonCount++;
                            WinnerUser.CSGO_WonTotalPrice += LastLottery.JackpotPrice - WinnersBetsPrice;
                            WinnerUser.CSGO_RUB_WonTotalPrice += ((LastLottery.JackpotPrice - WinnersBetsPrice) * LastLottery.RubCurrency);
                            WinnerUser.CSGO_WonItemsCount += SteamItems.Count + Chips.Count - WinnersBetsItemsCount;
                        }


                        int WinnerBetPercentage = Convert.ToInt32(Math.Round((100 * WinnersBetsPrice) / LastLottery.JackpotPrice));
                        if (WinnerBetPercentage <= 80)
                        {
                            XLotteryPercentage[] XLotteryPercentages;
                            if (this.TablePercentage.SelectArr(data => data.LotteryID == LastLottery.ID, out XLotteryPercentages))
                            {
                                for(int i = 0; i < XLotteryPercentages.Length; i++)
                                {
                                    if(XLotteryPercentages[i].UserID == WinnerUser.ID)
                                    {
                                        continue;
                                    }

                                    XUser GroupOwner = Helper.UserHelper.Table.SelectByID(XLotteryPercentages[i].UserID);
                                    double GroupGetItemPrice = XLotteryPercentages[i].SteamItemsPrice;
                                    double GroupGetChipPrice = XLotteryPercentages[i].ChipPrice;

                                    for (int st = 0; st < SteamItems.Count; st++ )
                                    {
                                        if (WinnersSteamItems.Contains(SteamItems[st].AssertID))
                                        {
                                            continue;
                                        }

                                        if (GroupGetItemPrice < Configs.MIN_ITEMS_PRICE)
                                        {
                                            break;
                                        }

                                        if (SteamItems[st].Price <= GroupGetItemPrice)
                                        {

                                            if (LastLottery.SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                                            {
                                                GroupOwner.DOTA_GotItemsFromGroup++;
                                                GroupOwner.DOTA_GotPriceFromGroup += SteamItems[st].Price;
                                                GroupOwner.DOTA_RUB_GotPriceFromGroup += SteamItems[st].Price * LastLottery.RubCurrency;
                                            }
                                            else if (LastLottery.SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                                            {
                                                GroupOwner.CSGO_GotItemsFromGroup++;
                                                GroupOwner.CSGO_GotPriceFromGroup += SteamItems[st].Price;
                                                GroupOwner.CSGO_RUB_GotPriceFromGroup += SteamItems[st].Price * LastLottery.RubCurrency;
                                            }

                                            XSItemUsersInventory XSItemUsersInventory = new XSItemUsersInventory();
                                            XSItemUsersInventory.UserID = GroupOwner.ID;
                                            XSItemUsersInventory.SteamItemID = SteamItems[st].ID;
                                            XSItemUsersInventory.AssertID = SteamItems[st].AssertID;
                                            XSItemUsersInventory.SteamGameID = SteamItems[st].SteamGameID;
                                            XSItemUsersInventory.SteamBotID = SteamItems[st].SteamBotID;

                                            Helper.UserHelper.Table_SteamItemUsersInventory.Insert(XSItemUsersInventory);
                                            GroupGetItemPrice -= SteamItems[st].Price;
                                            SteamItems.Remove(SteamItems[st--]);
                                        }
                                    }

                                    for (int ch = 0; ch < Chips.Count; ch++)
                                    {
                                        if (WinnersChipItems.Contains(Chips[ch].AssertID))
                                        {
                                            continue;
                                        }

                                        if (GroupGetChipPrice < Configs.MIN_ITEMS_PRICE)
                                        {
                                            break;
                                        }

                                        if (Chips[ch].Cost <= GroupGetChipPrice)
                                        {
                                            if (LastLottery.SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                                            {
                                                GroupOwner.DOTA_GotItemsFromGroup++;
                                                GroupOwner.DOTA_RUB_GotPriceFromGroup += Chips[ch].Cost * LastLottery.RubCurrency;
                                            }
                                            else if (LastLottery.SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                                            {

                                                GroupOwner.CSGO_GotItemsFromGroup++;
                                                GroupOwner.CSGO_RUB_GotPriceFromGroup += Chips[ch].Cost * LastLottery.RubCurrency;
                                            }

                                            XChipUsersInventory XChipUsersInventory = new XChipUsersInventory();
                                            XChipUsersInventory.UserID = GroupOwner.ID;
                                            XChipUsersInventory.ChipID = Chips[ch].ID;
                                            XChipUsersInventory.AssertID = Chips[ch].AssertID;

                                            Helper.UserHelper.Table_ChipUsersInventory.Insert(XChipUsersInventory);
                                            GroupGetChipPrice -= Chips[ch].Cost;
                                            Chips.Remove(Chips[ch--]);
                                        }
                                    }

                                    Helper.UserHelper.Table.UpdateByID(GroupOwner, GroupOwner.ID);
                                }
                            }
                        }

                        foreach (USteamItem SteamItem in SteamItems)
                        {
                            XSItemUsersInventory XSItemUsersInventory = new XSItemUsersInventory();
                            XSItemUsersInventory.UserID = WinnerUserID;
                            XSItemUsersInventory.SteamItemID = SteamItem.ID;
                            XSItemUsersInventory.AssertID = SteamItem.AssertID;
                            XSItemUsersInventory.SteamGameID = SteamItem.SteamGameID;
                            XSItemUsersInventory.SteamBotID = SteamItem.SteamBotID;

                            Logger.ConsoleLog("SetWinner:" + WinnerUserID + " SteamUserID: " + Helper.UserHelper.Table.SelectByID(WinnerUserID).SteamID + " Bot ID: " + SteamItem.SteamBotID + " ItemID:" + SteamItem.AssertID, ConsoleColor.Cyan, LogLevel.Info);
                            Helper.UserHelper.Table_SteamItemUsersInventory.Insert(XSItemUsersInventory);

                        }
                        //Logger.ConsoleLog(Chips.Count + "|||" + WinnerUserID);
                        foreach (Chip chip in Chips)
                        {
                            XChipUsersInventory XChipUsersInventory = new XChipUsersInventory();
                            XChipUsersInventory.UserID = WinnerUserID;
                            XChipUsersInventory.ChipID = chip.ID;
                            XChipUsersInventory.AssertID = chip.AssertID;
                            Helper.UserHelper.Table_ChipUsersInventory.Insert(XChipUsersInventory);
                        }

                        XLotteryUsersMemory XLotteryUsersMemory;
                        if (WinnerUser.GroupOwnerID >= 0 && this.TableUsersMemory.SelectOne(data => data.LotteryID == LastLottery.ID && data.GroupOwnerID == WinnerUser.GroupOwnerID, out XLotteryUsersMemory))
                        {
                            if (LastLottery.SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                            {
                                if (WinnerUser.GroupOwnerID == WinnerUser.ID)
                                {
                                    WinnerUser.DOTA_GroupWonCount++;
                                    WinnerUser.DOTA_GroupWonItemsCount += (SteamItems.Count - WinnersBetsItemsCount);

                                    WinnerUser.DOTA_GroupWonTotalPrice += (LastLottery.JackpotPrice - WinnersBetsPrice);
                                    WinnerUser.DOTA_RUB_GroupWonTotalPrice += ((LastLottery.JackpotPrice - WinnersBetsPrice) * LastLottery.RubCurrency);
                                }
                                else
                                {
                                    XUser GroupOwner = Helper.UserHelper.Table.SelectByID((uint)WinnerUser.GroupOwnerID);
                                    GroupOwner.DOTA_GroupWonCount++;
                                    GroupOwner.DOTA_GroupWonItemsCount += (SteamItems.Count - WinnersBetsItemsCount);

                                    GroupOwner.DOTA_GroupWonTotalPrice += (LastLottery.JackpotPrice - WinnersBetsPrice);
                                    GroupOwner.DOTA_RUB_GroupWonTotalPrice += ((LastLottery.JackpotPrice - WinnersBetsPrice) * LastLottery.RubCurrency);

                                    Helper.UserHelper.Table.UpdateByID(GroupOwner, GroupOwner.ID);
                                }
                            }
                            else if (LastLottery.SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                            {
                                if (WinnerUser.GroupOwnerID == WinnerUser.ID)
                                {
                                    WinnerUser.CSGO_GroupWonCount++;
                                    WinnerUser.CSGO_GroupWonItemsCount += (SteamItems.Count - WinnersBetsItemsCount);

                                    WinnerUser.CSGO_GroupWonTotalPrice += (LastLottery.JackpotPrice - WinnersBetsPrice);
                                    WinnerUser.CSGO_RUB_GroupWonTotalPrice += ((LastLottery.JackpotPrice - WinnersBetsPrice) * LastLottery.RubCurrency);
                                }
                                else
                                {
                                    XUser GroupOwner = Helper.UserHelper.Table.SelectByID((uint)WinnerUser.GroupOwnerID);
                                    GroupOwner.CSGO_GroupWonCount++;
                                    GroupOwner.CSGO_GroupWonItemsCount += (SteamItems.Count - WinnersBetsItemsCount);

                                    GroupOwner.CSGO_GroupWonTotalPrice += (LastLottery.JackpotPrice - WinnersBetsPrice);
                                    GroupOwner.CSGO_RUB_GroupWonTotalPrice += ((LastLottery.JackpotPrice - WinnersBetsPrice) * LastLottery.RubCurrency);
                                    Helper.UserHelper.Table.UpdateByID(GroupOwner, GroupOwner.ID);
                                }
                            }
                        }

                        Helper.UserHelper.Table.UpdateByID(WinnerUser, WinnerUser.ID);
                    }

                    Thread.Sleep(100);
                }
            }).Start();
        }

        public void GetBetItems(uint LotteryID, Client client, out List<USteamItem> SteamItems, out List<Chip> Chips)
        {
            XLotteryBet[] XLotteryBets;
            SteamItems = new List<USteamItem>();
            Chips = new List<Chip>();

            XLottery xlot;
            if (this.Table.SelectByID(LotteryID, out xlot) && this.TableBet.SelectArr(data => data.LotteryID == xlot.ID, out XLotteryBets))
            {
                ushort currency = Helper.UserHelper.GetCurrency(client);
               // Logger.ConsoleLog("here!~nna");
                // make shit
                for(uint i = 0; i < XLotteryBets.Length; i++)
                {
                    XLotteryBet XLotteryBet = XLotteryBets[i];

                    // steam items
                    for (int x = 0; x < XLotteryBet.SteamItemsNum; x++)
                    {
                        USteamItem SteamItem;
                        if (Helper.SteamItemsHelper.SelectByID(XLotteryBet.SteamItemIDs[x], xlot.SteamGameID, out SteamItem, currency))
                        {
                            //Logger.ConsoleLog(x + "lolz" + SteamItem.ID, ConsoleColor.DarkGreen);
                            SteamItem.AssertID = XLotteryBet.ItemAssertIDs[x];
                            SteamItem.SteamBotID = XLotteryBet.SteamBotIDs[x];
                            if (currency == 1)
                            {
                                SteamItem.Price = XLotteryBet.SteamItemsPrice[x] * xlot.RubCurrency;
                                SteamItem.Price_Str = SteamItem.Price.ToString("###,###,##0");
                            }
                            else
                            {
                                SteamItem.Price = XLotteryBet.SteamItemsPrice[x];
                                SteamItem.Price_Str = SteamItem.Price.ToString("###,##0.00");
                            }

                            SteamItems.Add(SteamItem);
                        }
                    }

                    // chips
                    for (int x = 0; x < XLotteryBet.ChipsNum; x++)
                    {
                        Chip Chip;
                        if (Helper.ChipHelper.SelectByID(XLotteryBet.ChipIDs[x], out Chip))
                        {
                            Chip = Chip.Clone();
                            Chip.AssertID = XLotteryBet.ChipAssertIDs[x];

                            if (currency == 1)
                            {
                                Chip.Cost *= xlot.RubCurrency;
                                Chip.Cost_Str = Chip.Cost.ToString("###,###,##0");
                            }
 
                            Chips.Add(Chip);
                        }
                    }
                }

                SteamItems = (from it in SteamItems orderby it.Price descending select it).ToList();
                Chips = (from it in Chips orderby it.Cost descending select it).ToList();
            }         
        }

        public XUser GetUserByToken(int Token, uint LotteryID)
        {
            XLotteryBet LotteryBet;
            // 1 >= 2685 &&  5000 <=2685
            this.TableBet.SelectOne(data => data.LotteryID == LotteryID && Token >= data.FisrtToken && Token <= data.LastToken, out LotteryBet);

            XUser user;
            Helper.UserHelper.Table.SelectByID(LotteryBet.UserID, out user);
            return user;
        }

        public Lottery GetCurrent(uint SteamGameID, Client client)
        {
            List<XLottery> xlots;
            this.Table.Select(data => data.SteamGameID == SteamGameID && (data.StartTime < Helper.GetCurrentTime() || data.StartTime == 0), out xlots);

            Lottery lot;
            this.GetLottery(xlots[xlots.Count - 1].ID, client, out lot);
            return lot;
        }

        public bool GetLottery(uint ID, Client client, out Lottery Lottery)
        {
            XLottery xlot;
            if (this.Table.SelectByID(ID, out xlot))
            {
                Lottery = new Lottery();
                Lottery.ID = xlot.ID;
                Lottery.IDStr = xlot.ID.ToString("000-000-000");
                Lottery.CentBank = xlot.JackpotPrice * 100;
                ushort currency = Helper.UserHelper.GetCurrency(client);
                if(currency == 1)
                {
                    Lottery.JackpotPrice = this.GetBank(xlot.ID, out Lottery.JackpotItems) * xlot.RubCurrency;
                    Lottery.JackpotPrice_Str = Lottery.JackpotPrice.ToString("###,###,##0");
                }
                else
                {
                    Lottery.JackpotPrice = this.GetBank(xlot.ID, out Lottery.JackpotItems);
                    Lottery.JackpotPrice_Str = Lottery.JackpotPrice.ToString("###,##0.00");
                }

                //Logger.ConsoleLog(xlot.EndTime, ConsoleColor.Blue);
                Lottery.LeftTime = (xlot.EndTime > 0) ? this.CalcLeftTime(xlot.ID) : -1;

                Lottery.RaundNumber = xlot.RaundNumber;
                Lottery.RaundNumber_MD5 = BaseFuncs.MD5(xlot.RaundNumber.ToString());

                //Lottery.Bets = this.GetBets(xlot.ID);
                Lottery.SteamGameID = xlot.SteamGameID;
                Lottery.UsersCount = xlot.GamersCount;

                Lottery.RubCurrency = xlot.RubCurrency;

                if (xlot.EndTime > 0 && xlot.EndTime <= Helper.GetCurrentTime() + 1)
                {
                    XLottery new_xlot;
                    if(this.Table.SelectOne(data => data.SteamGameID == xlot.SteamGameID && data.WinnersToken == 0 && data.ID != xlot.ID, out new_xlot))
                    {
                        Lottery.StartTimeNextGame = new_xlot.StartTime;
                        Lottery.LeftTimeNextGame = (new_xlot.StartTime - Helper.GetCurrentTime() > 0) ? new_xlot.StartTime - Helper.GetCurrentTime() : 0;
                    }

                    Lottery.WinnersToken = xlot.WinnersToken;
                    Lottery.Winner = Helper.UserHelper.Table.SelectByID(xlot.Winner);
                    Lottery.RaundNumber = xlot.RaundNumber;
                    Lottery.Wonrate = xlot.Wonrate;

                    Lottery.WinnersBetItemsNum = xlot.WinnersBetItemsNum;

                    if (currency == 1)
                    {
                        Lottery.WinnersBetPrice = xlot.WinnersBetPrice * xlot.RubCurrency;
                        Lottery.WinnersBetPrice_Str = Lottery.WinnersBetPrice.ToString("###,###,##0");
                    }
                    else
                    {
                        Lottery.WinnersBetPrice = xlot.WinnersBetPrice;
                        Lottery.WinnersBetPrice_Str = xlot.WinnersBetPrice.ToString("###,##0.00");
                    }
                }
                return true;
            }
            Lottery = null;
            return false;
        }

        public List<Bet> GetBets(uint LotteryID, Client client, bool unique_users = false)
        {
            ushort currency = Helper.UserHelper.GetCurrency(client);

            uint size = 0;
            Helper.UserHelper.Table.GetData(out size);

            List<Bet>[] UserBets = new List<Bet>[size];
            double[] UserBetsTotalPrice = new double[size];
            int[] UserBetsItemsNum = new int[size];

            List<Bet> Bets = new List<Bet>();
            XLottery XLottery;
            if (this.Table.SelectByID(LotteryID, out XLottery))
            {
                XLotteryBet[] XBets;
                //if (this.TableBet.SelectArrFromEnd(data => data.LotteryID == LotteryID, out XBets, From, ItemsNum))
                bool search = false;
                if (unique_users)
                {
                    Dictionary<uint, byte> usrs = new Dictionary<uint, byte>();
                    search = this.TableBet.SelectArrFromEnd(data => 
                    {
                        if(data.LotteryID == LotteryID && !usrs.ContainsKey(data.UserID))
                        {
                            usrs.Add(data.UserID, 1);
                            return true;
                        }
                        return false;
                    }, out XBets);
                }
                else
                {
                    search = this.TableBet.SelectArrFromEnd(data => data.LotteryID == LotteryID, out XBets);
                }

                if (search)
                {
                    for (int bb = 0; bb < XBets.Length; bb++)
                    {
                        Bet bet = new Bet();
                        bet.ID = XBets[bb].ID;
                        bet.SteamItems = new List<USteamItem>();
                        bet.Chips = new List<Chip>();

                        // steam items
                        for (uint i = 0; i < XBets[bb].SteamItemsNum; i++)
                        {
                            USteamItem Item;
                            if (Helper.SteamItemsHelper.SelectByID(XBets[bb].SteamItemIDs[i], XLottery.SteamGameID, out Item, currency))
                            {
                                Item.AssertID = XBets[bb].ItemAssertIDs[i];
                                if (currency == 1)
                                {
                                    bet.Price += Item.Price = XBets[bb].SteamItemsPrice[i] * XLottery.RubCurrency;
                                    Item.Price_Str = Item.Price.ToString("###,###,##0");
                                }
                                else
                                {
                                    bet.Price += Item.Price = XBets[bb].SteamItemsPrice[i];
                                    Item.Price_Str = Item.Price.ToString("###,##0.00");
                                }
                                bet.SteamItems.Add(Item);
                            }
                        }

                        // chips
                        for (uint i = 0; i < XBets[bb].ChipsNum; i++)
                        {
                            Chip Chip;
                            if (Helper.ChipHelper.SelectByID(XBets[bb].ChipIDs[i], out Chip))
                            {
                                Chip.AssertID = XBets[bb].ChipAssertIDs[i];
                                if (currency == 1)
                                {
                                    bet.Price += Chip.Cost = Chip.Cost * XLottery.RubCurrency;
                                    Chip.Cost_Str = Chip.Cost.ToString("###,###,##0");
                                }
                                else
                                {
                                    bet.Price += Chip.Cost;
                                }

                                bet.Chips.Add(Chip);
                            }
                        }

                        bet.ItemsNum = bet.Chips.Count + bet.SteamItems.Count;


                        XLotteryUsersBetsPrice XLotteryUsersBetsPrice;
                        this.TableUsersBetsPrice.SelectOne(data => data.UserID == XBets[bb].UserID && data.LotteryID == LotteryID, out XLotteryUsersBetsPrice);

                        if (currency == 1)
                        {
                            bet.Price_Str = bet.Price.ToString("###,###,##0");
                            bet.TotalPrice_Str = (bet.TotalPrice * XLottery.RubCurrency).ToString("###,###,##0");

                            bet.TotalPrice = XLotteryUsersBetsPrice.TotalBetsPrice * XLottery.RubCurrency;
                            bet.TotalPrice_Str = bet.TotalPrice.ToString("###,###,##0");
                        }
                        else
                        {
                            bet.Price_Str = bet.Price.ToString("###,##0.00");
                            bet.TotalPrice_Str = bet.TotalPrice.ToString("###,##0.00");

                            bet.TotalPrice = XLotteryUsersBetsPrice.TotalBetsPrice;
                            bet.TotalPrice_Str = bet.TotalPrice.ToString("###,##0.00");
                        }

                        bet.FirstToken = XBets[bb].FisrtToken;
                        bet.LastToken = XBets[bb].LastToken;
                        bet.BetsNum = XLotteryUsersBetsPrice.BetsCount;
                        bet.TotalItemsNum = XLotteryUsersBetsPrice.TotalBetsItemsNum;

                        bet.XUser = Helper.UserHelper.Table.SelectByID(XBets[bb].UserID);
                        int bank_items;
                        bet.Winrate = (int)Math.Round(XLotteryUsersBetsPrice.TotalBetsPrice / (this.GetBank(XLottery.ID, out bank_items) / 100));
                        if (bet.Winrate <= 0)
                        {
                            bet.Winrate = 1;
                        }

                        Bets.Add(bet);
                    }   
                }
            }

            return Bets;
        }

        public ushort SetBet(uint LotteryID, uint UserID, List<USteamItem> SteamItems, List<Chip> Chips, Client client)
        {
            XLottery XLottery;
            XUser User;
            if (Helper.UserHelper.Table.SelectByID(UserID, out User) && this.Table.SelectByID(LotteryID, out XLottery) && XLottery.WinnersToken == 0 && SteamItems.Count + Chips.Count > 0 && SteamItems.Count + Chips.Count <= 24)
            {
                //Logger.ConsoleLog("Main container started...", ConsoleColor.Yellow);
                double TotalPrice = 0d;

                ProcessingBet ProcessingBet = new ProcessingBet();
                ProcessingBet.SteamItems = new List<USteamItem>();
                ProcessingBet.Chips = new List<Chip>();

                bool ItemsFromSteamInventory = false;
                UTRequestSteamMain UTRequestSteamMain = new UTRequestSteamMain();
                XSteamBotProcessItems XSteamBotProcessItems = new XSteamBotProcessItems();
                XSteamBotProcessItems.SteamItemIDs = new uint[24];
                XSteamBotProcessItems.ItemAssertIDs = new ulong[24];
                XSteamBotProcessItems.SteamItemsNum = 0;

                foreach(USteamItem SteamItem in SteamItems)
                {
                    // check in local inventory
                    XSteamItem XSteamItem;
                    if (Helper.SteamItemsHelper.Table.SelectByID(SteamItem.ID, out XSteamItem))
                    {
                        SteamItem.Price = XSteamItem.Price;
                        if(SteamItem.Price < Configs.MIN_ITEMS_PRICE)
                        {
                            //Logger.ConsoleLog("Low price :(");
                            return 0;
                        }

                        if (Helper.UserHelper.IsUserHaveSteamItem(SteamItem.AssertID, SteamItem.ID, User.ID))
                        {
                            //Logger.ConsoleLog("Local item found! :: " + SteamItem.AssertID);

                            XSItemUsersInventory XUsersItem;
                            Helper.UserHelper.Table_SteamItemUsersInventory.SelectOne(data => !data.Deleted && data.AssertID == SteamItem.AssertID, out XUsersItem);

                            SteamItem.SteamBotID = XUsersItem.SteamBotID;
                            TotalPrice += SteamItem.Price;
                            ProcessingBet.SteamItems.Add(SteamItem);
                        }

                        // check in steam inventory
                        else
                        {
                            if (Helper.UserHelper.IsUserHaveSteamItem_SteamInventory(SteamItem.AssertID, SteamItem.ID, User, XLottery.SteamGameID))
                            {
                                //Logger.ConsoleLog("steam item found! :: " + SteamItem.AssertID);
                                if (!ItemsFromSteamInventory)
                                {
                                    ItemsFromSteamInventory = true;
                                }

                                UTRequestSteamItem UTRequestSteamItem = new UTRequestSteamItem();
                                UTRequestSteamItem.appid = (int)XLottery.SteamGameID;
                                UTRequestSteamItem.contextid = 2;
                                UTRequestSteamItem.assertid = (long)SteamItem.AssertID;
                                UTRequestSteamMain.Items.Add(UTRequestSteamItem);


                                XSteamBotProcessItems.SteamItemIDs[XSteamBotProcessItems.SteamItemsNum] = SteamItem.ID;
                                XSteamBotProcessItems.ItemAssertIDs[XSteamBotProcessItems.SteamItemsNum] = SteamItem.AssertID;
                                XSteamBotProcessItems.SteamItemsNum++;

                                ProcessingBet.SteamItems.Add(SteamItem);
                            }
                            else
                            {
                                //Logger.ConsoleLog("Steam item not found :(");
                                return 3;
                            }
                        }
                    }
                    else
                    {
                        //Logger.ConsoleLog("Item not found in DB!");
                        return 0;                    
                    }
                }

                foreach(Chip Chip in Chips)
                {
                    Chip CurrentChip;
                    if(Helper.UserHelper.SelectChipByAssertID(Chip.AssertID, User.ID, out CurrentChip))
                    {
                        ProcessingBet.Chips.Add(CurrentChip);
                        TotalPrice += CurrentChip.Cost;
                    }
                    else
                    {
                        return 0;
                    }
                }

                if(!ItemsFromSteamInventory)
                {
                    int BankItemsNum;
                    double BankPrice = this.GetBank(XLottery.ID, out BankItemsNum);

                    int LotteryExtraTime = -1;

                    XLotteryUsersMemory XLotteryUsersMemory;
                    if (!this.TableUsersMemory.SelectOne(data => data.UserID == User.ID && data.LotteryID == XLottery.ID, out XLotteryUsersMemory))
                    {
                        if (XLottery.SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                        {
                            User.DOTA_GamesCount++;
                        }
                        else if(XLottery.SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                        {
                            User.CSGO_GamesCount++;
                        }

                        XLottery.GamersCount++;

                        XLotteryUsersMemory = new XLotteryUsersMemory();
                        XLotteryUsersMemory.LotteryID = XLottery.ID;
                        XLotteryUsersMemory.UserID = (int)User.ID;
                        XLotteryUsersMemory.GroupOwnerID = -1;

                        this.TableUsersMemory.Insert(XLotteryUsersMemory);
                    }

                    if (XLottery.GamersCount > 1)
                    {
                        if (XLottery.EndTime < 0)
                        {
                            XLottery.EndTime = Helper.GetCurrentTime() + Configs.LOTTERY_GAME_TIME;
                        }
                        else
                        {
                            double oneproc = BankPrice / 100;
                            double proctime = TotalPrice / oneproc;
                            double fl = Configs.LOTTERY_EXTRA_TIME / 100;
                            LotteryExtraTime = (int)Math.Round(proctime * fl);

                            int EndTimeBefore = XLottery.EndTime;
                            XLottery.EndTime += LotteryExtraTime;

                            if (XLottery.EndTime > Helper.GetCurrentTime() + Configs.LOTTERY_GAME_TIME)
                            {
                                XLottery.EndTime = Helper.GetCurrentTime() + Configs.LOTTERY_GAME_TIME;
                                LotteryExtraTime = XLottery.EndTime - EndTimeBefore;
                            }
                        }
                    }

                    XLotteryBet XLotteryBet = new XLotteryBet();
                    XLotteryBet.LotteryID = XLottery.ID;
                    XLotteryBet.UserID = User.ID;
                    XLotteryBet.GroupOwnerID = User.GroupOwnerID;

                    XLotteryBet[] XBets;
                    this.TableBet.SelectArr(data => data.LotteryID == XLottery.ID, out XBets);
                    XLotteryBet.FisrtToken = (XBets.Length > 0) ? XBets[XBets.Length-1].LastToken +1 : 1;
                    XLotteryBet.LastToken = XLotteryBet.FisrtToken + (int)(TotalPrice * 100 / Configs.TOKEN_PRICE) - 1;

                    XLotteryBet.SteamItemIDs = new uint[24];
                    XLotteryBet.ItemAssertIDs = new ulong[24];

                    XLotteryBet.ChipIDs = new uint[24];
                    XLotteryBet.ChipAssertIDs = new ulong[24];

                    XLotteryBet.SteamBotIDs = new uint[24];
                    XLotteryBet.SteamItemsPrice = new double[24];

                    XLotteryBet.TotalPrice = TotalPrice;

                    XLottery.JackpotItemsNum += ProcessingBet.SteamItems.Count + ProcessingBet.Chips.Count;
                    XLottery.JackpotPrice += TotalPrice;

                    double TotalPrice_Items = 0d;
                    double TotalPrice_Chips = 0d;

                    // foreach and delete from user item
                    foreach(USteamItem Item in ProcessingBet.SteamItems)
                    {
                        XLotteryBet.SteamItemIDs[XLotteryBet.SteamItemsNum] = Item.ID;
                        XLotteryBet.ItemAssertIDs[XLotteryBet.SteamItemsNum] = Item.AssertID;    
                        TotalPrice_Items += XLotteryBet.SteamItemsPrice[XLotteryBet.SteamItemsNum] = Item.Price;
                        XLotteryBet.SteamBotIDs[XLotteryBet.SteamItemsNum] = Item.SteamBotID;
                        XLotteryBet.SteamItemsNum++;
                        Logger.ConsoleLog("Making Bet:" + User.ID + " SteamUserID: " + User.SteamID + " Bot ID: " + Item.SteamBotID + " ItemID:" + Item.AssertID, ConsoleColor.Cyan, LogLevel.Info);

                        XSItemUsersInventory XSItemUsersInventory;
                        Helper.UserHelper.Table_SteamItemUsersInventory.SelectOne(data => !data.Deleted && data.AssertID == Item.AssertID, out XSItemUsersInventory);
                        XSItemUsersInventory.Deleted = true;
                        Helper.UserHelper.Table_SteamItemUsersInventory.UpdateByID(XSItemUsersInventory, XSItemUsersInventory.ID);
                    }

                    foreach(Chip Chip in ProcessingBet.Chips)
                    {
                        Logger.ConsoleLog("Chip asserID: " + Chip.AssertID, ConsoleColor.Red);
                        XLotteryBet.ChipIDs[XLotteryBet.ChipsNum] = Chip.ID;
                        XLotteryBet.ChipAssertIDs[XLotteryBet.ChipsNum] = Chip.AssertID;

                        Chip chip;
                        Helper.ChipHelper.SelectByID(XLotteryBet.ChipIDs[XLotteryBet.ChipsNum], out chip);
                        TotalPrice_Chips += chip.Cost;

                        XLotteryBet.ChipsNum++;

                        XChipUsersInventory XChipUsersInventory;
                        Helper.UserHelper.Table_ChipUsersInventory.SelectOne(data => !data.Deleted && data.AssertID == Chip.AssertID, out XChipUsersInventory);
                        XChipUsersInventory.Deleted = true;
                       // Logger.ConsoleLog("removed " + XChipUsersInventory.AssertID + "From " + Chip.AssertID);
                        Helper.UserHelper.Table_ChipUsersInventory.UpdateByID(XChipUsersInventory, XChipUsersInventory.ID);
                    }

                    //Logger.ConsoleLog("Bet created! :D");

                    if (XLottery.SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                    {
                        User.DOTA_TotalBetItemsCount += XLotteryBet.ChipsNum + XLotteryBet.SteamItemsNum;
                        User.DOTA_TotalBetPrice += XLotteryBet.TotalPrice;
                        User.DOTA_RUB_TotalBetPrice += XLotteryBet.TotalPrice * XLottery.RubCurrency;
                    }
                    else if (XLottery.SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                    {
                        User.CSGO_TotalBetItemsCount += XLotteryBet.ChipsNum + XLotteryBet.SteamItemsNum;
                        User.CSGO_TotalBetPrice += XLotteryBet.TotalPrice;
                        User.CSGO_RUB_TotalBetPrice += XLotteryBet.TotalPrice * XLottery.RubCurrency;
                    }

                    XLotteryPercentage GroupPercentage;
                    int GameSlot_Percentage = Configs.GAMESLOT_PERCENTAGE;

                    if(User.GroupOwnerID >= 0)
                    {
                        GameSlot_Percentage -= Configs.GROUP_PERCENTAGE;
                        XUser GroupOwner;

                        if(this.TablePercentage.SelectOne(data => data.LotteryID == XLottery.ID && data.UserID == User.GroupOwnerID, out GroupPercentage))
                        {
                            GroupPercentage.ChipPrice += ((TotalPrice_Chips * Configs.GROUP_PERCENTAGE) / 100);
                            GroupPercentage.SteamItemsPrice += ((TotalPrice_Items * Configs.GROUP_PERCENTAGE) / 100);
                            this.TablePercentage.UpdateByID(GroupPercentage, GroupPercentage.ID);
                        }
                        else
                        {
                            GroupPercentage = new XLotteryPercentage();
                            GroupPercentage.ChipPrice += ((TotalPrice_Chips * Configs.GROUP_PERCENTAGE) / 100);
                            GroupPercentage.SteamItemsPrice += ((TotalPrice_Items * Configs.GROUP_PERCENTAGE) / 100);

                            GroupPercentage.UserID = (uint)User.GroupOwnerID;
                            GroupPercentage.LotteryID = XLottery.ID;
                            this.TablePercentage.Insert(GroupPercentage);
                        }

                        if(User.GroupOwnerID == User.ID)
                        {

                            if (XLottery.SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                            {
                                User.DOTA_GroupTotalBetItemsCount += XLotteryBet.ChipsNum + XLotteryBet.SteamItemsNum;
                                User.DOTA_GroupTotalBetPrice += XLotteryBet.TotalPrice;
                                User.DOTA_RUB_GroupTotalBetPrice += XLotteryBet.TotalPrice * XLottery.RubCurrency;
                            }
                            else if (XLottery.SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                            {
                                User.CSGO_GroupTotalBetItemsCount += XLotteryBet.ChipsNum + XLotteryBet.SteamItemsNum;
                                User.CSGO_GroupTotalBetPrice += XLotteryBet.TotalPrice;
                                User.CSGO_RUB_GroupTotalBetPrice += XLotteryBet.TotalPrice * XLottery.RubCurrency;
                            }

                            if (!this.TableUsersMemory.SelectOne(data => data.GroupOwnerID == User.ID && data.LotteryID == XLottery.ID, out XLotteryUsersMemory))
                            {

                                if (XLottery.SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                                {
                                    User.DOTA_GroupGamesCount++;
                                }
                                else if (XLottery.SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                                {
                                    User.CSGO_GroupGamesCount++;
                                }

                                XLotteryUsersMemory = new XLotteryUsersMemory();
                                XLotteryUsersMemory.LotteryID = XLottery.ID;
                                XLotteryUsersMemory.UserID = -1;
                                XLotteryUsersMemory.GroupOwnerID = User.GroupOwnerID;
                                this.TableUsersMemory.Insert(XLotteryUsersMemory);
                            }
                        }
                        else
                        {
                            if(Helper.UserHelper.Table.SelectByID((uint)User.GroupOwnerID, out GroupOwner))
                            {

                                if (XLottery.SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                                {
                                    GroupOwner.DOTA_GroupTotalBetItemsCount += XLotteryBet.ChipsNum + XLotteryBet.SteamItemsNum;
                                    GroupOwner.DOTA_GroupTotalBetPrice += XLotteryBet.TotalPrice;
                                    GroupOwner.DOTA_RUB_GroupTotalBetPrice += XLotteryBet.TotalPrice * XLottery.RubCurrency;
                                }
                                else if (XLottery.SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                                {
                                    User.CSGO_GroupGamesCount++; GroupOwner.CSGO_GroupTotalBetItemsCount += XLotteryBet.ChipsNum + XLotteryBet.SteamItemsNum;
                                    GroupOwner.CSGO_GroupTotalBetPrice += XLotteryBet.TotalPrice;
                                    GroupOwner.CSGO_RUB_GroupTotalBetPrice += XLotteryBet.TotalPrice * XLottery.RubCurrency;
                                }

                                if (!this.TableUsersMemory.SelectOne(data => data.GroupOwnerID == User.GroupOwnerID && data.LotteryID == XLottery.ID, out XLotteryUsersMemory))
                                {
                                    if (XLottery.SteamGameID == Configs.DOTA2_STEAM_GAME_ID)
                                    {
                                        GroupOwner.DOTA_GroupGamesCount++;
                                    }
                                    else if (XLottery.SteamGameID == Configs.CSGO_STEAM_GAME_ID)
                                    {
                                        GroupOwner.CSGO_GroupGamesCount++;
                                    }

                                    XLotteryUsersMemory = new XLotteryUsersMemory();
                                    XLotteryUsersMemory.LotteryID = XLottery.ID;
                                    XLotteryUsersMemory.UserID = -1;
                                    XLotteryUsersMemory.GroupOwnerID = User.GroupOwnerID;
                                    this.TableUsersMemory.Insert(XLotteryUsersMemory);
                                }

                                Helper.UserHelper.Table.UpdateByID(GroupOwner, GroupOwner.ID);
                            }
                        }
                    }


                    if (this.TablePercentage.SelectOne(data => data.LotteryID == XLottery.ID && data.UserID == Configs.ADMIN_ACCOUNT, out GroupPercentage))
                    {
                        GroupPercentage.ChipPrice += ((TotalPrice_Chips * GameSlot_Percentage) / 100);
                        GroupPercentage.SteamItemsPrice += ((TotalPrice_Items * GameSlot_Percentage) / 100);
                        this.TablePercentage.UpdateByID(GroupPercentage, GroupPercentage.ID);
                    }
                    else
                    {
                        GroupPercentage = new XLotteryPercentage();
                        GroupPercentage.ChipPrice += ((TotalPrice_Chips * GameSlot_Percentage) / 100);
                        GroupPercentage.SteamItemsPrice += ((TotalPrice_Items * GameSlot_Percentage) / 100);

                        GroupPercentage.UserID = Configs.ADMIN_ACCOUNT;
                        GroupPercentage.LotteryID = XLottery.ID;
                        this.TablePercentage.Insert(GroupPercentage);
                    }

                    uint xbet_id = this.TableBet.Insert(XLotteryBet);

                    XLotteryUsersBetsPrice XLotteryUsersBetsPrice;
                    if(this.TableUsersBetsPrice.SelectOne(data => data.UserID == User.ID && data.LotteryID == LotteryID, out XLotteryUsersBetsPrice))
                    {
                        XLotteryUsersBetsPrice.BetsCount++;
                        XLotteryUsersBetsPrice.TotalBetsItemsNum += XLotteryBet.ChipsNum + XLotteryBet.SteamItemsNum;
                        XLotteryUsersBetsPrice.TotalBetsPrice += XLotteryBet.TotalPrice;
                        this.TableUsersBetsPrice.UpdateByID(XLotteryUsersBetsPrice, XLotteryUsersBetsPrice.ID);
                    }
                    else
                    {
                        XLotteryUsersBetsPrice.UserID = User.ID;
                        XLotteryUsersBetsPrice.LotteryID = LotteryID;
                        XLotteryUsersBetsPrice.BetsCount++;
                        XLotteryUsersBetsPrice.TotalBetsItemsNum += XLotteryBet.ChipsNum + XLotteryBet.SteamItemsNum;
                        XLotteryUsersBetsPrice.TotalBetsPrice += XLotteryBet.TotalPrice;

                        this.TableUsersBetsPrice.Insert(XLotteryUsersBetsPrice);
                    }

                    this.Table.UpdateByID(XLottery, XLottery.ID);
                    Helper.UserHelper.Table.UpdateByID(User, User.ID);               

                    new Thread(delegate()
                    {
                        WebSocketPage.AddNewLotteryBet(this.TableBet.SelectByID(xbet_id), LotteryExtraTime, XLottery.EndTime - Helper.GetCurrentTime());
                        this.UpdateRecords(XLottery.SteamGameID);
                    }).Start();

                    client.SendWebsocket("BetDone" + BaseFuncs.WSplit + "1");
                    return 1;
                }

                else
                {
                    XSteamBot xbot;
                    if (Helper.SteamBotHelper.GetFreeBot(out xbot))
                    {
                        ProcessingBet.UserID = User.ID;
                        ProcessingBet.SteamGameID = XLottery.SteamGameID;
                        ProcessingBet.ProcessCreatedTime = Helper.GetCurrentTime();
                        ProcessingBet.LotteryID = XLottery.ID;
                        ProcessingBet.client = client;

                        this.RemoveProcessingBet(User.ID, XLottery.SteamGameID);
                        LotteryHelper.ProcessingBets.Add(ProcessingBet);

                        string ProtectionCode = "";
                        Random rnd = new Random();
                        for (ushort i = 0; i < 4; i++)
                        {
                            ProtectionCode += (char)rnd.Next(0x41, 0x5A);
                            //Thread.Sleep(1);
                        }

                        XSteamBotProcessItems.ProtectionCode = BaseFuncs.XSSReplacer(ProtectionCode);

                        UTRequestSteamMain.message = "Код протекции: " + XSteamBotProcessItems.ProtectionCode;
                        UTRequestSteamMain.BotID = (int)xbot.ID;
                        UTRequestSteamMain.steamid = User.SteamID.ToString();
                        UTRequestSteamMain.trade_acc_id = User.TradeToken;
                        UTRequestSteamMain.SendItems = false;

                        XSteamBotProcessItems.UserSteamID = User.SteamID;
                        XSteamBotProcessItems.UserID = User.ID;
                        XSteamBotProcessItems.SteamBotID = xbot.ID;
                        XSteamBotProcessItems.SteamGameID = XLottery.SteamGameID;

                        XSteamBotProcessItems.Status = 0;

                        // canceling other offer requests
                        XSteamBotProcessItems[] ProcessItems;
                        while (Helper.SteamBotHelper.Table_ProcessItems.SelectArr(data => data.UserSteamID == User.SteamID && (data.Status <= 1), out ProcessItems))
                        {
                            for (int i = 0; i < ProcessItems.Length; i++ )
                            {
                                if (ProcessItems[i].Status == 0)
                                {
                                    XSteamBotProcessItems stb = Helper.SteamBotHelper.Table_ProcessItems.SelectByID(ProcessItems[i].ID);
                                    stb.Status = 8;
                                    stb.StatusChangedTime = Helper.GetCurrentTime();
                                    Helper.SteamBotHelper.Table_ProcessItems.UpdateByID(stb, stb.ID);
                                }
                                else if (ProcessItems[i].Status == 1)
                                {
                                    Logger.ConsoleLog("Trying to decline offer: " + ProcessItems[i].OfferID);
                                    UpTunnel.Sender.Send(UTSteam.sk, "decline:" + ProcessItems[i].OfferID);

                                    XSteamBotProcessItems CancelProcess = ProcessItems[i];
                                    while (CancelProcess.Status == 1)
                                    {
                                        CancelProcess = Helper.SteamBotHelper.Table_ProcessItems.SelectByID(ProcessItems[i].ID);
                                        Thread.Sleep(100);
                                    }
                                }
                            }
                        }

                        Helper.SteamBotHelper.Table_ProcessItems.Insert(XSteamBotProcessItems);
                        if (UTSteam.ClientsOffer.ContainsKey(User.SteamID))
                        {
                            UTSteam.ClientsOffer.Remove(User.SteamID);
                        }

                        UTSteam.ClientsOffer.Add(User.SteamID, client);
                        UpTunnel.Sender.Send(UTSteam.sk, UTRequestSteamMain);

                        //client.SendWebsocket("TradeSent" + BaseFuncs.WSplit + xbot.Name + BaseFuncs.WSplit + ProtectionCode);
                        Logger.ConsoleLog("Send offer! bot id: " + UTRequestSteamMain.BotID, ConsoleColor.Green);
                        return 2;
                    }
                }
            }

            // 0: false, 1: true, 2: process, 3: нету шмотки
            return 0;
        }

        public double GetBank(uint LotteryID, out int ItemsNum)
        {
            double price = 0d;
            int itms = 0;
            XLottery xlot;
            if(this.Table.SelectByID(LotteryID, out xlot))
            {
                price = xlot.JackpotPrice;
                itms = xlot.JackpotItemsNum;
            }

            ItemsNum = itms;
            return price;
        }

        public int CalcLeftTime(uint LotteryID)
        {
            XLottery lottery;
            if (this.Table.SelectByID(LotteryID, out lottery))
            {
                int time = lottery.EndTime - Helper.GetCurrentTime();
                if (time >= 0)
                {
                    return time;
                }
            }

            return 0;
        }

        public bool SelectProcessingBet(uint UserID, uint SteamGameID, out ProcessingBet ProcessingBet)
        {
            for (int i = 0; i < LotteryHelper.ProcessingBets.Count; i++ )
            {
                if (LotteryHelper.ProcessingBets[i].UserID == UserID && LotteryHelper.ProcessingBets[i].SteamGameID == SteamGameID)
                {
                    ProcessingBet = LotteryHelper.ProcessingBets[i]; 
                    return true;
                }
            }

            ProcessingBet = null;
            return false;
        }

        public void RemoveProcessingBet( uint UserID, uint SteamGameID)
        {
            for (int i = 0; i < LotteryHelper.ProcessingBets.Count; i++)
            {
                if (LotteryHelper.ProcessingBets[i].UserID == UserID && LotteryHelper.ProcessingBets[i].SteamGameID == SteamGameID)
                {
                    LotteryHelper.ProcessingBets.Remove(LotteryHelper.ProcessingBets[i]);
                    return;
                }
            }
        }

        public void CancelBet(ulong OfferID, ulong UserSteamID, ushort Status)
        {
            XSteamBotProcessItems XSteamBotProcessItem;
            if (Helper.SteamBotHelper.SelectByOfferID(OfferID, out XSteamBotProcessItem))
            {
                XSteamBotProcessItem.Status = Status;
                XSteamBotProcessItem.StatusChangedTime = Helper.GetCurrentTime();
                Helper.SteamBotHelper.Table_ProcessItems.UpdateByID(XSteamBotProcessItem, XSteamBotProcessItem.ID);

                this.RemoveProcessingBet(XSteamBotProcessItem.UserID, XSteamBotProcessItem.SteamGameID);
            }
        }

        public XLottery[] TodaysGames(uint SteamGameID, out double JackpotPriceRecord, out int JackpotItemsRecord, ushort currency)
        {
            int time = (int)(DateTime.Today - new DateTime(2015, 7, 18)).TotalSeconds;
            // TODO: закэшировать эту информацию
            XLottery[] csgo;
            double price = 0d;
            int items = 0;
            Helper.LotteryHelper.Table.SelectArr(data => 
            {
                if (data.SteamGameID == SteamGameID && data.EndTime >= time)
                {
                    if (currency == 1)
                    {
                        if ((data.JackpotPrice * data.RubCurrency) > price)
                        {
                            price = data.JackpotPrice * data.RubCurrency;
                        }
                    }
                    else
                    {
                        if (data.JackpotPrice > price)
                        {
                            price = data.JackpotPrice;
                        }
                    }

                    if (data.JackpotItemsNum > items)
                    {
                        items = data.JackpotItemsNum;
                    }

                    if (data.WinnersToken > 0)
                    {
                        return true;
                    }
                }
                return false;
            }, out csgo);

            JackpotPriceRecord = price;
            JackpotItemsRecord = items;
            return csgo;
        }

        private double MaxJackpot(uint SteamGameID, out int ItemsRecord, ushort currency)
        {
            // TODO: закэшировать эту информацию
            double price = 0d;
            int JackpotItemsNum = 0;

            XLottery csgo;
            Helper.LotteryHelper.Table.SelectOne(data =>
            {
                if (data.SteamGameID == SteamGameID)
                {
                    if (currency == 1)
                    {
                        if ((data.JackpotPrice *data.RubCurrency) > price)
                        {
                            price = data.JackpotPrice * data.RubCurrency;
                        }
                    }
                    else
                    {
                        if (data.JackpotPrice > price)
                        {
                            price = data.JackpotPrice;
                        }
                    }

                    if (data.JackpotItemsNum > JackpotItemsNum)
                    {
                        JackpotItemsNum = data.JackpotItemsNum;
                    }
                }
                return false;
            }, out csgo);

            ItemsRecord = JackpotItemsNum;
            return price;
        }

        public List<TopItem> GetTopItems(uint LotteryID, int num, Client client)
        {
            List<TopItem> TopItems = new List<TopItem>();
            //ushort currency = Helper.UserHelper.GetCurrency(client);

            List<USteamItem> BankSteamItems;
            List<Chip> BankChips;
            Helper.LotteryHelper.GetBetItems(LotteryID, client, out BankSteamItems, out BankChips);

            for (int i = 0; i < Math.Min(num, BankSteamItems.Count); i++)
            {
                TopItem TopItem = new TopItem();
                TopItem.Name = BankSteamItems[i].Name;
                TopItem.Price = BankSteamItems[i].Price;
                TopItem.Price_Str = BankSteamItems[i].Price_Str;
                TopItem.Image = "/steam-image/" + BankSteamItems[i].SteamGameID + "/" + BankSteamItems[i].ID;

                TopItem.Color = BankSteamItems[i].Color;
                TopItem.Rarity = BankSteamItems[i].Rarity;
                TopItem.RarityColor = BankSteamItems[i].RarityColor;

                TopItems.Add(TopItem);
            }

            for (int i = 0; i < Math.Min(num, BankChips.Count); i++)
            {
                TopItem TopItem = new TopItem();
                TopItem.Name = "Фишка на сумму " + Helper.ChipHelper.SelectByID(BankChips[i].ID).Cost + "$";
                TopItem.Price = BankChips[i].Cost;
                TopItem.Price_Str = BankChips[i].Cost_Str;
                TopItem.Image = "/chip-image/" + BankChips[i].ID;

                TopItems.Add(TopItem);
            }

            TopItems = (from it in TopItems orderby it.Price descending select it).ToList();

            return TopItems.GetRange(0, Math.Min(num, TopItems.Count));
        }
    }
}
