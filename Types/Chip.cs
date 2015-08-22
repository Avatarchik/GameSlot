using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Types
{
    public class Chip
    {
        public uint ID;
        public double Cost;
        public string Cost_Str;

        public string Image;

        public ulong AssertID;

        public Chip Clone()
        {
            return (Chip)this.MemberwiseClone();
        }
    }
}
