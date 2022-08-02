using System;

using Booth.PortfolioManager.Domain.Transactions;

namespace Booth.PortfolioManager.Domain
{

    public class EffectiveDateException : Exception
    {
        public EffectiveDateException(string message) : base(message) { }
    }

    public class NoSharesOwnedException : Exception
    {
        public NoSharesOwnedException(string message) : base(message) { }
    }

    public class NotEnoughSharesForDisposal : Exception
    {
        public NotEnoughSharesForDisposal(string message) : base(message) { }
    }

    public class StockNotActiveException : Exception
    {
        public StockNotActiveException(string message) : base(message) { }
    }

    public class StockNotFoundException : Exception
    {
        public StockNotFoundException(string message) : base(message) { }
    }

    public class StockChangedException : Exception
    {
        public StockChangedException(string message) : base(message) { }
    }

}
