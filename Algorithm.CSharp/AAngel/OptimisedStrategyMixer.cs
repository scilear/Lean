using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Indicators;

namespace QuantConnect.Algorithm.CSharp.AAngel
{
    public class OptimisedStrategyMixer : IStrategyMixer
    {
        private readonly List<SubAlgoValidator> _algos;
        private readonly Dictionary<string, RollingWindow<decimal>> _dailyReturns = new Dictionary<string, RollingWindow<decimal>>();
        private readonly Dictionary<string, decimal> _previousPnl = new Dictionary<string, decimal>();
        private readonly Dictionary<string, decimal> _weights = new Dictionary<string, decimal>();
        private readonly int _subset;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="algos">list of all algos to be compared</param>
        /// <param name="lookbackPeriod">how far back to we go for relative inspection</param>
        /// <param name="subset">default 0: all algos, else only the top 'subset' algos</param>
        OptimisedStrategyMixer(List<SubAlgoValidator> algos, int lookbackPeriod, int subset = 0)
        {
            _algos = algos;
            foreach (var a in _algos)
            {
                _dailyReturns[a.Algo.Name] = new RollingWindow<decimal>(lookbackPeriod);
                _previousPnl[a.Algo.Name] = a.Algo.Portfolio.StartingCash;
                _weights[a.Algo.Name] = 0;
            }

            _subset = subset;
        }
        public decimal GetWeight(string strategy)
        {
            var weight = 0m;
            if (_weights.TryGetValue(strategy, out weight))
                return weight;
            return 0;
        }

        private int _previousDay = 0;

        public void Update(QCAlgorithm algo)
        {
            if (algo.Time.Day == _previousDay)
            {
                return;
            }
            
            foreach (var a in _algos)
            {
                var perf = a.Algo.Portfolio.TotalPortfolioValue / _previousPnl[a.Algo.Name];
                _previousPnl[a.Algo.Name] = a.Algo.Portfolio.TotalPortfolioValue;
                _dailyReturns[a.Algo.Name].Add(perf);
            }

            _previousDay = algo.Time.Day;
            
            
            //then we compute new weights valid for the period/day
            //we compute relative weights 2 by 2 and then take the average

            var pfeScores = new Dictionary<string, decimal>();
            
            var processed = new HashSet<string>();
            foreach (var a1 in _algos)
            {
                var name1 = a1.Algo.Name;
                pfeScores[name1] = 0;
                foreach (var a2 in _algos)
                {
                    var name2 = a2.Algo.Name;
                    if (name1 == name2 || processed.Contains(name2))
                    {
                        continue;
                    }
                    
                    //computes pfe for a1/a2
                    
                    //sanity check
                    var ret1 = _dailyReturns[name1];
                    var ret2 = _dailyReturns[name2];
                    if (ret2 != null && ret1 != null && (!ret1.IsReady || !ret2.IsReady || ret1.Count != ret2.Count))
                    {
                        throw new Exception("why are we here?");
                        continue;
                    }
                    var pfe = new PolarisedFractalEfficiency();
                    DateTime dt = DateTime.Now;
                    var p1 = 1m;
                    var p2 = 1m;
                    pfe.Update(new IndicatorDataPoint(dt, p1 / p2));
                    for (int i = 0; i < ret1.Count; i++)
                    {
                        p1 *= ret1.ElementAt(0);
                        p2 *= ret2.ElementAt(0);
                        pfe.Update(new IndicatorDataPoint(dt.AddDays(i+1), p1 / p2));
                    }

                    pfeScores[name1] += pfe+100;
                    pfeScores[name2] += -pfe+100;
                }
                processed.Add(name1);
                _weights[name1] = Math.Max(pfeScores[name1], 0);
            }
            
            //now rescale the weights
            
            // note than when there is a subset if there are ties, they will all be included
            // but scaled properly in the weights so we should not trigger leverage breach from here
            var scalingFactor = (_subset == 0) ? _weights.Values.Sum()
                    : _weights.Values.GroupBy(x => x)
                            .OrderByDescending(x => x)
                            .Take(_subset)
                            .SelectMany(x => x)
                            .Sum();;
            var threshold = -100m;
            if (_subset != 0) // then we set the threshold to be @ the subset level
            {
                threshold = _weights.Values.GroupBy(x => x)
                    .OrderByDescending(x => x)
                    .Skip(_subset)
                    .Take(1)
                    .SelectMany(x => x).Average();
            }
            foreach (var key in _weights.Keys.ToList())
            {
                var w = _weights[key];
                if (w > threshold)
                {
                    _weights[key] = w / scalingFactor;
                }
                else
                {
                    _weights[key] = 0;
                }
            }
        }
    }
}
