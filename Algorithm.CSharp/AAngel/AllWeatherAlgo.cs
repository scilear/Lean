/*
20201013: added MinVar code but not live
20201015: fixe on VarOpti ComputeCoeff length mismatch
20201111: removed all calls to GetCorrel that seemed to crash every now and then
20210217: added new version of VxxBlackSwan and changed allocation of VAROPTI_REF_CASH from 25 to 75k
20210329: new version with correlation control on TMFUPRO, increase PSR and it woudl have been nice to have that a few weeks ago!

*/

using System;
using QuantConnect.Algorithm.CSharp.AAngel;
using QuantConnect.Data;
using QuantConnect.Data.Consolidators;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;

namespace QuantConnect.Algorithm.CSharp
{
	public class AllWeatherAlgo : SubAlgo
    {
    	private Symbol SYM = Symbol.Create("QQQ", SecurityType.Equity, Market.USA);

		private SimpleMovingAverage ma = new SimpleMovingAverage(10);
		
        public AllWeatherAlgo(QCAlgorithm algorithm)
            : base(algorithm)
        {


        }
		
		
        bool prevRebalCondition = false;
		bool prevCondition = false;
		bool prevPrevCondition = false;
		bool fullOn = false;
		
        public override void Initialize()
        {
            AddEquity(SYM, Resolution.Minute);
            var week5Consolidator = new TradeBarConsolidator(TimeSpan.FromDays(22));
			AddEquity("TLT", Resolution.Minute);
			AddEquity("IEF", Resolution.Minute);
			AddEquity("DBC", Resolution.Minute);
			//AddEquity("VEA", Resolution.Minute);
			AddEquity("GLD", Resolution.Minute);
			
            
            week5Consolidator.DataConsolidated += (sender, consolidated) => OnWeek5(sender, (TradeBar) consolidated);

            // this call adds our 3 day to the manager to receive updates from the engine
            SubscriptionManager.AddConsolidator(SYM, week5Consolidator);

            // There are other assets with similar methods. See "Selecting Options" etc for more details.
            // AddFuture, AddForex, AddCfd, AddOption
            Schedule.On(DateRules.EveryDay(SYM), TimeRules.AfterMarketOpen(SYM, 5), () =>
            {
                //Log("EveryDay.SPY 10 min after open: Fired at: " + Time);
                if (fullOn)
                {
                	if (Securities[SYM].Price < ma && prevRebalCondition)
                	{
                		var ratio = 1;
                		SetAllWeatherHoldings(ratio);
						fullOn = false;
                	}
                	prevRebalCondition = Securities[SYM].Price > ma;
                }
            });
        }
	
		public void OnWeek5(object sender, TradeBar consolidated)
		{
			ma.Update(new IndicatorDataPoint(consolidated.EndTime, consolidated.Close));
			Log($"{Time} OnWeek5 ma ready {ma.IsReady}");
			if (ma.IsReady && !IsWarmingUp)
			{
				//SetHoldings(SYM,1);
				if (consolidated.Close > ma && prevCondition && !prevPrevCondition)
				{
					var ratio = 0.4m/0.7m;
					var eq_ratio = 2m;
					SetAllWeatherHoldings(ratio, eq_ratio);
					fullOn = true;
				}
				else if (consolidated.Close < ma && prevCondition || !Portfolio.Invested)
				{
					var ratio = 1m;//0.6;
					SetAllWeatherHoldings(ratio);
					fullOn = false;
				}
				prevPrevCondition = prevCondition;
				prevCondition = consolidated.Close > ma;
			}
		}
		
        protected override string GetName()
        {
            return $"AllWeatherAlgo";
        }

        public override void OnData(Slice data)
        {
        	if (!IsWarmingUp)
        	{
        		if (!Portfolio.Invested)
        		{
					var ratio = 0.6m;
					SetAllWeatherHoldings(ratio);
					fullOn = false;
        		}
        	}
        }
            
        private void SetAllWeatherHoldings(decimal ratio, decimal eq_ratio = 1)
	    {

	    	var factor = 1;//(BasicTemplateAlgorithm.REINVEST) ? BasicTemplateAlgorithm.ALLWEATHER_REF_CASH / GetStartingCash() :BasicTemplateAlgorithm.ALLWEATHER_REF_CASH / (double)Portfolio.TotalPortfolioValue;
	        
	    	if (eq_ratio < 0.01m)
			{
				SetHoldings(SYM,   0.3m * ratio * factor);
			}
			else
			{
				SetHoldings(SYM, 0.3m * eq_ratio * factor);
			}
			
			SetHoldings("TLT", 0.4m * ratio * factor);
			SetHoldings("IEF", 0.15m * ratio * factor);
			SetHoldings("DBC", 0.075m * ratio * factor);
			SetHoldings("GLD", 0.075m * ratio * factor);
	    }
	    
	    /*void ManageOpenOrders()
        {
        	foreach(var s in Portfolio.Keys)
        	{
    			foreach (var o in Transactions.GetOpenOrderTickets(s))
    			{
    				o.UpdateLimitPrice(Math.Ceiling(Math.Min(Securities[s].Price, Securities[s].AskPrice * 0.5m + Securities[s].BidPrice * 0.5m)*100)/100);
    			}
        	}
        }*/


	    
	    void PrintAllWeatherState()
	    {
	    	if (LiveMode)
	    	{
	    		Log($"*********** ALL WEATHER **************");
	    		Log($"{Time} OnWeek5 ma ready {ma.IsReady}, Close>ma: {Securities[SYM].Price > ma}");
	    	}
	    }
    }
    

    /*
    public class AllWeatherAlgo : SubAlgo
    {
     
    	protected string GetName()
    	{
    		return "AllWeatherAlgo";
    	}
    	private static bool REINVEST = true;
    	private static bool USE_LIMIT = false;
    	
    	
    	public static double VAROPTI_REF_CASH = 75000;
    	public static double MINVAR_REF_CASH = 25000;
    	public static double ALLWEATHER_REF_CASH = 200000;
    	public static double BLACKSWAN_REF_CASH = 25000;
    	
    	bool ALLWEATHER = true;
    	bool MINVAR = false;
    	bool BLACKSWAN = false;
    	bool VAROPTI = true;

*/

}