using System;

using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain
{

    public class EffectiveDateException : Exception
    {
        public EffectiveDateException(string message) : base(message) { }
    }

    public class NoSharesOwned : Exception
    {
        public NoSharesOwned(string message) : base(message) { }
    }

    public class NotEnoughSharesForDisposal : Exception
    {
        public NotEnoughSharesForDisposal(string message) : base(message) { }
    }

    public class StockNotActive : Exception
    {
        public StockNotActive(string message) : base(message) { }
    }

    public class StockNotFound : Exception
    {
        public StockNotFound(string message) : base(message) { }
    }

}
