using System;
using System.Collections.Generic;
using System.Text;

namespace Booth.PortfolioManager.Domain
{
    public interface IEntity 
    {
        Guid Id { get; }
    }

}
