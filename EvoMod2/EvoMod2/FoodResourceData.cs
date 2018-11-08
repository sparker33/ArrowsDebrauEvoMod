using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoMod2
{
	public class FoodResourceData
	{
		public int ResourceIndex { get; private set; }
		public float Nourishment { get; private set; }

		public FoodResourceData(int index, float nourishment)
		{
			ResourceIndex = index;
			Nourishment = nourishment;
		}
	}
}
