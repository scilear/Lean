/*
////////////////////////////////////////////////////////////
//  Copyright by HPotter v1.0 30/06/2014
// The Polarized Fractal Efficiency (PFE) indicator measures the efficiency 
// of price movements by drawing on concepts from fractal geometry and chaos 
// theory. The more linear and efficient the price movement, the shorter the 
// distance the prices must travel between two points and thus the more efficient 
// the price movement.
////////////////////////////////////////////////////////////
study(title="PFE (Polarized Fractal Efficiency)", shorttitle="PFE (Polarized Fractal Efficiency)")
Length = input(9, minval=1)
LengthEMA = input(5, minval=1)
hline(50, color=green, linestyle=line, title = "TopBand")
hline(-50, color=red, linestyle=line, title = "LowBand")
PFE = sqrt(pow(close - close[Length], 2) + 100)
C2C = sum(sqrt(pow((close - close[1]), 2) + 1), Length)
xFracEff = iff(close - close[Length] > 0,  round((PFE / C2C) * 100) , round(-(PFE / C2C) * 100))
xEMA = ema(xFracEff, LengthEMA)
plot(xEMA, color=blue, title="PFE")

*/

using QuantConnect.Data.Market;
using System;
using System.Linq;

namespace QuantConnect.Indicators
{
	
    /// <summary>
    /// The Aroon Oscillator is the difference between AroonUp and AroonDown. The value of this
    /// indicator fluctuates between -100 and +100. An upward trend bias is present when the oscillator
    /// is positive, and a negative trend bias is present when the oscillator is negative. AroonUp/Down
    /// values over 75 identify strong trends in their respective direction.
    /// </summary>
    public class PolarisedFractalEfficiency : BarIndicator, IIndicatorWarmUpPeriodProvider
    {
        
        public RollingWindow<double> C2C;
        public RollingWindow<decimal> Src;
        public ExponentialMovingAverage xEma;
        public double PFE;

        /// <summary>
        /// Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady => Src.IsReady && C2C.IsReady && xEma.IsReady;

        /// <summary>
        /// Required period, in data points, for the indicator to be ready and fully initialized.
        /// </summary>
        public int WarmUpPeriod { get; }


		
        /// <summary>
        /// Creates a new EhlerTrend from the specified up/down periods.
        /// </summary>
        /// <param name="upPeriod">The lookback period to determine the highest high for the AroonDown</param>
        /// <param name="downPeriod">The lookback period to determine the lowest low for the AroonUp</param>
        public PolarisedFractalEfficiency(Source source=Source.Close, int lookback=9, int period=5)
            : this($"PFE({source},{lookback},{period})", source, lookback, period)
        {
        }

        /// <summary>
        /// Creates a new EhlerTrend from the specified up/down periods.
        /// </summary>
        /// <param name="name">The name of this indicator</param>
        /// <param name="upPeriod">The lookback period to determine the highest high for the AroonDown</param>
        /// <param name="downPeriod">The lookback period to determine the lowest low for the AroonUp</param>
        private Source _source;
        private decimal _alpha;
        public PolarisedFractalEfficiency(string name, Source source, int lookback, int period)
            : base(name)
        {
			_source = source;
			xEma = new ExponentialMovingAverage(period);
			Src = new RollingWindow<decimal>(lookback+1);
			C2C = new RollingWindow<double>(lookback);
            WarmUpPeriod = period+lookback;
        }

        /// <summary>
        /// Computes the next value of this indicator from the given state
        /// </summary>
        /// <param name="input">The input given to the indicator</param>
        /// <returns>A new value for this indicator</returns>
        public decimal PrevCycle { get; set;}
        public decimal Cycle { get; set;}
        
        protected override decimal ComputeNextValue(IBaseDataBar input)
        {
            var stop =0;
            var src = GetSource(input);
            if (src == Src.FirstOrDefault())
            {
            	return xEma;
            }
            Src.Add(src);
            
            if (Src.IsReady)
            {
            	//PFE = sqrt(pow(close - close[Length], 2) + 100)
            	var s1 = Src.Skip(1).FirstOrDefault();
            	var sL = Src.LastOrDefault();
            	var perfL = src - sL;
            	PFE = Math.Sqrt((double)(perfL*perfL) + 100);
            	
            	//C2C = sum(sqrt(pow((close - close[1]), 2) + 1), Length)
            	var perf1 = src - s1;
            	var tosum = Math.Sqrt((double)(perf1*perf1) + 1);
            	C2C.Add(tosum);
            	
        		//xFracEff = iff(close - close[Length] > 0,  round((PFE / C2C) * 100) , round(-(PFE / C2C) * 100))
        		double c2csum = 0;
        		double xFracEff = 0;
        		if (C2C.IsReady)
        		{
        			c2csum = C2C.Sum();
	        		xFracEff = (perfL>0) ? Math.Round(PFE/c2csum* 100)  : Math.Round(-PFE/c2csum* 100) ;
	        		xEma.Update(input.Time, (decimal)xFracEff);
        		}
				if (xEma.IsReady)
	        	{
	        		return xEma;//Math.Min(Math.Max(xEma, -100), 100);
	        	}
            }
            return 0;
        }
		
		
		private decimal GetSource(IBaseDataBar input)
		{
			switch(_source)
			{
				case Source.Close:
					return input.Close;
				case Source.HL2:
					return 0.5m * (input.High+input.Low);
				case Source.High:
					return input.High;
				case Source.Low:
					return input.Low;
			}
			return 0;
			
		}
		
        
        
        public override void Reset()
        {
            C2C.Reset();
            Src.Reset();
            xEma.Reset();
            base.Reset();
        }
    }
}