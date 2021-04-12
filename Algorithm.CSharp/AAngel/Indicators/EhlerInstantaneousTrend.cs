/*
//
// @author LazyBear 
// 
// List of my public indicators: http://bit.ly/1LQaPK8 
// List of my app-store indicators: http://blog.tradingview.com/?p=970 
//
study(title="Ehlers Instantaneous Trend [LazyBear]", shorttitle="EIT_LB", overlay=true, precision=3)
src=input(hl2, title="Source")
a= input(0.07, title="Alpha", step=0.01) 
fr=input(false, title="Fill Trend Region")
ebc=input(false, title="Enable barcolors")
hr=input(false, title="Hide Ribbon")
it=(a-((a*a)/4.0))*src+0.5*a*a*Src[1]-(a-0.75*a*a)*Src[2]+2*(1-a )*nz(it[1], ((src+2*Src[1]+Src[2])/4.0))-(1-a )*(1-a )*nz(it[2], ((src+2*Src[1]+Src[2])/4.0))
lag=2.0*it-nz(it[2])
dl=plot(fr and (not hr)?(it>lag?lag:it):na, color=gray, style=circles, linewidth=0, title="Dummy")
itl=plot(hr?na:it, color=fr?gray:red, linewidth=1, title="Trend")
ll=plot(hr?na:lag, color=fr?gray:blue, linewidth=1, title="Trigger")
fill(dl, ll, green, title="UpTrend", transp=70)
fill(dl, itl, red, title="DownTrend", transp=70)
bc=not ebc?na:(it>lag?red:lime)
barcolor(bc)


*/

using QuantConnect.Data.Market;
using System;

namespace QuantConnect.Indicators
{
	
	public enum Source
	{
		Close = 0,
		HL2,
		Low,
		High
	}
    /// <summary>
    /// The Aroon Oscillator is the difference between AroonUp and AroonDown. The value of this
    /// indicator fluctuates between -100 and +100. An upward trend bias is present when the oscillator
    /// is positive, and a negative trend bias is present when the oscillator is negative. AroonUp/Down
    /// values over 75 identify strong trends in their respective direction.
    /// </summary>
    public class EhlerInstantaneousTrend : BarIndicator, IIndicatorWarmUpPeriodProvider
    {
        public RollingWindow<decimal> It = new RollingWindow<decimal>(4);
        public RollingWindow<decimal> Src = new RollingWindow<decimal>(4);

        /// <summary>
        /// Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady => It.IsReady && Src.IsReady;

        /// <summary>
        /// Required period, in data points, for the indicator to be ready and fully initialized.
        /// </summary>
        public int WarmUpPeriod { get; }


		
        /// <summary>
        /// Creates a new EhlerTrend from the specified up/down periods.
        /// </summary>
        /// <param name="upPeriod">The lookback period to determine the highest high for the AroonDown</param>
        /// <param name="downPeriod">The lookback period to determine the lowest low for the AroonUp</param>
        public EhlerInstantaneousTrend(Source source=Source.Close, decimal alpha=0.07m)
            : this($"ECCT({source},{alpha})", source, alpha)
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
        public EhlerInstantaneousTrend(string name, Source source, decimal alpha=0.07m)
            : base(name)
        {
            // var max = new Maximum(name + "_Max", upPeriod + 1);
            // AroonUp = new FunctionalIndicator<IndicatorDataPoint>(name + "_AroonUp",
            //     input => ComputeAroonUp(upPeriod, max, input),
            //     aroonUp => max.IsReady,
            //     () => max.Reset()
            //     );

            // var min = new Minimum(name + "_Min", downPeriod + 1);
            // AroonDown = new FunctionalIndicator<IndicatorDataPoint>(name + "_AroonDown",
            //     input => ComputeAroonDown(downPeriod, min, input),
            //     aroonDown => min.IsReady,
            //     () => min.Reset()
            //     );
			
			_source = source;
			_alpha = alpha;
			
            WarmUpPeriod = 11;
        }

        /// <summary>
        /// Computes the next value of this indicator from the given state
        /// </summary>
        /// <param name="input">The input given to the indicator</param>
        /// <returns>A new value for this indicator</returns>
        public decimal InstantaneousTrend { get; set;}
        public decimal Lag { get; set;}
        
        protected override decimal ComputeNextValue(IBaseDataBar input)
        {
            // AroonUp.Update(input.Time, input.High);
            // AroonDown.Update(input.Time, input.Low);
            var src = GetSource(input);
            Src.Add(src);
            
            if(Src.IsReady)
            {
	            var it1 = (It.IsReady) ? It[0] :  ((src+2*Src[1]+Src[2])/4);
	            var it2 = (It.IsReady) ? It[1] :  ((src+2*Src[1]+Src[2])/4);
	            
				var it = (_alpha - ((_alpha*_alpha)/4)) * src 
							+ 0.5m *_alpha*_alpha * Src[1]
							- (_alpha - 0.75m * _alpha * _alpha) * Src[2]
							+ 2 * (1-_alpha ) * it1
							- (1 - _alpha ) * (1 - _alpha ) * it2;
				Lag = 2 * it - it2;
				It.Add(it);
				InstantaneousTrend = it;
            }
			return Lag - InstantaneousTrend;
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
            It.Reset();
            Src.Reset();
            base.Reset();
        }
    }
}