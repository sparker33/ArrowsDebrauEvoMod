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
		public static float TRAITSPREAD;

		// Private fields
		private PointF position = new PointF();
		private Kinematics kinematics = new Kinematics(2);
		private Destination destination = new Destination();
		// Dynamic traits
		private float happiness;
		private float health;
		private float mobility;
		// Fixed traits
		private float intelligence;
		private float conscientiousness;
		private float agreeableness;
		private float neuroticism;
		private float openness;
		private float extraversion;
		// Additional objects
		private float happinessBonus;
		private float[] happinessWeights = new float[3]; // 0: wealth, 1: health, 2: location
		private float[] interactionCoefficients = new float[2];
		private TraderModule trader;
		private MatrixMath.Matrix resourceUseHistory;
		private Vector happinessPercentChangeHistory;
		private Vector percentTradesSuccessfulHistory;

		// Public
		public int Age { get; private set; }
		public List<PointF> KnownLocations { get; private set; }
		public PointF Position { get => position; private set => position = value; }
		public int Size { get => 10; }
		public Color ElementColor { get; private set; }

		/// <summary>
		/// Default class constructor. Not intended for use
		/// </summary>
		private Element()
		{

		}

		/// <summary>
		/// Basic class constructor with initial physics configuration
		/// </summary>
		/// <param name="random"> Randomizer. </param>
		public Element(Random random)
		{
			Age = 0;

			position.X = (float)(random.NextDouble() * DisplayForm.SCALE);
			position.Y = (float)(random.NextDouble() * DisplayForm.SCALE);
			KnownLocations.Add(position);
			int r = (int)(255.0 / (1.0 + Math.Exp((15.0 / DisplayForm.SCALE) * (position.X - DisplayForm.SCALE / 2.0))));
			int g = (int)(255.0 / (1.0 + Math.Exp((15.0 / DisplayForm.SCALE) * (position.X * position.Y / (2 * DisplayForm.SCALE * DisplayForm.SCALE)))));
			int b = (int)(255.0 / (1.0 + Math.Exp((15.0 / DisplayForm.SCALE) * (position.Y - DisplayForm.SCALE / 2.0))));
			ElementColor = Color.FromArgb(r, g, b);

			double rand = random.NextDouble();
			intelligence = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = random.NextDouble();
			conscientiousness = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = random.NextDouble();
			agreeableness = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = random.NextDouble();
			neuroticism = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = random.NextDouble();
			openness = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			rand = random.NextDouble();
			extraversion = (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);

			rand = random.NextDouble();
			health = 3000.0f * (float)StatFunctions.GaussRandom(rand, TRAITSPREAD, TRAITSPREAD);
			mobility = DisplayForm.ELESPEED;
		}

		/// <summary>
		/// Method to add a resource to the list of possible resources and all affected components.
		/// </summary>
		public void AddResource()
		{
			trader.AddResource();
			resourceUseHistory.InsertColumn(resourceUseHistory.Count);
			resourceUseHistory.Add(new Vector(resourceUseHistory.Count));
		}

		/// <summary>
		/// Method to updte the position of this element
		/// </summary>
		public void Move()
		{
			float[] temp = new float[2]; // Utility array to hold destination distance, accelleration, and displacement

			// Check for destination acquisition
			if (destination.IsEmpty && /*meet criteria for new destination == */true)
			{
				PointF newLocation = new PointF();
				destination.Set(this.position, newLocation);
			}
			else if (!destination.IsEmpty && /*meet criteria to cancel destination == */true)
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
				temp[0] = happinessPercentChangeHistory[0] * kinematics.GetVelocity(0) / speed + temp[0] / DisplayForm.SCALE;
				temp[1] = happinessPercentChangeHistory[0] * kinematics.GetVelocity(1) / speed + temp[1] / DisplayForm.SCALE;
			}
			else
			{
				temp[0] = 0.0f;
				temp[1] = 0.0f;
			}

			// Apply force vector to kinematics; get and apply displacements
			if (mobility != 0.0f)
			{
				temp = kinematics.GetDisplacement(temp, 1.0f / mobility).ToArray();
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

		public void DoAction(List<Resource> environmentResources)
		{
			// Make decision

			// Execute action
		}

		public void DoInteraction(Random random, Element otherElement)
		{
			// Make decision

			// Execute interaction
		}

		/// <summary>
		/// Method to make this element reproduce
		/// </summary>
		/// <param name="random"> Random variable for mutation. </param>
		/// <param name="mutationChance"> Decimal chance of random mutation. </param>
		/// <returns> The progeny element. </returns>
		public Element Reproduce(Element mate, Random random, float mutationChance)
		{
			return new Element(this, mate);
		}

		/// <summary>
		/// Class constructor for reproduction method
		/// </summary>
		public Element(Element parent1, Element parent2)
		{
			position.X = parent1.Position.X;
			position.Y = parent1.Position.Y;
		}

		/// <summary>
		/// Method to update this element's decision matrices
		/// </summary>
		/// <param name="environmentHappiness"></param>
		public void Learn(float environmentHappiness)
		{
			// Update happiness
			float nextHappiness = happinessBonus; // add formula ?(bonus + weights * values)?
			happinessPercentChangeHistory.RemoveAt(happinessPercentChangeHistory.Count);
			happinessPercentChangeHistory.Insert(0, (nextHappiness - happiness) / happiness);
			happiness = nextHappiness;
			// Train decision matrices
			trader.Train(resourceUseHistory, happinessPercentChangeHistory, percentTradesSuccessfulHistory);
		}
	}
}
