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
		/// <param name="lowSpread"> Double spreading factor. Inversely proportional to desired output standard deviation in lower half of output. </param>
		/// <param name="highSpread"> Double spreading factor. Inversely proportional to desired output standard deviation in upper half of output. </param>
		/// <returns> Double output over (0,1) with Gaussian distribution. </returns>
		public static double GaussRandom(double random, double lowSpread, double highSpread)
		{
			return (0.5 * (1.0 - Math.Exp(-lowSpread * random) + Math.Exp(highSpread * (random - 1.0))));
		}
	}
}
