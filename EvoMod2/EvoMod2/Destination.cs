using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace EvoMod2
{
	public class Destination
	{
		// Private objects
		private float startDistance;
		private PointF destination;

		// Public objects
		public float X { get => destination.X; }
		public float Y { get => destination.Y; }
		public bool IsEmpty { get => destination.IsEmpty; }

		/// <summary>
		/// Default empty constructor
		/// </summary>
		public Destination()
		{
			Clear();
		}

		/// <summary>
		/// Constructor taking inputs of current and intended destinations
		/// </summary>
		/// <param name="currentLocation"> Current location PointF. </param>
		/// <param name="destination"> Desired destination PointF. </param>
		public Destination(PointF currentLocation, PointF destination)
		{
			if (!currentLocation.IsEmpty && !destination.IsEmpty)
			{
				Set(currentLocation, destination);
			}
			else
			{
				Clear();
			}
		}

		/// <summary>
		/// Method to get percent of trip yet to be completed.
		/// </summary>
		/// <param name="currentLocation"> Current location PointF. </param>
		/// <returns> Float between 0.0 (arrived at destination) and 1.0 (at starting point). </returns>
		public float GetProgress(PointF currentLocation)
		{
			float xDist = destination.X - currentLocation.X;
			float yDist = destination.Y - currentLocation.Y;
			return ((float)Math.Sqrt(xDist * xDist + yDist * yDist) / startDistance);
		}

		// Method to empty this Destination
		public void Clear()
		{
			startDistance = 0.0f;
			destination = new PointF();
		}

		/// <summary>
		/// Method to establish a destination.
		/// </summary>
		/// <param name="currentLocation"> Current location PointF. </param>
		/// <param name="destination"> Desired destination PointF. </param>
		public void Set(PointF currentLocation, PointF destination)
		{
			destination = new PointF(destination.X, destination.Y);
			startDistance = (float)Math.Sqrt((destination.X - currentLocation.X) * (destination.X - currentLocation.X) + (destination.Y - currentLocation.Y) * (destination.Y - currentLocation.Y));
		}
	}
}
