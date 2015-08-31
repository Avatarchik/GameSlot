using GameSlot.Database;
using GameSlot.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpServer;
using XData;

namespace GameSlot.Helpers
{
    public class ChipHelper
    {
        public XTable<XChipCustomer> Table = new XTable<XChipCustomer>();
        private static List<Chip> Chips = new List<Chip>();

        public ChipHelper()
        {
            this.CreateChip(5);
            this.CreateChip(10);
            this.CreateChip(25);
            this.CreateChip(50);
            this.CreateChip(100);
            this.CreateChip(0.5);
        }

        private void CreateChip(double cost)
        {
            Chip chip = new Chip();
            chip.ID = (Chips.Count > 0) ? ChipHelper.Chips.Last().ID + 1 : 0;
            chip.Cost = cost;
            chip.Cost_Str = cost.ToString("###,##0.00");

            string path = "FileStorage\\Upload\\Chips\\" + chip.ID + ".jpg";
            if (File.Exists(path))
            {
                chip.Image = FilesProcessor.CacheFile(File.ReadAllBytes(path), ".jpg");
            }

            ChipHelper.Chips.Add(chip);
        }

        public bool SelectByID(uint ID, out Chip chip)
        {
            for (int i = 0; i < ChipHelper.Chips.Count; i++)
            {
                if (ChipHelper.Chips[i].ID == ID)
                {
                    chip = ChipHelper.Chips[i];
                    return true;
                }
            }
            

            chip = new Chip();
            return true;
        }

        public void AddChipToUser(uint ChipID, uint UserID)
        {
            Chip chip;
            if(Helper.UserHelper.UserExist(UserID) && this.SelectByID(ChipID, out chip))
            {
                XChipUsersInventory xchip = new XChipUsersInventory();
                XChipCustomer XChipCustomer = new XChipCustomer();

                XChipCustomer.ChipID = xchip.ChipID = ChipID;
                XChipCustomer.UserID = xchip.UserID = UserID;
                xchip.AssertID = this.Table.Insert(XChipCustomer);

                Helper.UserHelper.Table_ChipUsersInventory.Insert(xchip);
            }
        }
    }
}
