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
					baseCost[i] = (float)random.NextDouble();
				}
				else
				{
					baseProduction[i] = (float)random.NextDouble();
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
				baseCost[i] = (float)random.NextDouble();
				baseProduction[i] = 0.0f;
				localResourceLevelsProductionModifier[i] = new MatrixMath.Vector(DisplayForm.NaturalResourceTypesCount);
				inventoryResourcesDecision[i] = 0.0f;
			}

			float magnitude = (baseCost.Magnitude + baseProduction.Magnitude) / 2.0f;
			baseCost.Magnitude = magnitude;
			baseProduction.Magnitude = magnitude;

			bias = 0.0f;
			HappinessBonus = 2.0f * (float)StatFunctions.GaussRandom(random.NextDouble(), 10.0, 10.0) - 1.0f;
			HealthBonus = 2.0f * (float)StatFunctions.GaussRandom(random.NextDouble(), 10.0, 10.0) - 1.0f;
			MobilityBonus = 2.0f * (float)StatFunctions.GaussRandom(random.NextDouble(), 10.0, 10.0) - 1.0f;
		}
	}
}
