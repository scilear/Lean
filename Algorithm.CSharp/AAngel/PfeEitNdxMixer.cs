using System;
using System.Collections.Generic;
using QuantConnect.Indicators;

namespace QuantConnect.Algorithm.CSharp.AAngel
{


    public partial class PortfolioMixer : QCAlgorithm
    {


        public override void Initialize()
        {
            
            // TODO: move all that in a Configuration static class, that calls set methods on Mixer 
            SetStartDate(2010, 3, 1);  //Set Start Date
            SetEndDate(2021, 3, 15);    //Set End Date
            int CASH = 1000000;

            SetCash(1000000);
            //SetWarmup(TimeSpan.FromDays(180));
            SetWarmup(TimeSpan.FromDays(4*400));

            SubAlgo algo = new EitPfeNdxAlgo(this);
            algos.Add(new SubAlgoValidator(algo, 
                TimeSpan.FromDays(1), 
                new Dictionary<string, IIndicator>()
                { {"PFE", new PolarisedFractalEfficiency()},
                    {"SMA10", new ExponentialMovingAverage(100)}
                	
                },
                (a) =>
                {
                    //return 1;
                    //if (a.Algo.Portfolio.TotalPortfolioValue >= a.GetIndicator("SMA10"))
                    if (StrategyPfeActivator.Activate(a.Algo.Name, a.GetIndicator("PFE")))
                        return 1m;
                    return 0;
                }));
            strategyAllocation[algo.Name] = 1m;
            
            
            foreach (var a in algos)
            {
                a.Algo.SetCash(CASH);
                a.Algo.Initialize();
                prevConfidence[a.Algo.Name] = 0;
            }
        }
    }

    public static class StrategyPfeActivator
    {
        private static Dictionary<string, decimal> _values = new Dictionary<string, decimal>();
        private static Dictionary<string, decimal> _previousValues = new Dictionary<string, decimal>();
        private static HashSet<string> _activated = new HashSet<string>();
        public static bool Activate(string strategy, decimal indicator)
        {
            if (!_values.ContainsKey(strategy))
            {
                _values[strategy] = indicator;
                _previousValues[strategy] = indicator;
                if (indicator > 0)
                {
                    _activated.Add(strategy);
                    return true;
                }

                return false;
            }
            else
            {
                var cv = _values[strategy];
                var pv = _previousValues[strategy];
                if (cv > pv && cv > -50)
                {
                    _activated.Add(strategy);
                }
                else if (pv > 75 && cv < 75 || cv < -50)
                {
                    _activated.Remove(strategy);
                }
            }

            return _activated.Contains(strategy);
        }
    }
}
