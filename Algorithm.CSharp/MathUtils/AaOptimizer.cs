using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;
using Accord.Math.Optimization;
using Accord.Math.Differentiation;
using Accord.Statistics;


namespace QuantConnect.Algorithm.CSharp
{
    public static class AaOptimizer
    {
        
		public static double[] MergeVector(double [] a, double [] b)
		{
			return a.Concatenate(b);
		}
		
        
        public static double[] OptimiseWeights(double[,] allHistoryBars, double [] retNorm)
        {
            var covMatrix = allHistoryBars.Transpose().Covariance();
            
            var maxRet = 0.9 * retNorm.Max();
            
            
			Func<double[], double> retNormFunc = (x) => x.Dot(retNorm) - maxRet;
			
            // The scoring function
            var f = new NonlinearObjectiveFunction(covMatrix.GetLength(0), x => x.DotAndDot(covMatrix, x));
            // Under the following constraints
            var constraints = new[]
            {
                //the sum of all weights is equal to 1 (ish)
                //using Enumerable sum because of Accord.Math extension redefinition
                //https://stackoverflow.com/questions/32380592/why-am-i-required-to-reference-system-numerics-with-this-simple-linq-expression
                new NonlinearConstraint(covMatrix.GetLength(0), x => -Math.Pow(Enumerable.Sum(x) - 1, 2) >= -1e-6),
				new NonlinearConstraint(covMatrix.GetLength(0), x =>  retNormFunc(x) >= 0),
                //the values are bounded between 0 and 1
                new NonlinearConstraint(covMatrix.GetLength(0), x => x.Min() >= -1),
                new NonlinearConstraint(covMatrix.GetLength(0), x => -x.Max() >= -1),
            };
			
			
			//var nbVar =3;
			
			//NelderMead algo = new NelderMead(f);
			
			
            
   //         var function  = new NonlinearObjectiveFunction(covMatrix.GetLength(0),
   //             function: (x) => x.DotAndDot(covMatrix, x)//,
   //             //gradient: (x) => x.DotProduct(covMatrix, x)
   //         );
            
			// var inner = new BroydenFletcherGoldfarbShanno(4);
   //         inner.LineSearch = LineSearch.BacktrackingArmijo;
   //         inner.Corrections = 10;

   //         var algo = new AugmentedLagrangian(inner, function, constraints);

            
   //         bool s = algo.Minimize();
            
			//var g =  new FiniteDifferences(nbVar, f);
            //
            //var algo = new NonlinearConjugateGradient (f, constraints);
            //var algo = new BroydenFletcherGoldfarbShanno(numberOfVariables: nbVar, function: f, gradient: g);
            //var algo = new AugmentedLagrangian(f, constraints);
            // Optimize it
            //TODO handle !success

			
			
			var algo = new Cobyla(f, constraints);
			bool success = algo.Minimize();
			if (!success) return null;
            double minimum = algo.Value;
            double[] weights = algo.Solution;

            
            return weights;
        }
    }
}