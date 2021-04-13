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
	
	
    public class EitPfeNdxAlgo : SubAlgo
    {

/*
	remove outliers in computing avg performance
	take perf over period of time instead of number of trades
*/
		bool LATEST_DEPLOY = false;
		
		
		const int MAX_POS = 10;
		const int stockPickingPeriod = 400;
		const int longPeriod = 240;
		const bool VOL_PROTECT = false;
		PolarisedFractalEfficiency _pfeStrategy = new PolarisedFractalEfficiency();
		
		//Dictionary<string, PolarisedFractalEfficiency> _pfeReference = new Dictionary<string, PolarisedFractalEfficiency>();
		Dictionary<string, PolarisedFractalEfficiency> pfe = new Dictionary<string, PolarisedFractalEfficiency>();
		Dictionary<string, decimal> ppfe = new Dictionary<string, decimal>();
		Dictionary<string, EhlerInstantaneousTrend> eit = new Dictionary<string, EhlerInstantaneousTrend>();
		
		HashSet<string> symbols = new HashSet<string>()
		{
			// "LBTYA","AAL","FOX","UAL","LBTYK","EXPE","FOXA","NTAP","TCOM","WDC","ULTA","TTWO","MXIM","ALGN","CDW","BMRN","CHKP","SWKS","CPRT","MCHP","DLTR","CTXS","SPLK","FAST","CTAS","INCY","ASML","CDNS","CERN","ANSS","XLNX","IDXX","ALXN","SNPS","CSGP","PCAR","SIRI","SGEN","PAYX","WDAY","VRSN","NXPI","VRSK","KLAC","NTES","WLTW","MAR","LULU","ORLY","BIDU","CTSH","MELI","EBAY","ROST","MNST","EA","KHC","XEL","EXC","ADI","ADSK","WBA","LRCX","JD","ILMN","AMAT","CSX","MU","ATVI","ISRG","ADP","REGN","BKNG","BIIB","AMD","FISV","INTU","VRTX","MDLZ","TMUS","QCOM","SBUX","AVGO","TXN","GILD","CHTR","PYPL","TSLA","AMGN","COST","ADBE","CMCSA","NVDA","CSCO","NFLX","PEP","INTC","FB","GOOG","AMZN","AAPL","MSFT",
		
			//"VXX", "VXX.1", 
			"TEVA", "TTD", "QQQ", "AAPL", "NFLX", "SHOP", "TSLA", "AMZN", "NVDA", "SPLK",
			"QQQ"
			
			
			//ECOMM
			// "BABA", "AMZN", "FLWS", "APRN", "BKNG", "CARS", "CVNA", "CHWY", "CPRT", "DLTH", "EBAY",
			// "ETSY", "EXPE", "FVRR", "GRPN", "GRUB", "JD", "JMIA", "LMPX", "MELI", "NETE", "OSTK",
			// "PETS", "PDD", "QRTEA", "RVLV", "SECO", "SHOP", "STMP", "SFIX", "TZOO", "TRIP", "MEDS",
			// "W", "YJ",
		};
		
		
		//kings list
		//{"ABM","AWR","CBSH","CINF","CL","CWT","DOV","EMR","FMCB","FRT","GPC","HRL","JNJ","KO","LANC","LOW","MMM","NDSN","NWN","PG","PH","SCL","SJW","SWK","TR","MO","FUL","SYY","UVV","NFG",};
		

		//HashSet<string> riskProtectSymbols = new HashSet<string>(){"VXX", "VXX.1"};
		HashSet<string> tradableSymbols = new HashSet<string>(){};
		int POS_COUNT = 0;
		double CASH = 0;
		bool FIXED_AMOUNT = false;

		protected override string GetName()
		{
			return $"EitPfeNdxAlgo_{"NDX"}";
		}

		public override void Initialize()
        {
            /*SetStartDate(2020, 1, 1);            
            if (LATEST_DEPLOY) SetStartDate(2021, 2, 6);
            */
            //SetStartDate(2016, 1, 1);  
            //SetEndDate(2021, 3, 15);  //Set Start Date
            
            
   //         SetStartDate(2011, 2, 1);  
			// SetEndDate(2020, 1, 15);
			
			POS_COUNT = symbols.Count();
            POS_COUNT -= (symbols.Contains("VXX")) ? 1: 0;
            POS_COUNT -= (symbols.Contains("VXX.1")) ? 1: 0;
            
        	//CASH = Math.Max(10000*POS_COUNT, 20000);//Max for when testing VXX only
        	//CASH = 145886;
        	if (LATEST_DEPLOY) CASH = 179860;
            //SetCash(CASH);             //Set Strategy Cash
            //SetStartDate(2018, 3, 1);  //Set Start Date
            
            // if (symbols.Count() > 5)
            // {
	           // //symbols.UnionWith(Universes.SP500);
	           // symbols.UnionWith(Universes.NDX);
	           // symbols.UnionWith(Universes.NDX99);
            // }
            
            //SetWarmup(TimeSpan.FromDays(stockPickingPeriod*4));

            var res = TimeSpan.FromHours(1);
            if (VOL_PROTECT) AddEquity("VIXM", Resolution.Hour);   
            foreach(var s in symbols)
            {
	            AddEquity(s, Resolution.Minute).SetLeverage(2);            
				
				pfe[s] = new PolarisedFractalEfficiency();
				Algo.RegisterIndicator(s, pfe[s], res);
				
				ppfe[s] = 0;
				
				eit[s] = new EhlerInstantaneousTrend();
				Algo.RegisterIndicator(s, eit[s], res);
				
				
				pvalue[s] = 0;
				activate[s] = false;
				
				// if (riskProtectSymbols.Contains(s))
				// {
				// 	prevPerf[s] = 1;
				// 	reference[s] = 1;
				// 	_pfeReference[s] = new PolarisedFractalEfficiency();	
				// }
            }
        }

		
		Dictionary<string, decimal> pvalue = new Dictionary<string, decimal>();
        Dictionary<string, bool> activate = new Dictionary<string, bool>();
        
		
		// public override void OnOrderEvent(OrderEvent orderEvent)
  //      {
  //          // // print debug messages for all order events
  //          // if (LiveMode || orderEvent.Status.IsFill() )
  //          // {
  //          //     LiveDebug("Filled: " + orderEvent.FillQuantity + " Price: " + orderEvent.FillPrice);
  //          // }

  //          // // if this is a fill and we now don't own any stock, that means we've closed for the day
  //          // if (!Security.Invested && orderEvent.Status == OrderStatus.Filled)
  //          // {
  //          //     // reset values for tomorrow
  //          //     LastExitTime = Time;
  //          //     var ticket = Transactions.GetOrderTickets(x => x.OrderId == orderEvent.OrderId).Single();
  //          //     Plot(symbol, "Exit", ticket.AverageFillPrice);
  //          // }
  //      }
        
        int pmonth = 0;
        int jitter = 0;
        public override void OnData(Slice data)
        {
        	if (!IsWarmingUp && Time.Month != pmonth && VOL_PROTECT)
        	{
        		SetHoldings("VIXM", 0.1m);
        		pmonth = Time.Month;
        	}
        	
			if (Time.Minute != 0) return;
        	
        	foreach(var s in symbols)
            {
            	if (!Securities.ContainsKey(s)) 
            		continue;
            	TradeMonitor.RecordPerf(s, Securities[s].Price);
            	
            	
            	if (pvalue[s]> 50 & pfe[s] < 50 || pfe[s] < -50)
                {
                	if (activate[s]) jitter++;
                	activate[s] = false;
                }
                else if (pvalue[s]<  -50 & pfe[s] > -50 || pfe[s] > 0 && pfe[s]>pvalue[s])
                {
                	if (!activate[s]) jitter++;
                	activate[s] = true;
                }
            	pvalue[s] = pfe[s];
            	
            	
            	
            	
            	if (eit[s].IsReady)
            	{
            		if ( Securities.ContainsKey(s) && data.Bars.ContainsKey(s))
            		{
	            		var ratio = 1.0m/MAX_POS;///(symbols.Contains("VXX.1")) ? 1.0/(symbols.Count()-1) : 1.0 / (symbols.Count());
	            		//if (POS_COUNT >= 10) ratio = 0.1;

            			if ( activate[s] && Portfolio[s].Quantity == 0)
		            	{
		            		if (eit[s] > 0 && data[s].Close > Math.Max(eit[s].Lag, eit[s].InstantaneousTrend))
		            		{
		            			BuyRatio(data, s, ratio);
		            		}
		            	}
		            	
		            	if (eit[s] < 0 || data[s].Close < Math.Min(eit[s].Lag, eit[s].InstantaneousTrend))
		        		{
		        			UpdateReference(s);
		        			if (Securities.ContainsKey(s)) 
		        				TradeMonitor.CloseAt(s, Securities[s].Price, Time);
		        			if (Portfolio.ContainsKey(s) && Portfolio[s].Quantity != 0 && !IsWarmingUp)
		        				SetHoldings(s, 0);//, tag:TradeMonitor.GetPerfReport(s));
		        			
		        		}
		        		else if (!activate[s] && data.Bars.ContainsKey(s) && data.Bars[s].Close < eit[s].Lag)//tightening the grip when trend slowing down according to PFE
		        		{
		        			UpdateReference(s);
		        			TradeMonitor.CloseAt(s, Securities[s].Price, Time);
		        			if (Portfolio[s].Quantity != 0 && !IsWarmingUp)
		        				SetHoldings(s, 0);//, tag:TradeMonitor.GetPerfReport(s));
		        		}
		        		//risk mgmt kinda redundant with algo so little impact on DD
		        		// else //if (data.Bars.ContainsKey(s))
		        		// {
		        		// 	// if (stopLoss.ContainsKey(s) && data[s].Close < stopLoss[s])
		        		// 	// {
			        	// 	// 	UpdateReference(s);
			        	// 	// 	TradeMonitor.CloseAt(s, Securities[s].Price, Time);
			        	// 	// 	if (Portfolio[s].Quantity != 0)
			        	// 	// 		SetHoldings(s, 0, tag:TradeMonitor.GetPerfReport(s));
		        		// 	// }
		        		// 	// else if (takeProfit.ContainsKey(s) && data[s].Close >= takeProfit[s])
		        		// 	// {
		        		// 	// 	if (Portfolio[s].Quantity != 0)
		        		// 	// 		MarketOrder(s, -0.5m * Portfolio[s].Quantity);
		        		// 	// 	takeProfit.Remove(s);
		        		// 	// }
		        		// 	// else if (!takeProfit.ContainsKey(s) && data[s].Close < Portfolio[s].AveragePrice)
		        		// 	// {
		        		// 	// 	UpdateReference(s);
		        		// 	// 	TradeMonitor.CloseAt(s, Securities[s].Price, Time);
		        		// 	// 	if (Portfolio[s].Quantity != 0)
			        	// 	// 		SetHoldings(s, 0, tag:TradeMonitor.GetPerfReport(s));
		        		// 	// }
		        		// }
            		}
            	}
            }
        }
        
        void BuyRatio(Slice data, Symbol s, decimal ratio)
        {
        	var sr = TradeMonitor.GetSuccessRatio(s);
        	TradeMonitor.AddTrade(s, Securities[s].Price, GetState(s));
        	//if (sr < 0.0 && !s.ToString().StartsWith("VXX")) return;
        	if (!tradableSymbols.Contains(s)) return;
        	if (IsWarmingUp) return;
        	
        	
        	if (Portfolio[s].Quantity == 0)
        	{
	        	SetHoldings(s, ratio);
        	}
        }
        
        List<decimal> GetState(string s)
        {
        	//pfe, pfe_inc, eit, eit_inc, PerfScoreLong, PerfScoreShort, successRatio
        	
        	//pfe, eit, PerfScore, successRatio
        	var list = new List<decimal>();
        	list.Add(pfe[s]);
        	list.Add(eit[s]);
        	list.Add(TradeMonitor.GetAveragePerfForDays(s, Time, stockPickingPeriod));
        	list.Add(TradeMonitor.GetAveragePerfForDays(s, Time, longPeriod));
        	return list;
        }
        

        // Dictionary<string, decimal> reference = new Dictionary<string, decimal>();
        // Dictionary<string, decimal> prevPerf = new Dictionary<string, decimal>();
        
        void UpdateReference(string sym)
        {
        	// if (!riskProtectSymbols.Contains(sym)) return;
        	
       // 	if (Portfolio.ContainsKey(sym) && Portfolio[sym].Quantity>0)
       // 	{
       // 		decimal perf = Portfolio[sym].Price/Portfolio[sym].AveragePrice;
	    		// var daily = perf / prevPerf[sym];
	    		// reference[sym] *= daily;
	    		// prevPerf[sym] = perf;
       // 	}
       // 	else
       // 	{
       // 		prevPerf[sym] = 1;
       // 	}
        }
        
        SimpleMovingAverage jitterSma = new SimpleMovingAverage(20);
        SimpleMovingAverage jitterSmaL = new SimpleMovingAverage(100);
        public override void OnEndOfDay()
        {
        	//Transactions.CancelOpenOrders();
    		//_pfeStrategy.Update(new TradeBar(Time, symbols.FirstOrDefault(), Portfolio.TotalPortfolioValue, Portfolio.TotalPortfolioValue, Portfolio.TotalPortfolioValue, Portfolio.TotalPortfolioValue, Portfolio.TotalPortfolioValue));_pfeStrategy
    		_pfeStrategy.Update(new IndicatorDataPoint(Time, Portfolio.TotalPortfolioValue));
    		Plot("PFE", "Strategy", _pfeStrategy.Current.Value);
    		
    		if (jitter < 1000)
    		{
	    		Plot("Jitter", "Jitter", (decimal)jitter);
	    		jitterSma.Update(Time, jitter);
	    		jitterSmaL.Update(Time, jitter);
	    		Plot("Jitter", "JitterSMA", jitterSma.Current.Value);
	    		Plot("Jitter", "JitterLong", jitterSmaL.Current.Value);
    			
    		}
    		jitter = 0;
    		
    		var perf = new Dictionary<string, decimal>();
    		var perfS = new Dictionary<string, decimal>();
    		var bull1 = 0;
    		var bull2 = 0;
    		var bull3 = 0;
    		foreach(var s in symbols)
    		{
    			// if (riskProtectSymbols.Contains(s))
    			// {
	    		// 	UpdateReference(s);
		    	// 	_pfeReference[s].Update(new TradeBar(Time, s, reference[s], reference[s], reference[s], reference[s], reference[s]));
    			// }
    			
    			if (eit[s] > 0) bull1++;
    			if (pfe[s] > 0) bull2++;
    			if (pfe[s] > ppfe[s] && pfe[s] > -50) bull3++;
    			Plot("BULLISH", "EIT", (decimal)bull1);
    			Plot("BULLISH", "PFE", (decimal)bull2);
    			Plot("BULLISH", "PFE+", (decimal)bull2);
    			perf[s] = TradeMonitor.GetAveragePerfForDays(s, Time, stockPickingPeriod);
    			perfS[s] = TradeMonitor.GetAveragePerfForDays(s, Time, longPeriod);
    			
    			ppfe[s] = pfe[s];
    		}
    		
    		
    		tradableSymbols.Clear();
    		if (!IsWarmingUp)
    		{
    			var l1 = (from entry in perf where perf[entry.Key]>0  orderby perf[entry.Key] descending select entry.Key).Take(MAX_POS*2);
    			var l2 = (from entry in l1 where perfS[entry]>0  orderby perfS[entry] descending select entry).Take(MAX_POS*2);
    			var lr = l2;//l1.Intersect(l2);
	    		foreach( var s in lr)
	    		{
	    			tradableSymbols.Add(s);
	    		}
	    			//tradableSymbols.Add("QQQ");
	    		// tradableSymbols.Add("VXX");
	    		// tradableSymbols.Add("VXX.1");
    		}
        }
        
        bool IsTradable(KeyValuePair<string, decimal> entry)
        {
        	return entry.Value > -0;
        }

        public EitPfeNdxAlgo(QCAlgorithm algo, decimal cash = 0)
	        : base(algo, cash)
        {
        }
    }
}
