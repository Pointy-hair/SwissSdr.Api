using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
	public struct GeoCoordinate : IEquatable<GeoCoordinate>
	{
		public double Latitude { get; private set; }
		public double Longitude { get; private set; }

		public GeoCoordinate(double latitude, double longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		/// <summary>
		/// Creates a <see cref="GeoCoordinate"/> from a string of coordinates.
		/// </summary>
		/// <param name="value">The coordinate string ('Latitude,Longitude')</param>
		public static GeoCoordinate Parse(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentNullException(nameof(value));
			}

			GeoCoordinate result;
			if (!TryParse(value, out result))
			{
				throw new ArgumentException("must be two doubles separated by a comma (eg. 47.210221, 7.867063)", nameof(value));
			}

			return result;
		}
		public static bool TryParse(string value, out GeoCoordinate result)
		{
			if (!string.IsNullOrEmpty(value))
			{
				var coordinates = value.Split(',').Select(s => s.Trim().ToNullableDoubleInvariant()).ToArray();
				if (coordinates.Length == 2 || coordinates.All(n => n.HasValue))
				{
					result = new GeoCoordinate(coordinates[0].Value, coordinates[1].Value);
					return true;
				}
			}

			result = new GeoCoordinate();
			return false;
		}

		public override string ToString()
		{
			return string.Format("{0},{1}", Latitude, Longitude);
		}
		public override bool Equals(Object other)
		{
			return other is GeoCoordinate && Equals((GeoCoordinate)other);
		}

		public bool Equals(GeoCoordinate other)
		{
			return Latitude == other.Latitude && Longitude == other.Longitude;
		}

		public override int GetHashCode()
		{
			return Latitude.GetHashCode() ^ Longitude.GetHashCode();
		}
	}
}
