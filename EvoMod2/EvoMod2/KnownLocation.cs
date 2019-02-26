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
		public float Preference { get; set; }

		public KnownLocation(PointF point)
		{
			Location = new PointF(point.X, point.Y);
			Preference = 0.0f;
		}

		/// <summary>
		/// Generates new KnownLocation that copies an existing location, inhereting preference multiplied by the preferenceScaling input.
		/// </summary>
		/// <param name="location"> Location to copy. </param>
		/// <param name="preferenceScaling"> Scale factor for preference inheretance. </param>
		public KnownLocation(KnownLocation location, float preferenceScaling)
		{
			Location = new PointF(location.Location.X, location.Location.Y);
			Preference = preferenceScaling * location.Preference;
		}
	}
}
