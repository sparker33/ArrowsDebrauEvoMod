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
		public static double DESTINATIONACQUISITIONTHRESHOLD;
		public static float DESTINATIONACCEL;
		public static float COLORMUTATIONRATE;
		public static float TRAITSPREAD;
		public static float INTERACTCOUNT;
		public static float INTERACTRANGE;
		public static double RELATIONSHIPSCALE;
		public static float INTERACTIONCHOICESCALE;
		public static float FOODREQUIREMENT;
		public static float STARTRESOURCES;
		public static int MAXRELATIONSHIPS;
		public static int MAXLOCATIONSCOUNT;
		public static int MAXACTIONSCOUNT;
		public static int MAXRESOURCECOUNT;
		public static float DISCOVERYRATE;
		public static float KNOWLEDGETRANSFERRATE;
		public static float TRADEROUNDOFF;
		public static float REPRODUCTIONCHANCE;
		public static float MINGLECHANCE;
		public static float TRADECHANCE;
		public static float ATTACKCHANCE;
		public static float CHILDCOST;
		public static float INFANTMORTALITY;
		public static float INHERITANCE;
		public static bool INCESTALLOWED;
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
		private Vector cumulativePriceExperience;
		private Vector foodConsumptionRates;
		private Vector resourceUse;
		private float healthHappiness { get => happinessWeights.Health * Health / MIDDLEAGE; }
		private float wealthHappiness { get => happinessWeights.Wealth * (inventory * prices) / (inventory.Magnitude + Single.Epsilon); }

		// Public objects
		// Fixed traits
		public int Name { get; private set; }
		public float Intelligence { get; private set; }
		public float Conscientiousness { get; private set; }
		public float Agreeableness { get; private set; }
		public float Neuroticism { get; private set; }
		public float Openness { get; private set; }
		public float Extraversion { get; private set; }
		public int[] Parents = new int[2];
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
		public List<KnownLocation> KnownLocations { get; private set; }
		public List<Action> KnownActions { get; private set; }
		private int ActionsTakenTotal;
		public bool IsDead { get; private set; }
		public List<int> RecentAttacks { get; private set; }
		public float Lethality { get => Health + lethalityBonus; }
		public HappinessWeights HappinessWeights { get => happinessWeights; }
		public Vector Inventory { get => inventory; }
		// Display data and general accessors
		public PointF Position { get => position; }
		public int Size { get => (int)(17.0 * StatFunctions.Sigmoid(0.5 * Age / MIDDLEAGE, -DisplayForm.SIZESCALING, 0.5) + 3.0); }
		public int Opacity { get => (int)(255.0 * StatFunctions.Sigmoid(healthHappiness / happinessWeights.Health, -DisplayForm.OPACITYSCALING, 0.5)); }
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
		public Element(List<Resource> environmentResources)
		{
			Name = DisplayForm.GLOBALRANDOM.Next();
			Parents[0] = this.Name;
			Parents[1] = this.Name;
			Age = 0;
			IsDead = false;
			RecentAttacks = new List<int>();

			position.X = (float)(DisplayForm.GLOBALRANDOM.NextDouble() * DisplayForm.SCALE);
			position.Y = (float)(DisplayForm.GLOBALRANDOM.NextDouble() * DisplayForm.SCALE);
			KnownLocations = new List<KnownLocation>();
			KnownLocations.Add(new KnownLocation(position));
			int r = (int)(255.0 / (1.0 + Math.Exp((15.0 / DisplayForm.SCALE) * (position.X - DisplayForm.SCALE / 2.0))));
			int g = (int)(255.0 / (1.0 + Math.Exp((15.0 / DisplayForm.SCALE) * (position.X * position.Y / (2 * DisplayForm.SCALE * DisplayForm.SCALE)))));
			int b = (int)(255.0 / (1.0 + Math.Exp((15.0 / DisplayForm.SCALE) * (position.Y - DisplayForm.SCALE / 2.0))));
			ElementColor = Color.FromArgb(r, g, b);

			double rand = 1.0 - DisplayForm.GLOBALRANDOM.NextDouble();
			Intelligence = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = 1.0 - DisplayForm.GLOBALRANDOM.NextDouble();
			Conscientiousness = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = 1.0 - DisplayForm.GLOBALRANDOM.NextDouble();
			Agreeableness = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = 1.0 - DisplayForm.GLOBALRANDOM.NextDouble();
			Neuroticism = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = 1.0 - DisplayForm.GLOBALRANDOM.NextDouble();
			Openness = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = 1.0 - DisplayForm.GLOBALRANDOM.NextDouble();
			Extraversion = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);

			rand = DisplayForm.GLOBALRANDOM.NextDouble();
			Health = MIDDLEAGE / 40.0f * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			lethalityBonus = Health;
			Mobility = 1.0f;

			happinessWeights[0] = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = DisplayForm.GLOBALRANDOM.NextDouble();
			happinessWeights[1] = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = DisplayForm.GLOBALRANDOM.NextDouble();
			happinessWeights[2] = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);

			inventory = new Vector(DisplayForm.NaturalResourceTypesCount);
			for (int i = 0; i < inventory.Count; i++)
			{
				rand = DisplayForm.GLOBALRANDOM.NextDouble();
				inventory[i] = STARTRESOURCES * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			}
			prices = new Vector(DisplayForm.NaturalResourceTypesCount);
			cumulativePriceExperience = new Vector(DisplayForm.NaturalResourceTypesCount);
			for (int i = 0; i < inventory.Count; i++)
			{
				rand = 1.0 - DisplayForm.GLOBALRANDOM.NextDouble();
				prices[i] = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
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
			if (DisplayForm.GLOBALRANDOM.Next() < DisplayForm.GLOBALRANDOM.Next())
			{
				KnownActions.Add(new HarvestAction(inventory.Count, DisplayForm.GLOBALRANDOM, GetLocalResourceLevels(environmentResources), inventory));
			}
			else
			{
				KnownActions.Add(new RefinementAction(inventory.Count, DisplayForm.GLOBALRANDOM, (1.0f / STARTRESOURCES) * inventory));
			}
		}

		/// <summary>
		/// Method to add a resource to the list of possible resources and all affected components.
		/// </summary>
		public void AddResource(bool isFood)
		{
			resourceUse.Add(0.0f);
			inventory.Add(0.0f);
			prices.Add(Openness);
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
				else if (consumption[i] < 0.0f)
				{
					consumption[i] = 0.0f;
				}
				inventory[FoodResources[i].ResourceIndex] -= consumption[i];
				hunger -= consumption[i] * FoodResources[i].Nourishment;
			}
			float temp0 = hunger / FOODREQUIREMENT;
			Health -= temp0 * temp0;
			happinessBonus -= timePreference * temp0 + (1.0f - timePreference) * happinessBonus;

			float wealthPctChg = (prevWealthHappiness - wealthHappiness) / prevWealthHappiness;
			float trainingMetric = prevHealthHappiness / wealthPctChg;
			if (Math.Abs(trainingMetric) < 0.001f)
			{
				trainingMetric = (healthHappiness - prevHealthHappiness) / 0.001f;
			}
			else
			{
				trainingMetric = (healthHappiness - prevHealthHappiness) / (Math.Sign(trainingMetric) * 0.001f + trainingMetric);
			}
			for (int i = 0; i < foodConsumptionRates.Count; i++)
			{
				if (foodConsumptionRates[i] == 0.0f)
				{
					foodConsumptionRates[i] = 1.0f / (FoodResources[i].Nourishment * FoodResources.Count);
				}
				else
				{
					foodConsumptionRates[i] *= (1.0f + Math.Sign(hunger) * consumption[i] * ((float)StatFunctions.Sigmoid(trainingMetric, Math.Abs(hunger), 0.0) - 0.5f));
				}
			}
			// Reflect food eaten in resourceUse record
			for (int i = 0; i < FoodResources.Count; i++)
			{
				resourceUse[FoodResources[i].ResourceIndex] += consumption[i];
			}
		}

		/// <summary>
		/// Method to updte the position of this element
		/// </summary>
		public void Move()
		{
			/* Update Happiness */
			// Determine environment happiness
			float environmentHappiness = 0.0f;
			for (int i = 0; i < relationships.Keys.Count; i++)
			{
				if (relationships.Keys.ToArray()[i].IsDead)
				{
					relationships.Remove(relationships.Keys.ToArray()[i]);
				}
			}
			foreach (Element e in relationships.Keys)
			{
				try
				{
					environmentHappiness += relationships[e]
							* (float)Math.Sqrt((position.X - e.Position.X) * (position.X - e.Position.X)
								+ (position.Y - e.Position.Y) * (position.Y - e.Position.Y))
							/ (INTERACTRANGE * INTERACTIONCHOICESCALE * relationships.Count);
				}
				catch (NullReferenceException)
				{
					continue;
				}
			}
			environmentHappiness = environmentHappiness * happinessWeights[2];
			// Calculate updated happiness
			float nextHappiness = timePreference * (happinessBonus
				+ wealthHappiness
				+ healthHappiness
				+ environmentHappiness) / 4.0f
				+ (1.0f - timePreference) * Happiness;
			// Determine percent change (with thresholding) and update Happiness
			if (Happiness <= 0.1f && Happiness >= -0.1f)
			{
				happinessPercentChangeHistory = Math.Sign(nextHappiness - Happiness) * 0.001f;
			}
			else
			{
				happinessPercentChangeHistory = (nextHappiness - Happiness) / Happiness;
			}
			Happiness = nextHappiness;

			// Update KnownLocation preferences
			float maxPref = Single.MinValue;
			float minPref = Single.MaxValue;
			for (int i = 0; i < KnownLocations.Count; i++)
			{
				float proximity = 1.0f / (1.0f + (float)Math.Sqrt((KnownLocations[i].Location.X - position.X) * (KnownLocations[i].Location.X - position.X)
					+ (KnownLocations[i].Location.Y - position.Y) * (KnownLocations[i].Location.Y - position.Y)));
				KnownLocations[i].Preference += proximity * happinessPercentChangeHistory;
				if (KnownLocations[i].Preference > maxPref)
				{
					maxPref = KnownLocations[i].Preference;
				}
				if (KnownLocations[i].Preference < minPref)
				{
					minPref = KnownLocations[i].Preference;
				}
			}

			// Check for destination acquisition
			float[] temp = new float[2]; // Utility array to hold destination distance, accelleration, and displacement
			double destinationAcquisitionCheck = StatFunctions.GaussRandom(DisplayForm.GLOBALRANDOM.NextDouble(), 10.0 * Openness, 10.0 * (1.0 - Openness));
			if (destination.IsEmpty && destinationAcquisitionCheck > DESTINATIONACQUISITIONTHRESHOLD)
			{
				PointF newLocation;
				if (Math.Exp(DISCOVERYRATE * (KnownLocations.Count - MAXLOCATIONSCOUNT))
					< ((1.0 / (1.0 - DESTINATIONACQUISITIONTHRESHOLD)) * (destinationAcquisitionCheck - DESTINATIONACQUISITIONTHRESHOLD)))
				{
					newLocation = new PointF((float)(DisplayForm.GLOBALRANDOM.NextDouble() * DisplayForm.SCALE),
						(float)(DisplayForm.GLOBALRANDOM.NextDouble() * DisplayForm.SCALE));
					KnownLocations.Add(new KnownLocation(newLocation));
				}
				else
				{
					if (maxPref == minPref)
					{
						newLocation = KnownLocations[DisplayForm.GLOBALRANDOM.Next(KnownLocations.Count - 1)].Location;
					}
					else
					{
						int destinationChoice = 0;
						float maxLocationPriority = Single.MinValue;
						float priority;
						for (int i = 0; i < KnownLocations.Count; i++)
						{
							priority = (float)DisplayForm.GLOBALRANDOM.NextDouble() * (KnownLocations[i].Preference - minPref) / (maxPref - minPref);
							if (priority > maxLocationPriority)
							{
								destinationChoice = i;
								maxLocationPriority = priority;
							}
						}
						newLocation = KnownLocations[destinationChoice].Location;
					}
				}
				destination.Set(this.position, newLocation);
			}
			float progress = destination.GetProgress(position);

			if (destination.IsEmpty)
			{
				kinematics.Damping = Kinematics.DEFAULTDAMPING;
				temp[0] = 0.0f;
				temp[1] = 0.0f;
			}
			else
			{
				kinematics.Damping = Kinematics.DEFAULTDAMPING * (DESTINATIONACCEL / (2.0f - progress));
				temp[0] = (destination.X - position.X) / DisplayForm.SCALE;
				temp[1] = (destination.Y - position.Y) / DisplayForm.SCALE;
			}
			if (progress > 0.99)
			{
				destination.Clear();
			}

			// Determine driving force vector
			if (temp[0] == 0.0f && temp[1] == 0.0f)
			{
				float speed = kinematics.Speed;
				if (speed >= 0.01f)
				{
					if (Math.Abs(happinessPercentChangeHistory) > 2.0f)
					{
						temp[0] = 2.0f * Math.Sign(happinessPercentChangeHistory) * kinematics.GetVelocity(0) / speed;
						temp[1] = 2.0f * Math.Sign(happinessPercentChangeHistory) * kinematics.GetVelocity(1) / speed;
					}
					else
					{
						temp[0] = happinessPercentChangeHistory * kinematics.GetVelocity(0) / speed;
						temp[1] = happinessPercentChangeHistory * kinematics.GetVelocity(1) / speed;
					}
				}
				else
				{
					temp[0] = 2.0f * (float)DisplayForm.GLOBALRANDOM.NextDouble() - 1.0f;
					temp[1] = 2.0f * (float)DisplayForm.GLOBALRANDOM.NextDouble() - 1.0f;
				}
			}
			temp[0] *= ELESPEED;
			temp[1] *= ELESPEED;

			// Apply force vector to kinematics; get and apply displacements
			temp = kinematics.GetDisplacement(temp, 1.0f).ToArray();
			position.X += temp[0];
			position.Y += temp[1];

			// Handle domain boundary collisions with elastic collisions (impacts performance)
			if (DisplayForm.BOUNDARYCOLLISIONS)
			{
				if (position.X < 0.0f)
				{
					position.X = 0.0f;
					kinematics.ReverseDirection(0);
				}
				else if (position.X > DisplayForm.SCALE)
				{
					position.X = (float)DisplayForm.SCALE;
					kinematics.ReverseDirection(0);
				}
				if (position.Y < 0.0f)
				{
					position.Y = 0.0f;
					kinematics.ReverseDirection(1);
				}
				else if (position.Y > DisplayForm.SCALE)
				{
					position.Y = (float)DisplayForm.SCALE;
					kinematics.ReverseDirection(1);
				}
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
			Vector localResourceLevels = GetLocalResourceLevels(environmentResources);
			int actionsThisTurn = (int)(2.0 * DisplayForm.GLOBALRANDOM.NextDouble() * Conscientiousness * MAXACTIONSCOUNT);
			while (actionsThisTurn-- > 0)
			{
				// Decide which action to do
				float maxActionPriority = -1.0f;
				int actionChoice = 0;
				for (int i = 0; i < KnownActions.Count; i++)
				{
					float priority = (float)DisplayForm.GLOBALRANDOM.NextDouble() * KnownActions[i].GetActionPriority(localResourceLevels, inventory);
					if (priority > maxActionPriority)
					{
						actionChoice = i;
						maxActionPriority = priority;
					}
				}
				// If any actions are available, execute the preferred action
				if (maxActionPriority > -Single.Epsilon)
				{
					ActionsTakenTotal++;
					// Apply action effects
					float learnMetric = inventory * prices;
					inventory -= KnownActions[actionChoice].Cost;
					resourceUse += KnownActions[actionChoice].Cost;
					Vector productionUtilityVector = KnownActions[actionChoice].DoAction(localResourceLevels, Intelligence);
					inventory += productionUtilityVector;
					resourceUse -= productionUtilityVector;
					happinessBonus += FOODREQUIREMENT * FOODREQUIREMENT * KnownActions[actionChoice].HappinessBonus;
					Health += KnownActions[actionChoice].HealthBonus / (0.1f * MIDDLEAGE);
					Mobility += KnownActions[actionChoice].MobilityBonus / MIDDLEAGE;
					lethalityBonus += KnownActions[actionChoice].LethalityBonus;
					// Update resource usage information and apply learning to action
					if (learnMetric != 0.0f)
					{
						learnMetric = happinessWeights.Wealth * (learnMetric - inventory * prices) / learnMetric;
					}
					if (Happiness != 0.0f)
					{
						learnMetric += FOODREQUIREMENT * KnownActions[actionChoice].HappinessBonus / Math.Abs(Happiness);
					}
					if (Health != 0.0f)
					{
						learnMetric += happinessWeights.Health * KnownActions[actionChoice].HealthBonus / Math.Abs(Health);
					}
					learnMetric = (learnMetric + happinessPercentChangeHistory) / 4.0f;
					KnownActions[actionChoice].Learn(learnMetric, (1.0f - Openness));
					// Check for new Action discovery (can discover either Harvest or Refinement Action)
					if (Math.Exp(DISCOVERYRATE * (KnownActions.Count - MAXACTIONSCOUNT))
						< StatFunctions.GaussRandom(DisplayForm.GLOBALRANDOM.NextDouble(), 5.0 * (Intelligence + Openness), 20.0 / (Intelligence + Openness)))
					{
						if (DisplayForm.GLOBALRANDOM.NextDouble() > 0.75)
						{
							KnownActions.Add(new HarvestAction(inventory.Count, DisplayForm.GLOBALRANDOM, localResourceLevels, inventory));
						}
						else
						{
							// Check for new Resource discovery
							if (Math.Exp(3.0 * DISCOVERYRATE * (inventory.Count - MAXRESOURCECOUNT))
								< StatFunctions.GaussRandom(DisplayForm.GLOBALRANDOM.NextDouble(), 25.0 * (Intelligence + Openness), 100.0 / (Intelligence + Openness))
								&& !resourceDiscovered.HasValue)
							{
								resourceDiscovered = false; // Not a food resource (null is no resource)
								if (DisplayForm.GLOBALRANDOM.NextDouble() > 0.5)
								{
									FoodResources.Add(new FoodResourceData(inventory.Count - 1, 1.0f - (float)DisplayForm.GLOBALRANDOM.NextDouble()));
									resourceDiscovered = true; // Is a food resource (null is no resource)
								}
							}
							// Add newly invented Action
							KnownActions.Add(new RefinementAction(inventory.Count, DisplayForm.GLOBALRANDOM, inventory));
						}
					}
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
			// Clear recent kills
			RecentAttacks.Clear();

			// Update resourceUse. This portion must be called every turn. Used to drive trade decisions
			for (int i = 0; i < resourceUse.Count; i++)
			{
				if (resourceUse[i] > 0.0f)
				{
					resourceUse[i] = (1.0f - timePreference) * (1.0f / (Agreeableness + 0.5f)) * resourceUse[i];
				}
				else
				{
					resourceUse[i] = (1.0f - timePreference) * (1.0f / (1.5f - Agreeableness)) * resourceUse[i];
				}
			}

			List<Element> children = new List<Element>();
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
						Mingle(otherElement);
					}
					double actionChoice = StatFunctions.GaussRandom(random.NextDouble(),
						INTERACTIONCHOICESCALE + relationships[otherElement],
						INTERACTIONCHOICESCALE - relationships[otherElement]);
					if (actionChoice > 1.0f / (1.0f + REPRODUCTIONCHANCE)
						&& this != otherElement
						&& this.Age > MIDDLEAGE / 4.0f
						&& otherElement.Age > MIDDLEAGE / 4.0f
						&& (INCESTALLOWED || (Parents[0] != otherElement.Name
							&& Parents[1] != otherElement.Name
							&& otherElement.Parents[0] != this.Name
							&& otherElement.Parents[1] != this.Name)))
					{
						// Mate
						Element child = new Element(this, otherElement);
						relationships.Add(child, Conscientiousness * INTERACTIONCHOICESCALE);
						children.Add(child);
						_health -= Age / MIDDLEAGE;
					}
					if (actionChoice > 1.0f / (1.0f + MINGLECHANCE))
					{
						// Mingle
						Mingle(otherElement);
					}
					if (actionChoice > 1.0f / (1.0f + TRADECHANCE))
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
						float direction = -1.0f;
						tradeProposal = direction * tradeProposal;
						if (!otherElement.EvaluateTradeProposal(this, ref tradeProposal))
						{
							direction = 1.0f;
							if (!EvaluateTradeProposal(otherElement, ref tradeProposal))
							{
								continue;
							}
						}

						/*
						 * Interrogation code (not functionally critical)
						*/
						float normTradeValue1 = direction * tradeProposal * prices / prices.Magnitude;
						float normTradeValue2 = -1.0f * direction * tradeProposal * otherElement.prices / otherElement.prices.Magnitude;
						float netValue = normTradeValue1 + normTradeValue2;
						/*
						*/
						ExecuteTrade(direction * tradeProposal);
						otherElement.ExecuteTrade(-1.0f * direction * tradeProposal);
					}
					if (actionChoice < ATTACKCHANCE
						&& Lethality > otherElement.Health)
					{
						// Attack
						if (random.NextDouble() * Lethality > random.NextDouble() * otherElement.Lethality)
						{
							inventory = inventory + otherElement.GetRobbed();
							otherElement.Die();
							lethalityBonus *= (1.0f + 5.0f / (1.0f + (float)Math.Exp(lethalityBonus)));
						}
						else if (random.NextDouble() * Lethality < random.NextDouble() * otherElement.Lethality)
						{
							this.Die();
							return children;
						}
						RecentAttacks.Add(otherElement.Name);
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
			if (!relationships.ContainsKey(sender))
			{
				relationships.Add(sender, Openness - Neuroticism);
			}
			relationships[sender] += (2.0f * (float)RELATIONSHIPSCALE / (2.0f * MIDDLEAGE * INTERACTCOUNT / DisplayForm.ELEMENTCOUNT)
				* ((float)StatFunctions.Sigmoid(tradeValue, -1.0, 0.0) - 0.5f));

			if (Extraversion > DisplayForm.GLOBALRANDOM.NextDouble())
			{
				Mingle(sender);
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
			// Update prices
			for (int i = 0; i < prices.Count; i++)
			{
				if (effectivePrices[i] == 0.0f)
				{
					continue;
				}
				prices[i] = ((1.0f - timePreference) * Math.Abs(tradeProposal[i]) * prices[i] + timePreference * cumulativePriceExperience[i] * effectivePrices[i])
					/ ((1.0f - timePreference) * Math.Abs(tradeProposal[i]) + timePreference * cumulativePriceExperience[i]);
				cumulativePriceExperience[i] += Math.Abs(tradeProposal[i]);
			}

			// Make decision and return
			Vector thisTradeProposal = new Vector(resourceUse);
			RefineTradeProposal(sender, ref thisTradeProposal);
			float targetVal = relationships[sender] / (float)RELATIONSHIPSCALE;

			if ((tradeValue / (tradeProposal.Magnitude * prices.Magnitude) - targetVal) > (1.0f - Agreeableness)
				|| ((tradeProposal * thisTradeProposal) / (tradeProposal.Magnitude * thisTradeProposal.Magnitude)) > (1.0f - Agreeableness))
			{
				ScaleTrade(ref tradeProposal);
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
			if ((relationships[otherElement] < 0.001 * RELATIONSHIPSCALE) && (relationships[otherElement] > -0.001 * RELATIONSHIPSCALE))
			{
				tradeValueTarget = relationships[otherElement] / (float)RELATIONSHIPSCALE;
			}
			else
			{
				tradeValueTarget = relationships[otherElement];
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
			ScaleTrade(ref baseProposal);
		}

		/// <summary>
		/// Helper function to scale a trade proposal such that it will not have costs exceeding this Element's inventory.
		/// </summary>
		/// <param name="tradeProposal"> Vector of proposed trades. </param>
		private void ScaleTrade(ref Vector tradeProposal)
		{
			float tradePropScaling = 1.0f;
			float tradeMag = tradeProposal.Magnitude;
			for (int i = 0; i < tradeProposal.Count; i++)
			{
				if (Single.IsInfinity(tradeProposal[i]) || Single.IsNaN(tradeProposal[i]) || (Math.Abs(tradeProposal[i]) / tradeMag) < TRADEROUNDOFF)
				{
					tradeProposal[i] = 0.0f;
				}
				else if (tradeProposal[i] < -(1.0f - TRADEROUNDOFF) * inventory[i])
				{
					tradePropScaling = Math.Max(0.0f, Math.Min(tradePropScaling, (-(1.0f - TRADEROUNDOFF) * inventory[i]) / tradeProposal[i]));
				}
			}
			tradeProposal = tradePropScaling * tradeProposal;
			if (tradeProposal.Magnitude < TRADEROUNDOFF)
			{
				tradeProposal = new Vector(tradeProposal.Count);
			}
		}

		/// <summary>
		/// Method to carry out the input trade. Trade will be added to this Element's inventory.
		/// </summary>
		/// <param name="trade"> Input trade to be added to this Element's inventory. </param>
		public void ExecuteTrade(Vector trade)
		{
			inventory += trade;
			resourceUse -= trade;
		}

		/// <summary>
		/// Gain relationiship based upon happinessWeights similarity.
		/// </summary>
		/// <param name="values"></param>
		/// <returns></returns>
		public HappinessWeights Mingle(Element otherElement)
		{
			// Check for and apply parental relationship changes
			if (this.Parents[0] == otherElement.Name || this.Parents[1] == otherElement.Name)
			{
				if (!relationships.ContainsKey(otherElement))
				{
					relationships.Add(otherElement, (float)(0.5 * INTERACTIONCHOICESCALE * StatFunctions.Sigmoid(MIDDLEAGE / 2.0, -2.0 / MIDDLEAGE, Age) - 0.25 * INTERACTIONCHOICESCALE));
				}
				else
				{
					relationships[otherElement] += (float)RELATIONSHIPSCALE / (Age + 2.0f * MIDDLEAGE * INTERACTCOUNT / DisplayForm.ELEMENTCOUNT);
				}
			}

			// Apply standard relationship changes
			if (!relationships.ContainsKey(otherElement))
			{
				relationships.Add(otherElement, Openness - Neuroticism);
			}
			relationships[otherElement] += (float)RELATIONSHIPSCALE / (2.0f * MIDDLEAGE * INTERACTCOUNT / DisplayForm.ELEMENTCOUNT)
				 * (4.0f * ((otherElement.HappinessWeights[0] - 0.5f) * (happinessWeights[0] - 0.5f)
					+ (otherElement.HappinessWeights[1] - 0.5f) * (happinessWeights[1] - 0.5f)
					+ (otherElement.HappinessWeights[2] - 0.5f) * (happinessWeights[2] - 0.5f)) / 3.0f);
			foreach (int victim in otherElement.RecentAttacks)
			{
				foreach (Element relation in relationships.Keys)
				{
					if (relation.Name == victim)
					{
						relationships[otherElement] -= relationships[relation];
						break;
					}
				}
			}

			// Update "moral values"
			happinessWeights[0] += Agreeableness * Openness * (otherElement.HappinessWeights[0] - happinessWeights[0]) / (2.0f * MIDDLEAGE * INTERACTCOUNT / DisplayForm.ELEMENTCOUNT);
			happinessWeights[1] += Agreeableness * Openness * (otherElement.HappinessWeights[1] - happinessWeights[1]) / (2.0f * MIDDLEAGE * INTERACTCOUNT / DisplayForm.ELEMENTCOUNT);
			happinessWeights[2] += Agreeableness * Openness * (otherElement.HappinessWeights[2] - happinessWeights[2]) / (2.0f * MIDDLEAGE * INTERACTCOUNT / DisplayForm.ELEMENTCOUNT);

			// Check for learning actions, locations, and relationships
			if (Math.Exp(KNOWLEDGETRANSFERRATE * (KnownActions.Count - MAXACTIONSCOUNT))
				< StatFunctions.GaussRandom(DisplayForm.GLOBALRANDOM.NextDouble(), 10.0f * Intelligence, 10.0 / Intelligence))
			{
				double randomNumber = DisplayForm.GLOBALRANDOM.NextDouble();
				double cumulativeChance = 0.0f;
				foreach (Action action in otherElement.KnownActions)
				{
					cumulativeChance += action.UseCount;
					if (randomNumber < cumulativeChance / otherElement.ActionsTakenTotal)
					{
						this.LearnAction(action);
						break;
					}
				}
			}
			if (Math.Exp(KNOWLEDGETRANSFERRATE * (KnownLocations.Count - MAXLOCATIONSCOUNT))
				< StatFunctions.GaussRandom(DisplayForm.GLOBALRANDOM.NextDouble(), 10.0 * Openness, 10.0 / Openness))
			{
				double randomNumber = DisplayForm.GLOBALRANDOM.NextDouble();
				double maxChance = 0.0f;
				foreach (KnownLocation location in otherElement.KnownLocations)
				{
					if (location.Preference > 0.0f)
					{
						maxChance += location.Preference;
					}
				}
				if (maxChance != 0.0f)
				{
					double cumulativeChance = 0.0f;
					foreach (KnownLocation location in otherElement.KnownLocations)
					{
						if (location.Preference < 0.0f)
						{
							continue;
						}
						cumulativeChance += location.Preference;
						if (randomNumber < cumulativeChance / maxChance)
						{
							this.LearnLocation(location);
							break;
						}
					}
				}
			}
			if (Math.Exp(KNOWLEDGETRANSFERRATE * (relationships.Count - MAXRELATIONSHIPS))
				< StatFunctions.GaussRandom(DisplayForm.GLOBALRANDOM.NextDouble(), 10.0 * Extraversion, 10.0 / Extraversion))
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
			relationships[subject] += Agreeableness * Extraversion * (otherElementRelationship - relationships[subject]);

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
		public void LearnLocation(KnownLocation location)
		{
			for (int i = 0; i < KnownLocations.Count; i++)
			{
				if (Math.Sqrt((KnownLocations[i].Location.X - location.Location.X) * (KnownLocations[i].Location.X - location.Location.X)
					+ (KnownLocations[i].Location.Y - location.Location.Y) * (KnownLocations[i].Location.Y - location.Location.Y))
					< DisplayForm.SCALE / 100.0)
				{
					KnownLocations[i].Preference = (1.0f - Agreeableness) * KnownLocations[i].Preference + Agreeableness * location.Preference;
					return;
				}
			}
			KnownLocations.Add(new KnownLocation(location, Agreeableness));
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
			Health += 2.0f * (float)StatFunctions.Sigmoid(Age, MIDDLEAGE, MIDDLEAGE) - 1.0f;
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
			for (int i = 0; i < relationships.Keys.Count; i++)
			{
				Element e = relationships.Keys.ToArray()[i];
				if (e.IsDead)
				{
					relationships.Remove(e);
					continue;
				}
				if (relationships[e] > maxOpinion)
				{
					inheretor = e;
					maxOpinion = relationships[e];
				}
			}
			if (inheretor != this)
			{
				inheretor.ExecuteTrade(INHERITANCE * inventory);
				this.ExecuteTrade(-INHERITANCE * inventory);
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
			Parents[0] = parent1.Name;
			Parents[1] = parent2.Name;
			Name = DisplayForm.GLOBALRANDOM.Next();
			Age = 0;
			IsDead = false;
			RecentAttacks = new List<int>();

			position.X = parent1.Position.X;
			position.Y = parent1.Position.Y;
			KnownLocations = new List<KnownLocation>();
			KnownLocations.Add(new KnownLocation(position));
			int r = (int)((1.0 - COLORMUTATIONRATE) * (parent1.ElementColor.R + parent2.ElementColor.R) / 2);
			int g = (int)((1.0 - COLORMUTATIONRATE) * (parent1.ElementColor.G + parent2.ElementColor.G) / 2);
			int b = (int)((1.0 - COLORMUTATIONRATE) * (parent1.ElementColor.B + parent2.ElementColor.B) / 2);
			r += (int)(COLORMUTATIONRATE * (255.0 / (1.0 + Math.Exp((15.0 / DisplayForm.SCALE) * (position.X - DisplayForm.SCALE / 2.0)))));
			g += (int)(COLORMUTATIONRATE * (255.0 / (1.0 + Math.Exp((15.0 / DisplayForm.SCALE) * (position.X * position.Y / (2 * DisplayForm.SCALE * DisplayForm.SCALE))))));
			b += (int)(COLORMUTATIONRATE * (255.0 / (1.0 + Math.Exp((15.0 / DisplayForm.SCALE) * (position.Y - DisplayForm.SCALE / 2.0)))));
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
			Health = ((parent1.Health / parent1.Age + parent2.Health / parent2.Age)
				+ (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD))
				/ INFANTMORTALITY;
			lethalityBonus = Health;
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
			startResources += (parent1.Conscientiousness * CHILDCOST) * parent1.Inventory;
			parent1.ExecuteTrade(-1.0f * startResources);
			startResources += (parent2.Conscientiousness * CHILDCOST) * parent2.Inventory;
			parent2.ExecuteTrade(-1.0f * (parent2.Conscientiousness * CHILDCOST) * parent2.Inventory);

			prices = new Vector(parent1.Inventory.Count);
			for (int i = 0; i < prices.Count; i++)
			{
				prices[i] = parent1.prices[i];
			}
			foodConsumptionRates = new Vector(FoodResources.Count);
			for (int i = 0; i < FoodResources.Count; i++)
			{
				foodConsumptionRates[i] = parent1.foodConsumptionRates[i];
			}
			timePreference = (float)StatFunctions.GaussRandom(Intelligence, TRAITSPREAD, TRAITSPREAD);
			resourceUse = new Vector(parent1.Inventory.Count);
			KnownActions = new List<Action>();
			inventory = new Vector(parent1.Inventory.Count);
			ExecuteTrade(startResources);
			cumulativePriceExperience = new Vector(parent1.Inventory.Count);
			for (int i = 0; i < inventory.Count; i++)
			{
				cumulativePriceExperience[i] = inventory[i];
			}
			Mingle(parent1);
			Mingle(parent2);
		}
	}
}