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
			localResourceLevels.Magnitude = 1.0f;
			for (int i = 0; i < DisplayForm.NaturalResourceTypesCount; i++)
			{
				if (random.Next() > random.Next())
				{
					baseProduction[i] = 1.0f - (float)random.NextDouble();
				}
				localResourcesDecision[i] = 0.0f;
				inventoryResourcesDecision[i] = 0.0f;
			}

			for (int i = 0; i < DisplayForm.NaturalResourceTypesCount; i++)
			{
				localResourceLevelsProductionModifier[i] = new MatrixMath.Vector(DisplayForm.NaturalResourceTypesCount);
				for (int j = 0; j < DisplayForm.NaturalResourceTypesCount; j++)
				{
					localResourceLevelsProductionModifier[i][j] = baseProduction[j] / (1.0f + localResourceLevels[i]);
				}
			}

			for (int i = DisplayForm.NaturalResourceTypesCount; i < totalResourceCount; i++)
			{
				if (random.Next() > random.Next())
				{
					baseCost[i] = (float)random.NextDouble();
				}
				baseProduction[i] = 0.0f;
				localResourceLevelsProductionModifier[i] = new MatrixMath.Vector(DisplayForm.NaturalResourceTypesCount);
				inventoryResourcesDecision[i] = 0.0f;
			}

			bias = 0.0f;
			HappinessBonus = (float)StatFunctions.GaussRandom(random.NextDouble(), Element.TRAITSPREAD, Element.TRAITSPREAD) - 0.5f;
			HealthBonus = (float)StatFunctions.GaussRandom(random.NextDouble(), Element.TRAITSPREAD, Element.TRAITSPREAD) - 0.5f;
		}
	}
}