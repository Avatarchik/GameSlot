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
        //public List<>

        private static List<ProcessingBet> ProcessingBets = new List<ProcessingBet>();
        //private static Dictionary<uint, List<ProcessingBet>> ProcessingBets = new Dictionary<uint,List<ProcessingBet>>();
        private static Dictionary<uint, List<Bet>> LotteryBets = new Dictionary<uint, List<Bet>>();

        public LotteryHelper()
        {
            new Thread(delegate()
            {
                try
                {
                    while (true)
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
                }
                catch { }
            }).Start();
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

                while (true)
                {
                    XLottery = this.Table.SelectByID(XLottery.ID);
                    if(XLottery.EndTime > 0 && XLottery.EndTime <= Helper.GetCurrentTime())
                    {
                        List<USteamItem> SteamItems;
                        List<Chip> Chips;
                        XLottery.JackpotPrice = this.GetBank(XLottery.ID, out XLottery.JackpotItemsNum);

                        XLottery.WinnersToken = (uint)(XLottery.JackpotPrice * 100 * XLottery.RaundNumber);
                        if (XLottery.WinnersToken <= 0)
                        {
                            XLottery.WinnersToken = 1;
                        }
                        XUser User = this.GetUserByToken(XLottery.WinnersToken, XLottery.ID);
                        uint WinnerUserID = XLottery.Winner = User.ID;
                        //WINRATE = (int)Math.Round(bet.TotalPrice / (this.GetBank(XLottery.ID, out bank_items) / 100));

                        double TotalPrice_UserBets = 0d;
                        XLotteryBet[] UsersBets;
                        Helper.LotteryHelper.TableBet.SelectArr(data => 
                        {
                            if(data.LotteryID == XLottery.ID && data.UserID == XLottery.Winner)
                            {
                                TotalPrice_UserBets += data.TotalPrice;
                            }
                            return false;
                        }, out UsersBets);

                        XLottery.Wonrate = (int)Math.Round(TotalPrice_UserBets / XLottery.JackpotPrice / 100);
                        XLottery.WinnerGroupID = User.GroupOwnerID;

                        this.Table.UpdateByID(XLottery, XLottery.ID);
                        this.GetBetItems(XLottery.ID, out SteamItems, out Chips);
                        XLottery = this.CreateNew(SteamGameID);

                        foreach (USteamItem SteamItem in SteamItems)
                        {
                            XSItemUsersInventory XSItemUsersInventory = new XSItemUsersInventory();
                            XSItemUsersInventory.UserID = WinnerUserID;
                            XSItemUsersInventory.SteamItemID = SteamItem.ID;
                            XSItemUsersInventory.AssertID = SteamItem.AssertID;
                            XSItemUsersInventory.SteamGameID = SteamItem.SteamGameID;
                            XSItemUsersInventory.SteamBotID = SteamItem.SteamBotID;
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
                    }

                    Thread.Sleep(100);
                }
            }).Start();
        }

        public void GetBetItems(uint LotteryID, out List<USteamItem> SteamItems, out List<Chip> Chips)
        {
            XLotteryBet[] XLotteryBets;
            SteamItems = new List<USteamItem>();
            Chips = new List<Chip>();

            XLottery xlot;
            if (this.Table.SelectByID(LotteryID, out xlot) && this.TableBet.SelectArr(data => data.LotteryID == xlot.ID, out XLotteryBets))
            {
               // Logger.ConsoleLog("here!~nna");
                // make shit
                for(uint i = 0; i < XLotteryBets.Length; i++)
                {
                    XLotteryBet XLotteryBet = XLotteryBets[i];

                    // steam items
                    for (int x = 0; x < XLotteryBet.SteamItemsNum; x++)
                    {
                        USteamItem SteamItem;
                        if (Helper.SteamItemsHelper.SelectByID(XLotteryBet.SteamItemIDs[x], xlot.SteamGameID, out SteamItem))
                        {
                            //Logger.ConsoleLog(x + "lolz" + SteamItem.ID, ConsoleColor.DarkGreen);
                            SteamItem.AssertID = XLotteryBet.ItemAssertIDs[x];
                            //SteamItem.Price = XLotteryBet.SteamItemsPrice[x];
                            //SteamItem.Price_Str = SteamItem.Price.ToString("###,##0.00");
                            SteamItems.Add(SteamItem);
                        }
                    }

                   // Logger.ConsoleLog(XLotteryBet.ChipsNum + "| LOLS");
                    // chips
                    for (int x = 0; x < XLotteryBet.ChipsNum; x++)
                    {
                        Chip Chip;
                        if (Helper.ChipHelper.SelectByID(XLotteryBet.ChipIDs[x], out Chip))
                        {
                            Chip = Chip.Clone();
                            Chip.AssertID = XLotteryBet.ChipAssertIDs[x];
                            Chips.Add(Chip);
                        }
                    }
                }
            }
           
        }

        public XUser GetUserByToken(uint Token, uint LotteryID)
        {
            XLotteryBet LotteryBet;
            // 1 >= 2685 &&  5000 <=2685
            this.TableBet.SelectOne(data => data.LotteryID == LotteryID && Token >= data.FisrtToken && Token <= data.LastToken, out LotteryBet);

            XUser user;
            Helper.UserHelper.Table.SelectByID(LotteryBet.UserID, out user);
            return user;
        }

        public Lottery GetCurrent(uint SteamGameID)
        {
            List<XLottery> xlots;
            this.Table.Select(data => data.SteamGameID == SteamGameID && (data.StartTime <= Helper.GetCurrentTime() || data.StartTime == 0), out xlots);

            Lottery lot;
            this.GetLottery(xlots[xlots.Count - 1].ID, out lot);
            return lot;
        }

        public bool GetLottery(uint ID, out Lottery Lottery)
        {
            XLottery xlot;
            if (this.Table.SelectByID(ID, out xlot))
            {
                Lottery = new Lottery();
                Lottery.ID = xlot.ID;
                Lottery.IDStr = xlot.ID.ToString("000-000-000");
                Lottery.JackpotPrice = this.GetBank(xlot.ID, out Lottery.JackpotItems);
                Lottery.JackpotPrice_Str = Lottery.JackpotPrice.ToString("###,##0.00");

                //Logger.ConsoleLog(xlot.EndTime, ConsoleColor.Blue);
                Lottery.LeftTime = (xlot.EndTime > 0) ? this.CalcLeftTime(xlot.ID) : -1;
                
                Lottery.RaundNumber = xlot.RaundNumber;
                Lottery.RaundNumber_MD5 = BaseFuncs.MD5(xlot.RaundNumber.ToString());

                //Lottery.Bets = this.GetBets(xlot.ID);
                Lottery.SteamGameID = xlot.SteamGameID;

                Lottery.WinnersToken = xlot.WinnersToken;
                Lottery.Winner = Helper.UserHelper.Table.SelectByID(xlot.Winner);
                return true;
            }
            Lottery = null;
            return false;
        }

        public List<Bet> GetUsersBets(uint LotteryID, int FromID, int ItemsNum)
        {
            //Dictionary<uint, List<Bet>> UserBets = new Dictionary<uint, List<Bet>>();
            uint size = 0;
            Helper.UserHelper.Table.GetData(out size);

            List<Bet>[] UserBets = new List<Bet>[size];
            double[] UserBetsTotalPrice = new double[size];
            int[] UserBetsItemsNum = new int[size];
            //Dictionary<uint, double> UserBetsTotalPrice = new Dictionary<uint, double>();
            //Dictionary<uint, int> UserBetsItemsNum = new Dictionary<uint, int>();

            List<Bet> Bets = new List<Bet>();
            XLottery XLottery;
            if (this.Table.SelectByID(LotteryID, out XLottery))
            {
                XLotteryBet[] XBets;
                if (this.TableBet.SelectArrFromEnd(data => data.LotteryID == LotteryID, out XBets, FromID, 10))
                {
                    for (int bb = 0; bb < XBets.Length; bb++)
                    {
                        Bet bet = new Bet();
                        //Logger.ConsoleLog(bet.ID);
                        bet.SteamItems = new List<USteamItem>();
                        bet.Chips = new List<Chip>();

                        Logger.ConsoleLog("Steam items num in bet: " + XBets[bb].SteamItemsNum);
                        // steam items
                        for (uint i = 0; i < XBets[bb].SteamItemsNum; i++)
                        {
                            USteamItem Item;
                            if (Helper.SteamItemsHelper.SelectByID(XBets[bb].SteamItemIDs[i], XLottery.SteamGameID, out Item))
                            {
                                Item.AssertID = XBets[bb].ItemAssertIDs[i];
                                bet.Price += Item.Price = XBets[bb].SteamItemsPrice[i];
                                Item.Price_Str = Item.Price.ToString("###,##0.00");

                                bet.SteamItems.Add(Item);
                            }
                        }

                        // chips
                        for (uint i = 0; i < XBets[bb].ChipsNum; i++)
                        {
                            Chip Chip;
                            if (Helper.ChipHelper.SelectByID(XBets[bb].ChipIDs[i], out Chip))
                            {
                                Chip = Chip.Clone();
                                Chip.AssertID = XBets[bb].ChipAssertIDs[i];

                                bet.Price += Chip.Cost;
                                bet.Chips.Add(Chip);
                            }
                        }

                        bet.ItemsNum = bet.Chips.Count + bet.SteamItems.Count;
                        bet.UserBets = new List<Bet>();

                        if (UserBets[XBets[bb].UserID] != null)
                        {
                            bet.UserBets = UserBets[XBets[bb].UserID];
                            bet.TotalItemsNum = UserBetsItemsNum[XBets[bb].UserID];
                            bet.TotalPrice = UserBetsTotalPrice[XBets[bb].UserID];
                        }
                        else
                        {
                            XLotteryBet[] XUsersBets;
                            if (this.TableBet.SelectArrFromEnd(data => data.LotteryID == XLottery.ID && data.UserID == XBets[bb].UserID, out XUsersBets))
                            {
                                for (int gg = 0; gg < XUsersBets.Length; gg++)
                                {
                                    Bet UsersBet = new Bet();
                                    UsersBet.FirstToken = XUsersBets[gg].FisrtToken;
                                    UsersBet.LastToken = XUsersBets[gg].LastToken;

                                    UsersBet.ItemsNum = XUsersBets[gg].SteamItemsNum + XUsersBets[gg].ChipsNum;
                                    UsersBet.Price = XUsersBets[gg].TotalPrice;

                                    bet.TotalItemsNum += UsersBet.ItemsNum;
                                    bet.TotalPrice += UsersBet.Price;

                                    UsersBet.TotalPrice_Str = UsersBet.Price.ToString("###,##0.00");
                                    bet.UserBets.Add(UsersBet);
                                }

                                UserBets[XBets[bb].UserID] = bet.UserBets;
                                UserBetsItemsNum[XBets[bb].UserID] = bet.TotalItemsNum;
                                UserBetsTotalPrice[XBets[bb].UserID] = bet.TotalPrice;
                            }
                        }

                        bet.Price_Str = bet.Price.ToString("###,##0.00");
                        bet.TotalPrice_Str = bet.TotalPrice.ToString("###,##0.00");
                        bet.FirstToken = XBets[bb].FisrtToken;
                        bet.LastToken = XBets[bb].LastToken;
                        bet.BetsNum = bet.UserBets.Count;

                        bet.XUser = Helper.UserHelper.Table.SelectByID(XBets[bb].UserID);
                        int bank_items;
                        bet.Winrate = (int)Math.Round(bet.TotalPrice / (this.GetBank(XLottery.ID, out bank_items) / 100));

                        Bets.Add(bet);
                                         
                    }          
                }
            }

            return Bets;
        }

       /* public ushort SetBet(uint LotteryID, uint UserID, List<USteamItem> SteamItems, List<Chip> Chips, Client client)
        {
            XLottery XLottery;
            XUser User;
            if (Helper.UserHelper.Table.SelectByID(UserID, out User) && this.Table.SelectByID(LotteryID, out XLottery) && XLottery.WinnersToken == 0 && SteamItems.Count + Chips.Count <= 24)
            {
                XLotteryBet XLotteryBet = new XLotteryBet();
                XLotteryBet.TotalPrice = 0d;
                ushort ItemsFromSteamInventory = 0;
                ulong[] SteamItemAsserIDs_FromSteamInventory = new ulong[24];
                uint[] SteamItemIDs_FromSteamInventory = new uint[24];

                ProcessingBet ProcessingBet = new ProcessingBet();
                UTRequestSteamMain UTRequestSteamMain = new UTRequestSteamMain();

                // Steam items
                if(SteamItems.Count > 0)
                {
                    XLotteryBet.ItemAssertIDs = new ulong[24];
                    XLotteryBet.SteamItemIDs = new uint[24];
                    XLotteryBet.SteamBotIDs = new uint[24];
                    XLotteryBet.SteamItemsPrice = new double[24];

                    ushort array_index = 0;
                    List<USteamItem> st_itms = new List<USteamItem>();

                    string UsersSteamInventory_str;
                    Helper.UserHelper.GetUsersSteamInventory(User.ID, XLottery.SteamGameID, out UsersSteamInventory_str);

                    foreach(USteamItem SteamItem in SteamItems)
                    {
                        Logger.ConsoleLog("SteamItem Assert: " + SteamItem.AssertID);
                        XSteamItem xitem;
                        if (Helper.SteamItemsHelper.SelectByID(SteamItem.ID, XLottery.SteamGameID, out xitem) && SteamItem.Price >= Configs.MIN_ITEMS_PRICE)
                        {
                            // проверить, есть ли данный предмет у юзера
                            // сначала в локальном инвентаре
                            if (Helper.UserHelper.IsUserHaveSteamItem(SteamItem.AssertID, SteamItem.ID, User.ID))
                            {
                                st_itms.Add(SteamItem);
                                ProcessingBet.SteamItems.Add(SteamItem);
                            }
                            // steam инвентарь
                            else if (Helper.UserHelper.IsUserHaveSteamItem_SteamInventory(SteamItem.AssertID, UserID, XLottery.SteamGameID, UsersSteamInventory_str))
                            {
                                //Logger.ConsoleLog("SteamItem done: " + SteamItem.AssertID);
                                SteamItemAsserIDs_FromSteamInventory[ItemsFromSteamInventory] = SteamItem.AssertID;
                                SteamItemIDs_FromSteamInventory[ItemsFromSteamInventory] = xitem.ID;

                                // добалвяем счетчик
                                ItemsFromSteamInventory++;
                                ProcessingBet.SteamItems.Add(SteamItem);

                                UTRequestSteamItem UTRequestSteamItem = new UTRequestSteamItem();
                                UTRequestSteamItem.appid = (int) XLottery.SteamGameID;
                                UTRequestSteamItem.contextid = 2;
                                UTRequestSteamItem.assertid = (long) SteamItem.AssertID;
                                UTRequestSteamMain.Items.Add(UTRequestSteamItem);
                            }
                            else
                            {
                                return 3;
                            }
                        }
                    }

                    if(st_itms.Count > 0)
                    {
                        foreach (USteamItem SteamItem in st_itms)
                        {
                            XSItemUsersInventory XSItemUsersInventory;
                            XSteamItem xitem;
                            if (Helper.SteamItemsHelper.SelectByID(SteamItem.ID, XLottery.SteamGameID, out xitem) && Helper.UserHelper.GetSteamLocalItem(SteamItem.AssertID, SteamItem.ID, User.ID, out XSItemUsersInventory))
                            {
                                XLotteryBet.ItemAssertIDs[array_index] = SteamItem.AssertID;
                                XLotteryBet.SteamItemIDs[array_index] = SteamItem.ID;
                                XLotteryBet.SteamBotIDs[array_index] = XSItemUsersInventory.SteamBotID;

                                array_index++;
                                XLotteryBet.SteamItemsNum = array_index;

                                XSItemUsersInventory.Deleted = true;
                                Helper.UserHelper.Table_SteamItemUsersInventory.UpdateByID(XSItemUsersInventory, XSItemUsersInventory.ID);
                                XLotteryBet.TotalPrice += SteamItem.Price = xitem.Price;
                            }
                        }
                    }
                }

                // Chips
                if (Chips.Count > 0)
                {
                    XLotteryBet.ChipAssertIDs = new ulong[24];
                    XLotteryBet.ChipIDs = new uint[24];

                    List<XChipUsersInventory> ch_check = new List<XChipUsersInventory>();
                    ushort array_index = 0;
                    foreach (Chip chip in Chips)
                    {
                        XChipUsersInventory x_chip;

                        if (Helper.UserHelper.Table_ChipUsersInventory.SelectOne(data => data.UserID == UserID && data.AssertID == chip.AssertID && !data.Deleted, out x_chip)
                            && Helper.UserHelper.IsUserHaveChip(x_chip.AssertID, UserID))
                        {
                            if (ItemsFromSteamInventory > 0)
                            {
                                ProcessingBet.Chips.Add(chip);
                            }
                            else
                            {
                                // забираем предмет у пользователя
                                x_chip.Deleted = true;
                                ch_check.Add(x_chip);
                            }
                        }
                        else
                        {
                            //Logger.ConsoleLog("error chips: " + chip.AssertID);
                            return 3;
                        }
                    }

                    if (ch_check.Count > 0)
                    {
                        foreach (XChipUsersInventory x_ch in ch_check)
                        {
                            Chip ch;
                            if (Helper.UserHelper.IsUserHaveChip(x_ch.AssertID, UserID) && Helper.ChipHelper.SelectByID(x_ch.ChipID, out ch) && ch.Cost > Configs.MIN_ITEMS_PRICE)
                            {
                                XLotteryBet.ChipAssertIDs[array_index] = x_ch.AssertID;
                                XLotteryBet.ChipIDs[array_index] = x_ch.ChipID;
                                array_index++;
                                XLotteryBet.ChipsNum = array_index;
                                XLotteryBet.TotalPrice += ch.Cost;

                                Helper.UserHelper.Table_ChipUsersInventory.UpdateByID(x_ch, x_ch.ID);
                            }
                        }
                    }

                }
                if (ProcessingBet.SteamItems.Count + ProcessingBet.Chips.Count > 0) 
                {
                    if (XLotteryBet.TotalPrice >= Configs.MIN_ITEMS_PRICE)
                    {
                        if (ItemsFromSteamInventory <= 0)
                        {
                            int bank_items;
                            double Bank = this.GetBank(XLottery.ID, out bank_items);
                            //Logger.ConsoleLog(ff_bets, ConsoleColor.Yellow);

                            //ДОБАВЛЕНИЕ ВРЕМЕНИ
                            int LotteryExtraTime = -1;
                            if (Bank <= 0)
                            {
                                XLottery.EndTime = Helper.GetCurrentTime() + Configs.LOTTERY_GAME_TIME;
                            }
                            else
                            {
                                double oneproc = Bank / 100;
                                double proctime = XLotteryBet.TotalPrice / oneproc;
                                double fl = Configs.LOTTERY_EXTRA_TIME / 100;
                                LotteryExtraTime = (int)Math.Round(proctime * fl);
                                //Logger.ConsoleLog(Bank + ":" + Math.Round(main), ConsoleColor.DarkCyan);

                                int EndTimeBefore = XLottery.EndTime;
                                XLottery.EndTime += LotteryExtraTime;

                                if (XLottery.EndTime > Helper.GetCurrentTime() + Configs.LOTTERY_GAME_TIME)
                                {
                                    XLottery.EndTime = Helper.GetCurrentTime() + Configs.LOTTERY_GAME_TIME;
                                    LotteryExtraTime = XLottery.EndTime - EndTimeBefore;
                                }
                            }

                            List<XLotteryBet> lottery_bets;
                            bool LastBet = this.TableBet.Select(data => data.LotteryID == XLottery.ID, out lottery_bets);

                            XLotteryBet.FisrtToken = (LastBet) ? lottery_bets[lottery_bets.Count - 1].LastToken + 1 : 1;
                            XLotteryBet.LastToken = XLotteryBet.FisrtToken + (uint)(XLotteryBet.TotalPrice * 100 / Configs.TOKEN_PRICE);
                            //Logger.ConsoleLog(XLotteryBet.TotalPrice / Configs.TOKEN_PRICE + "::" + XLotteryBet.LastToken);

                            if (ProcessingBet.SteamItems.Count > 0)
                            {
                                // забираем предметы у пользователя
                                foreach (USteamItem itm in ProcessingBet.SteamItems)
                                {
                                    XSItemUsersInventory x_itm;
                                    Helper.UserHelper.Table_SteamItemUsersInventory.SelectOne(data => data.AssertID == itm.AssertID && data.UserID == UserID && !data.Deleted, out x_itm);
                                    x_itm.Deleted = true;
                                    Helper.UserHelper.Table_SteamItemUsersInventory.UpdateByID(x_itm, x_itm.ID);
                                }
                            }

                            XLotteryBet.LotteryID = XLottery.ID;
                            XLotteryBet.UserID = User.ID;
                            XLotteryBet.GroupOwnerID = User.GroupOwnerID;

                            uint xbet_id = this.TableBet.Insert(XLotteryBet);
                            //Logger.ConsoleLog(this.TableBet.SelectByID(xbet_id).LotteryID, ConsoleColor.Red);
                            this.Table.UpdateByID(XLottery, XLottery.ID);

                            new Thread(delegate()
                            {
                                WebSocketPage.AddNewLotteryBet(this.TableBet.SelectByID(xbet_id), Bank + XLotteryBet.TotalPrice, bank_items + XLotteryBet.ChipsNum + XLotteryBet.SteamItemsNum, LotteryExtraTime, XLottery.EndTime - Helper.GetCurrentTime());
                            }).Start();

                            return 1;
                        }
                        else if(User.TradeToken != null)
                        {
                            // SAVE PROCESSING BET
                            ProcessingBet.UserID = User.ID;
                            ProcessingBet.SteamGameID = XLottery.SteamGameID;
                            ProcessingBet.ProcessCreatedTime = Helper.GetCurrentTime();
                            ProcessingBet.LotteryID = XLottery.ID;
                            LotteryHelper.ProcessingBets.Add(ProcessingBet);

                            // send steam trade
                            XSteamBot xbot;
                            if (Helper.SteamBotHelper.GetFreeBot(out xbot))
                            {
                                string ProtectionCode = "";
                                for (ushort i = 0; i < 4; i++)
                                {
                                    ProtectionCode += (char)new Random().Next(0x30, 0x5B);
                                    Thread.Sleep(3);
                                }

                                UTRequestSteamMain.message = "Код протекции: " + ProtectionCode;
                                UTRequestSteamMain.BotID = (int)xbot.ID;
                                UTRequestSteamMain.steamid = User.SteamID.ToString();
                                UTRequestSteamMain.trade_acc_id = User.TradeToken;
                                UTRequestSteamMain.SendItems = false;


                                XSteamBotProcessItem XSteamBotProcessItem = new XSteamBotProcessItem();
                                XSteamBotProcessItem.UserSteamID = User.SteamID;
                                XSteamBotProcessItem.SteamBotID = xbot.ID;
                                XSteamBotProcessItem.SteamGameID = XLottery.SteamGameID;

                                XSteamBotProcessItem.ItemAssertIDs = SteamItemAsserIDs_FromSteamInventory;
                                XSteamBotProcessItem.SteamItemIDs = SteamItemIDs_FromSteamInventory;
                                XSteamBotProcessItem.SteamItemsNum = ItemsFromSteamInventory;
                                XSteamBotProcessItem.Status = 0;
                                XSteamBotProcessItem.UserID = User.ID;
                                XSteamBotProcessItem.ProtectionCode = ProtectionCode;

                                Helper.SteamBotHelper.Table_Items.Insert(XSteamBotProcessItem);
                                UpTunnel.Sender.Send(UTSteam.sk, UTRequestSteamMain);

                                client.SendWebsocket("TradeSent" + BaseFuncs.WSplit + xbot.Name + BaseFuncs.WSplit + ProtectionCode);
                                return 2;
                            }
                        }
                    }
                }
            }

            // 0: false, 1: true, 2: process, 3: нету шмотки
            return 0;
        }
        */
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

                string SteamInventory_String = null;

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
                            if(SteamInventory_String == null)
                            {
                                Helper.UserHelper.GetUsersSteamInventory_Json(User.ID, XLottery.SteamGameID, out SteamInventory_String);
                            }

                            if (Helper.UserHelper.IsUserHaveSteamItem_SteamInventory(SteamItem.AssertID, SteamItem.ID, User, XLottery.SteamGameID, SteamInventory_String))
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

                        //Logger.ConsoleLog("Chip found!");
                    }
                    else
                    {
                        //Logger.ConsoleLog("Chip not found!");
                        return 0;
                    }
                }

                if(!ItemsFromSteamInventory)
                {
                    int BankItemsNum;
                    double BankPrice = this.GetBank(XLottery.ID, out BankItemsNum);

                    int LotteryExtraTime = -1;
                    if (BankPrice <= 0)
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

                    XLotteryBet XLotteryBet = new XLotteryBet();
                    XLotteryBet.LotteryID = XLottery.ID;
                    XLotteryBet.UserID = User.ID;
                    XLotteryBet.GroupOwnerID = User.GroupOwnerID;

                    XLotteryBet[] XBets;
                    this.TableBet.SelectArr(data => data.LotteryID == XLotteryBet.ID, out XBets);
                    XLotteryBet.FisrtToken = (XBets.Length > 0) ? XBets[XBets.Length-1].LastToken +1 : 1;
                    XLotteryBet.LastToken = XLotteryBet.FisrtToken + (uint)(TotalPrice * 100 / Configs.TOKEN_PRICE);;

                    XLotteryBet.SteamItemIDs = new uint[24];
                    XLotteryBet.ItemAssertIDs = new ulong[24];

                    XLotteryBet.ChipIDs = new uint[24];
                    XLotteryBet.ChipAssertIDs = new ulong[24];

                    XLotteryBet.SteamBotIDs = new uint[24];
                    XLotteryBet.SteamItemsPrice = new double[24];

                    XLotteryBet.TotalPrice = TotalPrice;

                    // foreach and delete from user item
                    foreach(USteamItem Item in ProcessingBet.SteamItems)
                    {
                        XLotteryBet.SteamItemIDs[XLotteryBet.SteamItemsNum] = Item.ID;
                        XLotteryBet.ItemAssertIDs[XLotteryBet.SteamItemsNum] = Item.AssertID;    
                        XLotteryBet.SteamItemsPrice[XLotteryBet.SteamItemsNum] = Item.Price;
                        XLotteryBet.SteamBotIDs[XLotteryBet.SteamItemsNum] = Item.SteamBotID;
                        XLotteryBet.SteamItemsNum++;

                        XSItemUsersInventory XSItemUsersInventory;
                        Helper.UserHelper.Table_SteamItemUsersInventory.SelectOne(data => !data.Deleted && data.AssertID == Item.AssertID, out XSItemUsersInventory);
                        XSItemUsersInventory.Deleted = true;
                        Helper.UserHelper.Table_SteamItemUsersInventory.UpdateByID(XSItemUsersInventory, XSItemUsersInventory.ID);
                    }

                    foreach(Chip Chip in ProcessingBet.Chips)
                    {
                        XLotteryBet.ChipIDs[XLotteryBet.ChipsNum] = Chip.ID;
                        XLotteryBet.ChipAssertIDs[XLotteryBet.ChipsNum] = Chip.AssertID;
                        XLotteryBet.ChipsNum++;

                        XChipUsersInventory XChipUsersInventory;
                        Helper.UserHelper.Table_ChipUsersInventory.SelectOne(data => !data.Deleted && data.AssertID == Chip.AssertID, out XChipUsersInventory);
                        XChipUsersInventory.Deleted = true;
                        Helper.UserHelper.Table_ChipUsersInventory.UpdateByID(XChipUsersInventory, XChipUsersInventory.ID);
                    }

                    //Logger.ConsoleLog("Bet created! :D");

                    uint xbet_id = this.TableBet.Insert(XLotteryBet);
                    this.Table.UpdateByID(XLottery, XLottery.ID);

                    new Thread(delegate()
                    {
                        WebSocketPage.AddNewLotteryBet(this.TableBet.SelectByID(xbet_id), BankPrice + XLotteryBet.TotalPrice, BankItemsNum + XLotteryBet.ChipsNum + XLotteryBet.SteamItemsNum, LotteryExtraTime, XLottery.EndTime - Helper.GetCurrentTime());
                    }).Start();

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
                            ProtectionCode += (char)rnd.Next(0x30, 0x5B);
                            //Thread.Sleep(1);
                        }

                        UTRequestSteamMain.message = "Код протекции: " + ProtectionCode;
                        UTRequestSteamMain.BotID = (int)xbot.ID;
                        UTRequestSteamMain.steamid = User.SteamID.ToString();
                        UTRequestSteamMain.trade_acc_id = User.TradeToken;
                        UTRequestSteamMain.SendItems = false;

                        XSteamBotProcessItems.UserSteamID = User.SteamID;
                        XSteamBotProcessItems.UserID = User.ID;
                        XSteamBotProcessItems.SteamBotID = xbot.ID;
                        XSteamBotProcessItems.SteamGameID = XLottery.SteamGameID;

                        XSteamBotProcessItems.Status = 0;
                        XSteamBotProcessItems.UserID = User.ID;
                        XSteamBotProcessItems.ProtectionCode = ProtectionCode;

                        // canceling other offer requests
                        XSteamBotProcessItems[] ProcessItems;
                        if (Helper.SteamBotHelper.Table_Items.SelectArr(data => data.UserID == User.ID && (data.Status == 1), out ProcessItems))
                        {
                            for (int i = 0; i < ProcessItems.Length; i++ )
                            {
                                Logger.ConsoleLog("Trying to decline offer: " + ProcessItems[i].OfferID);
                                UpTunnel.Sender.Send(UTSteam.sk, "decline:" + ProcessItems[i].OfferID);
                            }
                        }

                        Helper.SteamBotHelper.Table_Items.Insert(XSteamBotProcessItems);
                        UpTunnel.Sender.Send(UTSteam.sk, UTRequestSteamMain);

                        client.SendWebsocket("TradeSent" + BaseFuncs.WSplit + xbot.Name + BaseFuncs.WSplit + ProtectionCode);

                        Logger.ConsoleLog("Send offer!", ConsoleColor.Green);
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
            int items_num = 0;
            XLotteryBet[] Bets;
  
            if(this.TableBet.SelectArr(data => data.LotteryID == LotteryID, out Bets))
            {
                for(uint i = 0; i < Bets.Length; i++)
                {
                    price += Bets[i].TotalPrice;
                    items_num += Bets[i].SteamItemsNum + Bets[i].ChipsNum;
                }
            }
            ItemsNum = items_num;
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
                Helper.SteamBotHelper.Table_Items.UpdateByID(XSteamBotProcessItem, XSteamBotProcessItem.ID);

                this.RemoveProcessingBet(XSteamBotProcessItem.UserID, XSteamBotProcessItem.SteamGameID);
            }
        }


        // RECORED: 
        public XLottery[] TodaysGames(uint SteamGameID, out double JackpotPriceRecord, out int JackpotItemsRecord)
        {
            XLottery[] csgo;
            double price = 0d;
            int items = 0;
            Helper.LotteryHelper.Table.SelectArr(data => 
            { 
                if(data.SteamGameID == SteamGameID && data.StartTime <= Helper.GetCurrentTime() && data.EndTime >= Helper.GetCurrentTime() - 86400)
                {
                    if (data.JackpotPrice > price)
                    {
                        price = data.JackpotPrice;
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

        public double MaxJackpot(uint SteamGameID, out int ItemsRecord)
        {
            double price = 0d;
            int JackpotItemsNum = 0;

            XLottery csgo;
            Helper.LotteryHelper.Table.SelectOne(data =>
            {
                if (data.SteamGameID == SteamGameID)
                {
                    if (data.JackpotPrice > price)
                    {
                        price = data.JackpotPrice;
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

        public uint CountGamers(uint SteamGameID)
        {
            uint count = 0;
            XLottery csgo;
            //Helper.LotteryHelper.

            return count;
        }
    }
}
