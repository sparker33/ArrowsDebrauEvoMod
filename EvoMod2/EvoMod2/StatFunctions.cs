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
		/// <param name="x"> A uniformly distributed random double input. </param>
		/// <param name="lowSpread"> Double spreading factor. Inversely proportional to desired output standard deviation in lower half of output. </param>
		/// <param name="highSpread"> Double spreading factor. Inversely proportional to desired output standard deviation in upper half of output. </param>
		/// <returns> Double output over (0,1) with Gaussian distribution. </returns>
		public static double GaussRandom(double x, double lowSpread, double highSpread)
		{
			return (0.5 * (1.0 - Math.Exp(-lowSpread * x) + Math.Exp(highSpread * (x - 1.0))));
		}

		/// <summary>
		/// Generates a sigmoid funciton output
		/// </summary>
		/// <param name="x"> Input x value of function. </param>
		/// <param name="slope"> Determines steepness. </param>
		/// <param name="shift"> Determines horizontal shift. </param>
		/// <returns> Sigmoid output (y). </returns>
		public static double Sigmoid(double x, double slope, double shift)
		{
			return (1.0 / (1.0 + Math.Exp(slope * (x - shift))));
		}
	}
}
