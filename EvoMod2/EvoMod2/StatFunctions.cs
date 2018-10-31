using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoMod2
{
	public static class StatFunctions
	{
		/// <summary>
		/// Converts an input uniformly distributed random double into a double output with Gaussian distribution
		/// </summary>
		/// <param name="random"> A uniformly distributed random double. </param>
		/// <param name="spread"> Double spreading factor. Inversely proportional to desired output standard deviation. </param>
		/// <returns> Double output over (0,1) with Gaussian distribution. </returns>
		public static double GaussRandom(double random, double spread)
		{
			return (0.5 * (Math.Exp(-spread * random) - Math.Exp(spread * (random - 1.0)) + 1.0));
		}
	}
}
