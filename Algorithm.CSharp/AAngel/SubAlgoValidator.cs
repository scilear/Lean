using System;
using System.Collections.Generic;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;

namespace QuantConnect.Algorithm.CSharp.AAngel
{
    public class SubAlgoValidator
    {
        DateTime _lastUpdate;
        TimeSpan TimeSpan;
        public SubAlgo Algo { get; private set; }
        private Dictionary<string, IIndicator>  _indicators;
        
        public SubAlgoValidator(SubAlgo algo, TimeSpan timeSpan, 
            Dictionary<string, IIndicator> indicators, 
            Func<SubAlgoValidator, decimal> confidenceFunction)
        {
            Algo = algo;
            TimeSpan = timeSpan;
            _indicators = indicators;
            _confidenceFunction = confidenceFunction;
        }

        public void Update()
        {
            if (_lastUpdate == null || Algo.Time >= _lastUpdate.Add(TimeSpan))
            {
                foreach (var indic in _indicators)
                {
                    var pnl = Algo.Portfolio.TotalPortfolioValue;
                    //var tb = new TradeBar(Algo.Time, Symbol.Empty, pnl, pnl, pnl, pnl, 0);
                    var idp = new IndicatorDataPoint(Algo.Time, pnl);
                    indic.Value.Update(idp);
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

        public decimal GetIndicator(string indicatorName)
        {
            return _indicators[indicatorName].Current.Value;
        }
    }
}
