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
		// Private fields
		private PointF position;
		private Kinematics kinematics;
		private Vector deltaResources;
		private Vector localResourceLevels;
		private Vector ownedResourceVolumes;
		private Matrix resourceExchangeRules;
		private Matrix moveRules;

		// Public accessors
		public Color ElementColor { get; private set; }
		public PointF Position { get => position; private set => position = value; }
		public readonly int Size;

		public Element(Random random)
		{

		}

		public void UpdateLocalResourceLevels(List<List<ResourceKernel>> nodes)
		{
			for (int i = 0; i < nodes.Count; i++)
			{
				deltaResources[i] = localResourceLevels[i];
				localResourceLevels[i] = 0.0f;
				for (int j = 0; j < nodes[i].Count; j++)
				{
					localResourceLevels[i] += nodes[i][j].GetResourceLevelAt(this.Position);
				}
				localResourceLevels.Magnitude = 1.0f;
				deltaResources = localResourceLevels - deltaResources;
			}
		}

		public List<ResourceKernel> ExchangeResources()
		{
			Vector exchangeVolumes = resourceExchangeRules * localResourceLevels;
			List<ResourceKernel> drops = new List<ResourceKernel>(localResourceLevels.Count);
			for (int i = 0; i < exchangeVolumes.Count; i++)
			{
				drops.Add(new ResourceKernel(-exchangeVolumes[i], this.Position));
				drops[i].ZeroMoveMatrix();
			}
			ownedResourceVolumes = ownedResourceVolumes + exchangeVolumes;

			return drops;
		}

		public void Move()
		{
			float[] temp = new float[2];
			try
			{
				temp[0] = (1.0f / kinematics.GetVelocity(0)) * moveRules[0] * deltaResources;
			}
			catch (DivideByZeroException)
			{
				temp[0] = moveRules[0].Magnitude;
			}
			try
			{
				temp[1] = (1.0f / kinematics.GetVelocity(1)) * moveRules[1] * deltaResources;
			}
			catch (DivideByZeroException)
			{
				temp[1] = moveRules[1].Magnitude;
			}
			temp = kinematics.GetDisplacement(temp, ownedResourceVolumes.Magnitude).ToArray();
			position.X += temp[0];
			position.Y += temp[1];
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
	}
}
