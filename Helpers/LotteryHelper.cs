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

        public static uint LastToken = 0;
        public static int GameTime = 180;

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
                new_lot.EndTime = Helper.GetCurrentTime() + GameTime;

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

        public double GetBank(uint LotteryID)
        {
            double price = 0d;
            XLottery lottery;
            if (this.Table.SelectByID(LotteryID, out lottery))
            {
                List<XLotteryBet> bets;
                this.TableBet.Select(data => data.LotteryID == LotteryID, out bets);
                foreach (XLotteryBet bet in bets)
                {
                    for (uint i = 0; i < bet.Prices.Length; i++)
                    {
                        price += bet.Prices[i];
                    }
                }
            }
            return price;
        }

        public List<Bet> GetBets(uint LotteryID)
        {
            List<Bet> bets = new List<Bet>();
            List<XLotteryBet> lotteryBets;
            if (this.TableBet.Select(data => data.LotteryID == LotteryID, out lotteryBets))
            {
                XLottery lottery = this.Table.SelectByID(LotteryID);

                foreach (XLotteryBet lotteryBet in lotteryBets)
                {
                    Bet bet = new Bet();
                    bet.ID = lotteryBet.ID;

                    for (uint i = 0; i < lotteryBet.SteamItemIDs.Length; i++)
                    {
                        SteamItem item = new SteamItem();
                        XSteamItem XItem;
                        Helper.SteamItemsHelper.SelectByID(i, lottery.SteamGameID, out XItem);
                        item.Name = XItem.Name;
                        //item.Rarity = ItemsShema.Rarity;


                        item.Price = lotteryBet.Prices[i];
                        bet.Items.Add(item);
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
