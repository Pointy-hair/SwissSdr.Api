using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissSdr.Api
{
	public static class StringExtensions
	{
		public static string ToTitleCase(this string str)
		{
			return string.Join(" ", str
				.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => s.Substring(0, 1).ToUpper() + s.Substring(1)));
		}

		// from https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/Utilities/StringUtils.cs
		public static string ToCamelCase(this string s)
		{
			if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
			{
				return s;
			}

			char[] chars = s.ToCharArray();

			for (int i = 0; i < chars.Length; i++)
			{
				if (i == 1 && !char.IsUpper(chars[i]))
				{
					break;
				}

				bool hasNext = (i + 1 < chars.Length);
				if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
				{
					break;
				}

				char c;
				c = char.ToLowerInvariant(chars[i]);
				chars[i] = c;
			}

			return new string(chars);
		}

		public static int? ToNullableInt(this string s)
		{
			int result;
			if (int.TryParse(s, out result))
			{
				return result;
			}
			return null;
		}

		public static double? ToNullableDouble(this string s)
		{
			double result;
			if (double.TryParse(s, out result))
			{
				return result;
			}
			return null;
		}

		public static DateTime? ToNullableDateTime(this string s)
		{
			DateTime result;
			if (DateTime.TryParse(s, out result))
			{
				return result;
			}
			return null;
		}
	}
}
