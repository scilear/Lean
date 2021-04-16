using System;
using System.Collections.Generic;
using QuantConnect.Indicators;

namespace QuantConnect.Algorithm.CSharp.AAngel
{


    public partial class PortfolioMixer : QCAlgorithm
    {

		HashSet<string> symbols = new HashSet<string>()
		{
			// "LBTYA","AAL","FOX","UAL","LBTYK","EXPE","FOXA","NTAP","TCOM","WDC","ULTA","TTWO","MXIM","ALGN","CDW","BMRN","CHKP","SWKS","CPRT","MCHP","DLTR","CTXS","SPLK","FAST","CTAS","INCY","ASML","CDNS","CERN","ANSS","XLNX","IDXX","ALXN","SNPS","CSGP","PCAR","SIRI","SGEN","PAYX","WDAY","VRSN","NXPI","VRSK","KLAC","NTES","WLTW","MAR","LULU","ORLY","BIDU","CTSH","MELI","EBAY","ROST","MNST","EA","KHC","XEL","EXC","ADI","ADSK","WBA","LRCX","JD","ILMN","AMAT","CSX","MU","ATVI","ISRG","ADP","REGN","BKNG","BIIB","AMD","FISV","INTU","VRTX","MDLZ","TMUS","QCOM","SBUX","AVGO","TXN","GILD","CHTR","PYPL","TSLA","AMGN","COST","ADBE","CMCSA","NVDA","CSCO","NFLX","PEP","INTC","FB","GOOG","AMZN","AAPL","MSFT",
		
			// "VXX",
			// "VXX.1", 
			 "TEVA", "TTD", "QQQ", "AAPL", "NFLX", "SHOP", "TSLA", "AMZN", "NVDA", "SPLK",
			 
			 //"AAPL"
			 
			 //Buffet
			 //"AMZN", "CVX", "PFE", "ABBV", "GOLD", "JPM", 
			 //"BMY", "DAL", "COST", "MRK", "USG", "IBM", "GE", "BAC", "KO", "KHC", "AXP", "VZ", "USB",
			 //"GM", "BK", "WFC"
			 
			 // top 2018
			 //"TRIP", "AMD", "AAP", "ABMD", "FTNT", "HCA", "CMG", "UAA", "ILMN", "MKC"
			 //"VXX"
			 //"VXX", "SVXY"
		};
        public override void Initialize()
        {
            
            // TODO: move all that in a Configuration static class, that calls set methods on Mixer 
            SetStartDate(2003, 1, 1);
            SetEndDate(2021, 3, 15);
            // SetStartDate(2020, 6, 1);
            // SetEndDate(2020, 8, 15);
            
            int CASH = 1000000;

            SetCash(1000000);
            //SetWarmup(TimeSpan.FromDays(180));
            //SetWarmup(TimeSpan.FromDays(4*400));
            SetWarmup(TimeSpan.FromDays(50));

		/*	foreach(var s in symbols)
			{
				var symbol = AddEquity(s, Resolution.Minute).Symbol;
	            SubAlgo algo = new EitPfeSingleAlgo(this, symbol, false);
	            //SubAlgo algo = new RbbAlgo(this, symbol, false);
	            algos.Add(new SubAlgoValidator(algo, 
	                TimeSpan.FromDays(1), 
	                new Dictionary<string, IIndicator>()
	                { //{"PFE", new PolarisedFractalEfficiency()},
	                //{"EIT", new EhlerInstantaneousTrend()},
	                    //{"SMA10", new ExponentialMovingAverage(10)},
	                    //{"BB", new BollingerBands(20, 2, MovingAverageType.Exponential)},
	                    {"RBB", new RollingBB()}
	                	
	                },
	                (a) =>
	                {
	                	return 1;
	                    // if (!a.Algo.Name.Contains("VXX"))
	                    // 	return 1; 
	                    
	                    //if (a.Algo.Portfolio.TotalPortfolioValue >= a.GetIndicatorValue("SMA10"))
	                    if (StrategyRbbActivator.Activate(a.Algo.Name, a.Algo.Portfolio.TotalPortfolioValue, (RollingBB)a.Indicators["RBB"]))
	                    //if (StrategyPfeActivator.Activate(a.Algo.Name, (PolarisedFractalEfficiency)a.Indicators["PFE"]))
	                    //if (StrategyEitActivator.Activate(a.Algo.Name, a.Algo.Portfolio.TotalPortfolioValue, (EhlerInstantaneousTrend)a.Indicators["EIT"]))
	                        return 1m;
	                    return 0;
	                }));
	            //strategyAllocation[algo.Name] = 1m/(symbols.Count());
	            strategyAllocation[algo.Name] = 0.5m/(symbols.Count());
			}
			*/

			foreach(var s in symbols)
			{
				var symbol = AddEquity(s, Resolution.Minute).Symbol;
	            //SubAlgo algo = new EitPfeSingleAlgo(this, symbol, false);
	            SubAlgo algo = new RbbAlgo(this, symbol, false);
	            algos.Add(new SubAlgoValidator(algo, 
	                TimeSpan.FromDays(1), 
	                new Dictionary<string, IIndicator>()
	                { //{"PFE", new PolarisedFractalEfficiency()},
	                //{"EIT", new EhlerInstantaneousTrend()},
	                    //{"SMA10", new ExponentialMovingAverage(10)},
	                    //{"BB", new BollingerBands(20, 2, MovingAverageType.Exponential)},
	                    {"RBB", new RollingBB()}
	                	
	                },
	                (a) =>
	                {
	                	return 1;
	                    // if (!a.Algo.Name.Contains("VXX"))
	                    // 	return 1; 
	                    
	                    //if (a.Algo.Portfolio.TotalPortfolioValue >= a.GetIndicatorValue("SMA10"))
	                    if (StrategyRbbActivator.Activate(a.Algo.Name, a.Algo.Portfolio.TotalPortfolioValue, (RollingBB)a.Indicators["RBB"]))
	                    //if (StrategyPfeActivator.Activate(a.Algo.Name, (PolarisedFractalEfficiency)a.Indicators["PFE"]))
	                    //if (StrategyEitActivator.Activate(a.Algo.Name, a.Algo.Portfolio.TotalPortfolioValue, (EhlerInstantaneousTrend)a.Indicators["EIT"]))
	                        return 1m;
	                    return 0;
	                }));

	            var defaultWeight = 1m / symbols.Count();
	            _strategyMixer = new DefaultStrategyMixer(defaultWeight);
	            //strategyAllocation[algo.Name] = 0.5m/(symbols.Count());
			}
			            
            
            foreach (var a in algos)
            {
                a.Algo.SetCash(CASH);
                a.Algo.Initialize();
                prevConfidence[a.Algo.Name] = 0;
            }
        }
    }

	public static class StrategyRbbActivator
    {
        private static Dictionary<string, decimal> _values = new Dictionary<string, decimal>();
        //private static Dictionary<string, decimal> _previousValues = new Dictionary<string, decimal>();
        private static HashSet<string> _activated = new HashSet<string>();
        private static int _pday =  0;
        public static bool Activate(string strategy, decimal pnl, RollingBB indicator)
        {
        	var day = indicator.Current.EndTime.Day;
        	if (_pday != day)
        	{
        		_pday = day;
        		if (pnl < indicator.LowerBand)// || indicator > 0)
        		{
        			_activated.Remove(strategy);
        		}
        		else if  (pnl >= indicator.UpperBand)
        		//else if  (pnl >= (indicator.UpperBand * 0.5m + indicator.LowerBand * 0.5m))
        		{
        			_activated.Add(strategy);
        		}
        	}
        	var value = _activated.Contains(strategy);
            return value;
        }
    }
        
	public static class StrategySmaActivator
    {
        private static Dictionary<string, decimal> _values = new Dictionary<string, decimal>();
        //private static Dictionary<string, decimal> _previousValues = new Dictionary<string, decimal>();
        private static HashSet<string> _activated = new HashSet<string>();
        private static int _pday =  0;
        public static bool Activate(string strategy, decimal pnl, ExponentialMovingAverage indicator)
        {
        	var day = indicator.Current.EndTime.Day;
        	if (_pday != day)
        	{
        		_pday = day;
        		if (pnl > indicator)// || indicator > 0)
        		{
        			_activated.Add(strategy);
        		}
        		else
        		{
        			_activated.Remove(strategy);
        		}
        	}
        	var value = _activated.Contains(strategy);
            return value;
        }
    }
    

	public static class StrategyEitActivator
    {
        private static Dictionary<string, decimal> _values = new Dictionary<string, decimal>();
        //private static Dictionary<string, decimal> _previousValues = new Dictionary<string, decimal>();
        private static HashSet<string> _activated = new HashSet<string>();
        private static int _pday =  0;
        public static bool Activate(string strategy, decimal pnl, EhlerInstantaneousTrend indicator)
        {
        	var day = indicator.Current.EndTime.Day;
        	if (_pday != day)
        	{
        		_pday = day;
        		if (pnl > Math.Max(indicator.Lag, indicator.InstantaneousTrend))// || indicator > 0)
        		{
        			_activated.Add(strategy);
        		}
        		else
        		{
        			_activated.Remove(strategy);
        		}
        	}
        	var value = _activated.Contains(strategy);
            return value;
        }
    }
    
    public static class StrategyPfeActivator
    {
        private static Dictionary<string, decimal> _values = new Dictionary<string, decimal>();
        //private static Dictionary<string, decimal> _previousValues = new Dictionary<string, decimal>();
        private static HashSet<string> _activated = new HashSet<string>();
        private static int _pday =  0;
        public static bool Activate(string strategy, PolarisedFractalEfficiency indicator)
        {
        	var day = indicator.Current.EndTime.Day;
        	if (_pday != day)
        	{
        		_pday = day;
        		if (!_values.ContainsKey(strategy))
	            {
	                _values[strategy] = indicator.Current.Value;
	                //_previousValues[strategy] = indicator;
	                if (indicator > 0)
	                {
	                    _activated.Add(strategy);
	                    return true;
	                }
	
	                return false;
	            }
	            else
	            {
	                var pv = _values[strategy];
	                var cv = indicator.Current.Value;
	                if (cv > pv && cv > -50)
	                {
	                    _activated.Add(strategy);
	                    return true;
	                }
	                else if (pv > 75 && cv < 75 || cv < -50)
	                {
	                    _activated.Remove(strategy);
	                    return false;
	                }
	                _values[strategy] = cv;
	            }
        	}
            return _activated.Contains(strategy);
        }
    }
}
