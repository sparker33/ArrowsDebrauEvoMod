using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoMod2
{
	public struct HappinessWeights
	{
		public float Wealth { get; set; }
		public float Health { get; set; }
		public float Location { get; set; }

		public float this[int index]
		{
			get
			{
				if (index == 0)
				{
					return Wealth;
				}
				else if (index == 1)
				{
					return Health;
				}
				else if (index == 2)
				{
					return Location;
				}
				else
				{
					throw new Exception("Bad index in HappinessWeights");
				}
			}
			set
			{
				if (index == 0)
				{
					Wealth = value;
				}
				else if (index == 1)
				{
					Health = value;
				}
				else if (index == 2)
				{
					Location = value;
				}
				else
				{
					throw new Exception("Bad index in HappinessWeights");
				}
			}
		}
	}
}
