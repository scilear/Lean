using System;
using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Securities;





namespace QuantConnect.Algorithm.CSharp.AAngel
{
    public class DummyPortfolio 
    {
        public bool Invested { get { return CashAccount != TotalPortfolioValue; } }
        public decimal StartingCash { get; internal set; }
        public decimal CashAccount { get; internal set; }
        public decimal TotalPortfolioValue { get; private set; }

        private SecurityManager Securities;
        public Dictionary<Symbol, Position> Positions { get; private set; }

        public DummyPortfolio(decimal startingCash, SecurityManager securities)
        {
            Positions = new Dictionary<Symbol, Position>();
            SetCash(startingCash);

            Securities = securities;
        }
        public Position this[Symbol symbol]
        {
            get
            {
                Position p;
                return Positions.TryGetValue(symbol, out p) ? p : new Position(symbol);
            }
            //set { InnerList[i] = value; }
        }
        
        
        public void Trade(Symbol symbol, decimal qty, decimal tradePrice)
        {
            if (!Positions.ContainsKey(symbol))
            {
                Positions[symbol] = new Position(symbol);
            }

            var cashImpact = Positions[symbol].AddTrade(qty, tradePrice);
            CashAccount -= cashImpact;
        }

        public decimal Percent(SubAlgo subAlgo, Symbol symbol, decimal target, decimal price)
        {
            if (!Positions.ContainsKey(symbol))
            {
                Positions[symbol] = new Position(symbol);
            }

            var actual = Positions[symbol].GetMarketValue() / TotalPortfolioValue;
            var toExecute = target - actual;

            // TODO: deal multiplicator for futures, options,  etc...
            // TODO: add bid ask & fees
            var qty = (price > 0) ? TotalPortfolioValue * toExecute / price : 0;
            return qty;
        }


        public void OnData(Slice data)
        {
            var portfolioValue = 0m;
            //update portoflio value
            foreach(var p in Positions)
            {
                if (data.Bars.ContainsKey(p.Key))
                {
                    portfolioValue += p.Value.UpdatePrice(data.Bars[p.Key].Close);
                }
                else if (data.QuoteBars.ContainsKey(p.Key))
                {
                    portfolioValue += p.Value.UpdatePrice(data.QuoteBars[p.Key].Close);
                }
                else
                {
                    portfolioValue += p.Value.GetMarketValue();
                }
            }
            TotalPortfolioValue = portfolioValue + CashAccount;
        }
        
        public void SetCash(decimal startingCash)
        {
            StartingCash = startingCash;
            CashAccount = StartingCash;
            TotalPortfolioValue = StartingCash;
        }

        internal void ProcessDividends(Dividends data)
        {
            foreach (var dividend in data)
            {
                if (Securities[dividend.Key].DataNormalizationMode == DataNormalizationMode.Raw ||
                    Securities[dividend.Key].DataNormalizationMode == DataNormalizationMode.SplitAdjusted ||
                    Securities[dividend.Key].DataNormalizationMode == DataNormalizationMode.TotalReturn)
                {
                    throw new NotImplementedException();        
                }
                else
                {
                    //ignore nothing to do dividends are included in the price
                }
            }
        }

        internal void ProcessSplits(Splits data)
        {
            foreach (var dividend in data)
            {
                if (Securities[dividend.Key].DataNormalizationMode == DataNormalizationMode.Raw )
                {
                    throw new NotImplementedException();        
                }
                else
                {
                    //ignore nothing to do dividends are included in the price
                }
            }
        }

        public void LiquidateReplica()
        {
            foreach (var position in Positions)
            {
                position.Value.GenerateReplicaLiquidationChanges();
            }
        }
        
        public void RestartReplication()
        {
        	foreach (var position in Positions)
            {
                position.Value.RestartReplication();
            }
        }

        public bool ContainsKey(string symbol)
        {
            return Positions.ContainsKey(symbol);
        }
    }
}
