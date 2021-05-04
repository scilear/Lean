using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Data;
using QuantConnect.Indicators;

namespace QuantConnect.Algorithm.CSharp.AAngel
{
    public class EtfMaRotation : QCAlgorithm
    {
        HashSet<string> etfs = new HashSet<string>{"XLB", "XLC","XLE", "XLF", "XLI", "XLK", "XLP", "XLU", "XLV", "XLY", 
        };
		
        HashSet<string> mm = new HashSet<string>{"TLT", "SHY"
        };

        private Dictionary<string, SimpleMovingAverage> mas = new Dictionary<string, SimpleMovingAverage>(); 
        public override void Initialize()
        {
            SetStartDate(2020, 11, 2);  //Set Start Date
            SetCash(100000);             //Set Strategy Cash
            SetWarmup(TimeSpan.FromDays(365));
            var resolution = Resolution.Daily;
            foreach (var etf in etfs)
            {
                AddEquity(etf, resolution);
                mas[etf] = new SimpleMovingAverage(12);
            }

            foreach (var etf in mm)
            {
                AddEquity(etf, resolution);
            }

            Schedule.On(DateRules.MonthStart(5), TimeRules.AfterMarketOpen(mm.ElementAt(0), 15),
                () => rebalance = true);
        }

        private bool rebalance = false;
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// Slice object keyed by symbol containing the stock data
        public override void OnData(Slice data)
        {
            // if (!Portfolio.Invested)
            // {
            //    SetHoldings("SPY", 1);
            //    Debug("Purchased Stock");
            //}
            if (rebalance)
            {
                OnRebalance(data);
                rebalance = false;
                
            }
        }

        private void OnRebalance(Slice data)
        {
            var targets = new List<PortfolioTarget>();
            var mmWeight = 0m;
            var ratio = 1m / etfs.Count();
            foreach (var etf in etfs)
            {
                var price = data[etf].Close;
                mas[etf].Update(Time, price);
                double weight = 0;
                if (mas[etf].IsReady && price > mas[etf])
                {
                    targets.Add(new PortfolioTarget(etf, ratio));
                }
                else
                {
                    targets.Add(new PortfolioTarget(etf, 0));
                    mmWeight += ratio;
                }
            }
            targets.Add(new PortfolioTarget(mm.ElementAt(0), mmWeight));
            SetHoldings(targets);
        }
    }
}
