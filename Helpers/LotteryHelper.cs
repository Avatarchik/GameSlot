using GameSlot.Database;
using GameSlot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;
using XData;

namespace GameSlot.Helpers
{
    public class LotteryHelper
    {
        public XTable<XLottery> Table = new XTable<XLottery>();
        public XTable<XLotteryBet> TableBet = new XTable<XLotteryBet>();

        public LotteryHelper()
        {

        }

        public void CreateNew(uint SteamGameID, out XLottery lottery)
        {
            if (this.Table.SelectOne(data => data.Winner == 0 && data.SteamGameID == SteamGameID, out lottery) == false)
            {
                XLottery new_lot = new XLottery();
                new_lot.SteamGameID = SteamGameID;
                new_lot.Winner = 0;
                new_lot.RaundNumber = new Random().NextDouble();
                //new_lot.EndTime = Helper.GetCurrentTime() + Configs.LOTTERY_GAME_TIME;
                new_lot.EndTime = -1;

                lottery = this.Table.SelectByID(this.Table.Insert(new_lot));

                Logger.ConsoleLog("Added new lottery!", ConsoleColor.Yellow);
                Logger.ConsoleLog("Lottery ID: [" + lottery.ID + "]");
                Logger.ConsoleLog("Steam game ID: [" + lottery.SteamGameID + "]");
                Logger.ConsoleLog("Raund number: [" + lottery.RaundNumber + "]");
                Logger.ConsoleLog("--------------------------------------------------\n");
            }

            lottery = new XLottery();
        }

        public void CreateNew(uint SteamGameID)
        {
            XLottery lottery;
            this.CreateNew(SteamGameID, out lottery);
        }

        public XLottery GetCurrent(uint SteamGameID)
        {
            XLottery lottery;
            this.Table.SelectOne(data => data.SteamGameID == SteamGameID && data.Winner <= 0, out lottery);
            return lottery;
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

                Lottery.LeftTime = (xlot.EndTime > 0) ? Lottery.LeftTime = this.CalcLeftTime(xlot.ID) : -1;
                
                Lottery.RaundNumber = xlot.RaundNumber;
                Lottery.Bets = this.GetBets(xlot.ID);
                return true;
            }

            Lottery = null;
            return false;
        }

        public bool SetBet(uint LotteryID, uint UserID, List<USteamItem> SteamItems, List<Chip> Chips)
        {
            XLottery XLottery;
            if(Helper.UserHelper.UserExist(UserID) && this.Table.SelectByID(LotteryID, out XLottery) && SteamItems.Count + Chips.Count <= 24)
            {
                XLotteryBet XLotteryBet = new XLotteryBet();
                double price = 0d;

                // Steam items
                if(SteamItems.Count > 0)
                {
                    XLotteryBet.ItemAssertIDs = new ulong[24];
                    XLotteryBet.SteamItemIDs = new uint[24];

                    ushort array_index = 0;
                    foreach(USteamItem SteamItem in SteamItems)
                    {
                        XSteamItem xitem;
                        if(Helper.SteamItemsHelper.SelectByID(SteamItem.ID, XLottery.SteamGameID, out xitem))
                        {
                            // проверить, есть ли данный предмет у юзера
                            if (Helper.SteamItemsHelper.IsUserHaveItem(SteamItem.AssertID, UserID, XLottery.SteamGameID) && SteamItem.Price >= Configs.MIN_ITEMS_PRICE)
                            {
                                XLotteryBet.ItemAssertIDs[array_index] = SteamItem.AssertID;
                                XLotteryBet.SteamItemIDs[array_index] = SteamItem.ID;
                                array_index++;
                                XLotteryBet.SteamItemsNum = array_index;
                                price += SteamItem.Price;
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
                            && Helper.ChipHelper.SelectByID(x_chip.ChipID, out c_chip) && Helper.ChipHelper.IsUserHaveItem(chip.AssertID, UserID))
                        {
                            XLotteryBet.ChipAssertIDs[array_index] = chip.AssertID;
                            XLotteryBet.ChipIDs[array_index] = c_chip.ID;
                            array_index++;
                            XLotteryBet.ChipsNum = array_index;
                            price += c_chip.Cost;
                        }
                    }
                }

                if(price >= Configs.MIN_ITEMS_PRICE)
                {
                    // TODO: обновить таймер лотереи и добавить билеты  && data.Transmitted
                    this.TableBet.Insert(XLotteryBet);
                    return true;
                }
            }

            return false;
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
                if (this.TableBet.Select(data => data.LotteryID == LotteryID && data.Transmitted, out bets))
                {
                    foreach (XLotteryBet bet in bets)
                    {
                        // steam items
                        for (uint i = 0; i < bet.SteamItemsNum; i++)
                        {
                            USteamItem Item;
                            if (Helper.SteamItemsHelper.SelectByID(bet.SteamItemIDs[i], lottery.ID, out Item))
                            {
                                price += Item.Price;
                                Item.AssertID = bet.ItemAssertIDs[i];
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

            if (this.TableBet.Select(data => data.LotteryID == LotteryID && data.Transmitted, out lotteryBets))
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
    }
}
