using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatrixMath;

namespace EvoMod2
{
	public class HarvestAction : Action
	{
		public HarvestAction(int totalResourceCount, Random random, Vector localResourceLevels) : base(totalResourceCount)
		{
			/*
			 * Initializes baseProduction in order to determine resource production priorities.
			 * Fills out localResourceLevelsProductionModifier according to production priorities and local resource levels.
			 * Clears baseProduction. This is done to make "HarvestAction" results more strictly tied to location.
			 */
			localResourceLevels.Magnitude = 1.0f;
			for (int i = 0; i < DisplayForm.NaturalResourceTypesCount; i++)
			{
				baseProduction[i] = 0.0f;
			}
			for (int i = 0; i < DisplayForm.NaturalResourceTypesCount; i++)
			{
				localResourceLevelsProductionModifier[i] = new MatrixMath.Vector(DisplayForm.NaturalResourceTypesCount);
				for (int j = 0; j < DisplayForm.NaturalResourceTypesCount; j++)
				{
					if (random.NextDouble() > 0.5)
					{
						localResourceLevelsProductionModifier[i][j] = (1.0f - (float)random.NextDouble()) * localResourceLevels[i] / DisplayForm.NaturalResourceTypesCount;
					}
				}
				localResourcesDecision[i] = 0.0f;
				inventoryResourcesDecision[i] = 0.0f;
			}

			for (int i = DisplayForm.NaturalResourceTypesCount; i < totalResourceCount; i++)
			{
				if (random.NextDouble() > 0.5)
				{
					baseCost[i] = 1.0f - (float)random.NextDouble();
					// If derived resources are being used in this action, then some baseProduction is possible
					for (int j = 0; j < DisplayForm.NaturalResourceTypesCount; j++)
					{
						if (random.NextDouble() > 0.5)
						{
							baseProduction[j] += (1.0f - (float)random.NextDouble()) / (totalResourceCount - DisplayForm.NaturalResourceTypesCount);
						}
					}
				}
				baseProduction[i] = 0.0f;
				localResourceLevelsProductionModifier[i] = new MatrixMath.Vector(DisplayForm.NaturalResourceTypesCount);
				inventoryResourcesDecision[i] = 0.0f;
			}

			HappinessBonus = (float)StatFunctions.GaussRandom(random.NextDouble(), Element.TRAITSPREAD, Element.TRAITSPREAD) - 0.5f;
			HealthBonus = (float)StatFunctions.GaussRandom(random.NextDouble(), Element.TRAITSPREAD, Element.TRAITSPREAD) - 0.5f;
		}
	}
}