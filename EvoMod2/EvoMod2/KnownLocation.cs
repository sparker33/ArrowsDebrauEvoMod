using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace EvoMod2
{
	public class KnownLocation
	{
		public PointF Location { get; private set; }
		public float Preference { get; private set; }

		public KnownLocation(PointF point)
		{
			Location = new PointF(point.X, point.Y);
			Preference = 0.0f;
		}

		public KnownLocation(KnownLocation location)
		{
			Location = new PointF(location.Location.X, location.Location.Y);
			Preference = location.Preference;
		}

		/// <summary>
		/// Updates this location's preference based on current proximity and happiness gradient.
		/// </summary>
		/// <param name="currentLocation"> Element's current location. </param>
		/// <param name="happinessChange"> Element's current change in happiness level. </param>
		public void UpdatePreference(PointF currentLocation, float happinessChange)
		{
			float proximity = (float)Math.Sqrt(((Location.X - currentLocation.X) * (Location.X - currentLocation.X)
				+ (Location.Y - currentLocation.Y) * (Location.Y - currentLocation.Y)) / 2.0) / DisplayForm.SCALE;
			Preference += proximity * happinessChange;
		}
	}
}
