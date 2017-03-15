using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SwissSdr.Datamodel
{
    public static class StringExtensions
	{
		public static double? ToNullableDoubleInvariant(this string s)
		{
			if (double.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double result))
			{
				return result;
			}
			return null;
		}
	}
}
