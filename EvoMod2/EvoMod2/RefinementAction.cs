﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatrixMath;

namespace EvoMod2
{
	public class RefinementAction : Action
	{
		public RefinementAction(int totalResourceCount, Random random, Vector resourceUsageLevels) : base(totalResourceCount)
		{
			float maxUse = 0.01f;
			foreach (float lvl in resourceUsageLevels)
			{
				maxUse += Math.Abs(lvl);
			}
			maxUse /= resourceUsageLevels.Count;

			for (int i = 0; i < DisplayForm.NaturalResourceTypesCount; i++)
			{
				if (Math.Abs(resourceUsageLevels[i] / maxUse) > random.NextDouble())
				{
					baseCost[i] = (1.0f - (float)random.NextDouble()) * resourceUsageLevels[i];
				}
				baseProduction[i] = 0.0f;
				localResourceLevelsProductionModifier[i] = new MatrixMath.Vector(DisplayForm.NaturalResourceTypesCount);
				localResourcesDecision[i] = 0.0f;
				inventoryResourcesDecision[i] = 0.0f;
			}

			for (int i = DisplayForm.NaturalResourceTypesCount; i < totalResourceCount; i++)
			{
				if (Math.Abs(resourceUsageLevels[i] / maxUse) > random.NextDouble())
				{
					baseCost[i] = (1.0f - (float)random.NextDouble()) * resourceUsageLevels[i];
				}
				else if (Math.Abs(resourceUsageLevels[i] / maxUse) < random.NextDouble())
				{
					baseProduction[i] = 1.0f - (float)random.NextDouble();
				}
				localResourceLevelsProductionModifier[i] = new MatrixMath.Vector(DisplayForm.NaturalResourceTypesCount);
				inventoryResourcesDecision[i] = 0.0f;
			}

			float magnitude = 1.0f; //(baseCost.Magnitude + baseProduction.Magnitude) / 2.0f;
			baseCost.Magnitude = magnitude;
			baseProduction.Magnitude = magnitude;

			HappinessBonus = 2.0f * (float)StatFunctions.GaussRandom(random.NextDouble(), Element.TRAITSPREAD, Element.TRAITSPREAD) - 1.0f;
			HealthBonus = 2.0f * (float)StatFunctions.GaussRandom(random.NextDouble(), Element.TRAITSPREAD, Element.TRAITSPREAD) - 1.0f;
			MobilityBonus = 2.0f * (float)StatFunctions.GaussRandom(random.NextDouble(), Element.TRAITSPREAD, Element.TRAITSPREAD) - 1.0f;
			LethalityBonus = 10.0f * (float)StatFunctions.GaussRandom(random.NextDouble(), 0.0, Element.TRAITSPREAD);
		}
	}
}
