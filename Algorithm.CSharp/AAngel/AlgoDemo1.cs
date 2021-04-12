using System;
using QuantConnect.Data;
using QuantConnect.Indicators;

namespace QuantConnect.Algorithm.CSharp.AAngel
{
    public class AlgoDemo1 : SubAlgo
    {
        private MovingAverageConvergenceDivergence _macd;
        private Symbol _symbol;
        private DateTime _previous;
        private Resolution _resolution;
        private string _ticker;

        public AlgoDemo1(QCAlgorithm algorithm, string ticker,  Resolution resolution)
            : base(algorithm)
        {
            _resolution = resolution;
            _ticker = ticker;
        }

        public override void Initialize()
        {
            _symbol = AddForex(_ticker, Resolution.Minute).Symbol;
            _macd = Algo.MACD(_symbol, 12, 26, 9, MovingAverageType.Exponential,_resolution);
        }

        protected override string GetName()
        {
            return $"SubAlgoTest1_{_resolution}";
        }

        public override void OnData(Slice data)
        {
            /*
            if (!Portfolio.Invested)
            {
                if (data.QuoteBars.ContainsKey("EURUSD"))
                    SetHoldings("EURUSD", 0.5m);
            }
            else if (Positions["EURUSD"].Quantity > 35000)
            {
                SetHoldings("EURUSD", 0.25m);
            }
            */
            // only once per day
            if (_previous.Date == Time.Date) return;

            if (!_macd.IsReady) return;

            var holding = Portfolio[_symbol];

            var signalDeltaPercent = (_macd - _macd.Signal)/_macd.Fast;
            var tolerance = 0.0m;

            // if our macd is greater than our signal, then let's go long
            if (holding.Quantity <= 0 && signalDeltaPercent > tolerance) // 0.01%
            {
                // longterm says buy as well
                SetHoldings(_symbol, 1.0m);
            }
            // of our macd is less than our signal, then let's go short
            else if (holding.Quantity >= 0 && signalDeltaPercent < -tolerance)
            {
                //Liquidate(_symbol);
                SetHoldings(_symbol, -1.0m);
            }

            // plot both lines
            //Plot("MACD", _macd, _macd.Signal);
            //Plot(_symbol, "Open", data[_symbol].Open);
            //Plot(_symbol, _macd.Fast, _macd.Slow);

            _previous = Time;
        }
    }
}
