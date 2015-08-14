using GameSlot.Database;
using GameSlot.Types;
using SteamBotUTRequest;
using System;
using System.Collections.Generic;
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

        private static List<ProcessingBet> ProcessingBets = new List<ProcessingBet>();

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

                Logger.ConsoleLog("Added new lottery!", ConsoleColor.Yellow);
                Logger.ConsoleLog("Lottery ID: [" + lottery.ID + "]");
                Logger.ConsoleLog("Steam game ID: [" + lottery.SteamGameID + "]");
                Logger.ConsoleLog("Raund number: [" + lottery.RaundNumber + "]");
                Logger.ConsoleLog("StartTime: [" + lottery.StartTime + "]");
                Logger.ConsoleLog("--------------------------------------------------\n");

                return lottery;
            }

            this.Table.SelectOne(data => data.SteamGameID == SteamGameID && data.WinnersToken == 0, out lottery);
            return lottery;
        }

        public void StartLottery(uint SteamGameID)
        {
            new Thread(delegate()
            {
                XLottery lottery = this.CreateNew(SteamGameID);

                while (true)
                {
                    lottery = this.Table.SelectByID(lottery.ID);
                    if(lottery.EndTime > 0 && lottery.EndTime <= Helper.GetCurrentTime())
                    {
                        List<USteamItem> SteamItems;
                        List<Chip> Chips;
                        lottery.WinnersToken = (uint) (this.GetBank(lottery.ID, out SteamItems, out Chips) * 100 * lottery.RaundNumber);
                        if (lottery.WinnersToken <= 0)
                        {
                            lottery.WinnersToken = 1;
                        }
                        uint WinnerUserID = lottery.Winner = this.GetUserByToken(lottery.WinnersToken, lottery.ID).ID;

                        this.Table.UpdateByID(lottery, lottery.ID);
                        lottery = this.CreateNew(SteamGameID);
                        Thread.Sleep(15000);

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

                        foreach (Chip chip in Chips)
                        {
                            XChipUsersInventory XChipUsersInventory = new XChipUsersInventory();
                            XChipUsersInventory.UserID = WinnerUserID;
                            XChipUsersInventory.ChipID = chip.ID;
                            XChipUsersInventory.AssertID = chip.AssertID;
                            Helper.UserHelper.Table_ChipUsersInventory.Insert(XChipUsersInventory);
                        }
                    }

                    Thread.Sleep(1000);
                }
            }).Start();
        }

        public XUser GetUserByToken(uint Token, uint LotteryID)
        {
            XLotteryBet LotteryBet;
            this.TableBet.SelectOne(data => data.LotteryID == LotteryID && data.FisrtToken >= Token && data.LastToken <= Token, out LotteryBet);
            XUser user;
            Helper.UserHelper.Table.SelectByID(LotteryBet.UserID, out user);
            return user;
        }

        public Lottery GetCurrent(uint SteamGameID)
        {
            List<XLottery> xlots;
            this.Table.Select(data => data.SteamGameID == SteamGameID && (data.StartTime <= Helper.GetCurrentTime() || data.StartTime == 0), out xlots);

            Lottery lot;
            this.GetLottery(xlots.Last().ID, out lot);
            return lot;
        }

        public bool GetLottery(uint ID, out Lottery Lottery)
        {
            XLottery xlot;
            if (this.Table.SelectByID(ID, out xlot))
            {
                Lottery = new Lottery();
                Lottery.ID = xlot.ID;
                Lottery.IDStr = xlot.ID.ToString("");

                Lottery.JackpotPrice = this.GetBank(xlot.ID, out Lottery.SteamItems, out Lottery.Chips);
                Lottery.JackpotItems = Lottery.SteamItems.Count;

                //Logger.ConsoleLog(this.CalcLeftTime(xlot.ID), ConsoleColor.Blue);
                Lottery.LeftTime = (xlot.EndTime > 0) ? this.CalcLeftTime(xlot.ID) : -1;
                
                Lottery.RaundNumber = xlot.RaundNumber;
                Lottery.Bets = this.GetBets(xlot.ID);
                return true;
            }

            Lottery = null;
            return false;
        }

        public ushort SetBet(uint LotteryID, uint UserID, List<USteamItem> SteamItems, List<Chip> Chips)
        {
            XLottery XLottery;
            XUser User;
            if (Helper.UserHelper.Table.SelectByID(UserID, out User) && this.Table.SelectByID(LotteryID, out XLottery) && XLottery.WinnersToken == 0 && SteamItems.Count + Chips.Count <= 24)
            {
                XLotteryBet XLotteryBet = new XLotteryBet();
                double price = 0d;
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

                    ushort array_index = 0;

                    foreach(USteamItem SteamItem in SteamItems)
                    {
                        XSteamItem xitem;
                        if(Helper.SteamItemsHelper.SelectByID(SteamItem.ID, XLottery.SteamGameID, out xitem))
                        {
                            // проверить, есть ли данный предмет у юзера
                            // сначала в локальном инвентаре
                            XSItemUsersInventory XSItemUsersInventory;
                            if (Helper.UserHelper.GetSteamLocalItem(SteamItem.AssertID, User.ID, out XSItemUsersInventory))
                            {
                                if(SteamItem.Price >= Configs.MIN_ITEMS_PRICE)
                                {
                                    XLotteryBet.ItemAssertIDs[array_index] = SteamItem.AssertID;
                                    XLotteryBet.SteamItemIDs[array_index] = SteamItem.ID;
                                    XLotteryBet.SteamBotIDs[array_index] = XSItemUsersInventory.SteamBotID;

                                    array_index++;
                                    XLotteryBet.SteamItemsNum = array_index;

                                    SteamItem.Price = xitem.Price;
                                    price += xitem.Price;

                                    ProcessingBet.SteamItems.Add(SteamItem);
                                }
                            }
                            // steam инвентарь
                            else if (Helper.UserHelper.IsUserHaveSteamItem_SteamInventory(SteamItem.AssertID, UserID, XLottery.SteamGameID) && SteamItem.Price >= Configs.MIN_ITEMS_PRICE)
                            {
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
                }

                // Chips
                if (Chips.Count > 0)
                {
                    XLotteryBet.ChipAssertIDs = new ulong[24];
                    XLotteryBet.ChipIDs = new uint[24];

                    ushort array_index = 0;
                    foreach (Chip chip in Chips)
                    {
                        XChipUsersInventory x_chip;
                        Chip c_chip;
                        if (Helper.UserHelper.Table_ChipUsersInventory.SelectOne(data => data.UserID == UserID && data.AssertID == chip.AssertID, out x_chip) 
                            && Helper.ChipHelper.SelectByID(x_chip.ChipID, out c_chip) && Helper.UserHelper.IsUserHaveChip(chip.AssertID, UserID))
                        {
                            if (ItemsFromSteamInventory > 0)
                            {
                                ProcessingBet.Chips.Add(chip);
                            }
                            else
                            {
                                XLotteryBet.ChipAssertIDs[array_index] = x_chip.AssertID;
                                XLotteryBet.ChipIDs[array_index] = x_chip.ChipID;
                                array_index++;
                                XLotteryBet.ChipsNum = array_index;
                                price += c_chip.Cost;

                                // забираем предмет у пользователя
                                x_chip.Deleted = true;
                                Helper.UserHelper.Table_ChipUsersInventory.UpdateByID(x_chip, x_chip.ID);
                            }
                        }
                        else
                        {
                            return 3;
                        }
                    }
                }

                if (price >= Configs.MIN_ITEMS_PRICE)
                {
                    if (ItemsFromSteamInventory <= 0)
                    {
                        List<XLotteryBet> ltrbts;
                        bool ff_bets = this.TableBet.Select(data => data.LotteryID == LotteryID, out ltrbts);
                        //Logger.ConsoleLog(ff_bets, ConsoleColor.Yellow);

                        //ДОБАВЛЕНИЕ ВРЕМЕНИ
                        if (!ff_bets)
                        {
                            XLottery.EndTime = Helper.GetCurrentTime() + Configs.LOTTERY_GAME_TIME;
                        }
                        else
                        {
                            List<USteamItem> usi;
                            List<Chip> chs;
                            double oneproc = this.GetBank(LotteryID, out usi, out chs) / 100;
                            double proctime = price / oneproc;
                            double fl = Configs.LOTTERY_EXTRA_TIME / 100;
                            double main = proctime * fl;
                            XLottery.EndTime += (int) main;
                            if(XLottery.EndTime > Helper.GetCurrentTime() + Configs.LOTTERY_GAME_TIME)
                            {
                                XLottery.EndTime = Helper.GetCurrentTime() + Configs.LOTTERY_GAME_TIME;
                            }
                        }

                        XLotteryBet.FisrtToken = (ff_bets) ? ltrbts.Last().LastToken + 1 : 1;
                        XLotteryBet.LastToken = Convert.ToUInt32(Math.Round(XLotteryBet.FisrtToken + price / Configs.TOKEN_PRICE));

                        if(ProcessingBet.SteamItems.Count > 0)
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
                        this.TableBet.Insert(XLotteryBet);
                        this.Table.UpdateByID(XLottery, XLottery.ID);
                        return 1;
                    }
                    else
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
                                Thread.Sleep(1);
                            }

                            UTRequestSteamMain.message = "Protection code: " + ProtectionCode;
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


                            return 2;
                        }
                    }
                }
            }

            // 0: false, 1: true, 2: process, 3: нету шмотки
            return 0;
        }

        public double GetBank(uint LotteryID, out List<USteamItem> SteamItems, out List<Chip> Chips)
        {
            double price = 0d;
            XLottery lottery;
            SteamItems = new List<USteamItem>();
            Chips = new List<Chip>();

            if (this.Table.SelectByID(LotteryID, out lottery))
            {
                List<XLotteryBet> bets;
                if (this.TableBet.Select(data => data.LotteryID == lottery.ID, out bets))
                {
                    foreach (XLotteryBet bet in bets)
                    {
                        // steam items
                        for (uint i = 0; i < bet.SteamItemsNum; i++)
                        {
                            USteamItem Item;
                            if (Helper.SteamItemsHelper.SelectByID(bet.SteamItemIDs[i], lottery.SteamGameID, out Item))
                            {
                                price += Item.Price;
                                Item.AssertID = bet.ItemAssertIDs[i];
                                Item.SteamBotID = bet.SteamBotIDs[i];
                                SteamItems.Add(Item);
                            }
                        }

                        // chips
                        for (uint i = 0; i < bet.ChipsNum; i++)
                        {
                            Chip Chip;
                            if (Helper.ChipHelper.SelectByID(bet.ChipIDs[i], out Chip))
                            {
                                Chip NChip;
                                NChip = Chip.Clone();

                                price += Chip.Cost;

                                NChip.AssertID = bet.ChipAssertIDs[i];
                                Chips.Add(NChip);
                            }
                        }
                    }
                }
            }

            return price;
        }

        public List<Bet> GetBets(uint LotteryID)
        {
            List<Bet> bets = new List<Bet>();
            List<XLotteryBet> lotteryBets = new List<XLotteryBet>();

            if (this.TableBet.Select(data => data.LotteryID == LotteryID, out lotteryBets))
            {
                XLottery lottery = this.Table.SelectByID(LotteryID);

                foreach (XLotteryBet lotteryBet in lotteryBets)
                {
                    Bet bet = new Bet();
                    bet.ID = lotteryBet.ID;

                    for (uint i = 0; i < lotteryBet.SteamItemsNum; i++)
                    {
                        USteamItem Item;
                        if(Helper.SteamItemsHelper.SelectByID(i, lottery.SteamGameID, out Item))
                        {
                            Item.AssertID = lotteryBet.ItemAssertIDs[i];
                            bet.Items.Add(Item);
                        }
                    }

                    for (uint i = 0; i < lotteryBet.ChipsNum; i++)
                    {
                        Chip Chip;
                        if (Helper.ChipHelper.SelectByID(lotteryBet.ChipIDs[i], out Chip))
                        {
                            Chip NChip;
                            NChip = Chip.Clone();
                            NChip.AssertID = lotteryBet.ChipAssertIDs[i];
                            bet.Chips.Add(NChip);
                        }
                    }
                }
            }

            return bets;
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

        public void RemoveProcessingBet(ProcessingBet ProcessingBet)
        {
            for (int i = 0; i < LotteryHelper.ProcessingBets.Count; i++)
            {
                if (LotteryHelper.ProcessingBets[i] == ProcessingBet)
                {
                    LotteryHelper.ProcessingBets.Remove(ProcessingBet);
                    return;
                }
            }
        }

        public void CancelBet(ulong OfferID, ulong UserSteamID, ushort Status)
        {
            XSteamBotProcessItem XSteamBotProcessItem;
            if (Helper.SteamBotHelper.SelectByOfferID(OfferID, UserSteamID, out XSteamBotProcessItem))
            {
                XSteamBotProcessItem.Status = Status;
                XSteamBotProcessItem.StatusChangedTime = Helper.GetCurrentTime();
                Helper.SteamBotHelper.Table_Items.UpdateByID(XSteamBotProcessItem, XSteamBotProcessItem.ID);

                ProcessingBet ProcessingBet;
                if (this.SelectProcessingBet(XSteamBotProcessItem.UserID, XSteamBotProcessItem.SteamGameID, out ProcessingBet))
                {
                    this.RemoveProcessingBet(ProcessingBet);
                }
            }
        }
    }
}
