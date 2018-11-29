using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatrixMath;

namespace EvoMod2
{
	public class Action
	{
		// Static objects
		public static double ACTIONLEARNRATE;
		public static int ActionTypesCount = 0;

		// Private objects
		protected Vector localResourcesDecision;
		protected Vector inventoryResourcesDecision;
		protected float bias;
		protected Vector lastDecisionLocalResources;
		protected Vector lastDecisionInventoryResources;
		protected Vector baseCost;
		protected Vector baseProduction;
		protected MatrixMath.Matrix localResourceLevelsProductionModifier;
		protected float proficiencyBonus;

		// Public objects
		public int UseCount { get; private set; }
		public int ActionID { get; protected set; }
		public Vector Cost { get => (1.0f / proficiencyBonus) * baseCost; protected set => baseCost = value; }
		public float HappinessBonus { get; protected set; }
		public float HealthBonus { get; protected set; }
		public float MobilityBonus { get; protected set; }
		public float LethalityBonus { get; protected set; }

		/// <summary>
		/// Class constructor for generic action.
		/// </summary>
		/// <param name="totalResourceCount"> Integer number of resource types to be considered. </param>
		public Action(int totalResourceCount)
		{
			UseCount = 0;
			ActionID = ActionTypesCount++;
			localResourcesDecision = new Vector(DisplayForm.NaturalResourceTypesCount);
			for (int i = 0; i < DisplayForm.NaturalResourceTypesCount; i++)
			{
				localResourcesDecision[i] = 0.0f;
			}
			inventoryResourcesDecision = new Vector(totalResourceCount);
			for (int i = 0; i < totalResourceCount; i++)
			{
				inventoryResourcesDecision[i] = 0.0f;
			}
			baseCost = new Vector(totalResourceCount);
			baseProduction = new Vector(totalResourceCount);
			localResourceLevelsProductionModifier = new Matrix(totalResourceCount, DisplayForm.NaturalResourceTypesCount);
			proficiencyBonus = 1.0f;
			lastDecisionLocalResources = new Vector(totalResourceCount);
			lastDecisionInventoryResources = new Vector(totalResourceCount);
		}

		/// <summary>
		/// Copies the base version of this Action. Used when elements learn this action.
		/// </summary>
		/// <returns> A copy of this action with blank decision matrices and reset proficiency. </returns>
		public Action Copy()
		{
			return new Action(this.baseCost, this.baseProduction, this.localResourceLevelsProductionModifier, this);
		}

		/// <summary>
		/// Private class constructor for use by the Copy() method.
		/// </summary>
		/// <param name="cost"> Base cost of this Action. </param>
		/// <param name="production"> Base production of this Action. </param>
		/// <param name="productionMod"> LocalResources productivity modifier for this Action. </param>
		/// <param name="baseAction"> The base Action being coppied. </param>
		protected Action(Vector cost, Vector production, MatrixMath.Matrix productionMod, Action baseAction)
		{
			UseCount = 0;
			proficiencyBonus = 1.0f;
			ActionID = baseAction.ActionID;
			localResourcesDecision = new Vector(DisplayForm.NaturalResourceTypesCount);
			for (int i = 0; i < localResourcesDecision.Count; i++)
			{
				localResourcesDecision[i] = 0.0f;
			}
			inventoryResourcesDecision = new Vector(production.Count);
			for (int i = 0; i < production.Count; i++)
			{
				inventoryResourcesDecision[i] = 0.0f;
			}
			baseProduction = new Vector(production);
			localResourceLevelsProductionModifier = new MatrixMath.Matrix(productionMod);
			baseCost = new Vector(cost);
			HappinessBonus = baseAction.HappinessBonus;
			HealthBonus = baseAction.HealthBonus;
			MobilityBonus = baseAction.MobilityBonus;
			LethalityBonus = baseAction.LethalityBonus;
			lastDecisionLocalResources = new Vector(production.Count);
			lastDecisionInventoryResources = new Vector(production.Count);
		}

		/// <summary>
		/// Method to add a resource to this Action's considerations
		/// </summary>
		public void AddResource()
		{
			inventoryResourcesDecision.Add(0.0f);
			baseCost.Add(0.0f);
			baseProduction.Add(0.0f);
			localResourceLevelsProductionModifier.Add(new Vector(DisplayForm.NaturalResourceTypesCount));
			lastDecisionLocalResources.Add(0.0f);
			lastDecisionInventoryResources.Add(0.0f);
		}

		/// <summary>
		/// Method to remove a resource at the input index from this Action's considerations
		/// </summary>
		/// <param name="index"> Integer index of the resource to be removed. </param>
		public void RemoveResourceAt(int index)
		{
			inventoryResourcesDecision.RemoveAt(index);
			baseCost.RemoveAt(index);
			baseProduction.RemoveAt(index);
			localResourceLevelsProductionModifier.RemoveAt(index);
			lastDecisionLocalResources.RemoveAt(index);
			lastDecisionInventoryResources.RemoveAt(index);
		}

		/// <summary>
		/// Method to retrieve the priority of this Action. Higher priority indicates a better Action.
		/// </summary>
		/// <param name="localResources"> Vector local resource level of the requesting element. </param>
		/// <param name="inventory"> Vector inventory levels of the requesting element. </param>
		/// <returns></returns>
		public float GetActionPriority(Vector localResources, Vector inventory)
		{
			for (int i = 0; i < inventory.Count; i++)
			{
				if (Cost[i] > inventory[i])
				{
					return -1.0f;
				}
			}

			lastDecisionLocalResources = new Vector(localResources);
			lastDecisionLocalResources.Magnitude = 1.0f;
			lastDecisionInventoryResources = new Vector(inventory);
			lastDecisionInventoryResources.Magnitude = 1.0f;

			return (bias + localResourcesDecision * lastDecisionLocalResources + inventoryResourcesDecision * lastDecisionInventoryResources);
		}

		/// <summary>
		/// Method to do the action and return the Vector of returned resources. Action proficiency increases according to Intelligence input.
		/// </summary>
		/// <param name="localResources"></param>
		/// <param name="intelligence"></param>
		/// <returns></returns>
		public Vector DoAction(Vector localResources, float intelligence)
		{
			UseCount++;
			Vector returns = proficiencyBonus * (baseProduction + localResourceLevelsProductionModifier * localResources);
			proficiencyBonus += proficiencyBonus / (100.0f * (1.0f - intelligence) + (float)Math.Exp(100.0 * (proficiencyBonus - 1.0)));
			return returns;
		}

		/// <summary>
		/// Trains this Action priority decision based upon most recent GetActionPriority() request inputs.
		/// </summary>
		/// <param name="deltaHappiness"> Super-signed percent change in Happiness as a result of having chosen this action. </param>
		/// <param name="plasticity"> Input between zero and one to control slope of learning sigmoid function. </param>
		public void Learn(float deltaHappiness, float plasticity)
		{
			float learnCoeff;
			// Train dependence on localResourceLevels
			for (int i = 0; i < localResourcesDecision.Count; i++)
			{
				learnCoeff = deltaHappiness * lastDecisionLocalResources[i];
				localResourcesDecision[i] = 2.0f * (float)StatFunctions.Sigmoid(localResourcesDecision[i] + learnCoeff, -ACTIONLEARNRATE * plasticity, 0.0) - 1.0f;
			}
			// Train dependence on inventoryResourceLevels
			for (int i = 0; i < inventoryResourcesDecision.Count; i++)
			{
				learnCoeff = deltaHappiness * lastDecisionInventoryResources[i];
				lastDecisionInventoryResources[i] = 2.0f * (float)StatFunctions.Sigmoid(lastDecisionInventoryResources[i] + learnCoeff, -ACTIONLEARNRATE * plasticity, 0.0) - 1.0f;
			}
			// Train general bias
			learnCoeff = deltaHappiness * bias;
			bias = 2.0f * (float)StatFunctions.Sigmoid(bias + learnCoeff, -ACTIONLEARNRATE * plasticity, 0.0) - 1.0f;
		}
	}
}
