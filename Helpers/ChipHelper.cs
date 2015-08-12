using GameSlot.Database;
using GameSlot.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpServer;

namespace GameSlot.Helpers
{
    public class ChipHelper
    {
        private static List<Chip> Chips = new List<Chip>();
        public ChipHelper()
        {
            this.CreateChip(5);
            this.CreateChip(10);
            this.CreateChip(25);
            this.CreateChip(50);
            this.CreateChip(100);
        }

        private void CreateChip(double cost)
        {
            Chip chip = new Chip();
            chip.ID = (Chips.Count > 0) ? ChipHelper.Chips.Last().ID + 1 : 0;
            chip.Cost = cost;
            chip.Image = "";

            ChipHelper.Chips.Add(chip);
        }

        public bool SelectByID(uint ID, out Chip chip)
        {
            foreach(Chip ch in ChipHelper.Chips)
            {
                if (ch.ID == ID)
                {
                    chip = ch;
                    return true;
                }
            }

            chip = null;
            return false;
        }

        public void AddChipToUser(uint ChipID, uint UserID)
        {
            Chip chip;
            if(Helper.UserHelper.UserExist(UserID) && this.SelectByID(ChipID, out chip))
            {
                XChipUsersInventory xchip = new XChipUsersInventory();
                xchip.ChipID = ChipID;
                xchip.UserID = UserID;
                xchip.AssertID = Convert.ToUInt64(new Random().Next() + UserID + new Random().Next());

                Helper.UserHelper.Table_ChipUsersInventory.Insert(xchip);
            }
        }
    }
}
