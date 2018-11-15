﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using MatrixMath;

namespace EvoMod2
{
	public class Element
	{
		// Public static fields
		public static float MIDDLEAGE;
		public static float ELESPEED;
		public static float TRAITSPREAD;
		public static float INTERACTCOUNT;
		public static int INTERACTRANGE;
		public static double RELATIONSHIPSCALE;
		public static float FOODREQUIREMENT;
		public static float STARTRESOURCES;
		public static int MAXRESOURCECOUNT;
		public static List<FoodResourceData> FoodResources = new List<FoodResourceData>();

		// Private fields
		private PointF position = new PointF();
		private Kinematics kinematics = new Kinematics(2);
		private Destination destination = new Destination();
		private float timePreference;
		private float lethalityBonus;
		private float happinessPercentChangeHistory;
		private float happinessBonus;
		private HappinessWeights happinessWeights = new HappinessWeights(); // 0: Wealth, 1: Health, 2: Location
		private Dictionary<Element, float> relationships = new Dictionary<Element, float>();
		private Vector inventory;
		private Vector prices;
		private Vector pricesUncertainty;
		private Vector cumulativePriceExperience;
		private Vector foodConsumptionRates;
		private Vector resourceUse;
		private float healthHappiness { get => happinessWeights.Health * Health / MIDDLEAGE; }
		private float wealthHappiness { get => happinessWeights.Wealth * (inventory * prices) / (inventory.Count * STARTRESOURCES * prices.Magnitude); }

		// Public objects
		// Fixed traits
		public float Intelligence { get; private set; }
		public float Conscientiousness { get; private set; }
		public float Agreeableness { get; private set; }
		public float Neuroticism { get; private set; }
		public float Openness { get; private set; }
		public float Extraversion { get; private set; }
		public Element[] Parents = new Element[2];
		private float _health;
		// Dynamic traits
		public float Happiness { get; private set; }
		public float Health
		{
			get
			{
				if (Single.IsPositiveInfinity(_health))
				{
					_health = 2.0f * MIDDLEAGE;
				}
				else if (Single.IsNegativeInfinity(_health))
				{
					_health = -2.0f * MIDDLEAGE;
				}
				else if (Single.IsNaN(_health))
				{
					_health = 0.0f;
				}
				return _health;
			}
			private set
			{
				_health = value;
			}
		}
		public float Mobility { get; private set; }
		public int Age { get; private set; }
		public List<PointF> KnownLocations { get; private set; }
		public List<Action> KnownActions { get; private set; }
		public bool IsDead { get; private set; }
		public int TurnsSinceMurder { get; private set; }
		public float Lethality { get => Health + lethalityBonus; }
		// Display data and general accessors
		public PointF Position { get => position; }
		public int Size { get => (int)Math.Max(3.0f, Math.Min(25.0f, wealthHappiness / HappinessWeights.Health)); }
		public Color ElementColor { get; private set; }
		public HappinessWeights HappinessWeights { get => happinessWeights; }
		public Vector Inventory { get => inventory; }
		/// <summary>
		/// Private default class constructor. Not intended for use
		/// </summary>
		private Element()
		{

		}

		/// <summary>
		/// Basic class constructor with initial physics configuration
		/// </summary>
		/// <param name="random"> Randomizer. </param>
		public Element(Random random, List<Resource> environmentResources)
		{
			Parents[0] = this;
			Parents[1] = this;
			Age = 0;
			IsDead = false;
			lethalityBonus = 0.0f;
			TurnsSinceMurder = 0;

			position.X = (float)(random.NextDouble() * DisplayForm.SCALE);
			position.Y = (float)(random.NextDouble() * DisplayForm.SCALE);
			KnownLocations = new List<PointF>();
			KnownLocations.Add(position);
			int r = (int)(255.0 / (1.0 + Math.Exp((15.0 / DisplayForm.SCALE) * (position.X - DisplayForm.SCALE / 2.0))));
			int g = (int)(255.0 / (1.0 + Math.Exp((15.0 / DisplayForm.SCALE) * (position.X * position.Y / (2 * DisplayForm.SCALE * DisplayForm.SCALE)))));
			int b = (int)(255.0 / (1.0 + Math.Exp((15.0 / DisplayForm.SCALE) * (position.Y - DisplayForm.SCALE / 2.0))));
			ElementColor = Color.FromArgb(r, g, b);

			double rand = 1.0 - random.NextDouble();
			Intelligence = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = 1.0 - random.NextDouble();
			Conscientiousness = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = 1.0 - random.NextDouble();
			Agreeableness = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = 1.0 - random.NextDouble();
			Neuroticism = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = 1.0 - random.NextDouble();
			Openness = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = 1.0 - random.NextDouble();
			Extraversion = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);

			rand = random.NextDouble();
			Health = MIDDLEAGE / 4.0f * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD) / 10.0f;
			Mobility = 1.0f;

			happinessWeights[0] = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = random.NextDouble();
			happinessWeights[1] = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = random.NextDouble();
			happinessWeights[2] = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);

			inventory = new Vector(DisplayForm.NaturalResourceTypesCount);
			for (int i = 0; i < inventory.Count; i++)
			{
				rand = random.NextDouble();
				inventory[i] = STARTRESOURCES * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			}
			prices = new Vector(DisplayForm.NaturalResourceTypesCount);
			pricesUncertainty = new Vector(DisplayForm.NaturalResourceTypesCount);
			cumulativePriceExperience = new Vector(DisplayForm.NaturalResourceTypesCount);
			for (int i = 0; i < inventory.Count; i++)
			{
				rand = 1.0 - random.NextDouble();
				prices[i] = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
				pricesUncertainty[i] = prices[i];
				cumulativePriceExperience[i] = inventory[i];
			}
			foodConsumptionRates = new Vector(FoodResources.Count);
			for (int i = 0; i < FoodResources.Count; i++)
			{
				foodConsumptionRates[i] = FOODREQUIREMENT / FoodResources.Count;
			}
			timePreference = (float)StatFunctions.GaussRandom(Intelligence, TRAITSPREAD, TRAITSPREAD);
			resourceUse = new Vector(DisplayForm.NaturalResourceTypesCount);
			KnownActions = new List<Action>();
			KnownActions.Add(new HarvestAction(inventory.Count, DisplayForm.GLOBALRANDOM, GetLocalResourceLevels(environmentResources)));
		}

		/// <summary>
		/// Method to add a resource to the list of possible resources and all affected components.
		/// </summary>
		public void AddResource(bool isFood)
		{
			resourceUse.Add(0.0f);
			inventory.Add(0.0f);
			prices.Add(Openness);
			pricesUncertainty.Add(Openness);
			cumulativePriceExperience.Add(Intelligence);
			for (int i = 0; i < KnownActions.Count; i++)
			{
				KnownActions[i].AddResource();
			}
			if (isFood)
			{
				foodConsumptionRates.Add(FOODREQUIREMENT / FoodResources.Count);
			}
		}

		/// <summary>
		/// Method to have this element consume food, experience effects from hunger/stiation, and train consumption habits accordingly.
		/// </summary>
		public void Eat()
		{
			float prevWealthHappiness = wealthHappiness;
			float prevHealthHappiness = healthHappiness;
			float hunger = FOODREQUIREMENT;
			float[] consumption = new float[FoodResources.Count];
			for (int i = 0; i < FoodResources.Count; i++)
			{
				consumption[i] = foodConsumptionRates[i] * prices[FoodResources[i].ResourceIndex] / FoodResources[i].Nourishment;
				if (consumption[i] > inventory[FoodResources[i].ResourceIndex])
				{
					consumption[i] = inventory[FoodResources[i].ResourceIndex];
				}
				inventory[FoodResources[i].ResourceIndex] -= consumption[i];
				hunger -= consumption[i] * FoodResources[i].Nourishment;
			}
			float temp0 = hunger / FOODREQUIREMENT;
			Health -= temp0 * temp0;
			happinessBonus -= temp0;

			float deltaWealth = prevWealthHappiness - wealthHappiness;
			float trainingMetric = 0.0f;
			if (deltaWealth != 0.0f)
			{
				trainingMetric = (healthHappiness - prevHealthHappiness) / deltaWealth;
			}
			if ((FOODREQUIREMENT - hunger) != 0.0f)
			{
				for (int i = 0; i < foodConsumptionRates.Count; i++)
				{
					if (foodConsumptionRates[i] == 0.0f)
					{
						foodConsumptionRates[i] += consumption[i] * (float)StatFunctions.Sigmoid(trainingMetric, Math.Abs(hunger), 0.0) * Math.Sign(hunger)
							* FOODREQUIREMENT / (FOODREQUIREMENT - hunger);
					}
					else
					{
						foodConsumptionRates[i] *= (1.0f + consumption[i] * (float)StatFunctions.Sigmoid(trainingMetric, Math.Abs(hunger), 0.0) * Math.Sign(hunger)
							* FOODREQUIREMENT / (FOODREQUIREMENT - hunger));
					}
				}
			}
			// Reflect food eaten in resourceUse record
			for (int i = 0; i < FoodResources.Count; i++)
			{
				resourceUse[FoodResources[i].ResourceIndex] += timePreference * consumption[i];
			}
		}

		/// <summary>
		/// Method to updte the position of this element
		/// </summary>
		public void Move()
		{
			float[] temp = new float[2]; // Utility array to hold destination distance, accelleration, and displacement

			// Check for destination acquisition
			if (destination.IsEmpty && StatFunctions.GaussRandom(DisplayForm.GLOBALRANDOM.NextDouble(), 10.0 * Openness, 1.0 / Openness) > 0.85)
			{
				PointF newLocation = new PointF();
				destination.Set(this.position, newLocation);
			}
			float progress = destination.GetProgress(position);
			if (!destination.IsEmpty && StatFunctions.Sigmoid(DisplayForm.GLOBALRANDOM.NextDouble(), 5.0, progress) < 0.5)
			{
				destination.Clear();
			}

			if (destination.IsEmpty)
			{
				kinematics.Damping = Kinematics.DEFAULTDAMPING;
				temp[0] = 0.0f;
				temp[1] = 0.0f;
			}
			else if (progress == 0.0f)
			{
				destination.Clear();
				temp[0] = 0.0f;
				temp[1] = 0.0f;
			}
			else
			{
				kinematics.Damping = Kinematics.DEFAULTDAMPING - progress;
				temp[0] = (destination.X - position.X) / DisplayForm.SCALE;
				temp[1] = (destination.Y - position.Y) / DisplayForm.SCALE;
			}

			// Update Happiness
			float environmentHappiness = 0.0f;
			foreach (Element e in relationships.Keys)
			{
				try
				{
					environmentHappiness += relationships[e]
							* (float)Math.Sqrt((position.X - e.Position.X) * (position.X - e.Position.X) + (position.Y - e.Position.Y) * (position.Y - e.Position.Y))
							 / ((float)RELATIONSHIPSCALE * INTERACTRANGE / Mobility);
				}
				catch (NullReferenceException)
				{
					continue;
				}
			}

			environmentHappiness = environmentHappiness * happinessWeights[2];
			float nextHappiness = timePreference * (happinessBonus
				+ wealthHappiness
				+ healthHappiness
				+ environmentHappiness) / 4.0f
				+ (1.0f - timePreference) * Happiness;
			if (Happiness <= 0.1f && Happiness >= -0.1f)
			{
				happinessPercentChangeHistory = Math.Sign(nextHappiness - Happiness) * 0.001f;
			}
			else
			{
				happinessPercentChangeHistory = (nextHappiness - Happiness) / Happiness;
			}
			Happiness = nextHappiness;

			// Determine driving force vector
			float speed = kinematics.Speed;
			if (speed >= 1.0f)
			{
				// Update acceleratons
				temp[0] += (happinessPercentChangeHistory * kinematics.GetVelocity(0) / speed);
				temp[1] += (happinessPercentChangeHistory * kinematics.GetVelocity(1) / speed);
			}
			else
			{
				temp[0] = (float)DisplayForm.GLOBALRANDOM.NextDouble();
				temp[1] = (float)DisplayForm.GLOBALRANDOM.NextDouble();
			}
			temp[0] *= ELESPEED;
			temp[1] *= ELESPEED;

			// Apply force vector to kinematics; get and apply displacements
			temp = kinematics.GetDisplacement(temp, 10.0f).ToArray();
			position.X += temp[0];
			position.Y += temp[1];

			// Handle domain boundary collisions
			if (position.X < 0.0f)
			{
				position.X = 0.0f;
				kinematics.ReverseDirection(0);
			}
			if (position.X > DisplayForm.SCALE)
			{
				position.X = (float)DisplayForm.SCALE;
				kinematics.ReverseDirection(0);
			}
			if (position.Y < 0.0f)
			{
				position.Y = 0.0f;
				kinematics.ReverseDirection(1);
			}
			if (position.Y > DisplayForm.SCALE)
			{
				position.Y = (float)DisplayForm.SCALE;
				kinematics.ReverseDirection(1);
			}
		}

		/// <summary>
		/// Helper function to turn the list of environment resources into a vector of resource levels at this element's location
		/// </summary>
		/// <param name="resources"> List of Resource entities reflecting all environment resources. </param>
		/// <returns> Vector of local resource levels. </returns>
		private Vector GetLocalResourceLevels(List<Resource> resources)
		{
			Vector localResourceLevels = new Vector(resources.Count);
			for (int i = 0; i < resources.Count; i++)
			{
				foreach (ResourceKernel k in resources[i])
				{
					localResourceLevels[i] += k.GetResourceLevelAt(position);
				}
			}
			return localResourceLevels;
		}

		/// <summary>
		/// Method to handle all checking and execution of actions for this element
		/// </summary>
		/// <param name="environmentResources"> List of Resource objects in the environment. </param>
		/// <param name="elements"> List of all elements in simulation. </param>
		/// <returns> A FoodResourceData object to be added to the </returns>
		public bool? DoAction(List<Resource> environmentResources, List<Element> elements)
		{
			bool? resourceDiscovered = null;
			if (DisplayForm.GLOBALRANDOM.NextDouble() < Conscientiousness)
			{
				bool didAction = false;
				// Populate the local resource levels
				Vector localResourceLevels = GetLocalResourceLevels(environmentResources);
				// Decide which action to do
				float maxActionPriority = -1.0f;
				int actionChoice = 0;
				for (int i = 0; i < KnownActions.Count; i++)
				{
					float priority = KnownActions[i].GetActionPriority(localResourceLevels, inventory);
					if (priority > maxActionPriority)
					{
						actionChoice = i;
						maxActionPriority = priority;
					}
				}
				// If any actions are available, execute the preferred action
				if (maxActionPriority > -1.0f)
				{
					didAction = true;
					// Apply action effects
					inventory -= KnownActions[actionChoice].Cost;
					Vector productionUtilityVector = KnownActions[actionChoice].DoAction(localResourceLevels, Intelligence);
					float inventoryValueUtilityVar = inventory * prices;
					inventory += productionUtilityVector;
					if (inventoryValueUtilityVar != 0.0f)
					{
						inventoryValueUtilityVar = (inventoryValueUtilityVar - inventory * prices) / inventoryValueUtilityVar;
					}
					happinessBonus += KnownActions[actionChoice].HappinessBonus;
					Health += KnownActions[actionChoice].HealthBonus;
					Mobility += KnownActions[actionChoice].MobilityBonus;
					lethalityBonus += KnownActions[actionChoice].LethalityBonus;
					// Update resource usage information and apply learning to action
					KnownActions[actionChoice].Learn(Math.Sign(Happiness) * (happinessBonus + happinessWeights[0] * inventoryValueUtilityVar) / (Happiness + 1.0f));
					productionUtilityVector = KnownActions[actionChoice].Cost - productionUtilityVector;
					resourceUse += timePreference * productionUtilityVector;
					// Check for new Resource discovery
					if ((Math.Exp(27.2f * (inventory.Count - MAXRESOURCECOUNT)))
						< StatFunctions.GaussRandom(DisplayForm.GLOBALRANDOM.NextDouble(), 25.0 * (Intelligence + Openness), 100.0 / (Intelligence + Openness)))
					{
						resourceDiscovered = false; // Not a food resource (null is no resource)
						if (DisplayForm.GLOBALRANDOM.NextDouble() > 0.8)
						{
							FoodResources.Add(new FoodResourceData(inventory.Count - 1, 1.0f - (float)DisplayForm.GLOBALRANDOM.NextDouble()));
							resourceDiscovered = true; // Is a food resource (null is no resource)
						}
					}
					// Check for new Action discovery (can discover either Harvest or Refinement Action)
					if (0.5 < StatFunctions.GaussRandom(DisplayForm.GLOBALRANDOM.NextDouble(), 5.0 * (Intelligence + Openness), 20.0 / (Intelligence + Openness)))
					{
						if (DisplayForm.GLOBALRANDOM.NextDouble() > 0.5)
						{
							KnownActions.Add(new HarvestAction(inventory.Count, DisplayForm.GLOBALRANDOM, localResourceLevels));
						}
						else
						{
							KnownActions.Add(new RefinementAction(inventory.Count, DisplayForm.GLOBALRANDOM, productionUtilityVector));
						}
					}
				}
				// Check for new HarvestAction discovery
				if (!didAction && 0.5 < StatFunctions.GaussRandom(DisplayForm.GLOBALRANDOM.NextDouble(), 5.0 * (Intelligence + Openness), 20.0 / (Intelligence + Openness)))
				{
					KnownActions.Add(new HarvestAction(inventory.Count, DisplayForm.GLOBALRANDOM, localResourceLevels));
				}
			}

			return resourceDiscovered;
		}

		/// <summary>
		/// Method to check for and do interactions
		/// </summary>
		/// <param name="random"> A random number generator. </param>
		/// <param name="otherElements"> The list of all other elements in this simulation. </param>
		/// <returns> A List of new child Elements. </returns>
		public List<Element> DoInteraction(Random random, List<Element> otherElements)
		{
			List<Element> children = new List<Element>();
			TurnsSinceMurder++;
			int interactionsPerTurn = (int)(Extraversion * INTERACTCOUNT);
			while (interactionsPerTurn-- > 0)
			{
				if (this.IsDead)
				{
					return children;
				}
				Element otherElement = otherElements[random.Next(otherElements.Count - 1)];
				if (otherElement == this || otherElement.IsDead)
				{
					interactionsPerTurn++;
					continue;
				}
				if ((Math.Sqrt((position.X - otherElement.Position.X) * (position.X - otherElement.Position.X)
					+ (position.Y - otherElement.Position.Y) * (position.Y - otherElement.Position.Y))) < (INTERACTRANGE * Mobility))
				{
					if (!relationships.ContainsKey(otherElement))
					{
						Mingle(otherElement, otherElement.HappinessWeights);
					}
					double actionChoice = StatFunctions.GaussRandom(random.NextDouble(),
						RELATIONSHIPSCALE + relationships[otherElement],
						RELATIONSHIPSCALE - relationships[otherElement]);
					if (actionChoice > 0.9
						&& this != otherElement
						&& this != otherElement.Parents[0]
						&& this != otherElement.Parents[1]
						&& this.Parents[0] != otherElement
						&& this.Parents[1] != otherElement)
					{
						// Mate
						Element child = new Element(this, otherElement);
						relationships.Add(child, (float)RELATIONSHIPSCALE * Conscientiousness);
						children.Add(child);
					}
					else if (actionChoice > 0.6)
					{
						// Mingle
						Mingle(otherElement, otherElement.HappinessWeights);
					}
					else if (actionChoice > 0.4)
					{
						// Trade
						// 1. Create trade proposal based on resource desires (use) and prices
						// 2. Invoke otherElement's public bool EvaluateTradeProposal(Element sender, ref Vector tradeProposal) method with the proposal
						// 3. If otherElement.EvaluateTradeProposal returns false,
						//		invoke this Element's public bool EvaluateTradeProposal(Element sender, ref Vector tradeProposal) method with the returned proposal
						// 4. If otherElement.EvaluateTradeProposal returns true or the reply this.EvaluateTradeProposal returns true, execute the trade
						// 4a. If trade goes through, update resourceUse
						Vector tradeProposal = new Vector(resourceUse);
						RefineTradeProposal(otherElement, ref tradeProposal);
						tradeProposal = -1.0f * tradeProposal;
						float direction = -1.0f;
						if (!otherElement.EvaluateTradeProposal(this, ref tradeProposal))
						{
							direction = 1.0f;
							if (!EvaluateTradeProposal(otherElement, ref tradeProposal))
							{
								continue;
							}
						}

						ExecuteTrade(direction * tradeProposal);
						otherElement.ExecuteTrade(-1.0f * direction * tradeProposal);
					}
					else if (actionChoice < 0.1 && Lethality > otherElement.Health)
					{
						// Attack
						if (random.NextDouble() * Lethality > random.NextDouble() * otherElement.Lethality)
						{
							inventory = inventory + otherElement.GetRobbed();
							otherElement.Die();
							lethalityBonus *= (1.0f + 5.0f / (1.0f + (float)Math.Exp(lethalityBonus)));
							TurnsSinceMurder = 1;
						}
						else if (random.NextDouble() * Lethality < random.NextDouble() * otherElement.Lethality)
						{
							this.Die();
							return children;
						}
						TurnsSinceMurder--;
					}
				}
			}
			return children;
		}

		/// <summary>
		/// Method to evaluate a reference trade proposal suggested by the input Element
		/// </summary>
		/// <param name="sender"> Element suggesting the proposed trade. </param>
		/// <param name="tradeProposal"> Proposed trade. Will be changed to a counter-offer if method return value is false. </param>
		/// <returns> Indicates that trade is acceptable (true) or unacceptable (false). If trade is unacceptable,
		/// tradeProposal will be modified to reflect an acceptable counter-offer. </returns>
		public bool EvaluateTradeProposal(Element sender, ref Vector tradeProposal)
		{
			float tradeValue = tradeProposal * prices;

			// Update relationship and check for social interaction;
			if (this.Parents[0] == sender || this.Parents[1] == sender)
			{
				if (!relationships.ContainsKey(sender))
				{
					relationships.Add(sender, ((float)RELATIONSHIPSCALE / (1.0f + (float)Math.Exp(MIDDLEAGE / 2.0 - Age)) - (float)RELATIONSHIPSCALE) / 2.0f);
				}
				else
				{
					relationships[sender] += (float)RELATIONSHIPSCALE / (1000.0f * MIDDLEAGE);
				}
			}
			if (!relationships.ContainsKey(sender))
			{
				relationships.Add(sender, 2.0f * (float)StatFunctions.Sigmoid(tradeValue, 1.0, 0.0) - 1.0f);
			}
			else
			{
				relationships[sender] += 2.0f * (float)StatFunctions.Sigmoid(tradeValue, 1.0, 0.0) - 1.0f;
			}

			if (Extraversion > DisplayForm.GLOBALRANDOM.NextDouble())
			{
				Mingle(sender, sender.HappinessWeights);
			}

			// Convert proposal to effective price Vector
			Vector effectivePrices = new Vector(tradeProposal.Count);
			for (int i = 0; i < effectivePrices.Count; i++)
			{
				if (tradeProposal[i] == 0.0f)
				{
					effectivePrices[i] = 0.0f;
				}
				else
				{
					effectivePrices[i] = -tradeValue;
					for (int j = 0; j < tradeProposal.Count; j++)
					{
						if (j == i)
						{
							continue;
						}
						effectivePrices[i] += prices[j] * tradeProposal[j];
					}
					effectivePrices[i] /= -tradeProposal[i];
				}
			}
			if (tradeProposal * effectivePrices != tradeValue)
			{
				float debug = tradeProposal * effectivePrices;
			}
			// Update standard deviations (pricesUncertainty)
			for (int i = 0; i < pricesUncertainty.Count; i++)
			{
				if (effectivePrices[i] == 0.0f)
				{
					continue;
				}
				float variance = (cumulativePriceExperience[i] * pricesUncertainty[i] * pricesUncertainty[i]
					+ ((Math.Abs(tradeProposal[i]) * effectivePrices[i] - cumulativePriceExperience[i] * prices[i]) / (Math.Abs(tradeProposal[i]) + cumulativePriceExperience[i]))
					* ((Math.Abs(tradeProposal[i]) * effectivePrices[i] - cumulativePriceExperience[i] * prices[i]) / (Math.Abs(tradeProposal[i]) + cumulativePriceExperience[i])))
					/ (Math.Abs(tradeProposal[i]) + cumulativePriceExperience[i]);
				pricesUncertainty[i] = (float)Math.Sqrt(variance);
				cumulativePriceExperience[i] += Math.Abs(tradeProposal[i]);
			}
			// Update prices
			for (int i = 0; i < prices.Count; i++)
			{
				if (effectivePrices[i] == 0.0f)
				{
					continue;
				}
				prices[i] = (1.0f - timePreference) * prices[i] + timePreference * effectivePrices[i];
			}

			// Make decision and return
			Vector thisTradeProposal = new Vector(resourceUse);
			RefineTradeProposal(sender, ref thisTradeProposal);
			float targetVal = relationships[sender] / (float)RELATIONSHIPSCALE;
			if (((tradeValue - targetVal) / targetVal) > (1.0f - Agreeableness)
				|| ((tradeProposal * thisTradeProposal) / (tradeProposal.Magnitude * thisTradeProposal.Magnitude)) < -1.0f + Agreeableness + targetVal)
			{
				return true;
			}
			else
			{
				RefineTradeProposal(sender, ref tradeProposal);
				tradeProposal = -1.0f * tradeProposal;
				return false;
			}
		}

		/// <summary>
		/// Takes a suggested trade proposal Vector reference input and refines it to the proposal that is
		/// optimal for this Element.
		/// </summary>
		/// <param name="otherElement"> Element who will be traded with. </param>
		/// <param name="baseProposal"> Baseline suggested trade proposal. </param>
		private void RefineTradeProposal(Element otherElement, ref Vector baseProposal)
		{
			float netValue = baseProposal * prices;
			float tradeMag = baseProposal.Magnitude;
			if (!relationships.ContainsKey(otherElement))
			{
				relationships.Add(otherElement, 0.0f);
			}
			float tradeValueTarget;
			if (relationships[otherElement] < 0.01f && relationships[otherElement] > -0.01f)
			{
				tradeValueTarget = relationships[otherElement] / (float)RELATIONSHIPSCALE;
			}
			else
			{
				tradeValueTarget = relationships[otherElement] / (float)RELATIONSHIPSCALE;
			}
			float convergence = (netValue - tradeValueTarget) / tradeValueTarget;

			// Recursively find an acceptable trade proposal
			int maxAttempts = 10;
			float previousConvergence = convergence;
			float adjustmentScaling = 1.0f;
			while (maxAttempts-- > 0)
			{
				if (convergence > -Agreeableness && convergence < 1.0f - Agreeableness)
				{
					break;
				}

				Vector previousTradeProposal = new Vector(baseProposal);
				for (int i = 0; i < baseProposal.Count; i++)
				{
					if (prices[i] == 0.0f)
					{
						baseProposal[i] = 0.0f;
					}
					baseProposal[i] += adjustmentScaling * Math.Abs(baseProposal[i]) * (tradeValueTarget - netValue) / prices[i];
				}
				baseProposal = (tradeMag / baseProposal.Magnitude) * baseProposal;

				netValue = prices * baseProposal;
				previousConvergence = convergence;
				convergence = (netValue - tradeValueTarget) / tradeValueTarget;
				if (Math.Abs(convergence) > Math.Abs(previousConvergence))
				{
					adjustmentScaling /= 2.0f;
					baseProposal = previousTradeProposal;
					netValue = prices * baseProposal;
					convergence = (netValue - tradeValueTarget) / tradeValueTarget;
				}
			}

			// Condition trade to remove any invalid numbers and ensure trade does not exceed this element's ability to pay
			double tradePropScaling = 1.0f;
			for (int i = 0; i < baseProposal.Count; i++)
			{
				if (Single.IsInfinity(baseProposal[i]) || Single.IsNaN(baseProposal[i]) || baseProposal[i] == 0.0f)
				{
					baseProposal[i] = 0.0f;
				}
				else if (baseProposal[i] < -inventory[i])
				{
					tradePropScaling = Math.Max(0.0, Math.Min(tradePropScaling, -inventory[i] / baseProposal[i]));
				}
			}
			baseProposal = (float)tradePropScaling * baseProposal;
		}

		/// <summary>
		/// Method to carry out the input trade. Trade will be added to this Element's inventory.
		/// </summary>
		/// <param name="trade"> Input trade to be added to this Element's inventory. </param>
		public void ExecuteTrade(Vector trade)
		{
			try
			{
				inventory += trade;
				resourceUse -= timePreference * trade;
			}
			catch (MatrixMath.SizeMismatchException)
			{
				while (inventory.Count < trade.Count)
				{
					inventory.Add(0.0f);
				}
				while (inventory.Count > trade.Count)
				{
					inventory.RemoveAt(inventory.Count - 1);
				}
				while (resourceUse.Count < trade.Count)
				{
					resourceUse.Add(0.0f);
				}
				while (resourceUse.Count > trade.Count)
				{
					resourceUse.RemoveAt(resourceUse.Count - 1);
				}
				inventory += trade;
				resourceUse -= timePreference * trade;
			}
		}

		/// <summary>
		/// Gain relationiship based upon happinessWeights similarity.
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		public HappinessWeights Mingle(Element otherElement, HappinessWeights values)
		{
			if (!relationships.ContainsKey(otherElement))
			{
				relationships.Add(otherElement, Openness - Neuroticism);
			}
			relationships[otherElement] += (values[0] * happinessWeights[0] + values[1] * happinessWeights[1] + values[2] * happinessWeights[2]);
			if (otherElement.Age != 0)
			{
				relationships[otherElement] -= (float)RELATIONSHIPSCALE * (otherElement.TurnsSinceMurder - otherElement.Age) / otherElement.Age;
			}

			happinessWeights[0] *= (1.0f + Agreeableness * Openness * values[0]) / (1.0f + Agreeableness * Openness);
			happinessWeights[1] *= (1.0f + Agreeableness * Openness * values[1]) / (1.0f + Agreeableness * Openness);
			happinessWeights[2] *= (1.0f + Agreeableness * Openness * values[2]) / (1.0f + Agreeableness * Openness);

			if (DisplayForm.GLOBALRANDOM.NextDouble() < Intelligence)
			{
				if (otherElement.KnownActions.Count > 2)
				{
					this.LearnAction(otherElement.KnownActions[DisplayForm.GLOBALRANDOM.Next(otherElement.KnownActions.Count - 1)]);
				}
				else if (otherElement.KnownActions.Count == 1)
				{
					this.LearnAction(otherElement.KnownActions[0]);
				}
			}
			if (DisplayForm.GLOBALRANDOM.NextDouble() < Openness)
			{
				this.LearnLocation(otherElement.KnownLocations[DisplayForm.GLOBALRANDOM.Next(otherElement.KnownLocations.Count - 1)]);
			}
			if (DisplayForm.GLOBALRANDOM.NextDouble() < Extraversion)
			{
				Element subject = relationships.Keys.ToArray()[DisplayForm.GLOBALRANDOM.Next(relationships.Count() - 1)];
				this.LearnRelationshipRating(subject, otherElement.LearnRelationshipRating(subject, relationships[subject]));
			}

			return this.happinessWeights;
		}

		/// <summary>
		/// Method to learn from another element information about their relationship with a third element.
		/// </summary>
		/// <param name="subject"> The third element being "talked about". </param>
		/// <param name="otherElementRelationship"> The other element's relationship toward the third element. </param>
		/// <returns> The relationship of this element towards the third after being modified. </returns>
		public float LearnRelationshipRating(Element subject, float otherElementRelationship)
		{
			if (!relationships.ContainsKey(subject))
			{
				relationships.Add(subject, 0.0f);
			}
			relationships[subject] *= (1.0f + Agreeableness * Extraversion * otherElementRelationship) / (1.0f + Agreeableness * Extraversion);

			return relationships[subject];
		}

		/// <summary>
		/// Method to add a new action to this Element's KnownActions list
		/// </summary>
		/// <param name="action"> Action to add. </param>
		public void LearnAction(Action action)
		{
			for (int i = 0; i < KnownActions.Count; i++)
			{
				if (KnownActions[i].ActionID == action.ActionID)
				{
					return;
				}
			}

			KnownActions.Add(action.Copy());
		}

		/// <summary>
		/// Method to add a new location to this Element's KnownLocations list
		/// </summary>
		/// <param name="location"> Location to add. </param>
		public void LearnLocation(PointF location)
		{
			if (!KnownLocations.Contains(location))
			{
				KnownLocations.Add(location);
			}
		}

		/// <summary>
		/// Method to inflict a robbery on this Element.
		/// </summary>
		/// <returns> Retruns Vector of loot obtained by the robber. </returns>
		public Vector GetRobbed()
		{
			Vector losses = new Vector(inventory.Count);
			for (int i = 0; i < inventory.Count; i++)
			{
				losses[i] = inventory[i] * (float)DisplayForm.GLOBALRANDOM.NextDouble();
			}
			inventory = inventory - losses;
			return losses;
		}

		/// <summary>
		/// Method to increment element age and check for death.
		/// </summary>
		/// <returns></returns>
		public void CheckForDeath(float deathChance)
		{
			Age++;
			resourceUse = (1.0f - timePreference) * resourceUse;
			Health += 2.0f * (float)StatFunctions.Sigmoid((Age - MIDDLEAGE), MIDDLEAGE, MIDDLEAGE) - 1.0f;
			if (Health < deathChance)
			{
				this.Die();
			}
		}

		/// <summary>
		/// Triggers any on-death effects such as inheritance.
		/// </summary>
		public void Die()
		{
			float maxOpinion = Single.MinValue;
			Element inheretor = this;
			foreach (Element e in relationships.Keys)
			{
				try
				{
					if (relationships[e] > maxOpinion)
					{
						inheretor = e;
						maxOpinion = relationships[e];
					}
				}
				catch (NullReferenceException)
				{
					continue;
				}
			}
			if (inheretor != this)
			{
				inheretor.ExecuteTrade(inventory);
				this.ExecuteTrade(-inventory);
			}
			this.IsDead = true;
		}

		/// <summary>
		/// Private class constructor for reproduction method.
		/// </summary>
		/// <param name="parent1"> Parent 1. </param>
		/// <param name="parent2"> Parent 2. </param>
		private Element(Element parent1, Element parent2)
		{
			Parents[0] = parent1;
			Parents[1] = parent2;
			Age = 0;
			IsDead = false;
			lethalityBonus = 0.0f;
			TurnsSinceMurder = 0;

			position.X = parent1.Position.X;
			position.Y = parent1.Position.Y;
			KnownLocations = new List<PointF>();
			KnownLocations.Add(position);
			int r = (parent1.ElementColor.R + parent2.ElementColor.R) / 2;
			int g = (parent1.ElementColor.G + parent2.ElementColor.G) / 2;
			int b = (parent1.ElementColor.B + parent2.ElementColor.B) / 2;
			ElementColor = Color.FromArgb(r, g, b);

			double rand = 1.0 - DisplayForm.GLOBALRANDOM.NextDouble();
			Intelligence = 0.5f * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD)
				+ 0.25f * parent1.Intelligence
				+ 0.25f * parent2.Intelligence;
			rand = 1.0 - DisplayForm.GLOBALRANDOM.NextDouble();
			Conscientiousness = 0.5f * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD)
				+ 0.25f * parent1.Conscientiousness
				+ 0.25f * parent2.Conscientiousness;
			rand = 1.0 - DisplayForm.GLOBALRANDOM.NextDouble();
			Agreeableness = 0.5f * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD)
				+ 0.25f * parent1.Agreeableness
				+ 0.25f * parent2.Agreeableness;
			rand = 1.0 - DisplayForm.GLOBALRANDOM.NextDouble();
			Neuroticism = 0.5f * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD)
				+ 0.25f * parent1.Neuroticism
				+ 0.25f * parent2.Neuroticism;
			rand = 1.0 - DisplayForm.GLOBALRANDOM.NextDouble();
			Openness = 0.5f * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD)
				+ 0.25f * parent1.Openness
				+ 0.25f * parent2.Openness;
			rand = 1.0 - DisplayForm.GLOBALRANDOM.NextDouble();
			Extraversion = 0.5f * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD)
				+ 0.25f * parent1.Extraversion
				+ 0.25f * parent2.Extraversion;

			rand = DisplayForm.GLOBALRANDOM.NextDouble();
			Health = MIDDLEAGE / 4.0f * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			Mobility = 1.0f;

			rand = DisplayForm.GLOBALRANDOM.NextDouble();
			happinessWeights[0] = 0.5f * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD)
				+ 0.25f * parent1.HappinessWeights[0]
				+ 0.25f * parent2.HappinessWeights[0];
			rand = DisplayForm.GLOBALRANDOM.NextDouble();
			happinessWeights[1] = 0.5f * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD)
				+ 0.25f * parent1.HappinessWeights[1]
				+ 0.25f * parent2.HappinessWeights[1];
			rand = DisplayForm.GLOBALRANDOM.NextDouble();
			happinessWeights[2] = 0.5f * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD)
				+ 0.25f * parent1.HappinessWeights[2]
				+ 0.25f * parent2.HappinessWeights[2];

			Vector startResources = new Vector(parent1.Inventory.Count);
			startResources += (parent1.Conscientiousness / 10.0f) * parent1.Inventory;
			parent1.ExecuteTrade(-1.0f * startResources);
			startResources += (parent2.Conscientiousness / 10.0f) * parent2.Inventory;
			parent2.ExecuteTrade(-1.0f * (parent2.Conscientiousness / 10.0f) * parent2.Inventory);

			prices = new Vector(parent1.Inventory.Count);
			pricesUncertainty = new Vector(parent1.Inventory.Count);
			for (int i = 0; i < prices.Count; i++)
			{
				rand = 1.0 - DisplayForm.GLOBALRANDOM.NextDouble();
				prices[i] = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
				pricesUncertainty[i] = prices[i];
			}
			foodConsumptionRates = new Vector(FoodResources.Count);
			timePreference = (float)StatFunctions.GaussRandom(Intelligence, TRAITSPREAD, TRAITSPREAD);
			resourceUse = new Vector(parent1.Inventory.Count);
			KnownActions = new List<Action>();
			foreach (Action action in parent1.KnownActions)
			{
				if (DisplayForm.GLOBALRANDOM.NextDouble() < Intelligence)
				{
					KnownActions.Add(action.Copy());
				}
			}
			foreach (Action action in parent2.KnownActions)
			{
				if (DisplayForm.GLOBALRANDOM.NextDouble() < Intelligence)
				{
					KnownActions.Add(action.Copy());
				}
			}
			inventory = new Vector(parent1.Inventory.Count);
			ExecuteTrade(startResources);
			cumulativePriceExperience = new Vector(parent1.Inventory.Count);
			for (int i = 0; i < inventory.Count; i++)
			{
				cumulativePriceExperience[i] = inventory[i];
			}
		}
	}
}
