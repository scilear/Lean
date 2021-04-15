/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System.Linq;

namespace QuantConnect.Indicators
{
    /// <summary>
    /// This indicator creates a moving average (middle band) with an upper band and lower band
    /// fixed at k standard deviations above and below the moving average.
    /// </summary>
    public class RollingBB : Indicator, IIndicatorWarmUpPeriodProvider
    {
        /// <summary>
        /// Gets the type of moving average
        /// </summary>
        public BollingerBands BB { get; }

        
           
        public decimal LowerBand { get
	        {
	        	return LB.Max();
	        }
         }
        public decimal UpperBand { get
	        {
	        	return UB.Min();
	        }
         }  
        
        public RollingWindow<decimal> LB { get; }
        public RollingWindow<decimal> UB { get; }

       
        public RollingBB()
            : this($"R_BB")
        {
        }

       
        public RollingBB(string name)
            : base(name)
        {
        	BB = new BollingerBands(20, 2, MovingAverageType.Exponential);
            WarmUpPeriod = 20*2;
            
            LB = new RollingWindow<decimal>(20);
            UB = new RollingWindow<decimal>(20);
        }

        /// <summary>
        /// Gets a flag indicating when this indicator is ready and fully initialized
        /// </summary>
        public override bool IsReady => LB.IsReady && UB.IsReady;

        /// <summary>
        /// Required period, in data points, for the indicator to be ready and fully initialized.
        /// </summary>
        public int WarmUpPeriod { get; }

        /// <summary>
        /// Computes the next value of the following sub-indicators from the given state:
        /// StandardDeviation, MiddleBand, UpperBand, LowerBand, BandWidth, %B
        /// </summary>
        /// <param name="input">The input given to the indicator</param>
        /// <returns>The input is returned unmodified.</returns>
        protected override decimal ComputeNextValue(IndicatorDataPoint input)
        {
            BB.Update(input);
            LB.Add(BB.LowerBand);
            UB.Add(BB.UpperBand);
            return BB.MiddleBand;
        }

        /// <summary>
        /// Resets this indicator and all sub-indicators (StandardDeviation, LowerBand, MiddleBand, UpperBand, BandWidth, %B)
        /// </summary>
        public override void Reset()
        {
            BB.Reset();
            LB.Reset();
            UB.Reset();
            
            base.Reset();
        }
    }
}