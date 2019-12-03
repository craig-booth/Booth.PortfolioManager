﻿using System;
using System.Collections.Generic;
using System.Text;

using Booth.Common;

using Booth.PortfolioManager.Domain.Stocks;
using Booth.PortfolioManager.Domain.Portfolios;
using Booth.PortfolioManager.Domain.Transactions;
using Booth.PortfolioManager.Domain.Utils;

namespace Booth.PortfolioManager.Domain.CorporateActions
{

    public enum CorporateActionType { Dividend, CapitalReturn, Transformation, SplitConsolidation, Composite }
    public abstract class CorporateAction : ITransaction
    {
        public Guid Id { get; private set; }
        public Stock Stock { get; private set; }
        public Date Date { get; private set; }
        public CorporateActionType Type { get; private set; }
        public string Description { get; private set; }

        internal CorporateAction(Guid id, Stock stock, CorporateActionType type, Date actionDate, string description)
        {
            Id = id;
            Stock = stock;
            Type = type;
            Date = actionDate;
            Description = description;
        }

        public abstract IEnumerable<Transaction> GetTransactionList(Holding holding);
        public abstract bool HasBeenApplied(ITransactionCollection transactions);
    }
}
