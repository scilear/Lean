using System;
using System.Collections.Generic;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;

namespace QuantConnect.Algorithm.CSharp.AAngel
{

/*
 * 
 * 
 The main difficulty comes from matching trades from a virtual strategy to trades in the real strategy. 
 Because ratio might matter, because allcoation fo cash might matter.
 Therefore we need to know which positions we are working on.
 As a result, our DummyPortfolio creates Positions from trade with a unique position id, across all strategies, held as a static variable in DummyPortfolio.

The real Portfolio keeps track of the Position Id that it traded, and when a positions changes comes in, it comes in with a 
PositionId    
a Type: open, increase, decrease, close
and a weight

when we get initially an open that is acted on in  the real portfolio
=> add to PositionTracked dict with the following information, realQuantity, virtualQuantity
=> when update comes the ratio virtualQuantity/real ives us a multiplier on the new quantity
=> the type allows us proepr rounding(ie a close will overrite it to ensure we are no left with any remainder)

a position decrease or increase
we update positionTracked with new realQuantity and virtualQuantity

by default only new position will be affected by a change in allocation, however on a change in overall alloction we may decide to cut existing position 
and reduce the quantities in PositionTracked or similarly increase existing position (though depending on strategy this may be a more risky proposition)


 * 
 * 
 * 
 * */


    public partial class PortfolioMixer : QCAlgorithm
    {
        protected List<SubAlgoValidator> algos = new List<SubAlgoValidator>();
        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary> 
        /// 

        private Dictionary<Symbol, decimal> prevConfidence = new Dictionary<Symbol, decimal>();
        
        private IStrategyMixer _strategyMixer = new DefaultStrategyMixer();
        
/*
	public override void Initialize()
        {
            
            // TODO: move all that in a Configuration static class, that calls set methods on Mixer 
            SetStartDate(2020, 3, 1);  //Set Start Date
            SetEndDate(2021, 3, 15);    //Set End Date
            int CASH = 1000000;

            SetCash(CASH);
            //SetWarmup(TimeSpan.FromDays(180));
            SetWarmup(TimeSpan.FromDays(30*12));

            SubAlgo algo = new AllWeatherAlgo(this);
            algos.Add(new SubAlgoValidator(algo, 
                TimeSpan.FromDays(1), 
                new Dictionary<string, IIndicator>()
                { {"SMA2", new ExponentialMovingAverage(10)},
                    {"SMA10", new ExponentialMovingAverage(100)}},
                (a) =>
                {
                   // return 1;
                    if (a.GetIndicator("SMA2") >= a.GetIndicator("SMA10"))
                        return 1m;
                    return 0;
                }));
            
            
            algo = new TMFUPROVarianceOptimisedAlgo(this);
            algos.Add(new SubAlgoValidator(new TMFUPROVarianceOptimisedAlgo(this), 
                TimeSpan.FromDays(1), 
                new Dictionary<string, IIndicator>()
                { {"SMA2", new ExponentialMovingAverage(4)},
                    {"SMA10", new ExponentialMovingAverage(200)}},
                (a) =>
                {
                    //return 1;
                    if (a.GetIndicator("SMA2") >= a.GetIndicator("SMA10"))
                        return 1m;
                    return 0;
                }));
                        
            foreach (var a in algos)
            {
                a.Algo.SetCash(CASH);
                a.Algo.Initialize();
                prevConfidence[a.Algo.Name] = 0;
            }
        }
*/        

        
        public void OnData(Slice data)
        {
            _strategyMixer.Update(this);
            foreach (var a in algos)
            {
                a.Update();
                a.Algo.OnDataWrapper(data);
                if (data.ContainsKey(a.Algo.Name))
                {
                    Console.WriteLine(data[a.Algo.Name].PnL);
                }
                
                var strategyWeight = _strategyMixer.GetWeight(a.Algo.Name); 
                
                foreach(var kv in a.Algo.Portfolio.Positions)
                {
                    //ratio during warmup is 0 because we want to trigger a weight increase on day 1
                    var ratio = (IsWarmingUp) ? 0 : a.GetConfidence();
                    var prevRatio = prevConfidence[a.Algo.Name];
                    
                    if (ratio != prevRatio)
                        a.Algo.OnStrategyWeightChange(prevRatio, ratio);
                    List<PositionChanges> trades = kv.Value.Changes;
                    foreach(var t in trades)
                    {
                    	// TODO : rounding issues with close position waiting to happen!!!
                    	// PositionId reconcliation in OnOrder seems to be 
                    	// the most efficient/safest way to deal with this
                        if (t.Type == PositionChanges.ChangeType.Close)
                        {
                            //TODO there is a need for reconciliation:
                            // to make this robust RealQuantity should come from a OnOrder Fill event ...
                            var closingQuantity = -kv.Value.RealQuantity;
                            if (closingQuantity != 0)
                            {
                            	MarketOrder(kv.Key, closingQuantity, tag:t.Type.ToString());
                            }
                            kv.Value.Replicate(0, closingQuantity);
                        }
                        else if (kv.Value.Replicated || t.Type == PositionChanges.ChangeType.Open)
                        {
                            //we probably need to keep the inital ratio here for increase and decrease
                            var changeRatio = (t.Type == PositionChanges.ChangeType.Decrease) ? 1 : ratio;//in case of decrease we don't want it to be ignored
                            
                            // scaling is required because the underlying non filtered strategy (or no fees/slippage) may become much bigger than the real strategy,
                            // so each new position requires a proper scaling
                            var scaling = (t.Type == PositionChanges.ChangeType.Open) ? Portfolio.TotalPortfolioValue / a.Algo.Portfolio.TotalPortfolioValue:
                            				(a.Algo.Portfolio[kv.Key].RealQuantity != 0) ? 	a.Algo.Portfolio[kv.Key].RealQuantity / a.Algo.Portfolio[kv.Key].Quantity : 0
                            				;
                            
                            
                            
                            var quantity = t.GetAddedQuantity() * strategyWeight * changeRatio * scaling;
                            
                            var weight = strategyWeight;// TODO not sure what to do with that, does not look to be useful yet (only on cash reallocation maybe
                            if (quantity != 0)// && (kv.Value.RealQuantity != 0 || t.Type == PositionChanges.ChangeType.Open))
                            {
                                MarketOrder(kv.Key, quantity, tag:t.Type.ToString());
                                kv.Value.Replicate(weight, quantity);    
                            }
                        }
                    }
                    kv.Value.DoneReplicating();
                    prevConfidence[a.Algo.Name] = (IsWarmingUp) ? 0 : a.GetConfidence();
                }
                //now we do a comparison of the strategy overall value versus the target weight
            }
        }

        public override void OnEndOfDay()
        {
            foreach (var a in algos)
            {
            	a.Plot();
                // var test = a.Algo.Portfolio.TotalPortfolioValue;
                // Plot(a.Algo.Name, "pnl", a.Algo.Portfolio.TotalPortfolioValue);
                // Plot(a.Algo.Name, "sma", a.GetIndicator("SMA2"));
                // Plot(a.Algo.Name, "sma10", a.GetIndicator("SMA10"));
                //Plot(a.Algo.Name, "PF", Portfolio.TotalPortfolioValue);
                // var t1 = a.Algo.Test1();
                // var t2 = ((TMFUPROVarianceOptimisedAlgo)a.Algo).Test1();
                a.Algo.OnEndOfDay();
                Plot("ConfidenceLevel", a.Algo.Name, a.GetConfidence());
            }
            base.OnEndOfDay();
        }

        public override void OnEndOfAlgorithm()
        {
            base.OnEndOfAlgorithm();
            foreach (var a in algos)
            {
                a.Algo.OnEndOfAlgorithm();
            }
        }

        public override void OnWarmupFinished()
        {
            base.OnWarmupFinished();
            foreach (var a in algos)
            {
                a.Algo.OnWarmupFinished();
            }
        }

        public void OnData(Dividends data) // update this to Dividends dictionary
        {
            foreach (var a in algos)
            {
                a.Algo.Portfolio.ProcessDividends(data);
                a.Algo.OnData(data);
            }
        }

        public void OnData(Splits data)
        {
            foreach (var a in algos)
            {
                a.Algo.Portfolio.ProcessSplits(data);
                a.Algo.OnData(data);
            }
        }
    }

    internal class DefaultStrategyMixer : IStrategyMixer
    {
        private readonly decimal _weight;

        public DefaultStrategyMixer(decimal weight = 1)
        {
            _weight = weight;
        }
        
        public decimal GetWeight(string strategy)
        {
            return _weight;
        }

        public void Update(QCAlgorithm algo)
        {
            //throw new NotImplementedException();
        }
    }
}
