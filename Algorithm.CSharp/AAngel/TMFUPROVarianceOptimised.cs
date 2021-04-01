using System;
using QuantConnect.Algorithm.CSharp.AAngel;
using QuantConnect.Data;
using QuantConnect.Data.Consolidators;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;

namespace QuantConnect.Algorithm.CSharp
{
	public class TMFUPROVarianceOptimised : SubAlgo
    {
	    protected override string GetName()
	    {
		    return $"TMFUPROVarianceOptimised";
	    } 	
        public TMFUPROVarianceOptimised(QCAlgorithm algorithm)
            : base(algorithm)
        {
	        
        }
		
		
        
        public override void Initialize()
        {
            
	        
        }
	
		
        
		


        public override void OnData(Slice data)
        {
        	
	        
        }


        void PrintState()
	    {
	    	if (LiveMode)
	    	{
	    		Log($"*********** {GetName()} **************");
	    		//Log($"{Time} OnWeek5 ma ready {ma.IsReady}, Close>ma: {Securities[SYM].Price > ma}");
	    	}
	    }
    }
    /*
     
         public class TMFUPROVarianceOptimisedAlgo : SubAlgo
    {
    	protected string GetName()
    	{
    		return "MinVarAlgo";
    	}
    	// ##### ALGO PARAM ############
    	private int allocationFrequency = 15;
    	// ##### END ALGO PARAM ############
    	
    	
        private HashSet<string> stocks = new HashSet<string>(){"UPRO", "TMF"};
		private bool allocationTime = false;
        private int allocate = 0;
        private double[] weights;
        
        BollingerBands bb1;
        BollingerBands bb;
        BollingerBands bbc;
        
        
        public void Initialize()
        {
            //SetStartDate(2010, 3, 1);
            //SetEndDate(2020, 2, 15);
            
            // SetStartDate(2019, 1, 1);
            
            //SetCash(REF_CASH);

            SetWarmup(TimeSpan.FromDays(45));

            var ref_etf = "UPRO";
            // stocks.Add("TMF");
            // stocks.Add(ref_etf);
            foreach(var s in stocks)
            {
            	AddEquity(s, Resolution.Minute);
            }
            
			AddEquity("VXX", Resolution.Minute);
            bb = BB("VXX", 20, 2m, MovingAverageType.Exponential, Resolution.Daily);
            //bbc = BB("UPRO", 100, 2m, MovingAverageType.Exponential, Resolution.Daily, x => (decimal)GetCorrel());
			if (!LiveMode) AddEquity("VXX.1", Resolution.Minute);
            if (!LiveMode) bb1 = BB("VXX.1", 20, 2m, MovingAverageType.Exponential, Resolution.Daily);
            
            int c = stocks.Count;
            weights = new double[c];
            

            
            //Schedule.On(DateRules.Every(DayOfWeek.Tuesday), TimeRules.AfterMarketOpen(ref_etf, 60), () =>
            Schedule.On(DateRules.EveryDay(ref_etf), TimeRules.AfterMarketOpen(ref_etf, 60), () =>
            {
                allocationTime = true;
                Log("allocationTime = true");
            });
            
            // Schedule.On(DateRules.EveryDay(ref_etf), TimeRules.AfterMarketOpen(ref_etf, 60), () =>
            // {
            //     if (!canAllocate) reallocateCheck = true;
            // });

        }
        
        // bool reallocateCheck = false;

		bool canAllocate = true;
        public void OnData(Slice data)
        {
        	// if (Time > new DateTime(2018,1,18,0,0,0) && stocks.Contains("VXX.1") )
        	// {
        	// 	stocks.Remove("VXX.1");
        	// 	stocks.Add("VXX");
        	// }
        	
        	if (allocationTime)
        	{
        		Log("OnDataVarOptimised allocationTime");
    			if (canAllocate && (data.ContainsKey("VXX.1") && bb1.IsReady && data["VXX.1"].Close > bb1.UpperBand
	            	|| data.ContainsKey("VXX") && bb.IsReady && data["VXX"].Close > bb.UpperBand))
	            {
	            	Log("VXX Burst Liquidating");
	            	Liquidate("UPRO");
	            	Liquidate("TMF");
	            	canAllocate = false;
	            }
	            else if (canAllocate)
	            {
	            	Log($"canAllocate");
	            	Allocate(data);
	            }
	            else 
	            {
	            	Log($"canNOTAllocate");
	            	var spreadToRecovery1 = 0.25m*(bb1.UpperBand-bb1.MiddleBand);
	            	var spreadToRecovery = 0.25m*(bb.UpperBand-bb.MiddleBand);
	            	if (data.ContainsKey("VXX.1") && bb1.IsReady && data["VXX.1"].Close < bb1.UpperBand - spreadToRecovery1
	            		|| data.ContainsKey("VXX") && bb.IsReady && data["VXX"].Close < bb.UpperBand - spreadToRecovery)
		            {
		            	Log($"Recovery Threashold reached: Allocating");
		            	Allocate(data);
		            	canAllocate = true;
		            }
	            }
            	Plot("CAN_ALLOCATE", "CA", (canAllocate) ? 1 : 0);
	            allocationTime = false;
        	}
        }

        

        private void Allocate(Slice data)
        {
        	// I tend to convert everythign to double because a lot of the matrix existing manip/functions, 
        	// only work on doubles and precision should have very little impact on that 
            var localWeights = GetWeights();
            if (localWeights  != null) 
        	{
        		Log("Non zero weights. Trade!");
    			weights = localWeights;//GetWeights(noVxx : true);
    			Trade(data);
        	}
        	else
        	{
        		Log("Zero weights. NOTHING TO DO!");
            	// Liquidate("UPRO");
            	// Liquidate("TMF");
        	}
        }
		
		double [] GetWeights()
		{
			var priceData = new List<double[]>();
            
            int lookbackPeriod = allocationFrequency * 3;
			var resolution = Resolution.Daily;
            int dataCount = lookbackPeriod;
            
            //will hold returns.mean/return.std for all stocks
            var retNorm = new double[stocks.Count];
            //constructing price matrix stocks.count x lookbackPeriod
            var allHistoryBars = new double[stocks.Count(), lookbackPeriod+1];
            
            int ii = 0;
            foreach (var security in stocks)
            {
            	if (!Securities[security].IsTradable) 
            		continue;
                var history = History(security, lookbackPeriod, resolution);
                if (history.Count() == 0) 
                	continue;
                //the tmp array dupe the price for the current security because in matrix 
                // I have no way that I know of of accessign columns
                var tmp = new double[history.Count()+1];
                //fill the price matrix    
                int jj = 0;
                foreach(var h in history)
                {
                	tmp[jj] = (double)h.Close;
                	jj++;
                }
                
                //Add current bar to history
                var p = (double)history.LastOrDefault().Value;
                // if (data.Bars.Keys.Contains(security))//just in case, it happened
                // {
                // 	p = (double)data.Bars[security].Close;
                // }
                allHistoryBars[ii, jj] = p;
                tmp[jj] = p;
                var pctChanges = tmp.PctChange();
                for (int j = 0; j < pctChanges.Length; j++)
            	{
            		allHistoryBars[ii, j] = pctChanges[j];
	        	}
                Log("RetNorm " + string.Join(", ", tmp));
                //StdDev is a custom extension on decimal and double
				var secRetNorm = (tmp.Average()-0.01) /tmp.StdDev();
				retNorm[ii] = secRetNorm;
			    priceData.Add(tmp);
                ii++;
            }
            
            //PrintArray(allHistoryBars);
            return AaOptimizer.OptimiseWeights(allHistoryBars, retNorm);
		}

        private void Trade(Slice data)
        {
        	
            Log("Trade");
            if (Transactions.GetOpenOrders().Count > 0)
            {
                return;
            }
            if (!Valid)
            {
            	foreach(var s in stocks)
	            {
	            	SetHoldings(s, 0);
	            }
            	return;
            }
            Log("Trade weights: " + string.Join(",", weights));
            var factor = BasicTemplateAlgorithm.VAROPTI_REF_CASH / (double)Portfolio.TotalPortfolioValue;
            for (int i = 0; i < weights.Count(); i++)
            {
            	var s = stocks.ElementAt(i);
            	var qty = CalculateOrderQuantity(s, weights[i] * factor);
            	if (qty!=0) 
            	{
            		if (USE_LIMIT)
            		{
            			LimitOrder(s, qty, 0.5m * (Securities[s].AskPrice  + Securities[s].BidPrice));
            		}
            		else
            		{
            			MarketOrder(s, qty);
            		}
            	}
                //SetHoldings(stocks.ElementAt(i), weights[i]);
            }
        }

		bool Valid = false;
        public void OnEndOfDayVarOptimised()
        {
        	Log("OnEndOfDayVarOptimised");
            //allocate++;
            var lw = GetWeights();
            if (lw != null)
            {
	            int c = 0;
	    		foreach(var w in lw)
	    		{
	    			Plot("weights", $"w{c}", w);
	    			c++;
	    		}	
            }
            
            try
            {
	            var correl = GetCorrel();
	            Valid = correl < -0.25;
				Plot("correl", $"corr", correl);	
            }
            catch(Exception e)
            {
            	Valid = false;
            }
            
			// if (bbc.IsReady)
			// {
			//   Plot("correl", $"MA", bbc.MiddleBand);
			//   Plot("correl", $"UP", bbc.UpperBand);
			// }
			//Plot("correl", $"corr", GetCorrel());
        }
        
        double GetCorrel()
		{
			var priceData = new List<double[]>();
            
            int lookbackPeriod = allocationFrequency * 3;
			var resolution = Resolution.Daily;
            
            //will hold returns.mean/return.std for all stocks
            var ret = new double[stocks.Count];
            
            int ii = 0;
            foreach (var security in stocks)
            {
            	if (!Securities[security].IsTradable) 
            		continue;
                var history = History(security, lookbackPeriod, resolution);
                if (history.Count() == 0) 
                	continue;
                //the tmp array dupe the price for the current security because in matrix 
                // I have no way that I know of of accessign columns
                var tmp = new double[history.Count()+1];
                //fill the price matrix    
                int jj = 0;
                foreach(var h in history)
                {
                	tmp[jj] = (double)h.Close;
                	jj++;
                }
                var p = (double)history.LastOrDefault().Value;
                tmp[jj] = p;
                priceData.Add(tmp);
                ii++;
            }
            
            return ComputeCoeff(priceData[0].PctChange(), priceData[1].PctChange());
		}
		
        public double ComputeCoeff(double[] values1, double[] values2)
		{
		    if(values1.Length != values2.Length)
	        {
	        	//throw new ArgumentException("values must be the same length");
	        	return 0;
	        }
		        
		
		    var avg1 = values1.Average();
		    var avg2 = values2.Average();
		
		    var sum1 = values1.Zip(values2, (x1, y1) => (x1 - avg1) * (y1 - avg2)).Sum();
		
		    var sumSqr1 = values1.Sum(x => Math.Pow((x - avg1), 2.0));
		    var sumSqr2 = values2.Sum(y => Math.Pow((y - avg2), 2.0));
		
		    var result = sum1 / Math.Sqrt(sumSqr1 * sumSqr2);
		
		    return result;
		}
        
        private void PrintArray(double[,] arr)
        {
        	Log("########### print matrix to optimize");
            int rowLength = arr.GetLength(0);
            int colLength = arr.GetLength(1);
            Log(string.Join(",", stocks));
            for (int j = 0; j < colLength; j++)
            {
                string msg = "";
                for (int i = 0; i < rowLength; i++)
                {
                    msg += " " + string.Format("{0} ", arr[i, j].ToString("0.000000"));
                }
                Log(msg);
            }
            Log("#########################################");
        }
        
    }
     
     */

    
	

}
