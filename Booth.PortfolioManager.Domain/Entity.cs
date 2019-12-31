using System;
using System.Collections.Generic;
using System.Linq;

using Booth.Common;

namespace Booth.PortfolioManager.Domain
{
    public interface IEntity
    {
        Guid Id { get; }
    }
}
