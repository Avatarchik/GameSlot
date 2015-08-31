using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSlot.Pages.Includes
{
    public class Payment : SiteGameSlot
    {
        public override string TemplateAddr
        {
            get { return "Includes.Payment.html"; }
        }
    }
}
