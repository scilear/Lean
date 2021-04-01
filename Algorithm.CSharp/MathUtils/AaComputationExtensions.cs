using System;
using System.Collections.Generic;
using System.Linq;


namespace QuantConnect.Algorithm.CSharp
{
    public static class AaComputationExtensions
    {
        public static decimal[] RollingMax(this decimal[] array, int period)
        {
        	if (period == 0)
        		return (decimal[])array.Clone();
        	var answer = new decimal[array.Length];
            var rollingData = new Queue<decimal>();
            int count = 0;
            foreach (var v in array)
            {
                count++;
                rollingData.Enqueue(v);
                if (count > period)
                {
                    rollingData.Dequeue();
                }
                answer[count - 1] = rollingData.Max();
            }
            return answer;
        }
	
	 	public static double[] RollingMax(this double[] array, int period)
        {
            var answer = new double[array.Length];
            var rollingData = new Queue<double>();
            //for (int i = 0; i < array.Length; i++)
            int count = 0;
            foreach (var v in array)
            {
                count++;
                rollingData.Enqueue(v);
                if (count > period)
                {
                    rollingData.Dequeue();
                }
                answer[count - 1] = rollingData.Max();
            }
            return answer;
        }
        
        public static double StdDev(this IEnumerable<double> values)
        {
            double ret = 0;
            if (values.Any())
            {
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }

        public static decimal StdDev(this IEnumerable<decimal> values)
        {
            double ret = 0;
            if (values.Any())
            {
                //Compute the Average      
                decimal avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                decimal sum2 = values.Sum(d => (decimal)Math.Pow((double)(d - avg), 2));
                decimal sum = values.Sum(d => (d - avg) * (d - avg));
                //Put it all together      
                ret = Math.Sqrt((double)(sum) / (values.Count() - 1));
            }
            return (decimal)ret;
        }

        public static decimal[] RollingStd(this IEnumerable<decimal> array, int period)
        {
            return array.ToArray().RollingStd(period);
        }

        public static decimal[] LastN(this decimal[] array, int count)
        {
            if (count > array.Length)
                return null;
            var lastN = new decimal[count];
            Array.Copy(array, array.Length - count, lastN, 0, count);
            return lastN;
        }
        
        public static double[] LastN(this double[] array, int count)
        {
            if (count > array.Length)
                return null;
            var lastN = new double[count];
            Array.Copy(array, array.Length - count, lastN, 0, count);
            return lastN;
        }
        
        public static decimal[] RollingStd(this decimal[] array, int period)
        {
            var answer = new decimal[array.Length];
            var rollingData = new Queue<decimal>();
            //for (int i = 0; i < array.Length; i++)
            int count = 0;
            foreach (var v in array)
            {
                count++;
                rollingData.Enqueue(v);
                if (count >= period)
                {
                    rollingData.Dequeue();
                    answer[count - 1] = rollingData.StdDev();
                }
            }
            return answer;
        }

        public static double[] PctChange(this double[] array, int period = 1)
        {
            var result = new double[array.Length];
            var prevs = new Queue<double>();
            int count = 0;
            foreach (var v in array)
            {
                if (count >= period)
                {
                    result[count] = v / prevs.ElementAt(0) - 1;
                    prevs.Dequeue();
                }
                else
                {
                    result[count] = 0;
                }
                prevs.Enqueue(v);
                count++;
            }
            return result;
        }
    }
}