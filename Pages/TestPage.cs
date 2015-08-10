using GameSlot.Database;
using GameSlot.Helpers;
using GameSlot.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Pages
{
    public class TestPage : SiteGameSlot
    {
        public override string URL
        {
            get { return "/test"; }
        }
        public override string TemplateAddr
        {
            get { return "Lottery.html"; }
        }
        public override bool FilterBefore
        {
            get { return true; }
        }
        public override bool Init(Client client)
        {
            //Image ing = Image.FromStream(new MemoryStream());

            uint[] uu = new uint[24];
            uu[0] = 123;
            Helper.ChipHelper.AddChipToUser(4, 1);

            List<Chip> Chips = new List<Chip>();
            Chip chip = new Chip();
            chip.AssertID = 6311613731;
            Chips.Add(chip);

            List<USteamItem> usi = new List<USteamItem>();
            Logger.ConsoleLog(Helper.LotteryHelper.SetBet(0, 1, usi, Chips));

            client.HttpSend(Helper.LotteryHelper.GetBank(0, out usi, out Chips).ToString());
            return true;
        }
    }
}
