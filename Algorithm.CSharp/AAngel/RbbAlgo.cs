using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Algorithm.CSharp.AAngel;
//using QuantConnect.Algorithm.CSharp.AAngel.Indicators;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;

/*
20201217 : protected te SetHoldings by !isWarmingUp to avoid stream of messages on deploy
20210207 : added bull bear graph
20210303 : added another bullish indicator

nned to add a state print to see the perf and state of TradeMonitor/EIT/pfe
*/

namespace QuantConnect.Algorithm.CSharp
{
	
	
    public class RbbAlgo : SubAlgo
    {

/*
	remove outliers in computing avg performance
	take perf over period of time instead of number of trades
*/
		Symbol Symbol;
		bool perfControl = false;
		public RbbAlgo(QCAlgorithm algo, Symbol symbol, bool perfControl_) : base(algo)
		{
			Symbol = symbol;
			perfControl = perfControl_;
			symbols.Add(symbol);
		}

		
		//Dictionary<string, PolarisedFractalEfficiency> _pfeReference = new Dictionary<string, PolarisedFractalEfficiency>();
		//Dictionary<string, PolarisedFractalEfficiency> pfe = new Dictionary<string, PolarisedFractalEfficiency>();
		//Dictionary<string, decimal> ppfe = new Dictionary<string, decimal>();
		Dictionary<string, RollingBB> RBB = new Dictionary<string, RollingBB>();
		
		HashSet<Symbol> symbols = new HashSet<Symbol>()
		{
		};
		
		
		protected override string GetName()
		{
			return $"RbbAlgo_{Symbol}";
		}

		public override void Initialize()
        {


            var res = TimeSpan.FromHours(1);
              
            foreach(var s in symbols)
            {
	            AddEquity(s, Resolution.Minute).SetLeverage(2);            
				
				// pfe[s] = new PolarisedFractalEfficiency();
				// Algo.RegisterIndicator(s, pfe[s], res);
				
				// ppfe[s] = 0;
				
				// eit[s] = new EhlerInstantaneousTrend();
				// Algo.RegisterIndicator(s, eit[s], res);
				RBB[s] = new RollingBB();
				Algo.RegisterIndicator(s, RBB[s], res);
				
				// pvalue[s] = 0;
				// activate[s] = false;
            }
        }

		protected override  void OnWeightIncrease(decimal oldWeight, decimal newWeight)
		{
			if (oldWeight != 0 || newWeight != 1 || IsWarmingUp) return;
			
			foreach(var kv in Portfolio.Positions)
			{
				if (kv.Value.Replicated 
				|| kv.Value.RealQuantity != 0
				|| kv.Value.Changes.Count() > 0) 
					continue;
					
				kv.Value.Changes.Add(new PositionChanges
                	{
                    PositionId = kv.Value.PositionId,
                    Type = PositionChanges.ChangeType.Open,
                    //Symbol = Symbol,
                    QuantityBefore = 0,
                    QuantityAfter = kv.Value.Quantity,
                	}
                );
			}
		}
		
		// Dictionary<string, decimal> pvalue = new Dictionary<string, decimal>();
  //      Dictionary<string, bool> activate = new Dictionary<string, bool>();
        
        public override void OnData(Slice data)
        {
			if (Time.Minute != 0) return;
        	
        	foreach(var s in symbols)
            {
            	if (!Securities.ContainsKey(s)) 
            		continue;
            	
            	
            	if (RBB[s].IsReady)
            	{
            		if ( Securities.ContainsKey(s) && data.Bars.ContainsKey(s))
            		{
			        	if (data[s].Close >= RBB[s].UpperBand && Portfolio[s].Quantity == 0)
			        	{
			        		SetHoldings(s, 1);
			        	}
			        	else if (data[s].Close < RBB[s].LowerBand && Portfolio[s].Quantity != 0)
			        	{
			        		SetHoldings(s, 0);
			        	}
            		}
            	}
            }
        }
    }
}