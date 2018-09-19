using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatrixMath;
using System.Drawing;

namespace EvoMod2
{
	public class ResourceKernel : GaussKernel
	{
		// Global values
		public static float DispersionThreshold;

		// Private objects
		private PointF position;
		private Matrix moveMatrix;

		// Public objects
		public float Volume { get; set; }
		public PointF Position
		{
			get
			{
				return position;
			}
			set
			{
				mu[0] = value.X;
				mu[1] = value.Y;
			}
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public ResourceKernel() : base(2)
		{
			H[0][0] = 1.0f;
			H[1][1] = 1.0f;
			H[1][0] = 0.0f;
			position = new PointF(mu[0], mu[1]);
			moveMatrix = new Matrix(2, 2);
			Volume = 500.0f;
		}

		public float GetResourceLevelAt(PointF location)
		{
			Vector v = new Vector(2);
			v[0] = location.X;
			v[1] = location.Y;
			return (this.Volume * this.ProbabilityAt(v));
		}

		public float Update(Random random)
		{
			Vector v = new Vector();
			v[0] = (float)(2.0f * random.NextDouble() - 1.0f);
			v[1] = (float)(2.0f * random.NextDouble() - 1.0f);
			this.Move(v);
			return this.Spread();
		}

		private void Move(Vector v)
		{
			Vector dX = moveMatrix * v;
			position.X += dX[0];
			position.Y += dX[1];
			if (position.X < 0.0f)
			{
				position.X = 5.0f;
				moveMatrix[0] = -1.0f * moveMatrix[0];
			}
			if (position.X > DisplayForm.SCALE)
			{
				position.X = DisplayForm.SCALE - 5.0f;
				moveMatrix[0] = -1.0f * moveMatrix[0];
			}
			if (position.Y < 0.0f)
			{
				position.Y = 5.0f;
				moveMatrix[1] = -1.0f * moveMatrix[1];
			}
			if (position.Y > DisplayForm.SCALE)
			{
				position.Y = DisplayForm.SCALE - 5.0f;
				moveMatrix[1] = -1.0f * moveMatrix[1];
			}
		}

		private float Spread()
		{
			this.H = (1.0f / Volume) * this.H + this.H;
			if (Volume / H[0][0] < DispersionThreshold)
			{
				return Volume;
			}
			return 0.0f;
		}
	}
}
