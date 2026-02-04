using System;
using System.Collections.Generic;
using System.Text;


namespace Booth.PortfolioManager.Web.Models.Portfolio
{
    public class ChangeDrpParticipationCommand
    {
        public Guid Holding { get; set; }
        public bool Participate { get; set; }
    }
}
