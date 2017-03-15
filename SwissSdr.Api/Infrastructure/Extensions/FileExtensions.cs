using SwissSdr.Datamodel;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SwissSdr.Api
{
	public static class FileExtensions
	{
		private static char[] _invalidChars = System.IO.Path.GetInvalidFileNameChars();

		public static string GetSafeFilename(this File file)
		{
			var chars = file.Name
				.ValueByCascade()
				.SelectMany(c => // replace umlauts with correct sequences
					{
					switch (c)
					{
						case 'ä':
						case 'ö':
						case 'ü':
						case 'Ä':
						case 'Ö':
						case 'Ü':
							return new[] { c, 'e' };
						default:
							return new[] { c };
					}
				})
				.ToArray();

			chars = new string(chars).Normalize(NormalizationForm.FormD)
					.Select(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark ? new char?(c) : null) // remove diacritics
					.Select(c => 31 < c && c < 127 ? c : null) // limit to ascii
					.UnboxNullable()
					.Where(c => !_invalidChars.Contains(c)) // remove characters invalid in a filename
					.Concat(file.Extension)
					.ToArray();

			return new string(chars).Normalize(NormalizationForm.FormC);
		}
	}
}
