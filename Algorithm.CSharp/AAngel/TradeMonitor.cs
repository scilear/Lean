using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Indicators;

namespace QuantConnect {

    //
    //	Make sure to change "BasicTemplateAlgorithm" to your algorithm class name, and that all
    //	files use "public partial class" if you want to split up your algorithm namespace into multiple files.
    //

    //public partial class BasicTemplateAlgorithm : QCAlgorithm, IAlgorithm
    //{
    //  Extension functions can go here...(ones that need access to QCAlgorithm functions e.g. Debug, Log etc.)
    //}

    //public class Indicator 
    //{
    //  ...or you can define whole new classes independent of the QuantConnect Context
    //}
    
    
    public static class Extensions
	{
	    public static decimal StdDev(this IEnumerable<decimal> values)
	    {
	       decimal ret = 0;
	       int count = values.Count();
	       if (count  > 1)
	       {
	          //Compute the Average
	          decimal avg = values.Average();
	
	          //Perform the Sum of (value-avg)^2
	          decimal sum = values.Sum(d => (d - avg) * (d - avg));
	
	          //Put it all together
	          ret = (decimal)Math.Sqrt((double)(sum / count));
	       }
	       return ret;
	    }
	}
    
    public class TradeMonitor
    {
    	static Dictionary<string, TradeMonitor> MON = new Dictionary<string, TradeMonitor>();
    	public static void AddTrade(string symbol , decimal price, List<decimal> state)
    	{
    		if (!MON.ContainsKey(symbol))
    			MON[symbol] = new TradeMonitor();
    		MON[symbol].AddTrade(price, state);
    	}
    	public static void CloseAt(string symbol , decimal price, DateTime time)
    	{
    		if (MON.ContainsKey(symbol))
    			MON[symbol].CloseAt(price, time);
    	}
    	public static void RecordPerf(string symbol , decimal price)
    	{
    		if (MON.ContainsKey(symbol))
    			MON[symbol].RecordPerf(price);
    	}
    	public static string GetPerfReport(string symbol)
    	{
    		return (MON.ContainsKey(symbol) ? MON[symbol].GetPerfReport() : "NA");
    	}
    	public static double GetSuccessRatio(string symbol)
    	{
    		return (MON.ContainsKey(symbol) ? MON[symbol].GetSuccessRatio() : 1);
    	}
    	
		public static decimal GetAveragePerf(string symbol)
    	{
    		return (MON.ContainsKey(symbol) ? MON[symbol].GetAveragePerf() : -1);
    	}
    	
    	public static decimal GetAveragePerfForDays(string symbol, DateTime time, int days)
    	{
    		return (MON.ContainsKey(symbol) ? MON[symbol].GetAverageRiskScoreForDays(time, days) : -1);
    	}

    	//decimal currentPrice = 0;
    	decimal openPrice = 0;
    	decimal maxPerf = -1;
    	decimal maxLoss = 0;
    	decimal maxDd = 0;
    	decimal curPerf = 0;
    	bool opened = false;
    	List<decimal> openingState;
    	RollingWindow<IndicatorDataPoint> pastPerf = new RollingWindow<IndicatorDataPoint>(500);
    	void AddTrade(decimal price, List<decimal> state)
    	{
    		if (opened) return;
    		
    		openPrice = price;
    		opened = true;
			maxPerf = -1;
			maxLoss = 0;
			maxDd = 0;
			curPerf = 0;
			openingState = state;
    	}
    	
    	void CloseAt(decimal price, DateTime time)
    	{
    		if (!opened) return;
    		
    		RecordPerf(price);
    		pastPerf.Add(new IndicatorDataPoint(time, curPerf));
    		opened = false;
    	}
    	
    	void RecordPerf(decimal price)
    	{
    		if (!opened) return;
    		curPerf = price/openPrice-1;
    		maxPerf = Math.Max(maxPerf, curPerf);
    		maxLoss = Math.Min(maxLoss, curPerf);
    		maxDd = Math.Max(maxDd, maxPerf-curPerf);
    	}
    	
    	string GetPerfReport()
    	{
    		if (pastPerf.Where(x => x > 0).Count() > 1)
			{
				int stop = 1;
				
			}
    		var successRatio = GetSuccessRatio();
    		//return $"{maxPerf,4},{maxLoss,4},{maxDd,4},{curPerf,4},{pastPerf.Average(),4},{successRatio,4}";
    		var msg = "";
    		if (openingState != null)
	    		foreach(var v in openingState)
	    		{
	    			msg += $"{maxPerf,4},";
	    		}
    		return msg;
    	}
    	
    	double GetSuccessRatio()
    	{
    		return (pastPerf.Count() > 2) ? pastPerf.Where(x => x > 0).Count()*1.0/pastPerf.Count() : 0;
    	}
    	
    	decimal GetAveragePerf()
    	{
    		return 0;//(pastPerf.Count() > 10) ? pastPerf.Average() : -1;
    	}
    	
    	decimal GetAverageSRForDays(DateTime time, int days)
    	{
    		if (pastPerf.Count() < 40) 
    			return -1;
    		var perf = pastPerf.Where(x => x.EndTime > time.AddDays(-days)).Select(x => x.Value);
    		
    		//we need at least 5 values otherwise too random
    		if (perf.Count() >=5) return perf.Average()/perf.StdDev();
    		
    		var perf2 = pastPerf.Skip(Math.Max(0, pastPerf.Count() - 5)).Select(x => x.Value);
    		return perf2.Average()/perf.StdDev();	
    	}
    	

    	decimal GetAverageRiskScoreForDays(DateTime time, int days)
    	{
    		if (pastPerf.Count() < 40) 
    			return -1;
    		var perf = pastPerf.Where(x => x.EndTime > time.AddDays(-days)).Select(x => x.Value);
     		
    		//we need at least 5 values otherwise too random
    		if (perf.Count() <=5) 
    			perf = pastPerf.Skip(Math.Max(0, pastPerf.Count() - 5)).Select(x => x.Value);
    		if (perf.Count() < 5)
    			return -1;
    		decimal max = 1;
    		decimal min= 1e10m;
    		decimal running = 1;
    		foreach(var p in perf)
    		{
    			running *= (1+p);
    			max = Math.Max(running, max);
    			min = Math.Min(min, running-max);
    		}
    		//if we have only winning trades we still need to divide 1% seems like a good compromise
    		if (min == 0) min = -0.01m;
    		return (running-1) / -min;
    	
    	}
    	
    	
    	
    	decimal GetAveragePerfForDays(DateTime time, int days)
    	{
    		if (pastPerf.Count() < 40) 
    			return -1;
    		var perf = pastPerf.Where(x => x.EndTime > time.AddDays(-days)).Select(x => x.Value);
    		
    		//we need at least 5 values otherwise too random
    		if (perf.Count() >=5) return perf.Average();
    		
    		var perf2 = pastPerf.Skip(Math.Max(0, pastPerf.Count() - 5)).Select(x => x.Value);
    		return perf2.Average();	
    	}
    	
    }
}