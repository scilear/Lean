using System;
using System.Collections.Generic;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;

namespace QuantConnect.Algorithm.CSharp.AAngel
{
    public class SubAlgoValidator
    {
        private DateTime _lastUpdate;
        readonly TimeSpan _timeSpan;
        public SubAlgo Algo { get; private set; }
        public Dictionary<string, IIndicator>  Indicators { get; private set; }
        private readonly Dictionary<string, decimal>  _previous;
        
        public SubAlgoValidator(SubAlgo algo, TimeSpan timeSpan, 
            Dictionary<string, IIndicator> indicators, 
            Func<SubAlgoValidator, decimal> confidenceFunction,
            bool keepPreviousValues = false)
        {
            Algo = algo;
            _timeSpan = timeSpan;
            Indicators = indicators;
            _confidenceFunction = confidenceFunction;
            if (keepPreviousValues)
            {
            	_previous = new Dictionary<string, decimal>();
            	foreach(var i in indicators)
            	{
            		_previous[i.Key] = i.Value.Current;
            	}
            }
        }

        public void Update()
        {
            if (_lastUpdate == null || Algo.Time >= _lastUpdate.Add(_timeSpan))
            {
                foreach (var indic in Indicators)
                {
                    var pnl = Algo.Portfolio.TotalPortfolioValue;
                    //var tb = new TradeBar(Algo.Time, Symbol.Empty, pnl, pnl, pnl, pnl, 0);
                    var idp = new IndicatorDataPoint(Algo.Time, pnl);
                    if (_previous != null)
                    {
                    	_previous[indic.Key] = indic.Value.Current;
                    }
                    indic.Value.Update(idp);
                    // if (pnl > 1010000 || indic.Value.Current != 0)
                    // {
                    // 	int stop = 1;
                    // }
                    //indic.Update(Algo.Time, );
                }

                _lastUpdate = Algo.Time;
            }
        }
        readonly Func<SubAlgoValidator, decimal> _confidenceFunction;
        public decimal GetConfidence()
        {
            return _confidenceFunction(this);
        }

        public decimal GetIndicatorValue(string indicatorName)
        {
            return Indicators[indicatorName].Current;
        }
        
        public decimal GetIndicatorPreviousValue(string indicatorName)
        {
            return _previous[indicatorName];
        }
        
        public void Plot()
        {
            // var test = a.Algo.Portfolio.TotalPortfolioValue;
            Algo.Plot($"{Algo.Name}_pnl", "PnL", Algo.Portfolio.TotalPortfolioValue);
            if (Indicators.ContainsKey("EIT")) Algo.Plot($"{Algo.Name}_pnl", "Lag", ((EhlerInstantaneousTrend)Indicators["EIT"]).Lag);
            if (Indicators.ContainsKey("EIT")) Algo.Plot($"{Algo.Name}_pnl", "Eit", ((EhlerInstantaneousTrend)Indicators["EIT"]).InstantaneousTrend);
            if (Indicators.ContainsKey("SMA10")) Algo.Plot($"{Algo.Name}_pnl", "SMA10", ((ExponentialMovingAverage)Indicators["SMA10"]));
            if (Indicators.ContainsKey("BB")) Algo.Plot($"{Algo.Name}_pnl", "BB+", ((BollingerBands)Indicators["BB"]).UpperBand);
            if (Indicators.ContainsKey("BB")) Algo.Plot($"{Algo.Name}_pnl", "BB-", ((BollingerBands)Indicators["BB"]).LowerBand);
            if (Indicators.ContainsKey("RBB")) Algo.Plot($"{Algo.Name}_pnl", "RBB+", ((RollingBB)Indicators["RBB"]).UpperBand);
            if (Indicators.ContainsKey("RBB")) Algo.Plot($"{Algo.Name}_pnl", "RBB-", ((RollingBB)Indicators["RBB"]).LowerBand);
            // Plot(a.Algo.Name, "sma", a.GetIndicator("SMA2"));
            // Plot(a.Algo.Name, "sma10", a.GetIndicator("SMA10"));
            // foreach (var indic in Indicators)
            // {
            // 	Algo.Plot((string)Algo.Name, (string)indic.Value.Name, (decimal)indic.Value.Current.Value);
            // }
        }
    }
}
