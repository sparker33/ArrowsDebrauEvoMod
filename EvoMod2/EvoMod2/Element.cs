using System;
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
		public static List<FoodResourceData> FoodResources = new List<FoodResourceData>();

		// Private fields
		private PointF position = new PointF();
		private Kinematics kinematics = new Kinematics(2);
		private Destination destination = new Destination();
		private float timePreference;
		private float lethalityBonus;
		private float happinessPercentChangeHistory;
		private float happinessBonus;
		private HappinessWeights happinessWeights= new HappinessWeights(); // 0: Wealth, 1: Health, 2: Location
		private Dictionary<Element, float> relationships = new Dictionary<Element, float>();
		private Vector inventory;
		private Vector prices;
		private Vector foodConsumptionRates;
		private Vector resourceUse;

		// Public objects
			// Fixed traits
		public float Intelligence { get; private set; }
		public float Conscientiousness { get; private set; }
		public float Agreeableness { get; private set; }
		public float Neuroticism { get; private set; }
		public float Openness { get; private set; }
		public float Extraversion { get; private set; }
			// Dynamic traits
		public float Happiness { get; private set; }
		public float Health { get; private set; }
		public float Mobility { get; private set; }
		public int Age { get; private set; }
		public List<PointF> KnownLocations { get; private set; }
		public List<Action> KnownActions { get; private set; }
		public bool IsDead { get; private set; }
		public int TurnsSinceMurder { get; private set; }
		public float Lethality { get => Health + lethalityBonus; }
			// Display data
		public PointF Position { get => position; }
		public int Size { get => (int)Math.Max(Math.Min(25.0f, prices * inventory), 3.0f); }
		public Color ElementColor { get; private set; }

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
			Health = 100.0f * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			Mobility = 1.0f;

			happinessWeights[0] = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			happinessWeights[1] = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			happinessWeights[2] = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);

			inventory = new Vector(DisplayForm.NaturalResourceTypesCount);
			prices = new Vector(DisplayForm.NaturalResourceTypesCount);
			foodConsumptionRates = new Vector(FoodResources.Count);
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
			prices.Add(0.0f);
			for (int i = 0; i < KnownActions.Count; i++)
			{
				KnownActions[i].AddResource();
			}
			if (isFood)
			{
				foodConsumptionRates.Add(0.0f);
			}
		}

		/// <summary>
		/// Method to have this element consume food, experience effects from hunger/stiation, and train consumption habits accordingly.
		/// </summary>
		public void Eat()
		{
			float healthHappiness = happinessWeights.Health * Health;
			float wealthHappiness = happinessWeights.Wealth * (inventory * prices);
			float hunger = FOODREQUIREMENT;
			float[] consumption = new float[FoodResources.Count];
			for (int i = 0; i < FoodResources.Count; i++)
			{
				consumption[i] = foodConsumptionRates[FoodResources[i].ResourceIndex] * prices[FoodResources[i].ResourceIndex] / FoodResources[i].Nourishment;
				if (consumption[i] > inventory[FoodResources[i].ResourceIndex])
				{
					consumption[i] = inventory[FoodResources[i].ResourceIndex];
				}
				inventory[FoodResources[i].ResourceIndex] -= consumption[i];
				hunger -= consumption[i] * FoodResources[i].Nourishment;
			}
			Health -= hunger * hunger;
			happinessBonus -= hunger;

			float trainingMetric = (happinessWeights.Health * Health - healthHappiness - hunger) / (wealthHappiness - happinessWeights.Wealth * (inventory * prices));
			for (int i = 0; i < foodConsumptionRates.Count; i++)
			{
				if (foodConsumptionRates[i] == 0.0f)
				{
					foodConsumptionRates[i] += consumption[i] * (float)StatFunctions.Sigmoid(trainingMetric, Math.Abs(hunger), 0.0) * Math.Sign(hunger) / (FOODREQUIREMENT - hunger);
				}
				else
				{
					foodConsumptionRates[i] *= (1.0f + consumption[i] * (float)StatFunctions.Sigmoid(trainingMetric, Math.Abs(hunger), 0.0) * Math.Sign(hunger) / (FOODREQUIREMENT - hunger));
				}
			}
			// Reflect food eaten in resourceUse record
			for (int i = 0; i < FoodResources.Count; i++)
			{
				resourceUse[FoodResources[i].ResourceIndex] = timePreference * consumption[i] + (1.0f - timePreference) * resourceUse[FoodResources[i].ResourceIndex];
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
			else if (!destination.IsEmpty && StatFunctions.Sigmoid(DisplayForm.GLOBALRANDOM.NextDouble(), 100.0 * destination.GetProgress(position), 0.0) > 0.45)
			{
				destination.Clear();
			}

			if (destination.IsEmpty)
			{
				kinematics.Damping = Kinematics.DEFAULTDAMPING;
				temp[0] = 0.0f;
				temp[1] = 0.0f;
			}
			else
			{
				kinematics.Damping = 1.0f / destination.GetProgress(position);
				temp[0] = destination.X - position.X;
				temp[1] = destination.Y - position.Y;
			}

			// Determine driving force vector
			float speed = kinematics.Speed;
			if (speed != 0.0f)
			{
				// Update Happiness
				float environmentHappiness = 0.0f;
				foreach (Element e in relationships.Keys)
				{
					environmentHappiness += relationships[e]
							* (float)Math.Sqrt((position.X - e.Position.X) * (position.X - e.Position.X) + (position.Y - e.Position.Y) * (position.Y - e.Position.Y))
							 / ((float)RELATIONSHIPSCALE * DisplayForm.DomainMaxDistance);
				}

				float nextHappiness = timePreference * (happinessBonus
					+ inventory * prices * happinessWeights[0]
					+ Health * happinessWeights[1]
					+ environmentHappiness * happinessWeights[2])
					+ (1.0f - timePreference) * Happiness;
				happinessPercentChangeHistory = (nextHappiness - Happiness) / Happiness;
				Happiness = nextHappiness;
				// Update acceleratons
				temp[0] = ELESPEED * (happinessPercentChangeHistory * kinematics.GetVelocity(0) / speed + temp[0] / DisplayForm.SCALE);
				temp[1] = ELESPEED * (happinessPercentChangeHistory * kinematics.GetVelocity(1) / speed + temp[1] / DisplayForm.SCALE);
			}
			else
			{
				temp[0] = ELESPEED * (float)DisplayForm.GLOBALRANDOM.NextDouble();
				temp[1] = ELESPEED * (float)DisplayForm.GLOBALRANDOM.NextDouble();
			}

			// Apply force vector to kinematics; get and apply displacements
			if (Mobility != 0.0f)
			{
				temp = kinematics.GetDisplacement(temp, 1.0f / Mobility).ToArray();
			}
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
		public void DoAction(List<Resource> environmentResources, List<Element> elements)
		{
			if (DisplayForm.GLOBALRANDOM.NextDouble() < Conscientiousness)
			{
				bool discoveryOccurred = false;
				// Populate the local resource levels
				Vector localResourceLevels = GetLocalResourceLevels(environmentResources);
				// Decide which action to do
				float maxActionPriority = -1.0f;
				int actionChoice = 0;
				for (int i = 0; i < KnownActions.Count; i++)
				{
					if (KnownActions[i].GetActionPriority(localResourceLevels, inventory) > maxActionPriority)
					{
						actionChoice = i;
					}
				}
				// If any actions are available, execute the preferred action
				if (maxActionPriority > -1.0f)
				{
					// Apply action effects
					inventory -= KnownActions[actionChoice].Cost;
					Vector productionUtilityVector = KnownActions[actionChoice].DoAction(localResourceLevels, Intelligence);
					float inventoryValueUtilityVar = inventory * prices;
					inventory += productionUtilityVector;
					inventoryValueUtilityVar = (inventoryValueUtilityVar - inventory * prices) / inventoryValueUtilityVar;
					happinessBonus += KnownActions[actionChoice].HappinessBonus;
					Health += KnownActions[actionChoice].HealthBonus;
					Mobility += KnownActions[actionChoice].MobilityBonus;
					lethalityBonus += KnownActions[actionChoice].LethalityBonus;
					// Update resource usage information and apply learning to action
					KnownActions[actionChoice].Learn(Math.Sign(Happiness) * (happinessBonus + happinessWeights[0] * inventoryValueUtilityVar) / Happiness);
					productionUtilityVector = productionUtilityVector - KnownActions[actionChoice].Cost;
					resourceUse = timePreference * productionUtilityVector + (1.0f - timePreference) * productionUtilityVector;
					// Check for new Resource discovery
					if (0.5 < StatFunctions.GaussRandom(DisplayForm.GLOBALRANDOM.NextDouble(), 50.0 * Intelligence * Openness, 50.0 / (Intelligence * Openness)))
					{
						bool newResourceIsFood = false;
						if (DisplayForm.GLOBALRANDOM.NextDouble() > 0.8)
						{
							FoodResources.Add(new FoodResourceData(inventory.Count - 1, 1.0f - (float)DisplayForm.GLOBALRANDOM.NextDouble()));
							newResourceIsFood = true;
						}
						for (int i = 0; i < elements.Count; i++)
						{
							elements[i].AddResource(newResourceIsFood);
						}
					}
					// Check for new Action discovery (can discover either Harvest or Refinement Action)
					if (0.5 < StatFunctions.GaussRandom(DisplayForm.GLOBALRANDOM.NextDouble(), 10.0 * Intelligence * Openness, 10.0 / (Intelligence * Openness)))
					{
						if (DisplayForm.GLOBALRANDOM.NextDouble() > 0.5)
						{
							KnownActions.Add(new HarvestAction(inventory.Count, DisplayForm.GLOBALRANDOM, localResourceLevels));
						}
						else
						{
							KnownActions.Add(new RefinementAction(inventory.Count, DisplayForm.GLOBALRANDOM, productionUtilityVector));
						}
						discoveryOccurred = true;
					}
				}
				// Check for new HarvestAction discovery
				if (!discoveryOccurred && 0.5 < StatFunctions.GaussRandom(DisplayForm.GLOBALRANDOM.NextDouble(), 10.0 * Intelligence * Openness, 10.0 / (Intelligence * Openness)))
				{
					KnownActions.Add(new HarvestAction(inventory.Count, DisplayForm.GLOBALRANDOM, localResourceLevels));
				}
			}
		}

		/// <summary>
		/// Method to check for and do interactions
		/// </summary>
		/// <param name="random"> A random number generator. </param>
		/// <param name="otherElements"> The list of all other elements in this simulation. </param>
		public void DoInteraction(Random random, List<Element> otherElements)
		{
			TurnsSinceMurder++;
			int interactionsPerTurn = (int)(Extraversion * INTERACTCOUNT);
			while (interactionsPerTurn-- > 0)
			{
				Element otherElement = otherElements[random.Next(otherElements.Count - 1)];
				if ((Math.Sqrt((position.X - otherElement.Position.X) * (position.X - otherElement.Position.X)
					+ (position.Y - otherElement.Position.Y) * (position.Y - otherElement.Position.Y))) < (INTERACTRANGE * Mobility))
				{
					if (!relationships.ContainsKey(otherElement))
					{
						this.Mingle(otherElement, otherElement.Mingle(this, happinessWeights));
					}
					double actionChoice = StatFunctions.GaussRandom(random.NextDouble(),
						RELATIONSHIPSCALE + relationships[otherElement],
						RELATIONSHIPSCALE - relationships[otherElement]);
					if (actionChoice > 0.9)
					{
						// Mate
					}
					else if (actionChoice > 0.6)
					{
						// Mingle and check for learning actions, locations, and relationships.
						this.Mingle(otherElement, otherElement.Mingle(this, happinessWeights));
						if (random.NextDouble() < Intelligence)
						{
							this.LearnAction(otherElement.KnownActions[random.Next(otherElement.KnownActions.Count - 1)]);
						}
						if (random.NextDouble() < Openness)
						{
							this.LearnLocation(otherElement.KnownLocations[random.Next(otherElement.KnownLocations.Count - 1)]);
						}
						if (random.NextDouble() < Extraversion)
						{
							Element subject = relationships.Keys.ToArray()[random.Next(relationships.Count() - 1)];
							this.LearnRelationshipRating(subject, otherElement.LearnRelationshipRating(subject, relationships[subject]));
						}
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
						// **EvaluateTradeProposal method affects relationships[sender]
						// **Trade willingness to be modified by agreeableness
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
							return;
						}
						TurnsSinceMurder--;
					}
				}
			}
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
			
		}

		/// <summary>
		/// Method to carry out the input trade. Trade will be added to this Element's inventory.
		/// </summary>
		/// <param name="trade"> Input trade to be added to this Element's inventory. </param>
		public void ExecuteTrade(Vector trade)
		{
			inventory += trade;
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

			relationships[otherElement] -= (float)RELATIONSHIPSCALE * (otherElement.TurnsSinceMurder - otherElement.Age) / otherElement.Age;


			happinessWeights[0] *= (1.0f + Agreeableness * Openness * values[0]) / (1.0f + Agreeableness * Openness);
			happinessWeights[1] *= (1.0f + Agreeableness * Openness * values[1]) / (1.0f + Agreeableness * Openness);
			happinessWeights[2] *= (1.0f + Agreeableness * Openness * values[2]) / (1.0f + Agreeableness * Openness);

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
			Health += 1.0f - Age / (1.5f * MIDDLEAGE) - (Age - MIDDLEAGE) * (Age - MIDDLEAGE) / (MIDDLEAGE * MIDDLEAGE);
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
			Element inheretor= this;
			foreach (Element e in relationships.Keys)
			{
				if (relationships[e] > maxOpinion)
				{
					inheretor = e;
					maxOpinion = relationships[e];
				}
			}
			inheretor.ExecuteTrade(inventory);
			this.IsDead = true;
		}
	}
}
